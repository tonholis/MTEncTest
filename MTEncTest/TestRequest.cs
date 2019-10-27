using MassTransit;

namespace MTEncTest
{
    public class TestRequest
    {
        public MessageData<byte[]> LargePayload { get; set; }
    }
}
