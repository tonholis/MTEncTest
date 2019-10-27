using MassTransit;

namespace MTEncTest
{
    public class TestMessage
    {
        public MessageData<byte[]> LargePayload { get; set; }
    }
}
