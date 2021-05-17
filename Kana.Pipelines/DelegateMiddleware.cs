using System;
using System.Threading.Tasks;

namespace Kana.Pipelines
{
    public class DelegateMiddleware<TState, TResult> : IMiddleware<TState, TResult>
    {

        private readonly Middleware<TState, TResult> _middleware;

        public DelegateMiddleware(Middleware<TState, TResult> middleware) =>
            this._middleware = middleware;

        public DelegateMiddleware(Func<TState, TResult> middleware) =>
            this._middleware = (s, n) => Task.FromResult(middleware(s));

        public Task<TResult> ExecuteAsync(TState state, Func<Task<TResult>> next) =>
            this._middleware(state, next);
    }

    public class DelegateMiddleware<TState> : IMiddleware<TState>
    {

        private readonly Middleware<TState> _middleware;

        public DelegateMiddleware(Middleware<TState> middleware) =>
            this._middleware = middleware;

        public DelegateMiddleware(Action<TState> middleware) =>
            this._middleware = (s, n) => { middleware(s); return n(); };

        public Task ExecuteAsync(TState state, Func<Task> next) =>
            this._middleware(state, next);

    }
}
