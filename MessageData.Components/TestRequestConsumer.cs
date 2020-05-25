using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.MessageData;
using MessageData.Components.Messages;

namespace MessageData.Components
{
    public class TestRequestConsumer : IConsumer<TestRequest>
    {
        
        private readonly IMessageDataRepository _messageDataRepository;

        public TestRequestConsumer(IMessageDataRepository messageDataRepository)
        {
            _messageDataRepository = messageDataRepository;
        }
        
        public async Task Consume(ConsumeContext<TestRequest> context)
        {
            Console.WriteLine("Message received in the consumer");
            Console.WriteLine($"  Message.Foo.Text={context.Message.Foo.Text}");
            
            var data1 = await context.Message.Data1.Value;

            if (context.RequestId.HasValue)
            {
                await context.RespondAsync<TestResponse>(new {
                    Payload = await _messageDataRepository.PutBytes(data1),
                    OriginalRequest = context.Message
                });
            }
            else
            {
                await context.Publish<TestResponse>(new {
                    Payload = await _messageDataRepository.PutBytes(data1)
                });
            }
        }
    }
}