using MassTransit;

namespace MessageData.Components.Messages
{
	// ReSharper disable once InconsistentNaming
	public interface TestResponse
	{
		public MessageData<byte[]> Payload { get; set; }
		
		public TestRequest OriginalRequest { get; set; }
	}
}
