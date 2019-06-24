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
        public ushort Address { get; set; }
        public byte Data { get; set; }
        public ushort StartsAt { get; set; }
        public ushort EndsAt { get; set; }

        public CMemoryBusDevice()
        {
            ReadOnly = false;
            WriteOnly = false;
            Accessible = true;
            //StartsAt = new Address();
            //EndsAt = new Address();
            //Address = new Address();
        }

        public void SetAddress(UInt16 InAddress)
        {
            if (InAddress >= StartsAt && InAddress <= EndsAt)
            {
                Address = InAddress;
            }
        }

        public virtual void Write(byte Data) { }
        public virtual byte Read() { return 0x00; }
    }
}
