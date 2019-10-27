using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.MessageData;
using MassTransit.MongoDbIntegration.MessageData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MTEncTest
{
    class Program
    {
        public static Uri TestRequestConsumerUri { get; private set; }

        static async Task Main(string[] args)
        {
            var serviceProvider = ConfigureServices();

            serviceProvider
                .GetService<ILoggerFactory>()
                .AddConsole(LogLevel.Debug);

            await serviceProvider.GetService<App>().Run();
        }

        static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddLogging();

            services.AddScoped<App>();

            //services.AddSingleton<IMessageDataRepository>(provider =>
            //{
            //    return new MongoDbMessageDataRepository("mongodb://localhost", "testdb");
            //});

            services.AddSingleton<IMessageDataRepository>(provider =>
            {
                return new InMemoryMessageDataRepository();
            });

            services.AddMassTransit(cfg =>
            {
                cfg.AddBus(ConfigureBus);
                cfg.AddConsumer<TestMessageConsumer>();
                cfg.AddConsumer<TestRequestConsumer>();
                cfg.AddRequestClient<TestRequest>();
            });

            return services.BuildServiceProvider();
        }

        static IBusControl ConfigureBus(IServiceProvider provider)
        {
            return Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                var host = cfg.Host("localhost", "/");

                var messageDataRepository = provider.GetRequiredService<IMessageDataRepository>();
                cfg.UseMessageData<TestMessage>(messageDataRepository);
                cfg.UseMessageData<TestRequest>(messageDataRepository);
                cfg.UseMessageData<TestResponse>(messageDataRepository);

                var key = new byte[32] { 115, 171, 121, 43, 89, 24, 199, 205, 23, 221, 178, 104, 163, 32, 45, 84, 171, 86, 93, 13, 198, 132, 38, 65, 130, 192, 6, 159, 227, 104, 245, 222 };

                cfg.ReceiveEndpoint(host, "test-queue", e => {
                    //e.UseEncryption(key);

                    e.ConfigureConsumer<TestMessageConsumer>(provider);
                });

                cfg.ReceiveEndpoint(host, "test-req-res-queue", e => {
                    e.UseEncryption(key);

                    e.ConfigureConsumer<TestRequestConsumer>(provider);
                    TestRequestConsumerUri = e.InputAddress;
                });
            });
        }
    }
}
