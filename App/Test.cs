using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Sk.InspectorAop;

namespace App
{
    public interface ITest
    {
        int GetInt(double d);
    }
    public class Test : ITest
    {
        public virtual int GetInt(double d)
        {
            return 1;
        }
    }
    public class TestProxy : Test
    {
        public override int GetInt(double d)
        {
            //Console.WriteLine("Proxy Class");
            return 1;
        }
    }

    public interface IHello
    {
        int GetInt(double d);
    }
    public class Hello : IHello
    {
        public virtual int GetInt(double d)
        {
            return 1;
        }
    }
    class Exec
    {
        public static void Work()
        {
            AopProxyBuilder.Init();

            Stopwatch watch = new Stopwatch();
            watch.Start();
            var dd = AopProxyBuilder.CreateProxy<ITest, Test, StandardInterceptor>();
            //dd.GetInt(1);
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds.ToString() + "ms");

            watch.Restart();
            var ss = AopProxyBuilder.CreateProxy<ITest, TestProxy, StandardInterceptor>();
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds.ToString() + "ms");


            watch.Restart();
            AopProxyBuilder.CreateProxy<IHello, Hello, StandardInterceptor>();
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds.ToString() + "ms");
        }
    }
}
