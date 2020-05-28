using System;
using System.IO;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.MessageData;
using MessageData.Components.Messages;

namespace MessageData.Components
{
    public class Message2Consumer : IConsumer<Message2>
    {   
        private readonly IMessageDataRepository _messageDataRepository;

        public Message2Consumer(IMessageDataRepository messageDataRepository)
        {
            _messageDataRepository = messageDataRepository;
        }
        
        public async Task Consume(ConsumeContext<Message2> context)
        {
            Console.WriteLine("----------- Consuming {0}\n", context.Message.TopText);
   
            var fileData = new byte[] { 255, 255, 255 };

            if (context.RequestId.HasValue)
            {
                await context.RespondAsync<Message2Completed>(new
                {
                    File = await _messageDataRepository.PutBytes(fileData),
                    DataReceived = context.Message,
                    TopText = "Message 2 consumed"
                });
            }
            else
            {
                await context.Publish<Message2Completed>(new
                {
                    File = await _messageDataRepository.PutBytes(fileData),
                    DataReceived = context.Message,
                    TopText = "Message 2 consumed"
                });
            }
        }
    }
}