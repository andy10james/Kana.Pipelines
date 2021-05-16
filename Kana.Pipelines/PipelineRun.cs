using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kana.Pipelines
{
    internal class PipelineRun<TState>
    {

        private readonly IMiddleware<TState>[] _steps;
        private readonly TState _state;
        private int _currentStep = -1;

        public PipelineRun(IEnumerable<IMiddleware<TState>> steps, TState state)
        {
            this._steps = steps.ToArray();
            this._state = state;
        }

        public Task RunAsync()
        {
            this._currentStep = 0;
            return this._steps[0].Execute(this._state, NextAsync);
        }

        private Task NextAsync()
        {
            ++this._currentStep;
            if (this._currentStep >= this._steps.Length)
                return Task.FromResult(0);
            return this._steps[this._currentStep].Execute(_state, NextAsync);
        }

    }

    internal class PipelineRun<TState, TResult>
    {

        private readonly IMiddleware<TState, TResult>[] _steps;
        private readonly TState _state;
        private int _currentStep = -1;

        public PipelineRun(IEnumerable<IMiddleware<TState, TResult>> steps, TState state)
        {
            this._steps = steps.ToArray();
            this._state = state;
        }

        public Task<TResult> RunAsync()
        {
            this._currentStep = 0;
            return this._steps[0].Execute(this._state, NextAsync);
        }

        private Task<TResult> NextAsync()
        {
            ++this._currentStep;
            if (this._currentStep >= this._steps.Length)
                return Task.FromResult(default(TResult));
            return this._steps[this._currentStep].Execute(_state, NextAsync);
        }

    }

}
