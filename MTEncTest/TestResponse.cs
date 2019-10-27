using MassTransit;

namespace MTEncTest
{
    public class TestResponse
    {
        public MessageData<byte[]> LargePayload { get; set; }
    }
}
