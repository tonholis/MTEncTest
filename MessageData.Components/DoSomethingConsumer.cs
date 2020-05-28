using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.MessageData;
using MessageData.Components.Messages;

namespace MessageData.Components
{
    public class DoSomethingConsumer : IConsumer<DoSomething>
    {
        
        private readonly IMessageDataRepository _messageDataRepository;

        public DoSomethingConsumer(IMessageDataRepository messageDataRepository)
        {
            _messageDataRepository = messageDataRepository;
        }
        
        public async Task Consume(ConsumeContext<DoSomething> context)
        {
            Console.WriteLine("----------- Consuming DoSomething message...");
            Console.WriteLine($" Message.Data1.Length={context.Message.Data1.Value.Result.Length}");
            Console.WriteLine($" Message.Foo.WhateverText={context.Message.Foo.WhateverText}");
            
            //MessageData inside lists/arrays
            var index = 0;
            foreach (var item in context.Message.Foo.Bars)
            {
                var itemData = await item.Data3.Value;
                Console.WriteLine($" Message.Foo.Bars[{index++}] - WhateverNumber={item.WhateverNumber}, WhateverText={item.WhateverText}, WhateverEnum={item.WhateverEnum}, WhateverDate={item.WhateverDate}, Data3.Length={itemData.Length}");
            }
            
            index = 0;
            foreach (var item in context.Message.Bars)
            {
                var itemData = await item.Data3.Value;
                Console.WriteLine($" Message.Bars[{index++}] - WhateverNumber={item.WhateverNumber}, WhateverText={item.WhateverText}, WhateverEnum={item.WhateverEnum}, WhateverDate={item.WhateverDate}, Data3.Length={itemData.Length}");
            }
            
            var data1 = await context.Message.Data1.Value;

            if (context.RequestId.HasValue)
            {
                await context.RespondAsync<SomethingDone>(new {
                    File = await _messageDataRepository.PutBytes(data1),
                    ComplexData = context.Message
                });
            }
            else
            {
                await context.Publish<SomethingDone>(new
                {
                    File = await _messageDataRepository.PutBytes(data1),
                    ComplexData = context.Message
                });
            }
            
            Console.WriteLine("----------- End of the consumer");
            Console.WriteLine();
        }
    }
}