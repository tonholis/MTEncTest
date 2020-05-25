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
            var consumer = harness.Consumer(() => new TestRequestConsumer(repo));
            
            var someBytes = new byte[]
            {
                115, 171, 121, 43, 89, 24, 199, 205, 23,
                221, 178, 104, 163, 32, 45, 84, 171, 86,
                93, 13, 198, 132, 38, 65, 130, 192, 6,
                159, 227, 104, 245, 222
            };
            
            var requestMessage = new TestRequestImpl {
                Data1 = await repo.PutBytes(someBytes),
                Foo = new FooImpl {
                    Data2 = await repo.PutBytes(someBytes),
                    Text = "Wild world",
                    Bars = new Bar[] {
                        new BarImpl { Number = 999, Date = DateTime.Now, Text = "It barks", Enum = AnimalType.Dog, Data3 = await repo.PutBytes(someBytes) }
                    }
                },
                Bars = new Bar[] {
                    new BarImpl { Number = 888, Date = DateTime.Now.AddDays(1), Text = "It meows", Enum = AnimalType.Cat, Data3 = await repo.PutBytes(someBytes) }
                }
            };

            await harness.Start();
            
            try
            {
                var requestClient = await harness.ConnectRequestClient<TestRequest>();
                var response = await requestClient.GetResponse<TestResponse>(requestMessage);
             
                Assert.That(consumer.Consumed.Select<TestRequest>().Any(), Is.True);

                Assert.That(response.Message.Payload.HasValue, Is.True);

                var responsePayloadValue = await response.Message.Payload.Value;
                Assert.AreEqual(someBytes.Length, responsePayloadValue.Length);

                var consumedMessage = consumer.Consumed.Select<TestRequest>().First().Context.Message;
                var b1 = requestMessage.Bars.First();
                var b2 = consumedMessage.Bars.First();
                
                //check MessageData in all levels
                Assert.AreEqual(requestMessage.Data1.Value.Result, consumedMessage.Data1.Value.Result);
                Assert.AreEqual(requestMessage.Foo.Data2.Value.Result, consumedMessage.Foo.Data2.Value.Result);
                Assert.AreEqual(b1.Data3.Value.Result, b2.Data3.Value.Result);

                //check other props
                Assert.AreEqual(requestMessage.Foo.Text, consumedMessage.Foo.Text);
                
                Assert.AreEqual(b1.Date, b2.Date);
                Assert.AreEqual(b1.Enum, b2.Enum);
                Assert.AreEqual(b1.Number, b2.Number);
                Assert.AreEqual(b1.Text, b2.Text);
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
            var consumer = harness.Consumer(() => new TestRequestConsumer(repo));
            
            var someBytes = new byte[]
            {
                115, 171, 121, 43, 89, 24, 199, 205, 23,
                221, 178, 104, 163, 32, 45, 84, 171, 86,
                93, 13, 198, 132, 38, 65, 130, 192, 6,
                159, 227, 104, 245, 222
            };

            var command = new TestRequestImpl {
                Data1 = await repo.PutBytes(someBytes),
                Foo = new FooImpl {
                    Data2 = await repo.PutBytes(someBytes),
                    Text = "SEND",
                    Bars = new Bar[] {
                        new BarImpl { Number = 999, Date = DateTime.Now, Text = "It barks", Enum = AnimalType.Dog, Data3 = await repo.PutBytes(someBytes) }
                    }
                },
                Bars = new Bar[] {
                    new BarImpl { Number = 888, Date = DateTime.Now.AddDays(1), Text = "It meows", Enum = AnimalType.Cat, Data3 = await repo.PutBytes(someBytes) }
                }
            };

            await harness.Start();
            
            try
            {
                await harness.InputQueueSendEndpoint.Send<TestRequest>(command);

                Assert.That(consumer.Consumed.Select<TestRequest>().Any(), Is.True);
                Assert.That(harness.Published.Select<TestResponse>().Any(), Is.True);

                var response = harness.Published.Select<TestResponse>().First().Context;
                
                Assert.That(response.Message.Payload.HasValue, Is.True);

                var responsePayloadValue = await response.Message.Payload.Value;
                Assert.AreEqual(someBytes.Length, responsePayloadValue.Length);

                var consumedMessage = consumer.Consumed.Select<TestRequest>().First().Context.Message;
                
                var b1 = command.Bars.First();
                var b2 = consumedMessage.Bars.First();
                
                //check MessageData in all levels
                Assert.AreEqual(command.Data1.Value.Result, consumedMessage.Data1.Value.Result);
                Assert.AreEqual(command.Foo.Data2.Value.Result, consumedMessage.Foo.Data2.Value.Result);
                Assert.AreEqual(b1.Data3.Value.Result, b2.Data3.Value.Result);

                //check other props
                Assert.AreEqual(command.Foo.Text, consumedMessage.Foo.Text);
                
                Assert.AreEqual(b1.Date, b2.Date);
                Assert.AreEqual(b1.Enum, b2.Enum);
                Assert.AreEqual(b1.Number, b2.Number);
                Assert.AreEqual(b1.Text, b2.Text);
                
                Assert.AreEqual(b1, b2);
            }
            finally
            {
                await harness.Stop();
            }

        }
    }
} 