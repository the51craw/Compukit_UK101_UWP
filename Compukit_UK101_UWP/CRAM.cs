using System;
using System.Collections.Generic;
using System.Text;

namespace Compukit_UK101_UWP
{
    public class CRAM : CMemoryBusDevice
    {
        public UInt16 RAMSize { get; set; }

        public byte[] pData;

        public CRAM()
        {
            pData = null;
            ReadOnly = false;
        }

        public Boolean SetRamSize(UInt16 newSize)
        {
            bool result = false;

            // Allow max 32 kb RAM:
            if (newSize > 0 && newSize <= 0x8000)
            {
                RAMSize = newSize;
                StartsAt.W = 0x0000;
                EndsAt.W = (UInt16)(newSize - 1);
                pData = new byte[newSize];
                result = true;
            }

            return result;
        }

        public byte Read()
        {
            return pData[Address.W];
        }

        public void Write(byte InData)
        {
            pData[Address.W] = InData;
        }
    }
}
