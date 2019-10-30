using System;
using System.Collections.Generic;
using MassTransit;

namespace MTEncTest
{
    public class TestRequest
    {
        public MessageData<byte[]> Payload { get; set; }

		public Foo Child { get;set; }

		public IList<Bar> List { get; set; }
    }

	public class Foo
	{
		public MessageData<byte[]> Payload { get; set; }
		
		public string Text { get; set; }

		public IList<Bar> List { get; set; }
	}

	public class Bar
	{
		public int Number { get; set; }
		public string Text { get; set; }
		
		public AnimalType Enum { get; set; }

		public DateTime Date { get; set; }

		public MessageData<byte[]> Payload { get; set; }
	}

	public enum AnimalType
	{
		Unknown,
		Dog,
		Cat,
		Bird
	}

	public class TestResponse
	{
		public MessageData<byte[]> Payload { get; set; }
	}
}
