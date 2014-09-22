using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sk.InspectorAop
{
    public class StandardInterceptor : IInterceptor
    {
        public object BeforeCall(string operationName, object[] inputs)
        {
            Console.WriteLine("Before call :{0}", operationName);
            return null;
        }

        public void AfterCall(string operationName, object returnValue, object correlationState)
        {
            Console.WriteLine("After call :{0} resule: {1}", operationName, returnValue ?? "Null");
        }
    }
}
