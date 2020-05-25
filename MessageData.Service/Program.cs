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
            var key = new byte[]
            {
                115, 171, 121, 43, 89, 24, 199, 205, 23,
                221, 178, 104, 163, 32, 45, 84, 171, 86,
                93, 13, 198, 132, 38, 65, 130, 192, 6,
                159, 227, 104, 245, 222
            };

            // var repo = new MongoDbMessageDataRepository("mongodb://localhost", "testdb");
            var repo = new InMemoryMessageDataRepository();

            // var bus = Bus.Factory.CreateUsingRabbitMq(sbc =>
            var bus = Bus.Factory.CreateUsingInMemory(sbc =>
            {
                // sbc.UseEncryption(key);
                sbc.UseMessageData(repo);
                sbc.ReceiveEndpoint("test-queue", e =>
                {
                    EndpointConvention.Map<TestRequest>(e.InputAddress);
                    e.Consumer(() => new TestRequestConsumer(repo));
                });
            });

            bus.Start();

            try
            {
                await SendTest(bus, repo);
                await SendTest(bus, repo, false);
            }
            finally
            {
                bus.Stop();
            }
        }

        private static async Task SendTest(IBusControl bus, IMessageDataRepository repo, bool isRequest = true)
        {
            var someData = new byte[] {221, 178, 104, 163};
            var fileData = await File.ReadAllBytesAsync("flag.jpg");

            var requestClient = bus.CreateRequestClient<TestRequest>();

            var requestMessage = new TestRequestImpl
            {
                Data1 = await repo.PutBytes(fileData),
                Foo = new FooImpl
                {
                    Data2 = await repo.PutBytes(someData),
                    Text = "Wild world",
                    Bars = new Bar[]
                    {
                        new BarImpl
                        {
                            Number = 999, Date = DateTime.Now, Text = "It barks", Enum = AnimalType.Dog,
                            Data3 = await repo.PutBytes(someData)
                        }
                    }
                },
                Bars = new Bar[]
                {
                    new BarImpl
                    {
                        Number = 888, Date = DateTime.Now.AddDays(1), Text = "It meows", Enum = AnimalType.Cat,
                        Data3 = await repo.PutBytes(someData)
                    }
                }
            };

            Console.WriteLine("SENDING REQUEST...");
            await DebugMessage(requestMessage);

            var response = await requestClient.GetResponse<TestResponse>(requestMessage);

            var responseMdValue = await response.Message.Payload.Value;
            Console.WriteLine($"MessageData value in the response message, length: {responseMdValue.Length}");
            await DebugMessage(response.Message.OriginalRequest);
        }

        private static async Task DebugMessage(TestRequest message)
        {
            Console.WriteLine("-----------");

            var rootData = await message.Data1.Value;
            Console.WriteLine($"Message.Data1.Length={rootData.Length}");

            //.Child details
            var childData = await message.Foo.Data2.Value;
            Console.WriteLine($"Message.Foo.Data2.Length={childData.Length}");
            Console.WriteLine($"Message.Foo.Text={message.Foo.Text}");

            //.List details
            var index = 0;
            foreach (var item in message.Bars)
            {
                var itemData = await item.Data3.Value;
                Console.WriteLine(
                    $"Message.Bars[{index++}] - Number={item.Number}, Text={item.Text}, Enum={item.Enum}, Date={item.Date}, Data3.Length={itemData.Length}");
            }

            index = 0;
            foreach (var item in message.Foo.Bars)
            {
                var itemData = await item.Data3.Value;
                Console.WriteLine(
                    $"Message.Foo.Bars[{index++}] - Number={item.Number}, Text={item.Text}, Enum={item.Enum}, Date={item.Date}, Data3.Length={itemData.Length}");
            }

            Console.WriteLine("-----------");
        }
    }
}