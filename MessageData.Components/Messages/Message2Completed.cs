using MassTransit;

namespace MessageData.Components.Messages
{
    public interface Message2Completed
    {   
        MessageData<byte[]> File { get; }
		
        Message2 DataReceived { get; }
		
        string TopText { get; }
    }
}