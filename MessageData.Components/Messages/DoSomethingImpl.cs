using MassTransit;
using MessageData.Components.Model;

namespace MessageData.Components.Messages
{
    public class DoSomethingImpl : DoSomething
    {
        public MessageData<byte[]> Data1 { get; set; }
        public Foo Foo { get; set; }
        public Bar[] Bars { get; set; }
    }
}