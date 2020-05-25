using MassTransit;

namespace MessageData.Components.Model
{
    public class FooImpl: Foo
    {
        public MessageData<byte[]> Data2 { get; set; }
        public string Text { get; set; }
        public Bar[] Bars { get; set; }
    }
}