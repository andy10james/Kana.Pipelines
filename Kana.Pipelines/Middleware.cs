using System;
using System.Threading.Tasks;

namespace Kana.Pipelines
{
    public delegate Task Middleware<T>(T state, Func<Task> next);

    public delegate Task<R> Middleware<T, R>(T state, Func<Task<R>> next);
}
