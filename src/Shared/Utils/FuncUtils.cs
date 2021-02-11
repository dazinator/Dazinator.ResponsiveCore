using System.Threading.Tasks;

namespace System
{
    public static class FuncUtils
    {



        /// <summary>
        /// Creates a factory that can be used concurrently to obtain <typeparamref name="TDisposable"/> instances, but will only keep the latest one alive, and will dispose of previous instances when the latest one is requested.
        /// </summary>
        /// <typeparam name="TDisposable"></typeparam>
        /// <param name="factory"></param>
        /// <returns></returns>
        public static Func<TDisposable> KeepLatest<TDisposable>(Func<TDisposable> factory, Action<TDisposable> onNewInstance = null)
    where TDisposable : IDisposable
        {
            TDisposable lastOne = default;
            var _lock = new object();

            Func<TDisposable> lastOneFactory = () =>
            {
                // only one thread should dispose or previos and intialise new source.
                lock (_lock)
                {
                    if (lastOne != null)
                    {
                        lastOne.Dispose();
                    }
                    lastOne = factory.Invoke();
                    onNewInstance?.Invoke(lastOne);
                    return lastOne;
                }
            };
            return lastOneFactory;
        }

        /// <summary>
        /// Creates a factory that can be used concurrently to obtain <typeparamref name="TDisposable"/> instances, but will only keep the latest one alive, and will dispose of previous instances when the latest one is requested.
        /// </summary>
        /// <typeparam name="TDisposable"></typeparam>
        /// <param name="factory"></param>
        /// <returns></returns>
        public static Func<TDisposable> KeepLatest<TDisposable>(Action<TDisposable> onNewInstance = null)
    where TDisposable : IDisposable, new()
        {
            return KeepLatest<TDisposable>(() => new TDisposable(), onNewInstance);
        }

        public static Func<TTarget> Convert<TSource, TTarget>(this Func<TSource> sourceFactory, Func<TSource, TTarget> transformer)
        {
            Func<TTarget> transformingFunc = () =>
            {
                var sourceInstance = sourceFactory();
                var targetInstance = transformer(sourceInstance);
                return targetInstance;
            };
            return transformingFunc;
        }


    //    /// <summary>
    //    /// Creates a factory that can be used concurrently to obtain <typeparamref name="TDisposable"/> instances, but will only keep the latest one alive, and will dispose of previous instances when the latest one is requested.
    //    /// </summary>
    //    /// <typeparam name="TDisposable"></typeparam>
    //    /// <param name="factory"></param>
    //    /// <returns></returns>
    //    public static Func<TArg, TDisposable> KeepLatest<TArg, TDisposable>(Func<TArg, TDisposable> factory, Action<TDisposable> onNewInstance = null)
    //where TDisposable : IDisposable
    //    {
    //        TDisposable lastOne = default;
    //        var _lock = new object();

    //        Func<TArg, TDisposable> lastOneFactory = (a) =>
    //        {
    //            // only one thread should dispose or previos and intialise new source.
    //            lock (_lock)
    //            {
    //                if (lastOne != null)
    //                {
    //                    lastOne.Dispose();
    //                }
    //                lastOne = factory.Invoke(a);
    //                onNewInstance?.Invoke(lastOne);
    //                return lastOne;
    //            }
    //        };
    //        return lastOneFactory;
    //    }


    //    /// <summary>
    //    /// Wraps an Action that takes one argument, with a Func. When the Func is invoked it will call the Action with the arugment specified, and will then fire a delegate to select a return value based on that argument.
    //    /// </summary>
    //    /// <typeparam name="TArg"></typeparam>
    //    /// <typeparam name="TTarget"></typeparam>
    //    /// <param name="inner"></param>
    //    /// <param name="arg"></param>
    //    /// <param name="getReturnValue"></param>
    //    /// <returns></returns>
    //    public static Func<TTarget> ToFunc<TArg, TTarget>(this Action<TArg> inner,
    //        TArg arg,
    //        Func<TArg, TTarget> getReturnValue)
    //    {
    //        Func<TTarget> changeTokenFactory = () =>
    //        {
    //            //var source = new CancellationTokenSource();
    //            inner.Invoke(arg);
    //            var result = getReturnValue(arg);

    //            //var changeToken = new CancellationChangeToken(source.Token);
    //            // return changeToken;
    //            return result;
    //        };

    //        return changeTokenFactory;
    //    }

    //    /// <summary>
    //    /// Wraps a Function with an Action, and returns the Action. When the Action is invoked, the function is called but its result is discarded.
    //    /// </summary>
    //    /// <typeparam name="TArg"></typeparam>
    //    /// <typeparam name="TResult"></typeparam>
    //    /// <param name="func"></param>
    //    /// <returns></returns>
    //    public static Action<TArg> ToAction<TArg, TResult>(this Func<TArg, TResult> func)
    //    {
    //        return (a) => func(a);            
    //    }
    }

}