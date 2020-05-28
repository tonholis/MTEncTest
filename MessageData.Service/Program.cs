using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.MessageData;
using MessageData.Components;
using MessageData.Components.Messages;
using MessageData.Components.Model;

namespace MessageData.Service
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            // var repo = new MongoDbMessageDataRepository("mongodb://localhost", "testdb");
            var repo = new InMemoryMessageDataRepository();

            // var bus = Bus.Factory.CreateUsingRabbitMq(sbc =>
            var bus = Bus.Factory.CreateUsingInMemory(sbc =>
            {
                // sbc.Host(new Uri("rabbitmq://localhost"));
                
                sbc.UseMessageData(repo);
                
                sbc.ReceiveEndpoint("test1-queue", e =>
                {
                    EndpointConvention.Map<Message1>(e.InputAddress);
                    e.Consumer(() => new Message1Consumer(repo));
                });
                
                sbc.ReceiveEndpoint("test2-queue", e =>
                {
                    EndpointConvention.Map<Message2>(e.InputAddress);
                    e.Consumer(() => new Message2Consumer(repo));
                });
            });

            bus.Start();

            try
            {
                await Test1.Run(bus, repo);
                
                await Test2.Run(bus, repo);
            }
            finally
            {
                bus.Stop();
            }
        }

        public static async Task DebugBarList(Bar[] list, string prefix)
        {
            var index = 0;
            foreach (var item in list)
            {
                var itemData = item.File.HasValue ? await item.File.Value : null;
                Console.WriteLine($" Message.{prefix}[{index++}] - SomeNumber={item.SomeNumber}, SomeText={item.SomeText}, SomeEnum={item.SomeEnum}, SomeDatetime={item.SomeDatetime}, File.Length={itemData?.Length}");
            }
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
}