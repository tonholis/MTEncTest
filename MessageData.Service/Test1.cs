using System;
using System.IO;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.MessageData;
using MessageData.Components.Messages;
using MessageData.Components.Model;

namespace MessageData.Service
{
    public static class Test1
    {   
        public static async Task Run(IBusControl bus, IMessageDataRepository repo)
        {
            Console.WriteLine(new string('=', 80));
            Console.WriteLine("TEST 1");
            Console.WriteLine(new string('=', 80));
            
            var requestClient = bus.CreateRequestClient<Message1>();
            var request = await CreateMessage1(repo);

            await DebugMessage(request, "Request message");
            
            var response = await requestClient.GetResponse<Message1Completed>(request);
            await DebugResponse(response.Message, "Response message");
        }
        
        private static async Task<Message1> CreateMessage1(IMessageDataRepository repo)
        {
            var someBytes = new byte[] {115, 171, 121, 43};
            
            return new Message1Impl {
                TopText = "A message WITH NO messageData at the top-level",
                Foo = new Program.FooImpl {
                    File = await repo.PutBytes(someBytes),
                    SomeText = "Animal world",
                    Bars = new Bar[] {
                        new Program.BarImpl
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
                    new Program.BarImpl
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

        private static async Task DebugResponse(Message1Completed message, string scope)
        {
            Console.WriteLine($"Start - {scope}--------------");
            Console.WriteLine($" Message.TopText={message.TopText}");
            
            var fileData = message.File.HasValue ? await message.File.Value : null;
            Console.WriteLine($" Message.File.Length={fileData?.Length}");

            //Foo data
            Console.WriteLine($" Message.DataReceived.Foo.SomeText={message.DataReceived?.Foo?.SomeText}");
            
            fileData = message.DataReceived?.Foo?.File != null ? await message.DataReceived.Foo.File.Value : null;
            Console.WriteLine($" Message.DataReceived.File.Length={fileData?.Length}");
            
            Console.WriteLine($" Message.DataReceived?.Foo?.Bars.Count={message.DataReceived?.Foo?.Bars?.Length}");
            if (message.DataReceived?.Foo?.Bars != null)
                await Program.DebugBarList(message.DataReceived.Foo.Bars, "DataReceived.Foo.Bars");
            
            //Bar data
            if (message.DataReceived?.Bars != null)
                await Program.DebugBarList(message.DataReceived.Bars, "DataReceived.Bars");

            Console.WriteLine($"End - {scope}--------------");
            Console.WriteLine();
        }

        private static async Task DebugMessage(Message1 message, string scope)
        {
            Console.WriteLine($"Start - {scope}--------------");
            Console.WriteLine($" Message.TopText={message.TopText}");
            
            //Foo data
            Console.WriteLine($" Message.Foo.SomeText={message.Foo?.SomeText}");
            
            var fileData = message.Foo?.File != null ? await message.Foo.File.Value : null;
            Console.WriteLine($" Message.Foo.File.Length={fileData?.Length}");
            
            Console.WriteLine($" Message.Foo.Bars.Count={message.Foo?.Bars?.Length}");
            if (message.Foo?.Bars != null)
                await Program.DebugBarList(message.Foo.Bars, "Foo.Bars"); 
            
            //Bar data
            if (message.Bars != null)
                await Program.DebugBarList(message.Bars, "Bars");

            Console.WriteLine($"End - {scope}--------------");
            Console.WriteLine();
        }
    }
    
    public class Message1Impl : Message1
    {
        public Foo Foo { get; set; }
        public Bar[] Bars { get; set; }
            
        public string TopText { get; set; }
    }
}