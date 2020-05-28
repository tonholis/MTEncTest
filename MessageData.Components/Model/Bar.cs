using System;
using MassTransit;

namespace MessageData.Components.Model
{
    public interface Bar
    {
        int WhateverNumber { get; }
        string WhateverText { get; }
		
        AnimalType WhateverEnum { get; }

        DateTime WhateverDate { get; }

        MessageData<byte[]> Data3 { get; }
    }
}