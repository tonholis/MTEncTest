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
            Console.WriteLine($" Message.Data1.Length={context.Message.Data1.Value.Result.Length}");
            Console.WriteLine($" Message.Foo.Text={context.Message.Foo.Text}");
            
            //MessageData inside lists/arrays
            var index = 0;
            foreach (var item in context.Message.Bars)
            {
                var itemData = await item.Data3.Value;
                Console.WriteLine($" Message.Bars[{index++}] - Number={item.Number}, Text={item.Text}, Enum={item.Enum}, Date={item.Date}, Data3.Length={itemData.Length}");
            }

            index = 0;
            foreach (var item in context.Message.Foo.Bars)
            {
                var itemData = await item.Data3.Value;
                Console.WriteLine($" Message.Foo.Bars[{index++}] - Number={item.Number}, Text={item.Text}, Enum={item.Enum}, Date={item.Date}, Data3.Length={itemData.Length}");
            }
            
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
            
            Console.WriteLine("-----------");
        }
    }
}