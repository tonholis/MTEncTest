using System;
using System.IO;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.ExtensionsLoggingIntegration;
using MassTransit.Logging;
using MassTransit.Logging.Tracing;
using MassTransit.MessageData;
using Microsoft.Extensions.Logging;

namespace MTEncTest
{
    public class App 
    {
        readonly IBusControl _bus;
        private readonly IMessageDataRepository _messageDataRepository;

        public App(
            IBusControl bus,
            ILoggerFactory loggerFactory,
            IMessageDataRepository messageDataRepository)
        {
            _bus = bus;
            _messageDataRepository = messageDataRepository;

            if (loggerFactory != null && Logger.Current.GetType() == typeof(TraceLogger))
                ExtensionsLogger.Use(loggerFactory);
        }

        public async Task Run()
        {
            await _bus.StartAsync();

            try
            {
                var fileData = await File.ReadAllBytesAsync("italia.jpg");

                //await TestPublishing(fileData);

                await TestRequestResponse(fileData);
            }
            catch
            {
                throw;
            }
            finally
            {
                await _bus.StopAsync();
            }
        }


        private async Task TestPublishing(byte[] fileData)
        {
            var message = new TestMessage();
            message.LargePayload = await _messageDataRepository.PutBytes(fileData);

            await _bus.Publish(message);
            Console.WriteLine("Message published - LargePayload.Length={0}", fileData.Length);
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

            Console.WriteLine("Response received - LargePayload.Length={0}", payload.Length);
        }
    }
}
