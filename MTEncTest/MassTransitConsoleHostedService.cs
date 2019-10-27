using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.ExtensionsLoggingIntegration;
using MassTransit.Logging;
using MassTransit.Logging.Tracing;
using MassTransit.MessageData;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MTEncTest
{
    public class MassTransitConsoleHostedService : IHostedService
    {
        readonly IBusControl _bus;
        private readonly IMessageDataRepository _messageDataRepository;

        public MassTransitConsoleHostedService(
            IBusControl bus,
            ILoggerFactory loggerFactory,
            IMessageDataRepository messageDataRepository)
        {
            _bus = bus;
            _messageDataRepository = messageDataRepository;

            if (loggerFactory != null && Logger.Current.GetType() == typeof(TraceLogger))
                ExtensionsLogger.Use(loggerFactory);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _bus.StartAsync(cancellationToken).ConfigureAwait(false);

            await RunTests();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _bus.StopAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task RunTests()
        {
            var fileData = await File.ReadAllBytesAsync("italia.jpg");

            ////simple message using publish
            var message = new TestMessage();
            message.LargePayload = await _messageDataRepository.PutBytes(fileData);

            await _bus.Publish(message);
            Console.WriteLine("Message published - LargePayload size {0}", fileData.Length);


            await TestRequestResponse(fileData);
        }

        private async Task TestRequestResponse(byte[] fileData)
        {
            var request = new TestRequest();
            request.LargePayload = await _messageDataRepository.PutBytes(fileData);

            Console.WriteLine("Sending TestRequest...");
            var client = _bus.CreateRequestClient<TestRequest>(Program.TestRequestConsumerUri);
            var response = await client.GetResponse<TestResponse>(request);

            //var payload = await response.Message.LargePayload.Value; //doesn't work
            var messageData = await _messageDataRepository.GetBytes(response.Message.LargePayload.Address);
            var payload = await messageData.Value;

            Console.WriteLine("Response received - LargePayload size {0}", payload.Length);
        }
    }
}
