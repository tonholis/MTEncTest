using System;
using System.Threading.Tasks;
using MassTransit;

namespace MTEncTest
{

    public class TestMessageConsumer : IConsumer<TestMessage>
    {
        public async Task Consume(ConsumeContext<TestMessage> context)
        {
            var payload = await context.Message.LargePayload.Value;
            Console.WriteLine("Message consumed - LargePayload.Length={0}", payload.Length);
        }
    }
}
