#pragma warning disable CS0693 // Type parameter has the same name as the type parameter from outer type

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kana.Pipelines
{
    public class Pipeline<TState, TResult> : IEnumerable<Middleware<TState, TResult>>
    {

        private readonly ICollection<Middleware<TState, TResult>> _middleware;


        #region Constructors
        public Pipeline() =>
            this._middleware = new List<Middleware<TState, TResult>>();

        public Pipeline(Pipeline<TState, TResult> existingPipeline) =>
            this._middleware = existingPipeline._middleware.ToList();

        public Pipeline(params Middleware<TState, TResult>[] middleware) =>
            this._middleware = new List<Middleware<TState, TResult>>(middleware);

        public Pipeline(IEnumerable<Middleware<TState, TResult>> middleware) =>
            this._middleware = middleware.ToList();
        #endregion


        #region Pipeline Addition
        public Pipeline<TState, TResult> Add(params Middleware<TState, TResult>[] middleware) =>
            this.Add((IEnumerable<Middleware<TState, TResult>>) middleware);

        public Pipeline<TState, TResult> Add(params Func<TState, TResult>[] middleware) =>
            this.Add((IEnumerable<Func<TState, TResult>>) middleware);

        public Pipeline<TState, TResult> Add(params Pipeline<TState, TResult>[] pipelines) =>
            this.Add((IEnumerable<Pipeline<TState, TResult>>) pipelines);

        public Pipeline<TState, TResult> Add(IEnumerable<Middleware<TState, TResult>> middlewares)
        {
            foreach (var middleware in middlewares)
            {
                this._middleware.Add(middleware);
            }
            return this;
        }

        public Pipeline<TState, TResult> Add(IEnumerable<Func<TState, TResult>> middlewares)
        {
            foreach (var middleware in middlewares)
            {
                this._middleware.Add((s, n) => {
                    return Task.FromResult(middleware(s));
                });
            }
            return this;
        }

        public Pipeline<TState, TResult> Add(IEnumerable<Pipeline<TState, TResult>> pipelines)
        {
            foreach (var pipeline in pipelines)
            {
                this.Add(pipeline._middleware);
            }
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

        public static Pipeline<TState, TResult> operator +(Pipeline<TState, TResult> pipeline, Pipeline<TState, TResult> otherPipeline) {
            var newPipeline = new Pipeline<TState, TResult>(pipeline);
            newPipeline.Add(otherPipeline);
            return newPipeline;
        }
        #endregion


        public Task<TResult> Run(TState obj) =>
            new PipelineRun<TState, TResult>(this._middleware, obj).RunAsync();


        #region Pipeline Enumeration
        public IEnumerator<Middleware<TState, TResult>> GetEnumerator() =>
            this._middleware.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            this._middleware.GetEnumerator();
        #endregion


        private class PipelineRun<TState, TResult>
        {

            private readonly Middleware<TState, TResult>[] _steps;
            private readonly TState _state;
            private int _currentStep = -1;

            public PipelineRun(IEnumerable<Middleware<TState, TResult>> steps, TState state)
            {
                this._steps = steps.ToArray();
                this._state = state;
            }

            public Task<TResult> RunAsync()
            {
                this._currentStep = 0;
                return this._steps[0](this._state, NextAsync);
            }

            private Task<TResult> NextAsync()
            {
                ++this._currentStep;
                if (this._currentStep >= this._steps.Length)
                    return Task.FromResult(default(TResult));
                return this._steps[this._currentStep](_state, NextAsync);
            }

        }

    }

}
