using MassTransit;
using MessageData.Components.Model;

namespace MessageData.Components.Messages
{
    public interface TestRequest
    {
        MessageData<byte[]> Data1 { get; }

        Foo Foo { get; }

        Bar[] Bars { get; }
    }
}