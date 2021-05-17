#pragma warning disable CS0693 // Type parameter has the same name as the type parameter from outer type

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kana.Pipelines
{
    public class Pipeline<TState, TResult> : IEnumerable<IMiddleware<TState, TResult>>
    {

        private readonly ICollection<IMiddleware<TState, TResult>> _middleware;
        private IServiceProvider _serviceProvider;

        #region Constructors
        public Pipeline() =>
            this._middleware = new List<IMiddleware<TState, TResult>>();

        public Pipeline(Pipeline<TState, TResult> existingPipeline) =>
            this._middleware = existingPipeline._middleware.ToList();

        public Pipeline(params IMiddleware<TState, TResult>[] middleware) =>
            this._middleware = new List<IMiddleware<TState, TResult>>(middleware);

        public Pipeline(IEnumerable<IMiddleware<TState, TResult>> middleware) =>
            this._middleware = middleware.ToList();

        public Pipeline(params Middleware<TState, TResult>[] middleware) : this((IEnumerable<Middleware<TState, TResult>>) middleware) { }

        public Pipeline(IEnumerable<Middleware<TState, TResult>> middleware) =>
            this._middleware = new List<IMiddleware<TState, TResult>>(middleware.Select(m => new DelegateMiddleware<TState, TResult>(m)));
        #endregion

        public Pipeline<TState, TResult> WithServiceProvider(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
            return this;
        }

        #region Pipeline Addition
        public Pipeline<TState, TResult> Add(params Middleware<TState, TResult>[] middleware) =>
            this.Add((IEnumerable<Middleware<TState, TResult>>)middleware);

        public Pipeline<TState, TResult> Add(params Func<TState, TResult>[] middleware) =>
            this.Add((IEnumerable<Func<TState, TResult>>)middleware);

        public Pipeline<TState, TResult> Add(params Pipeline<TState, TResult>[] pipelines) =>
            this.Add((IEnumerable<Pipeline<TState, TResult>>)pipelines);

        public Pipeline<TState, TResult> Add(params IMiddleware<TState, TResult>[] middlewares) =>
            this.Add((IEnumerable<IMiddleware<TState, TResult>>) middlewares);

        public Pipeline<TState, TResult> Add<TMiddleware>() where TMiddleware : class, IMiddleware<TState, TResult> =>
            this.Add(new TypeMiddleware<TMiddleware, TState, TResult>(this._serviceProvider));

        public Pipeline<TState, TResult> Add(IEnumerable<Middleware<TState, TResult>> middlewares)
        {
            foreach (var middleware in middlewares)
                this._middleware.Add(new DelegateMiddleware<TState, TResult>(middleware));
            return this;
        }

        public Pipeline<TState, TResult> Add(IEnumerable<Func<TState, TResult>> middlewares)
        {
            foreach (var middleware in middlewares)
                this._middleware.Add(new DelegateMiddleware<TState, TResult>(middleware));
            return this;
        }

        public Pipeline<TState, TResult> Add(IEnumerable<Pipeline<TState, TResult>> pipelines)
        {
            foreach (var pipeline in pipelines)
                this.Add(pipeline._middleware);
            return this;
        }

        public Pipeline<TState, TResult> Add(IEnumerable<IMiddleware<TState, TResult>> middlewares)
        {
            foreach (var middleware in middlewares)
                this._middleware.Add(middleware);
            return this;
        }
        #endregion

        #region Pipeline Arithmatic
        public static Pipeline<TState, TResult> operator +(Pipeline<TState, TResult> pipeline, Middleware<TState, TResult> middleware)
        {
            var newPipeline = new Pipeline<TState, TResult>(pipeline);
            newPipeline.Add(middleware);
            return newPipeline;
        }

        public static Pipeline<TState, TResult> operator +(Pipeline<TState, TResult> pipeline, Func<TState, TResult> middleware)
        {
            var newPipeline = new Pipeline<TState, TResult>(pipeline);
            newPipeline.Add(middleware);
            return newPipeline;
        }

        public static Pipeline<TState, TResult> operator +(Pipeline<TState, TResult> pipeline, IMiddleware<TState, TResult> middleware)
        {
            var newPipeline = new Pipeline<TState, TResult>(pipeline);
            newPipeline.Add(middleware);
            return newPipeline;
        }

        public static Pipeline<TState, TResult> operator +(Pipeline<TState, TResult> pipeline, Pipeline<TState, TResult> otherPipeline)
        {
            var newPipeline = new Pipeline<TState, TResult>(pipeline);
            newPipeline.Add(otherPipeline);
            return newPipeline;
        }
        #endregion

        public Task<TResult> RunAsync(TState obj) =>
            new PipelineRun<TState, TResult>(this._middleware, obj).RunAsync();

        #region Pipeline Enumeration
        public IEnumerator<IMiddleware<TState, TResult>> GetEnumerator() =>
            this._middleware.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            this._middleware.GetEnumerator();
        #endregion

    }


    public class Pipeline<TState> : IEnumerable<IMiddleware<TState>>
    {

        private readonly ICollection<IMiddleware<TState>> _middleware;
        private IServiceProvider _serviceProvider;

        #region Constructors
        public Pipeline() =>
            this._middleware = new List<IMiddleware<TState>>();

        public Pipeline(Pipeline<TState> existingPipeline) =>
            this._middleware = existingPipeline._middleware.ToList();

        public Pipeline(params IMiddleware<TState>[] middleware) =>
            this._middleware = new List<IMiddleware<TState>>(middleware);

        public Pipeline(IEnumerable<IMiddleware<TState>> middleware) =>
            this._middleware = middleware.ToList();

        public Pipeline(params Middleware<TState>[] middleware) : this((IEnumerable<Middleware<TState>>)middleware) { }

        public Pipeline(IEnumerable<Middleware<TState>> middleware) =>
            this._middleware = new List<IMiddleware<TState>>(middleware.Select(m => new DelegateMiddleware<TState>(m)));
        #endregion

        public Pipeline<TState> WithServiceProvider(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
            return this;
        }

        #region Pipeline Addition
        public Pipeline<TState> Add(params Middleware<TState>[] middleware) =>
            this.Add((IEnumerable<Middleware<TState>>) middleware);

        public Pipeline<TState> Add(params Action<TState>[] middleware) =>
            this.Add((IEnumerable<Action<TState>>) middleware);

        public Pipeline<TState> Add(params Pipeline<TState>[] pipelines) =>
            this.Add((IEnumerable<Pipeline<TState>>) pipelines);

        public Pipeline<TState> Add(params IMiddleware<TState>[] middlewares) =>
            this.Add((IEnumerable<IMiddleware<TState>>) middlewares);

        public Pipeline<TState> Add<TMiddleware>() where TMiddleware : class, IMiddleware<TState> =>
            this.Add(new TypeMiddleware<TMiddleware, TState>(this._serviceProvider));

        public Pipeline<TState> Add(IEnumerable<Middleware<TState>> middlewares)
        {
            foreach (var middleware in middlewares)
                this._middleware.Add(new DelegateMiddleware<TState>(middleware));
            return this;
        }

        public Pipeline<TState> Add(IEnumerable<Action<TState>> middlewares)
        {
            foreach (var middleware in middlewares)
                this._middleware.Add(new DelegateMiddleware<TState>(middleware));
            return this;
        }

        public Pipeline<TState> Add(IEnumerable<Pipeline<TState>> pipelines)
        {
            foreach (var pipeline in pipelines)
                this.Add(pipeline._middleware);
            return this;
        }

        public Pipeline<TState> Add(IEnumerable<IMiddleware<TState>> middlewares)
        {
            foreach (var middleware in middlewares)
                this._middleware.Add(middleware);
            return this;
        }
        #endregion

        #region Pipeline Arithmatic
        public static Pipeline<TState> operator+(Pipeline<TState> pipeline, Middleware<TState> middleware)
        {
            var newPipeline = new Pipeline<TState>(pipeline);
            newPipeline.Add(middleware);
            return newPipeline;
        }

        public static Pipeline<TState> operator +(Pipeline<TState> pipeline, Action<TState> middleware)
        {
            var newPipeline = new Pipeline<TState>(pipeline);
            newPipeline.Add(middleware);
            return newPipeline;
        }

        public static Pipeline<TState> operator +(Pipeline<TState> pipeline, IMiddleware<TState> middleware)
        {
            var newPipeline = new Pipeline<TState>(pipeline);
            newPipeline.Add(middleware);
            return newPipeline;
        }

        public static Pipeline<TState> operator +(Pipeline<TState> pipeline, Pipeline<TState> otherPipeline) {
            var newPipeline = new Pipeline<TState>(pipeline);
            newPipeline.Add(otherPipeline);
            return newPipeline;
        }
        #endregion

        public Task RunAsync(TState obj) =>
            new PipelineRun<TState>(this._middleware, obj).RunAsync();

        #region Pipeline Enumeration
        public IEnumerator<IMiddleware<TState>> GetEnumerator() =>
            this._middleware.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            this._middleware.GetEnumerator();
        #endregion

    }

}
