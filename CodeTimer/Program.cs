using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CodeTimer
{
    class Program
    {
        static void Main(string[] args)
        {
            CodeTimer.Initialize();
            //CodeTimer.Time("Thread Sleep", 1, () => { Thread.Sleep(3000); });
            //CodeTimer.Time("Empty Method", 10000000, () => { });

            int iteration = 100 * 1000;

            string s = "";
            CodeTimer.Time("String Concat", iteration, () => { s += "a"; });

            StringBuilder sb = new StringBuilder();
            CodeTimer.Time("StringBuilder", iteration, () => { sb.Append("a"); });


            Console.ReadLine();
        }
    }
}
