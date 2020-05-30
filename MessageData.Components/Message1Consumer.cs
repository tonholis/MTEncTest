using System;
using System.IO;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.MessageData;
using MessageData.Components.Messages;

namespace MessageData.Components
{
    public class Message1Consumer : IConsumer<Message1>
    {
        
        private readonly IMessageDataRepository _messageDataRepository;

        public Message1Consumer(IMessageDataRepository messageDataRepository)
        {
            _messageDataRepository = messageDataRepository;
        }
        
        public async Task Consume(ConsumeContext<Message1> context)
        {
            Console.WriteLine("----------- Consuming {0}", context.Message.TopText);

            var fooFile = await context.Message.Foo.File.Value;
            Console.WriteLine(" Message.Foo.File.Length={0}\n", fooFile?.Length);
            
            var fileData = new byte[1000];

            if (context.RequestId.HasValue)
            {
                await context.RespondAsync<Message1Completed>(new
                {
                    File = fileData,
                    DataReceived = context.Message,
                    TopText = "Message 1 consumed"
                });
            }
            else
            {
                await context.Publish<Message1Completed>(new
                {
                    File = fileData,
                    DataReceived = context.Message,
                    TopText = "Message 1 consumed"
                });
            }
        }
    }
}