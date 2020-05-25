using System;
using MassTransit;

namespace MessageData.Components.Model
{
    public interface Bar
    {
        int Number { get; }
        string Text { get; }
		
        AnimalType Enum { get; }

        DateTime Date { get; }

        MessageData<byte[]> Data3 { get; }
    }
}