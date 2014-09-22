using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Async
{
    public interface IAsync
    {
        void ExecuteStep(Action cout);
    }
    public class Result<T> : IAsync
    {
        public T ReturnValue { get; private set; }
        public Result(T value)
        {
            ReturnValue = value;
        }
        public void ExecuteStep(Action cout)
        {
            throw new InvalidOperationException
                  ("Cannot call ExecuteStep on IAsync create as 'Result'");
        }
    }
    public abstract class Async<T> : IAsync
    {
        protected T result;
        protected bool completed = false;
        public T Result
        {
            get
            {
                if (!completed) throw new Exception("Operation not completed, did you forgot 'yield return'?");
                return result;
            }
        }
        abstract public void ExecuteStep(Action cout);
    }

    public class AsyncPrimitive<T> : Async<T>
    {
        Action<Action<T>> func;

        public AsyncPrimitive(Func<AsyncCallback, object, IAsyncResult> begin, Func<IAsyncResult, T> end)
        {
            this.func = (next) =>
            {
                begin(ar =>
                {
                    next(end(ar));
                }, null);
            };
        }
        public override void ExecuteStep(Action cout)
        {
            func((res) =>
            {
                result = res;
                completed = true;
                cout();
            });
        }
    }
}
