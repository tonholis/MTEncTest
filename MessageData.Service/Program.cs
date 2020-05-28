using System;
using System.IO;
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
            // var encKey = new byte[]
            // {
            //     115, 171, 121, 43, 89, 24, 199, 205, 23,
            //     221, 178, 104, 163, 32, 45, 84, 171, 86,
            //     93, 13, 198, 132, 38, 65, 130, 192, 6,
            //     159, 227, 104, 245, 222
            // };

            // var repo = new MongoDbMessageDataRepository("mongodb://localhost", "testdb");
            var repo = new InMemoryMessageDataRepository();

            // var bus = Bus.Factory.CreateUsingRabbitMq(sbc =>
            var bus = Bus.Factory.CreateUsingInMemory(sbc =>
            {
                // sbc.Host(new Uri("rabbitmq://localhost"));
                
                // sbc.UseEncryption(encKey);
                sbc.UseMessageData(repo);
                sbc.ReceiveEndpoint("test-queue", e =>
                {
                    EndpointConvention.Map<DoSomething>(e.InputAddress);
                    e.Consumer(() => new DoSomethingConsumer(repo));
                });
            });

            bus.Start();

            try
            {
                await SendTest(bus, repo);
            }
            finally
            {
                bus.Stop();
            }
        }

        private static async Task SendTest(IBusControl bus, IMessageDataRepository repo)
        {
            var someData = new byte[] {221, 178, 104, 163};
            var fileData = await File.ReadAllBytesAsync("flag.jpg");

            var requestClient = bus.CreateRequestClient<DoSomething>();

            var requestMessage = new DoSomethingImpl
            {
                Data1 = await repo.PutBytes(fileData),
                Foo = new FooImpl
                {
                    Data2 = await repo.PutBytes(someData),
                    WhateverText = "Animal world",
                    Bars = new Bar[]
                    {
                        new BarImpl
                        {
                            WhateverNumber = 999, WhateverDate = DateTime.Now, WhateverText = "It barks", WhateverEnum = AnimalType.Dog,
                            Data3 = await repo.PutBytes(someData)
                        }
                    }
                },
                Bars = new Bar[]
                {
                    new BarImpl
                    {
                        WhateverNumber = 888, WhateverDate = DateTime.Now.AddDays(1), WhateverText = "It meows", WhateverEnum = AnimalType.Cat,
                        Data3 = await repo.PutBytes(someData)
                    }
                }
            };
            
            await DebugMessage(requestMessage, "Request message");
            
            var response = await requestClient.GetResponse<SomethingDone>(requestMessage);
            await DebugMessage(response.Message.ComplexData, "Response message");
        }

        private static async Task DebugMessage(DoSomething message, string scope)
        {
            Console.WriteLine($"Start - {scope}--------------");
            
            var rootData = await message.Data1.Value;
            Console.WriteLine($" Message.Data1.Length={rootData.Length}");

            //MessageData inside nested object
            var childData = await message.Foo.Data2.Value;
            Console.WriteLine($" Message.Foo.Data2.Length={childData.Length}");
            Console.WriteLine($" Message.Foo.WhateverText={message.Foo.WhateverText}");

            //MessageData inside lists/arrays
            var index = 0;
            foreach (var item in message.Foo.Bars)
            {
                var itemData = await item.Data3.Value;
                Console.WriteLine($" Message.Foo.Bars[{index++}] - WhateverNumber={item.WhateverNumber}, WhateverText={item.WhateverText}, WhateverEnum={item.WhateverEnum}, WhateverDate={item.WhateverDate}, Data3.Length={itemData.Length}");
            }
            
            index = 0;
            foreach (var item in message.Bars)
            {
                var itemData = await item.Data3.Value;
                Console.WriteLine($" Message.Bars[{index++}] - WhateverNumber={item.WhateverNumber}, WhateverText={item.WhateverText}, WhateverEnum={item.WhateverEnum}, WhateverDate={item.WhateverDate}, Data3.Length={itemData.Length}");
            }
            
            Console.WriteLine($"End - {scope}--------------");
            Console.WriteLine();
        }
    }
}