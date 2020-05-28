using MassTransit;
using MessageData.Components.Model;

namespace MessageData.Components.Messages
{
    public interface Message2
    {
        MessageData<byte[]> File { get; }
        
        Foo Foo { get; }

        Bar[] Bars { get; }
        
        string TopText { get; }
    }
}