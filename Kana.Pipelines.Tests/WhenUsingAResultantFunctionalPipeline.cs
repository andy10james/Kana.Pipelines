using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kana.Pipelines.Tests
{

    [TestFixture]
    public class WhenUsingAResultantFunctionalPipeline
    {

        record State(string FirstName, string LastName);

        [SetUp]
        public void Setup() { }

        [Test]
        public async Task ItPassesStateThroughPipeline()
        {
            var pipeline = new Pipeline<State, string>();
            pipeline.Add((state, next) => {
                Assert.AreEqual(state.FirstName, "Willam");
                return next();
            });
            pipeline.Add((state, next) => {
                Assert.AreEqual(state.FirstName, "Willam");
                return next();
            });
            pipeline.Add((state, next) => {
                Assert.AreEqual(state.FirstName, "Willam");
                return next();
            });

            await pipeline.RunAsync(new State("Willam", "Riker"));
        }

        [Test]
        public async Task ItCallsEachMiddlewareBidirectionally()
        {
            var orderCalled = new List<int>();

            var pipeline = new Pipeline<State, string>();
            pipeline.Add(
                async (state, next) => {
                    orderCalled.Add(1);
                    var result = await next();
                    orderCalled.Add(1);
                    return result;
                },
                async (state, next) => {
                    orderCalled.Add(2);
                    var result = await next();
                    orderCalled.Add(2);
                    return result;
                },
                async (state, next) => {
                    orderCalled.Add(3);
                    var result = await next();
                    orderCalled.Add(3);
                    return result;
                });

            await pipeline.RunAsync(new State("William", "Riker"));

            Assert.AreEqual(new[] { 1, 2, 3, 3, 2, 1 }, orderCalled);
        }

        [Test]
        public async Task ItReturnsTheCollectiveResultFromThePipeline()
        {
            var usernamePipeline = new Pipeline<State, string>();
            usernamePipeline.Add(
                async (state, next) => {
                    var result = await next();
                    return result.ToLower();
                },
                async (state, next) => {
                    if (state != null)
                        return state.FirstName + state.LastName[0];
                    return await next();
                },
                (state, next) => {
                    return Task.FromResult("Unnamed");
                });

            var result = await usernamePipeline.RunAsync(new State("William", "Riker"));

            Assert.AreEqual("williamr", result);
        }

        [Test]
        public async Task ItReturnsDefaultWhenThePipelineIsExhausted()
        {
            var orderCalled = new List<int>();

            var usernamePipeline = new Pipeline<State, string>();
            usernamePipeline.Add(
                (state, next) => next(),
                (state, next) => next(),
                (state, next) => next());

            var result = await usernamePipeline.RunAsync(new State("William", "Riker"));

            Assert.IsNull(result);
        }

        [TestFixture]
        public class AndAddingAnotherPipelineToIt
        {

            [Test]
            public void ItDoesNotCreateANewPipeline()
            {
                var pipeline = new Pipeline<State, string>()
                {
                    (state, next) => next(),
                    (state, next) => next()
                };

                var pipeline2 = new Pipeline<State, string>()
                {
                    (state, next) => next(),
                    (state, next) => next()
                };

                var unionPipeline = pipeline.Add(pipeline2);

                Assert.AreSame(unionPipeline, pipeline);
            }

            [Test]
            public async Task TheNewPipelineIsRanAsMiddleware()
            {
                var orderCalled = new List<int>();

                var pipeline = new Pipeline<State, string>
                {
                    async (state, next) => {
                        orderCalled.Add(1);
                        var result = await next();
                        orderCalled.Add(1);
                        return result;
                    },
                    async (state, next) => {
                        orderCalled.Add(2);
                        var result = await next();
                        orderCalled.Add(2);
                        return result;
                    }
                };

                var pipeline2 = new Pipeline<State, string>
                {
                    async (state, next) => {
                        orderCalled.Add(3);
                        var result = await next();
                        orderCalled.Add(3);
                        return result;
                    },
                    async (state, next) => {
                        orderCalled.Add(4);
                        var result = await next();
                        orderCalled.Add(4);
                        return result;
                    }
                };

                pipeline.Add(pipeline2);

                await pipeline.RunAsync(new State("William", "Riker"));

                Assert.AreEqual(new[] { 1, 2, 3, 4, 4, 3, 2, 1 }, orderCalled);
            }

        }

        [TestFixture]
        public class AndAddingAMiddlewareToIt
        {

            [Test]
            public void ItDoesNotCreateANewPipeline()
            {
                var pipeline = new Pipeline<State, string>()
                {
                    (state, next) => next(),
                    (state, next) => next()
                };

                var unionPipeline = pipeline.Add((s, n) => n());

                Assert.AreSame(unionPipeline, pipeline);
            }

            [Test]
            public async Task TheNewFuncIsRanAsMiddleware()
            {
                var orderCalled = new List<int>();

                var pipeline = new Pipeline<State, string>
                {
                    async (state, next) => {
                        orderCalled.Add(1);
                        var result = await next();
                        orderCalled.Add(1);
                        return result;
                    },
                    async (state, next) => {
                        orderCalled.Add(2);
                        var result = await next();
                        orderCalled.Add(2);
                        return result;
                    }
                };

                pipeline.Add(
                    async (state, next) => {
                        orderCalled.Add(3);
                        var result = await next();
                        orderCalled.Add(3);
                        return result;
                    });

                await pipeline.RunAsync(new State("William", "Riker"));

                Assert.AreEqual(new[] { 1, 2, 3, 3, 2, 1 }, orderCalled);
            }

        }

        [TestFixture]
        public class AndAddingAnFuncToIt
        {

            [Test]
            public void ItDoesNotCreateANewPipeline()
            {
                var pipeline = new Pipeline<State, string>()
                {
                    (state, next) => next(),
                    (state, next) => next()
                };

                var unionPipeline = pipeline.Add((state, next) => next());

                Assert.AreSame(unionPipeline, pipeline);
            }

            [Test]
            public async Task TheNewFuncIsRanAsMiddleware()
            {
                var orderCalled = new List<int>();

                var pipeline = new Pipeline<State, string>
                {
                    async (state, next) => {
                        orderCalled.Add(1);
                        var result = await next();
                        orderCalled.Add(1);
                        return result;
                    },
                    async (state, next) => {
                        orderCalled.Add(2);
                        var result = await next();
                        orderCalled.Add(2);
                        return result;
                    }
                };

                pipeline.Add(
                    (state) => {
                        orderCalled.Add(3);
                        return "Unnamed";
                    });

                await pipeline.RunAsync(new State("William", "Riker"));

                Assert.AreEqual(new[] { 1, 2, 3, 2, 1 }, orderCalled);
            }

        }


        [TestFixture]
        public class AndAddingAnIMiddlewareInstanceToIt
        {

            private class TestMiddleware : IMiddleware<State, string>
            {
                private readonly List<int> _callOrder;
                private readonly int _i;
                private readonly string _return;

                public TestMiddleware(List<int> callOrder, int i, string @return)
                {
                    this._callOrder = callOrder;
                    this._i = i;
                    this._return = @return;
                }

                public async Task<string> ExecuteAsync(State state, Func<Task<string>> next)
                {
                    this._callOrder?.Add(this._i);
                    if (this._return == null)
                    {
                        var result = await next();
                        this._callOrder?.Add(this._i);
                        return result;
                    }
                    return this._return;
                }
            }


            [Test]
            public void ItDoesNotCreateANewPipeline()
            {
                var pipeline = new Pipeline<State, string>()
                {
                    new TestMiddleware(null, 1, null),
                    new TestMiddleware(null, 2, null)
                };

                var unionPipeline = pipeline.Add(new TestMiddleware(null, 3, "Unnamed"));

                Assert.AreSame(unionPipeline, pipeline);
            }

            [Test]
            public async Task TheNewActionIsRanAsMiddleware()
            {
                var orderCalled = new List<int>();

                var pipeline = new Pipeline<State, string>
                {
                    new TestMiddleware(orderCalled, 1, null),
                    new TestMiddleware(orderCalled, 2, null)
                };

                pipeline.Add(new TestMiddleware(orderCalled, 3, "Unnamed"));

                await pipeline.RunAsync(new State("William", "Riker"));

                Assert.AreEqual(new[] { 1, 2, 3, 2, 1 }, orderCalled);
            }

        }

        [TestFixture]
        public class AndAddingAMiddlewareTypeToIt
        {

            private class TestMiddleware : IMiddleware<List<int>, string>
            {
                private readonly int _i;
                private readonly string _return;

                public TestMiddleware()
                {
                    this._i = 3;
                    this._return = "Unnamed";
                }

                public TestMiddleware(int i, string @return)
                {
                    this._i = i;
                    this._return = @return;
                }

                public async Task<string> ExecuteAsync(List<int> state, Func<Task<string>> next)
                {
                    state?.Add(this._i);
                    if (this._return == null)
                    {
                        var result = await next();
                        state?.Add(this._i);
                        return result;
                    }
                    return this._return;
                }
            }

            [Test]
            public void ItDoesNotCreateANewPipeline()
            {
                var pipeline = new Pipeline<List<int>, string>()
                {
                    new TestMiddleware(1, null),
                    new TestMiddleware(2, null)
                };

                var unionPipeline = pipeline.Add<TestMiddleware>();

                Assert.AreSame(unionPipeline, pipeline);
            }

            [Test]
            public async Task TheNewActionIsRanAsMiddleware()
            {
                var orderCalled = new List<int>();

                var pipeline = new Pipeline<List<int>, string>
                    {
                        new TestMiddleware(1, null),
                        new TestMiddleware(2, null)
                    };

                pipeline.Add<TestMiddleware>();

                await pipeline.RunAsync(orderCalled);

                Assert.AreEqual(new[] { 1, 2, 3, 2, 1 }, orderCalled);
            }

            [TestFixture]
            private class AndNoServiceProviderProvided
            {

                [Test]
                public void ItThrowsWhenThereIsNoParameterlessConstructor()
                {
                    var pipeline = new Pipeline<object, string>();
                    pipeline.Add<NoParameterlessConstructor>();

                    Assert.ThrowsAsync<TypeInitializationException>(async () =>
                    {
                        await pipeline.RunAsync(null);
                    });
                }

                private class NoParameterlessConstructor : IMiddleware<object, string>
                {
                    public NoParameterlessConstructor(object thing) { }
                    public Task<string> ExecuteAsync(object state, Func<Task<string>> next) { return next(); }
                }

            }

            [TestFixture]
            private class AndServiceProviderProvided
            {


                [Test]
                public async Task ItResolvesAndExecutesTheMiddlewareFromTheServiceProvider()
                {
                    var serviceCollection = new ServiceCollection();
                    serviceCollection.AddTransient<IService, Service>();
                    serviceCollection.AddTransient<NoParameterlessConstructor>();

                    var serviceProvider = serviceCollection.BuildServiceProvider();

                    var pipeline = new Pipeline<object, string>()
                        .WithServiceProvider(serviceProvider)
                        .Add<NoParameterlessConstructor>();

                    var result = await pipeline.RunAsync(null);
                    Assert.AreEqual("Unnamed", result);
                }

                private interface IService { string Get { get; } }
                private class Service : IService { public string Get => "Unnamed"; }
                private class NoParameterlessConstructor : IMiddleware<object, string>
                {
                    private readonly IService _service;
                    public NoParameterlessConstructor(IService service) =>
                        this._service = service;
                    public Task<string> ExecuteAsync(object state, Func<Task<string>> next) { return Task.FromResult(this._service.Get); }
                }

            }

        }

        [TestFixture]
        public class AndAddingItToAnother
        {

            [Test]
            public void ItCreatesANewPipeline()
            {
                var pipeline = new Pipeline<State, string>()
                {
                    (state, next) => next(),
                    (state, next) => next()
                };

                var pipeline2 = new Pipeline<State, string>()
                {
                    (state, next) => next(),
                    (state, next) => next()
                };

                var unionPipeline = pipeline + pipeline2;

                Assert.AreNotSame(unionPipeline, pipeline);
                Assert.AreNotSame(unionPipeline, pipeline2);
            }

            [Test]
            public async Task TheNewPipelineRunsAUnionOfTheTwo()
            {
                var orderCalled = new List<int>();

                var pipeline = new Pipeline<State, string>()
                {
                    async (state, next) => {
                        orderCalled.Add(1);
                        var result = await next();
                        orderCalled.Add(1);
                        return result;
                    },
                    async (state, next) => {
                        orderCalled.Add(2);
                        var result = await next();
                        orderCalled.Add(2);
                        return result;
                    }
                };

                var pipeline2 = new Pipeline<State, string>()
                {
                    async (state, next) => {
                        orderCalled.Add(3);
                        var result = await next();
                        orderCalled.Add(3);
                        return result;
                    },
                    async (state, next) => {
                        orderCalled.Add(4);
                        var result = await next();
                        orderCalled.Add(4);
                        return result;
                    }
                };

                var unionPipeline = pipeline + pipeline2;

                await unionPipeline.RunAsync(new State("William", "Riker"));

                Assert.AreEqual(new[] { 1, 2, 3, 4, 4, 3, 2, 1 }, orderCalled);
            }

        }

        [TestFixture]
        public class AndAddingItToAMiddleware
        {

            [Test]
            public void ItCreatesANewPipeline()
            {
                var pipeline = new Pipeline<State, string>()
                {
                    (state, next) => next(),
                    (state, next) => next()
                };

                Middleware<State, string> middleware = (s, n) => n();
                var unionPipeline = pipeline + middleware;

                Assert.AreNotSame(unionPipeline, pipeline);
            }

            [Test]
            public async Task TheNewPipelineRunsAUnionOfTheTwo()
            {
                var orderCalled = new List<int>();

                var pipeline = new Pipeline<State, string>
                {
                    async (state, next) => 
                    {
                        orderCalled.Add(1);
                        var result = await next();
                        orderCalled.Add(1);
                        return result;
                    },
                    async (state, next) => 
                    {
                        orderCalled.Add(2);
                        var result = await next();
                        orderCalled.Add(2);
                        return result;
                    }
                };

                Middleware<State, string> middleware = 
                    async (state, next) =>
                    {
                        orderCalled.Add(3);
                        var result = await next();
                        orderCalled.Add(3);
                        return result;
                    };

                var unionPipeline = pipeline + middleware;

                await unionPipeline.RunAsync(new State("William", "Riker"));

                Assert.AreEqual(new[] { 1, 2, 3, 3, 2, 1 }, orderCalled);
            }

        }

        [TestFixture]
        public class AndAddingItToAFunc
        {

            [Test]
            public void ItCreatesANewPipeline()
            {
                var pipeline = new Pipeline<State, string>()
                {
                    (state, next) => next(),
                    (state, next) => next()
                };

                Func<State, string> middleware = (state) => "Unnamed";
                var unionPipeline = pipeline + middleware;

                Assert.AreNotSame(unionPipeline, pipeline);
            }

            [Test]
            public async Task TheNewPipelineRunsAUnionOfTheTwo()
            {
                var orderCalled = new List<int>();

                var pipeline = new Pipeline<State, string>
                {
                    async (state, next) =>
                    {
                        orderCalled.Add(1);
                        var result = await next();
                        orderCalled.Add(1);
                        return result;
                    },
                    async (state, next) =>
                    {
                        orderCalled.Add(2);
                        var result = await next();
                        orderCalled.Add(2);
                        return result;
                    }
                };

                Func<State, string> middleware =
                    (state) =>
                    {
                        orderCalled.Add(3);
                        return "Unnamed";
                    };

                var unionPipeline = pipeline + middleware;

                await unionPipeline.RunAsync(new State("William", "Riker"));

                Assert.AreEqual(new[] { 1, 2, 3, 2, 1 }, orderCalled);
            }

        }


        [TestFixture]
        public class AndAddingItToAnIMiddlewareInstance
        {

            private class TestMiddleware : IMiddleware<State, string>
            {
                private readonly List<int> _callOrder;
                private readonly int _i;
                private readonly string _return;

                public TestMiddleware(List<int> callOrder, int i, string @return)
                {
                    this._callOrder = callOrder;
                    this._i = i;
                    this._return = @return;
                }

                public async Task<string> ExecuteAsync(State state, Func<Task<string>> next)
                {
                    this._callOrder?.Add(this._i);
                    if (this._return == null)
                    {
                        var result = await next();
                        this._callOrder?.Add(this._i);
                        return result;
                    }
                    return this._return;
                }
            }

            [Test]
            public void ItCreatesANewPipeline()
            {
                var pipeline = new Pipeline<State, string>()
                {
                    new TestMiddleware(null, 1, null),
                    new TestMiddleware(null, 2, null)
                };
;
                var unionPipeline = pipeline + new TestMiddleware(null, 3, "unnamed");

                Assert.AreNotSame(unionPipeline, pipeline);
            }

            [Test]
            public async Task TheNewPipelineRunsAUnionOfTheTwo()
            {
                var orderCalled = new List<int>();

                var pipeline = new Pipeline<State, string>
                {
                    new TestMiddleware(orderCalled, 1, null),
                    new TestMiddleware(orderCalled, 2, null)
                };

                var unionPipeline = pipeline + new TestMiddleware(orderCalled, 3, "Unnamed");

                await unionPipeline.RunAsync(new State("William", "Riker"));

                Assert.AreEqual(new[] { 1, 2, 3, 2, 1 }, orderCalled);
            }

        }


    }
}