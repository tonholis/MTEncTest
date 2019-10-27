﻿using System;
using System.IO;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.MessageData;

namespace MTEncTest
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
            //var payload = await context.Message.LargePayload.Value;
            var messageData = await _messageDataRepository.GetBytes(context.Message.LargePayload.Address);
            var payload = await messageData.Value;

            Console.WriteLine("Request received - LargePayload.Length={0}", payload.Length);

            var fileData = await File.ReadAllBytesAsync("italia.jpg");
            var message = new TestResponseImpl();
            message.LargePayload = await _messageDataRepository.PutBytes(fileData);

            await context.RespondAsync<TestResponse>(message);

            Console.WriteLine("  Message responded - LargePayload.Length={0}", payload.Length);
        }
    }
}
