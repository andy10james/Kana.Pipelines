using System;
using System.Threading.Tasks;

namespace Kana.Pipelines
{
    public interface IMiddleware<TState, TResult>
    {

        Task<TResult> Execute(TState state, Func<Task<TResult>> next);

    }

    public interface IMiddleware<TState>
    {

        Task Execute(TState state, Func<Task> next);

    }
}
