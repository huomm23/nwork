using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Async
{
    static public class AsyncExtensions
    {
        public static void Execte(this IEnumerable<IAsync> async)
        {
            IEnumerator<IAsync> iterator = async.GetEnumerator();
            AsyncExtensions.Run(iterator);
        }

        internal static void Run(IEnumerator<IAsync> iterator)
        {
            if (!iterator.MoveNext()) //UI 相关需要把这些放到UI线程上执行
                return;
            IAsync item = iterator.Current;
            item.ExecuteStep(() => AsyncExtensions.Run(iterator));
        }
    }
}
