using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.MessageData;

namespace MTEncTest
{
    class Program
    {
        public static Uri TestRequestConsumerUri { get; private set; }

        static async Task Main(string[] args)
        {
			var key = new byte[]
            {
                115, 171, 121, 43, 89, 24, 199, 205, 23,
                221, 178, 104, 163, 32, 45, 84, 171, 86,
                93, 13, 198, 132, 38, 65, 130, 192, 6,
                159, 227, 104, 245, 222
            };

			// var repo = new MongoDbMessageDataRepository("mongodb://localhost", "test-mongodb-database");
			var repo = new InMemoryMessageDataRepository();
			
            var bus = Bus.Factory.CreateUsingInMemory(sbc =>
			{
				sbc.UseEncryption(key);
				sbc.UseMessageData(repo);

                sbc.ReceiveEndpoint(ep =>
				{
					ep.Handler<TestRequest>(async context =>
					{
						await Console.Out.WriteLineAsync("Received...").ConfigureAwait(false);

						var v1 = await context.Message.Payload.Value.ConfigureAwait(false);
						await Console.Out.WriteLineAsync($"v1: {v1.Length}").ConfigureAwait(false);

						// var v2 = await context.Message.Nested.Payload.Value.ConfigureAwait(false);
						// await Console.Out.WriteLineAsync($"v2: {v2.Length}").ConfigureAwait(false);
						
                        foreach (var item in context.Message.Nested.List)
							await Console.Out.WriteLineAsync($"Item n: {item.Number}");					

                        await context.RespondAsync(new TestResponse {
							Payload = await repo.PutBytes(key)
                        });
					});
				});
			});

			bus.Start();

            var client = bus.CreateRequestClient<TestRequest>();
            var request = new TestRequest {
                Payload = await repo.PutBytes(key),
				Nested = new Foo {
					// Payload = await repo.PutBytes(key),
					List = new List<Bar>() {
                        new Bar { Number = 999, Payload = await repo.PutBytes(key) },
						new Bar { Number = 888, Payload = await repo.PutBytes(key) }
                    }
                }   
            };

			var response = await client.GetResponse<TestResponse>(request);
			// var responseValue = await response.Message.Payload.Value; //raises 'The message data was not loaded'
			var responseValue = await (await repo.GetBytes(response.Message.Payload.Address)).Value;
            Console.WriteLine($"Response value length: {responseValue.Length}");

			Console.WriteLine("Press any key to exit");
			Console.ReadKey();

			bus.Stop();
        }
    }
}
