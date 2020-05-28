using MassTransit;
using MessageData.Components.Model;

namespace MessageData.Components.Messages
{
    public interface DoSomething
    {
        MessageData<byte[]> Data1 { get; }

        Foo Foo { get; }

        Bar[] Bars { get; }
    }
}