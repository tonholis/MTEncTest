using MassTransit;

namespace MessageData.Components.Messages
{
	public interface Message1Completed
	{
		MessageData<byte[]> File { get; }
		
		Message1 DataReceived { get; }
		
		string TopText { get; }
	}
}
