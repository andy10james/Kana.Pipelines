using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kana.Pipelines.Tests
{

    [TestFixture]
    public class WhenUsingAResultlessFunctionalPipeline
    {

        record State(string Name);

        [SetUp]
        public void Setup() { }

        [Test]
        public void ItPassesStateThroughPipeline()
        {
            var pipeline = new Pipeline<State>();
            pipeline.Add((state) => Assert.AreEqual(state.Name, "random1"));
            pipeline.Add((state) => Assert.AreEqual(state.Name, "random1"));
            pipeline.Add((state) => Assert.AreEqual(state.Name, "random1"));

            pipeline.RunAsync(new State("random1"));
        }

        [Test]
        public void ItCallsEachMiddlewareBidirectionally()
        {
            var orderCalled = new List<int>();

            var pipeline = new Pipeline<State>();
            pipeline.Add(
                async (state, next) => {
                    orderCalled.Add(1);
                    await next();
                    orderCalled.Add(1);
                },
                async (state, next) => {
                    orderCalled.Add(2);
                    await next();
                    orderCalled.Add(2);
                },
                async (state, next) => {
                    orderCalled.Add(3);
                    await next();
                    orderCalled.Add(3);
                });

            pipeline.RunAsync(new State("random1"));

            Assert.AreEqual(new[] { 1, 2, 3, 3, 2, 1 }, orderCalled);
        }

        [TestFixture]
        public class AndAddingAnotherPipelineToIt
        {

            [Test]
            public void ItDoesNotCreateANewPipeline()
            {
                var pipeline = new Pipeline<State>()
                {
                    (state) => { },
                    (state) => { }
                };

                var pipeline2 = new Pipeline<State>()
                {
                    (state) => { },
                    (state) => { }
                };

                var unionPipeline = pipeline.Add(pipeline2);

                Assert.AreSame(unionPipeline, pipeline);
            }

            [Test]
            public void TheNewPipelineIsRanAsMiddleware()
            {
                var orderCalled = new List<int>();

                var pipeline = new Pipeline<State>
                {
                    async (state, next) => {
                        orderCalled.Add(1);
                        await next();
                        orderCalled.Add(1);
                    },
                    async (state, next) => {
                        orderCalled.Add(2);
                        await next();
                        orderCalled.Add(2);
                    }
                };

                var pipeline2 = new Pipeline<State>
                {
                    async (state, next) => {
                        orderCalled.Add(3);
                        await next();
                        orderCalled.Add(3);
                    },
                    async (state, next) => {
                        orderCalled.Add(4);
                        await next();
                        orderCalled.Add(4);
                    }
                };

                pipeline.Add(pipeline2);

                pipeline.RunAsync(new State("random1"));

                Assert.AreEqual(new[] { 1, 2, 3, 4, 4, 3, 2, 1 }, orderCalled);
            }

        }

        [TestFixture]
        public class AndAddingAMiddlewareToIt
        {

            [Test]
            public void ItDoesNotCreateANewPipeline()
            {
                var pipeline = new Pipeline<State>()
                {
                    (state) => { },
                    (state) => { }
                };

                var unionPipeline = pipeline.Add((s, n) => n());

                Assert.AreSame(unionPipeline, pipeline);
            }

            [Test]
            public void TheNewFuncIsRanAsMiddleware()
            {
                var orderCalled = new List<int>();

                var pipeline = new Pipeline<State>
                {
                    async (state, next) => {
                        orderCalled.Add(1);
                        await next();
                        orderCalled.Add(1);
                    },
                    async (state, next) => {
                        orderCalled.Add(2);
                        await next();
                        orderCalled.Add(2);
                    }
                };

                pipeline.Add(
                    async (state, next) => {
                        orderCalled.Add(3);
                        await next();
                        orderCalled.Add(3);
                    });

                pipeline.RunAsync(new State("random1"));

                Assert.AreEqual(new[] { 1, 2, 3, 3, 2, 1 }, orderCalled);
            }

        }

        [TestFixture]
        public class AndAddingAnActionToIt
        {

            [Test]
            public void ItDoesNotCreateANewPipeline()
            {
                var pipeline = new Pipeline<State>()
                {
                    (state) => { },
                    (state) => { }
                };

                var unionPipeline = pipeline.Add((s) => { });

                Assert.AreSame(unionPipeline, pipeline);
            }

            [Test]
            public void TheNewActionIsRanAsMiddleware()
            {
                var orderCalled = new List<int>();

                var pipeline = new Pipeline<State>
                {
                    async (state, next) => {
                        orderCalled.Add(1);
                        await next();
                        orderCalled.Add(1);
                    },
                    async (state, next) => {
                        orderCalled.Add(2);
                        await next();
                        orderCalled.Add(2);
                    }
                };

                pipeline.Add(
                    (state) => {
                        orderCalled.Add(3);
                    });

                pipeline.RunAsync(new State("random1"));

                Assert.AreEqual(new[] { 1, 2, 3, 2, 1 }, orderCalled);
            }

        }

        [TestFixture]
        public class AndAddingAnIMiddlewareInstanceToIt
        {

            private class TestMiddleware : IMiddleware<State>
            {
                private readonly List<int> _callOrder;
                private readonly int _i;

                public TestMiddleware(List<int> callOrder, int i)
                {
                    this._callOrder = callOrder;
                    this._i = i;
                }

                public async Task ExecuteAsync(State state, Func<Task> next)
                {
                    this._callOrder?.Add(this._i);
                    await next();
                    this._callOrder?.Add(this._i);
                }
            }


            [Test]
            public void ItDoesNotCreateANewPipeline()
            {
                var pipeline = new Pipeline<State>()
                {
                    new TestMiddleware(null, 1),
                    new TestMiddleware(null, 2)
                };

                var unionPipeline = pipeline.Add(new TestMiddleware(null, 3));

                Assert.AreSame(unionPipeline, pipeline);
            }

            [Test]
            public void TheNewActionIsRanAsMiddleware()
            {
                var orderCalled = new List<int>();

                var pipeline = new Pipeline<State>
                {
                    new TestMiddleware(orderCalled, 1),
                    new TestMiddleware(orderCalled, 2)
                };

                pipeline.Add(new TestMiddleware(orderCalled, 3));

                pipeline.RunAsync(new State("random1"));

                Assert.AreEqual(new[] { 1, 2, 3, 3, 2, 1 }, orderCalled);
            }

        }

        [TestFixture]
        public class AndAddingAMiddlewareTypeToIt
        {

            private class TestMiddleware : IMiddleware<List<int>>
            {
                private readonly int _i;

                public TestMiddleware()
                {
                    this._i = 3;
                }

                public TestMiddleware(int i)
                {
                    this._i = i;
                }

                public async Task ExecuteAsync(List<int> state, Func<Task> next)
                {
                    state?.Add(this._i);
                    await next();
                    state?.Add(this._i);
                }
            }

            [Test]
            public void ItDoesNotCreateANewPipeline()
            {
                var pipeline = new Pipeline<List<int>>()
                {
                    new TestMiddleware(1),
                    new TestMiddleware(2)
                };

                var unionPipeline = pipeline.Add<TestMiddleware>();

                Assert.AreSame(unionPipeline, pipeline);
            }

            [Test]
            public async Task TheNewActionIsRanAsMiddleware()
            {
                var orderCalled = new List<int>();

                var pipeline = new Pipeline<List<int>>
                    {
                        new TestMiddleware(1),
                        new TestMiddleware(2)
                    };

                pipeline.Add<TestMiddleware>();

                await pipeline.RunAsync(orderCalled);

                Assert.AreEqual(new[] { 1, 2, 3, 3, 2, 1 }, orderCalled);
            }

            [TestFixture]
            private class AndNoServiceProviderProvided
            {

                [Test]
                public void ItThrowsWhenThereIsNoParameterlessConstructor()
                {
                    var pipeline = new Pipeline<object>();
                    pipeline.Add<NoParameterlessConstructor>();

                    Assert.ThrowsAsync<TypeInitializationException>(async () =>
                    {
                        await pipeline.RunAsync(null);
                    });
                }

                private class NoParameterlessConstructor : IMiddleware<object>
                {
                    public NoParameterlessConstructor(object thing) { }
                    public Task ExecuteAsync(object state, Func<Task> next) { return next(); }
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

                    var pipeline = new Pipeline<State>()
                        .WithServiceProvider(serviceProvider)
                        .Add<NoParameterlessConstructor>();

                    var state = new State();

                    await pipeline.RunAsync(state);

                    Assert.AreEqual("Unnamed", state.ServiceReturn);
                }

                private class State { public string ServiceReturn; }
                private interface IService { string Get { get; } }
                private class Service : IService { public string Get => "Unnamed"; }
                private class NoParameterlessConstructor : IMiddleware<State>
                {
                    private readonly IService _service;
                    public NoParameterlessConstructor(IService service) =>
                        this._service = service;
                    public Task ExecuteAsync(State state, Func<Task> next) { state.ServiceReturn = this._service.Get; return next(); }
                }

            }

        }

        [TestFixture]
        public class AndAddingItToAnother
        {

            [Test]
            public void ItCreatesANewPipeline()
            {
                var pipeline = new Pipeline<State>()
                {
                    (state) => { },
                    (state) => { }
                };

                var pipeline2 = new Pipeline<State>()
                {
                    (state) => { },
                    (state) => { }
                };

                var unionPipeline = pipeline + pipeline2;

                Assert.AreNotSame(unionPipeline, pipeline);
                Assert.AreNotSame(unionPipeline, pipeline2);
            }

            [Test]
            public void ItCreatesAUnionPipeline()
            {
                var orderCalled = new List<int>();


                var pipeline = new Pipeline<State>()
                {
                    async (state, next) => {
                        orderCalled.Add(1);
                        await next();
                        orderCalled.Add(1);
                    },
                    async (state, next) => {
                        orderCalled.Add(2);
                        await next();
                        orderCalled.Add(2);
                    }
                };

                var pipeline2 = new Pipeline<State>()
                {
                    async (state, next) => {
                        orderCalled.Add(3);
                        await next();
                        orderCalled.Add(3);
                    },
                    async (state, next) => {
                        orderCalled.Add(4);
                        await next();
                        orderCalled.Add(4);
                    }
                };

                var unionPipeline = pipeline + pipeline2;

                unionPipeline.RunAsync(new State("random1"));

                Assert.AreEqual(new[] { 1, 2, 3, 4, 4, 3, 2, 1 }, orderCalled);
            }

        }

        [TestFixture]
        public class AndAddingItToAFunc
        {

            [Test]
            public void ItCreatesANewPipeline()
            {
                var pipeline = new Pipeline<State>()
                {
                    (state) => { },
                    (state) => { }
                };

                Middleware<State> middleware = (s, n) => n();
                var unionPipeline = pipeline + middleware;

                Assert.AreNotSame(unionPipeline, pipeline);
            }

            [Test]
            public void TheNewPipelineRunsAUnionOfTheTwo()
            {
                var orderCalled = new List<int>();

                var pipeline = new Pipeline<State>
                {
                    async (state, next) => 
                    {
                        orderCalled.Add(1);
                        await next();
                        orderCalled.Add(1);
                    },
                    async (state, next) => 
                    {
                        orderCalled.Add(2);
                        await next();
                        orderCalled.Add(2);
                    }
                };

                Middleware<State> middleware = 
                    async (state, next) =>
                    {
                        orderCalled.Add(3);
                        await next();
                        orderCalled.Add(3);
                    };

                var unionPipeline = pipeline + middleware;

                unionPipeline.RunAsync(new State("random1"));

                Assert.AreEqual(new[] { 1, 2, 3, 3, 2, 1 }, orderCalled);
            }

        }

        [TestFixture]
        public class AndAddingItToAnAction
        {

            [Test]
            public void ItCreatesANewPipeline()
            {
                var pipeline = new Pipeline<State>()
                {
                    (state) => { },
                    (state) => { }
                };

                Action<State> middleware = (s) => { };
                var unionPipeline = pipeline + middleware;

                Assert.AreNotSame(unionPipeline, pipeline);
            }

            [Test]
            public void TheNewPipelineRunsAUnionOfTheTwo()
            {
                var orderCalled = new List<int>();

                var pipeline = new Pipeline<State>
                {
                    async (state, next) =>
                    {
                        orderCalled.Add(1);
                        await next();
                        orderCalled.Add(1);
                    },
                    async (state, next) =>
                    {
                        orderCalled.Add(2);
                        await next();
                        orderCalled.Add(2);
                    }
                };

                Action<State> middleware =
                    (state) =>
                    {
                        orderCalled.Add(3);
                    };

                var unionPipeline = pipeline + middleware;

                unionPipeline.RunAsync(new State("random1"));

                Assert.AreEqual(new[] { 1, 2, 3, 2, 1 }, orderCalled);
            }

        }

        [TestFixture]
        public class AndAddingItToAnIMiddlewareInstance
        {

            private class TestMiddleware : IMiddleware<State>
            {
                private readonly List<int> _callOrder;
                private readonly int _i;

                public TestMiddleware(List<int> callOrder, int i)
                {
                    this._callOrder = callOrder;
                    this._i = i;
                }

                public async Task ExecuteAsync(State state, Func<Task> next)
                {
                    this._callOrder?.Add(this._i);
                    await next();
                    this._callOrder?.Add(this._i);
                }
            }


            [Test]
            public void ItCreatesANewPipeline()
            {
                var pipeline = new Pipeline<State>()
                {
                    new TestMiddleware(null, 1),
                    new TestMiddleware(null, 2)
                };

                var unionPipeline = pipeline + new TestMiddleware(null, 3);

                Assert.AreNotSame(unionPipeline, pipeline);
            }

            [Test]
            public void TheNewPipelineRunsAUnionOfTheTwo()
            {
                var orderCalled = new List<int>();

                var pipeline = new Pipeline<State>
                {
                    new TestMiddleware(orderCalled, 1),
                    new TestMiddleware(orderCalled, 2)
                };

                var unionPipeline = pipeline + new TestMiddleware(orderCalled, 3);

                unionPipeline.RunAsync(new State("random1"));

                Assert.AreEqual(new[] { 1, 2, 3, 3, 2, 1 }, orderCalled);
            }

        }

    }
}