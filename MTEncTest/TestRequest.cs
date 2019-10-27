using MassTransit;

namespace MTEncTest
{
    public interface TestRequest
    {
        MessageData<byte[]> LargePayload { get; }
    }

    public class TestRequestImpl : TestRequest
    {
        public MessageData<byte[]> LargePayload { get; set; }
    }
}
