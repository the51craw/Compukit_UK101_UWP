using System;
using System.Collections.Generic;
using System.Text;

namespace Compukit_UK101_UWP
{
    public class CMemoryBusDevice
    {
        public Boolean ReadOnly { get; set; }
        public Boolean WriteOnly { get; set; }
        public Boolean Accessible { get; set; }
        public Address Address { get; set; }
        public byte Data { get; set; }
        public Address StartsAt { get; set; }
        public Address EndsAt { get; set; }

        public CMemoryBusDevice()
        {
            ReadOnly = false;
            WriteOnly = false;
            Accessible = true;
            StartsAt = new Address();
            EndsAt = new Address();
            Address = new Address();
        }

        public void SetAddress(Address InAddress)
        {
            if (InAddress.W >= StartsAt.W && InAddress.W <= EndsAt.W)
            {
                Address = new Address(InAddress);
            }
        }
    }
}
