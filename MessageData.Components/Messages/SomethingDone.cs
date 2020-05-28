using MassTransit;

namespace MessageData.Components.Messages
{
	// ReSharper disable once InconsistentNaming
	public interface SomethingDone
	{
		MessageData<byte[]> File { get; }
		
		DoSomething ComplexData { get; }
	}
}
