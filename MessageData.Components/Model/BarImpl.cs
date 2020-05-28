using System;
using MassTransit;

namespace MessageData.Components.Model
{
    public class BarImpl : Bar
    {
        public int WhateverNumber { get; set; }
        public string WhateverText { get; set; }
        public AnimalType WhateverEnum { get; set; }
        public DateTime WhateverDate { get; set; }
        public MessageData<byte[]> Data3 { get; set; }
    }
}