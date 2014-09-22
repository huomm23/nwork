using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;

namespace Sk.InspectorAop
{
    /// <summary>
    /// thread safe
    /// </summary>
    public class AopProxyBuilder
    {
        private static System.Collections.Concurrent.ConcurrentDictionary<string, Type> typeDic = new ConcurrentDictionary<string, Type>();

        private static readonly Type VoidType = Type.GetType("System.Void");
        private static string namespacename = "Sky.AOP";

        private static string filename = "044628a3-ed44-400f-8f3d-eab88cc17643.dll";
        private static AssemblyBuilder assemblyBuilder;

        public static void Init()
        {
            //Console.WriteLine("Hello");
        }

        static AopProxyBuilder()  //静态构造函数 只会初始化一次
        {
            var assemblyName = new AssemblyName(namespacename);
            assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
        }

        private int _status = 0;

        public static I CreateProxy<I, T, K>()
            where T : I, new()
            where K : IInterceptor
        {
            Type classType = typeof(T);
            string keyName = classType.FullName;

            Type aopType = null;
            if (!typeDic.TryGetValue(keyName, out aopType))
            {
                ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(keyName, keyName + Guid.NewGuid().ToString());
                aopType = BulidType(classType, moduleBuilder, typeof(K));
                typeDic.TryAdd(keyName, aopType);   
            }

            return (I)Activator.CreateInstance(aopType);
        }

        private static Type BulidType(Type classType, ModuleBuilder moduleBuilder, Type intercetor)
        {
            string className = classType.Name + "_Proxy";

            //定义类型
            var typeBuilder = moduleBuilder.DefineType(className,
                                                       TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Class,
                                                       classType);
            //定义字段 _inspector
            var inspectorFieldBuilder = typeBuilder.DefineField("_inspector", typeof(IInterceptor),
                                                                FieldAttributes.Private | FieldAttributes.InitOnly);
            //构造函数
            BuildCtor(classType, inspectorFieldBuilder, typeBuilder, intercetor);

            //构造方法
            BuildMethod(classType, inspectorFieldBuilder, typeBuilder);
            Type aopType = typeBuilder.CreateType();
            return aopType;
        }

        private static void BuildMethod(Type classType, FieldBuilder inspectorFieldBuilder, TypeBuilder typeBuilder)
        {
            var methodInfos = classType.GetMethods();
            foreach (var methodInfo in methodInfos)
            {
                if (!methodInfo.IsVirtual && !methodInfo.IsAbstract) continue;
                if (methodInfo.Name == "ToString") continue;
                if (methodInfo.Name == "GetHashCode") continue;
                if (methodInfo.Name == "Equals") continue;

                var parameterInfos = methodInfo.GetParameters();
                var parameterTypes = parameterInfos.Select(p => p.ParameterType).ToArray();
                var parameterLength = parameterTypes.Length;
                var hasResult = methodInfo.ReturnType != VoidType;

                var methodBuilder = typeBuilder.DefineMethod(methodInfo.Name,
                                                             MethodAttributes.Public | MethodAttributes.Final |
                                                             MethodAttributes.Virtual
                                                             , methodInfo.ReturnType
                                                             , parameterTypes);

                var il = methodBuilder.GetILGenerator();

                //局部变量
                il.DeclareLocal(typeof(object)); //correlationState
                il.DeclareLocal(typeof(object)); //result
                il.DeclareLocal(typeof(object[])); //parameters

                //BeforeCall(string operationName, object[] inputs);
                il.Emit(OpCodes.Ldarg_0);

                il.Emit(OpCodes.Ldfld, inspectorFieldBuilder);//获取字段_inspector
                il.Emit(OpCodes.Ldstr, methodInfo.Name);//参数operationName

                if (parameterLength == 0)//判断方法参数长度
                {
                    il.Emit(OpCodes.Ldnull);//null -> 参数 inputs
                }
                else
                {
                    //创建new object[parameterLength];
                    il.Emit(OpCodes.Ldc_I4, parameterLength);
                    il.Emit(OpCodes.Newarr, typeof(Object));
                    il.Emit(OpCodes.Stloc_2);//压入局部变量2 parameters

                    for (int i = 0, j = 1; i < parameterLength; i++, j++)
                    {
                        //object[i] = arg[j]
                        il.Emit(OpCodes.Ldloc_2);
                        il.Emit(OpCodes.Ldc_I4, 0);
                        il.Emit(OpCodes.Ldarg, j);
                        if (parameterTypes[i].IsValueType) il.Emit(OpCodes.Box, parameterTypes[i]);//对值类型装箱
                        il.Emit(OpCodes.Stelem_Ref);
                    }
                    il.Emit(OpCodes.Ldloc_2);//取出局部变量2 parameters-> 参数 inputs
                }

                il.Emit(OpCodes.Callvirt, typeof(IInterceptor).GetMethod("BeforeCall"));//调用BeforeCall
                il.Emit(OpCodes.Stloc_0);//建返回压入局部变量0 correlationState

                //Call methodInfo
                il.Emit(OpCodes.Ldarg_0);
                //获取参数表
                for (int i = 1, length = parameterLength + 1; i < length; i++)
                {
                    il.Emit(OpCodes.Ldarg_S, i);
                }
                il.Emit(OpCodes.Call, methodInfo);
                //将返回值压入 局部变量1result void就压入null
                if (!hasResult) il.Emit(OpCodes.Ldnull);
                else if (methodInfo.ReturnType.IsValueType) il.Emit(OpCodes.Box, methodInfo.ReturnType);//对值类型装箱
                il.Emit(OpCodes.Stloc_1);

                //AfterCall(string operationName, object returnValue, object correlationState);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, inspectorFieldBuilder);//获取字段_inspector
                il.Emit(OpCodes.Ldstr, methodInfo.Name);//参数 operationName
                il.Emit(OpCodes.Ldloc_1);//局部变量1 result
                il.Emit(OpCodes.Ldloc_0);// 局部变量0 correlationState
                il.Emit(OpCodes.Callvirt, typeof(IInterceptor).GetMethod("AfterCall"));

                //result
                if (!hasResult)
                {
                    il.Emit(OpCodes.Ret);
                    return;
                }
                il.Emit(OpCodes.Ldloc_1);//非void取出局部变量1 result
                if (methodInfo.ReturnType.IsValueType) il.Emit(OpCodes.Unbox_Any, methodInfo.ReturnType);//对值类型拆箱
                il.Emit(OpCodes.Ret);
            }
        }

        private static void BuildCtor(Type classType, FieldBuilder inspectorFieldBuilder, TypeBuilder typeBuilder, Type intercetor)
        {
            var ctorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, Type.EmptyTypes);
            var il = ctorBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, classType.GetConstructor(Type.EmptyTypes));//调用base的默认ctor
            il.Emit(OpCodes.Ldarg_0);
            ConstructorInfo defaultConstructorInfo = intercetor.GetConstructor(Type.EmptyTypes);
            il.Emit(OpCodes.Newobj, defaultConstructorInfo);
            //将结果保存到字段_inspector
            il.Emit(OpCodes.Stfld, inspectorFieldBuilder);
            il.Emit(OpCodes.Ret);
        }
    }
}
