using MassTransit;

namespace MessageData.Components.Model
{
    public interface Foo
    {
        MessageData<byte[]> File { get; }
		
        string SomeText { get; }

        Bar[] Bars { get; }
    }
}