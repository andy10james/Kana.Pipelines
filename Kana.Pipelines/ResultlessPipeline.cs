#pragma warning disable CS0693 // Type parameter has the same name as the type parameter from outer type

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kana.Pipelines
{
    public class Pipeline<TState> : IEnumerable<Middleware<TState>>
    {

        private readonly ICollection<Middleware<TState>> _middleware;


        #region Constructors
        public Pipeline() =>
            this._middleware = new List<Middleware<TState>>();

        public Pipeline(Pipeline<TState> existingPipeline) =>
            this._middleware = existingPipeline._middleware.ToList();

        public Pipeline(params Middleware<TState>[] middleware) =>
            this._middleware = new List<Middleware<TState>>(middleware);

        public Pipeline(IEnumerable<Middleware<TState>> middleware) =>
            this._middleware = middleware.ToList();
        #endregion


        #region Pipeline Addition
        public Pipeline<TState> Add(params Middleware<TState>[] middleware) =>
            this.Add((IEnumerable<Middleware<TState>>) middleware);

        public Pipeline<TState> Add(params Action<TState>[] middleware) =>
            this.Add((IEnumerable<Action<TState>>) middleware);

        public Pipeline<TState> Add(params Pipeline<TState>[] pipelines) =>
            this.Add((IEnumerable<Pipeline<TState>>) pipelines);

        public Pipeline<TState> Add(IEnumerable<Middleware<TState>> middlewares)
        {
            foreach (var middleware in middlewares)
            {
                this._middleware.Add(middleware);
            }
            return this;
        }

        public Pipeline<TState> Add(IEnumerable<Action<TState>> middlewares)
        {
            foreach (var middleware in middlewares)
            {
                this._middleware.Add((s, n) => {
                    middleware(s);
                    return n();
                });
            }
            return this;
        }

        public Pipeline<TState> Add(IEnumerable<Pipeline<TState>> pipelines)
        {
            foreach (var pipeline in pipelines)
            {
                this.Add(pipeline._middleware);
            }
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

        public static Pipeline<TState> operator +(Pipeline<TState> pipeline, Pipeline<TState> otherPipeline) {
            var newPipeline = new Pipeline<TState>(pipeline);
            newPipeline.Add(otherPipeline);
            return newPipeline;
        }
        #endregion


        public Task Run(TState obj) =>
            new PipelineRun<TState>(this._middleware, obj).RunAsync();


        #region Pipeline Enumeration
        public IEnumerator<Middleware<TState>> GetEnumerator() =>
            this._middleware.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            this._middleware.GetEnumerator();
        #endregion


        private class PipelineRun<TState>
        {

            private readonly Middleware<TState>[] _steps;
            private readonly TState _state;
            private int _currentStep = -1;

            public PipelineRun(IEnumerable<Middleware<TState>> steps, TState state)
            {
                this._steps = steps.ToArray();
                this._state = state;
            }

            public Task RunAsync()
            {
                this._currentStep = 0;
                return this._steps[0](this._state, NextAsync);
            }

            private Task NextAsync()
            {
                ++this._currentStep;
                if (this._currentStep >= this._steps.Length)
                    return Task.FromResult(0);
                return this._steps[this._currentStep](_state, NextAsync);
            }

        }

    }

}
