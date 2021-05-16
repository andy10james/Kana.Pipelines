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

            pipeline.Run(new State("random1"));
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

            pipeline.Run(new State("random1"));

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

                pipeline.Run(new State("random1"));

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

                pipeline.Run(new State("random1"));

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

                pipeline.Run(new State("random1"));

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

                public async Task Execute(State state, Func<Task> next)
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

                pipeline.Run(new State("random1"));

                Assert.AreEqual(new[] { 1, 2, 3, 3, 2, 1 }, orderCalled);
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

                unionPipeline.Run(new State("random1"));

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

                unionPipeline.Run(new State("random1"));

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

                unionPipeline.Run(new State("random1"));

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

                public async Task Execute(State state, Func<Task> next)
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

                unionPipeline.Run(new State("random1"));

                Assert.AreEqual(new[] { 1, 2, 3, 3, 2, 1 }, orderCalled);
            }

        }

    }
}