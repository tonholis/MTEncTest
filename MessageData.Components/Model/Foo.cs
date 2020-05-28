using MassTransit;

namespace MessageData.Components.Model
{
    public interface Foo
    {
        MessageData<byte[]> Data2 { get; }
		
        string WhateverText { get; }

        Bar[] Bars { get; }
    }
}