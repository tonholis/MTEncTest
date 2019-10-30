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
						Console.WriteLine("MESSAGE RECEIVED...");
						await DebugMessage(context.Message);

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
				Child = new Foo {
					Payload = await repo.PutBytes(key),
					Text = "Child object",
					List = new List<Bar>() {
                        new Bar { Number = 999, Date = DateTime.Now, Text = "It barks", Enum = AnimalType.Dog, Payload = await repo.PutBytes(key) }
                    }
                },
				List = new List<Bar>() {
					new Bar { Number = 888, Date = DateTime.Now.AddDays(1), Text = "It meows", Enum = AnimalType.Cat, Payload = await repo.PutBytes(key) }
				}
            };
			
			Console.WriteLine("SENDING REQUEST...");
			await DebugMessage(request);

			var response = await client.GetResponse<TestResponse>(request);
			// var responseValue = await response.Message.Payload.Value; //raises 'The message data was not loaded'
			var responseValue = await (await repo.GetBytes(response.Message.Payload.Address)).Value;
            Console.WriteLine($"Response value length: {responseValue.Length}");

			Console.WriteLine("Press any key to exit");
			Console.ReadKey();

			bus.Stop();
        }

		static async Task DebugMessage(TestRequest message)
		{
			Console.WriteLine("-----------");

			var rootPayload = await message.Payload.Value.ConfigureAwait(false);
			Console.WriteLine($"Message.Payload.Lengtht={rootPayload.Length}");

			//.Child details
			var childPayload = await message.Child.Payload.Value.ConfigureAwait(false);
			Console.WriteLine($"Message.Child.Payload.Length={childPayload.Length}");
			Console.WriteLine($"Message.Child.Text={message.Child.Text}");

			int index = 0;
			foreach (var item in message.Child.List)
			{
				var pl = await item.Payload.Value.ConfigureAwait(false);
				Console.WriteLine($"Message.Child.List[{index++}] - Number={item.Number}, Text={item.Text}, Enum={item.Enum}, Date={item.Date}, Payload size={pl.Length}");
			}

			//.List details
			index=0;
			foreach (var item in message.List)
			{
				var pl = await item.Payload.Value.ConfigureAwait(false);
				Console.WriteLine($"Message.List[{index++}] - Number={item.Number}, Text={item.Text}, Enum={item.Enum}, Date={item.Date}, Payload size={pl.Length}");
			}

			Console.WriteLine("-----------");
		}
    }
}
