using MassTransit;

namespace MTEncTest
{
    public interface TestResponse
    {
        MessageData<byte[]> LargePayload { get; }
    }

    public class TestResponseImpl
    {
        public MessageData<byte[]> LargePayload { get; set; }
    }
}
