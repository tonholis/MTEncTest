using System;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.MessageData;
using MassTransit.Testing;
using MessageData.Components.Messages;
using MessageData.Components.Model;
using NUnit.Framework;

namespace MessageData.Components.Tests
{
    [TestFixture]
    public class MessageDataTests
    {
        [Test]
        public async Task Should_handle_nested_message_data_using_request_response()
        {
            var harness = new InMemoryTestHarness {TestTimeout = TimeSpan.FromSeconds(5)};
            var repo = new InMemoryMessageDataRepository();
            var consumer = harness.Consumer(() => new DoSomethingConsumer(repo));
            
            var someBytes = new byte[]
            {
                115, 171, 121, 43, 89, 24, 199, 205, 23,
                221, 178, 104, 163, 32, 45, 84, 171, 86,
                93, 13, 198, 132, 38, 65, 130, 192, 6,
                159, 227, 104, 245, 222
            };
            
            var requestMessage = new DoSomethingImpl {
                Data1 = await repo.PutBytes(someBytes),
                Foo = new FooImpl {
                    Data2 = await repo.PutBytes(someBytes),
                    WhateverText = "Wild world",
                    Bars = new Bar[] {
                        new BarImpl { WhateverNumber = 999, WhateverDate = DateTime.Now, WhateverText = "It barks", WhateverEnum = AnimalType.Dog, Data3 = await repo.PutBytes(someBytes) }
                    }
                },
                Bars = new Bar[] {
                    new BarImpl { WhateverNumber = 888, WhateverDate = DateTime.Now.AddDays(1), WhateverText = "It meows", WhateverEnum = AnimalType.Cat, Data3 = await repo.PutBytes(someBytes) }
                }
            };

            await harness.Start();
            
            try
            {
                var requestClient = await harness.ConnectRequestClient<DoSomething>();
                var response = await requestClient.GetResponse<SomethingDone>(requestMessage);
             
                Assert.That(consumer.Consumed.Select<DoSomething>().Any(), Is.True);

                Assert.That(response.Message.File.HasValue, Is.True);

                var responsePayloadValue = await response.Message.File.Value;
                Assert.AreEqual(someBytes.Length, responsePayloadValue.Length);

                var consumedMessage = consumer.Consumed.Select<DoSomething>().First().Context.Message;
                var b1 = requestMessage.Bars.First();
                var b2 = consumedMessage.Bars.First();
                
                //check MessageData in all levels
                Assert.AreEqual(requestMessage.Data1.Value.Result, consumedMessage.Data1.Value.Result);
                Assert.AreEqual(requestMessage.Foo.Data2.Value.Result, consumedMessage.Foo.Data2.Value.Result);
                Assert.AreEqual(b1.Data3.Value.Result, b2.Data3.Value.Result);

                //check other props
                Assert.AreEqual(requestMessage.Foo.WhateverText, consumedMessage.Foo.WhateverText);
                
                Assert.AreEqual(b1.WhateverDate, b2.WhateverDate);
                Assert.AreEqual(b1.WhateverEnum, b2.WhateverEnum);
                Assert.AreEqual(b1.WhateverNumber, b2.WhateverNumber);
                Assert.AreEqual(b1.WhateverText, b2.WhateverText);
            }
            finally
            {
                await harness.Stop();
            }
        }

        [Test]
        public async Task Should_handle_nested_message_data_using_send()
        {
            var harness = new InMemoryTestHarness {TestTimeout = TimeSpan.FromSeconds(5)};
            var repo = new InMemoryMessageDataRepository();
            var consumer = harness.Consumer(() => new DoSomethingConsumer(repo));
            
            var someBytes = new byte[]
            {
                115, 171, 121, 43, 89, 24, 199, 205, 23,
                221, 178, 104, 163, 32, 45, 84, 171, 86,
                93, 13, 198, 132, 38, 65, 130, 192, 6,
                159, 227, 104, 245, 222
            };

            var command = new DoSomethingImpl {
                Data1 = await repo.PutBytes(someBytes),
                Foo = new FooImpl {
                    Data2 = await repo.PutBytes(someBytes),
                    WhateverText = "SEND",
                    Bars = new Bar[] {
                        new BarImpl { WhateverNumber = 999, WhateverDate = DateTime.Now, WhateverText = "It barks", WhateverEnum = AnimalType.Dog, Data3 = await repo.PutBytes(someBytes) }
                    }
                },
                Bars = new Bar[] {
                    new BarImpl { WhateverNumber = 888, WhateverDate = DateTime.Now.AddDays(1), WhateverText = "It meows", WhateverEnum = AnimalType.Cat, Data3 = await repo.PutBytes(someBytes) }
                }
            };

            await harness.Start();
            
            try
            {
                await harness.InputQueueSendEndpoint.Send<DoSomething>(command);

                Assert.That(consumer.Consumed.Select<DoSomething>().Any(), Is.True);
                Assert.That(harness.Published.Select<SomethingDone>().Any(), Is.True);

                var response = harness.Published.Select<SomethingDone>().First().Context;
                
                Assert.That(response.Message.File.HasValue, Is.True);

                var responsePayloadValue = await response.Message.File.Value;
                Assert.AreEqual(someBytes.Length, responsePayloadValue.Length);

                var consumedMessage = consumer.Consumed.Select<DoSomething>().First().Context.Message;
                
                var b1 = command.Bars.First();
                var b2 = consumedMessage.Bars.First();
                
                //check MessageData in all levels
                Assert.AreEqual(command.Data1.Value.Result, consumedMessage.Data1.Value.Result);
                Assert.AreEqual(command.Foo.Data2.Value.Result, consumedMessage.Foo.Data2.Value.Result);
                Assert.AreEqual(b1.Data3.Value.Result, b2.Data3.Value.Result);

                //check other props
                Assert.AreEqual(command.Foo.WhateverText, consumedMessage.Foo.WhateverText);
                
                Assert.AreEqual(b1.WhateverDate, b2.WhateverDate);
                Assert.AreEqual(b1.WhateverEnum, b2.WhateverEnum);
                Assert.AreEqual(b1.WhateverNumber, b2.WhateverNumber);
                Assert.AreEqual(b1.WhateverText, b2.WhateverText);
                
                Assert.AreEqual(b1, b2);
            }
            finally
            {
                await harness.Stop();
            }

        }
    }
} 