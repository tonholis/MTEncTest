using System;
using MassTransit;

namespace MessageData.Components.Model
{
    public interface Bar
    {
        int SomeNumber { get; }
        string SomeText { get; }
		
        AnimalType SomeEnum { get; }

        DateTime SomeDatetime { get; }

        MessageData<byte[]> File { get; }
    }
}