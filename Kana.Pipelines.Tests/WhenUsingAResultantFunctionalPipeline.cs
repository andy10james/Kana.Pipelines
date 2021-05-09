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
        public void ItPassesStateThroughPipeline()
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

            pipeline.Run(new State("Willam", "Riker"));
        }

        [Test]
        public void ItCallsEachMiddlewareBidirectionally()
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

            pipeline.Run(new State("William", "Riker"));

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

            var result = await usernamePipeline.Run(new State("William", "Riker"));

            Assert.AreEqual("williamr", result);
        }


        [Test]
        public async Task ItReturnsDefaultWhenThePipelineIsExhausted()
        {
            var orderCalled = new List<int>();

            var usernamePipeline = new Pipeline<State, string>();
            usernamePipeline.Add(
                (state, next) =>
                {
                    return next();
                },
                (state, next) =>
                {
                    return next();
                },
                (state, next) =>
                {
                    return next();
                });

            var result = await usernamePipeline.Run(new State("William", "Riker"));

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
            public void TheNewPipelineIsRanAsMiddleware()
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

                pipeline.Run(new State("William", "Riker"));

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
            public void TheNewFuncIsRanAsMiddleware()
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

                pipeline.Run(new State("William", "Riker"));

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
            public void TheNewFuncIsRanAsMiddleware()
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

                pipeline.Run(new State("William", "Riker"));

                Assert.AreEqual(new[] { 1, 2, 3, 2, 1 }, orderCalled);
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
            public void TheNewPipelineRunsAUnionOfTheTwo()
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

                unionPipeline.Run(new State("William", "Riker"));

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
            public void TheNewPipelineRunsAUnionOfTheTwo()
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

                unionPipeline.Run(new State("William", "Riker"));

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
            public void TheNewPipelineRunsAUnionOfTheTwo()
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

                unionPipeline.Run(new State("William", "Riker"));

                Assert.AreEqual(new[] { 1, 2, 3, 2, 1 }, orderCalled);
            }

        }

    }
}