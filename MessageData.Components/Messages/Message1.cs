using MassTransit;
using MessageData.Components.Model;

namespace MessageData.Components.Messages
{
    public interface Message1
    {
        Foo Foo { get; }

        Bar[] Bars { get; }
        
        string TopText { get; }
    }
}