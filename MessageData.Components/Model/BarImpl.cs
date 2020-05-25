using System;
using MassTransit;

namespace MessageData.Components.Model
{
    public class BarImpl : Bar
    {
        public int Number { get; set; }
        public string Text { get; set; }
        public AnimalType Enum { get; set; }
        public DateTime Date { get; set; }
        public MessageData<byte[]> Data3 { get; set; }
    }
}