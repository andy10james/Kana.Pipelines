using System;
using System.Threading.Tasks;

namespace Kana.Pipelines
{
    public class TypeMiddleware<TType, TState, TResult> : IMiddleware<TState, TResult> 
        where TType : class, IMiddleware<TState, TResult>
    {

        private readonly IServiceProvider _provider;

        public TypeMiddleware(IServiceProvider provider) =>
            this._provider = provider;

        public Task<TResult> ExecuteAsync(TState state, Func<Task<TResult>> next)
        {
            TType middleware = null;

            if (this._provider != null)
                middleware = this._provider.GetService(typeof(TType)) as TType;

            if (middleware == null) 
                try
                {
                    middleware = Activator.CreateInstance<TType>();
                } 
                catch (MissingMemberException e)
                {
                    throw new TypeInitializationException($"Cannot initialize Middleware type {typeof(TType)}. No service provider was provided and no parameterless constructor existed on the type.", e);
                }

            return middleware.ExecuteAsync(state, next);
        }
    }

    public class TypeMiddleware<TType, TState> : IMiddleware<TState>
        where TType : class, IMiddleware<TState>
    {

        private readonly IServiceProvider _provider;

        public TypeMiddleware(IServiceProvider provider) =>
            this._provider = provider;

        public Task ExecuteAsync(TState state, Func<Task> next)
        {
            TType middleware = null;

            if (this._provider != null)
                middleware = this._provider.GetService(typeof(TType)) as TType;

            if (middleware == null)
                try
                {
                    middleware = Activator.CreateInstance<TType>();
                }
                catch (MissingMemberException e)
                {
                    throw new TypeInitializationException($"Cannot initialize Middleware type {typeof(TType)}. No service provider was provided and no parameterless constructor existed on the type.", e);
                }

            return middleware.ExecuteAsync(state, next);
        }
    }

}
