using System.Collections.Generic;
using MassTransit;

namespace MTEncTest
{
    public class TestRequest
    {
        public MessageData<byte[]> Payload { get; set; }

		public Foo Nested { get;set; }
    }

	public class TestResponse
	{
		public MessageData<byte[]> Payload { get; set; }
	}

	public class Foo
	{
		// public MessageData<byte[]> Payload { get; set; }

		public IList<Bar> List { get; set; }
	}

	public class Bar
	{
		public int Number { get; set; }

		public MessageData<byte[]> Payload { get; set; }
	}
}
