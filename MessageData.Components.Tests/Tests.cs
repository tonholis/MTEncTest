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
        private async Task<Message2> CreateMessage2(IMessageDataRepository repo)
        {
            var someBytes = new byte[] {115, 171, 121, 43};
            
            return new Message2Impl {
                TopText = "A message WITH messageData at the top-level",
                File = await repo.PutBytes(someBytes),
                Foo = new FooImpl {
                    File = await repo.PutBytes(someBytes),
                    SomeText = "Animal world",
                    Bars = new Bar[] {
                        new BarImpl
                        {
                            SomeNumber = 999,
                            SomeDatetime = DateTime.Now,
                            SomeText = "It barks",
                            SomeEnum = AnimalType.Dog,
                            File = await repo.PutBytes(someBytes)
                        }
                    }
                },
                Bars = new Bar[] {
                    new BarImpl
                    {
                        SomeNumber = 888,
                        SomeDatetime = DateTime.Now.AddDays(1),
                        SomeText = "It meows",
                        SomeEnum = AnimalType.Cat,
                        File = await repo.PutBytes(someBytes)
                    }
                }
            };
        }
        private async Task<Message1> CreateMessage1(IMessageDataRepository repo)
        {
            var someBytes = new byte[] {115, 171, 121, 43};
            
            return new Message1Impl {
                TopText = "A message with NO MESSAGEDATA at the top-level",
                Foo = new FooImpl {
                    File = await repo.PutBytes(someBytes),
                    SomeText = "Animal world",
                    Bars = new Bar[] {
                        new BarImpl
                        {
                            SomeNumber = 999,
                            SomeDatetime = DateTime.Now,
                            SomeText = "It barks",
                            SomeEnum = AnimalType.Dog,
                            File = await repo.PutBytes(someBytes)
                        }
                    }
                },
                Bars = new Bar[] {
                    new BarImpl
                    {
                        SomeNumber = 888,
                        SomeDatetime = DateTime.Now.AddDays(1),
                        SomeText = "It meows",
                        SomeEnum = AnimalType.Cat,
                        File = await repo.PutBytes(someBytes)
                    }
                }
            };
        }

        [Test]
        public async Task Should_work_when_no_message_data_at_top_level()
        {
            var harness = new InMemoryTestHarness {TestTimeout = TimeSpan.FromSeconds(5)};
            var repo = new InMemoryMessageDataRepository();
            var consumer = harness.Consumer(() => new Message1Consumer(repo));
            var request = await CreateMessage1(repo);

            await harness.Start();
            
            try
            {
                var requestClient = await harness.ConnectRequestClient<Message1>();
                var response = await requestClient.GetResponse<Message1Completed>(request);
             
                //checking the consumed message
                Assert.That(consumer.Consumed.Select<Message1>().Any(), Is.True);
                
                var consumedMessage = consumer.Consumed.Select<Message1>().First().Context.Message;

                //checking main data consumed
                Assert.That(consumedMessage.TopText, Is.Not.Null);
                Assert.That(consumedMessage.Foo, Is.Not.Null);
                Assert.That(consumedMessage.Bars, Is.Not.Null);
                
                //checking Foo consumed
                Assert.That(consumedMessage.Foo.SomeText, Is.Not.Null);
                Assert.That(consumedMessage.Foo.File, Is.Not.Null);
                Assert.That(consumedMessage.Foo.Bars, Is.Not.Null);

                //checking the response received back
                Assert.That(response.Message.File, Is.Not.Null);
                Assert.That(response.Message.TopText, Is.Not.Null);
                Assert.That(response.Message.DataReceived, Is.Not.Null);
                
                Assert.That(response.Message.DataReceived.Bars, Is.Not.Null);
                Assert.That(response.Message.DataReceived.Foo, Is.Not.Null);
                Assert.That(response.Message.DataReceived.TopText, Is.Not.Null);
                
                //comparing data sent vs received
                Assert.AreEqual(request.TopText, response.Message.DataReceived.TopText);
            }
            finally
            {
                await harness.Stop();
            }
        }
        
        [Test]
        public async Task Should_work_when_message_data_at_top_level()
        {
            var harness = new InMemoryTestHarness {TestTimeout = TimeSpan.FromSeconds(5)};
            var repo = new InMemoryMessageDataRepository();
            var consumer = harness.Consumer(() => new Message2Consumer(repo));
            var request = await CreateMessage2(repo);

            await harness.Start();
            
            try
            {
                var requestClient = await harness.ConnectRequestClient<Message2>();
                var response = await requestClient.GetResponse<Message2Completed>(request);
             
                //checking the consumed message
                Assert.That(consumer.Consumed.Select<Message2>().Any(), Is.True);
                
                var consumedMessage = consumer.Consumed.Select<Message2>().First().Context.Message;

                //checking main data consumed
                Assert.That(consumedMessage.File, Is.Not.Null);
                Assert.That(consumedMessage.TopText, Is.Not.Null.Or.Empty);
                Assert.That(consumedMessage.Foo, Is.Not.Null);
                Assert.That(consumedMessage.Bars, Is.Not.Null);
                
                //checking Foo consumed
                Assert.That(consumedMessage.Foo.SomeText, Is.Not.Null.Or.Empty);
                Assert.That(consumedMessage.Foo.File, Is.Not.Null);
                Assert.That(consumedMessage.Foo.Bars, Is.Not.Null);

                //checking the response received back
                Assert.That(response.Message.File, Is.Not.Null);
                Assert.That(response.Message.TopText, Is.Not.Null.Or.Empty);
                Assert.That(response.Message.DataReceived, Is.Not.Null);
                
                Assert.That(response.Message.DataReceived.File, Is.Not.Null);
                Assert.That(response.Message.DataReceived.Bars, Is.Not.Null);
                Assert.That(response.Message.DataReceived.Foo, Is.Not.Null);
                Assert.That(response.Message.DataReceived.TopText, Is.Not.Null.Or.Empty);
                
                //comparing data sent vs received
                Assert.AreEqual(request.TopText, response.Message.DataReceived.TopText);
                Assert.AreEqual(request.Foo.SomeText, response.Message.DataReceived.Foo.SomeText);

                Assert.That(response.Message.File.HasValue, Is.True);
                var receivedFile = await response.Message.File.Value;
                Assert.That(receivedFile.Length, Is.GreaterThan(0));
                
                Assert.AreEqual(request.Foo.SomeText, consumedMessage.Foo.SomeText);
            }
            finally
            {
                await harness.Stop();
            }
        }
    }
    
    public class Message1Impl : Message1
    {
        public Foo Foo { get; set; }
        public Bar[] Bars { get; set; }
            
        public string TopText { get; set; }
    }
    
    public class Message2Impl : Message2
    {
        public MessageData<byte[]> File { get; set;  }
        public Foo Foo { get; set; }
        public Bar[] Bars { get; set; }
            
        public string TopText { get; set; }
    }

    public class FooImpl : Foo
    {
        public MessageData<byte[]> File { get; set; }
        public string SomeText { get; set; }
        public Bar[] Bars { get; set;}
    }

    public class BarImpl : Bar
    {
        public int SomeNumber { get; set; }
        public string SomeText { get; set;}
        public AnimalType SomeEnum { get; set;}
        public DateTime SomeDatetime { get; set;}
        
        public MessageData<byte[]> File { get; set; }
    }
} 