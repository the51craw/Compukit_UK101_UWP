using System;
using System.Collections.Generic;
using System.Text;

namespace Compukit_UK101_UWP
{
    public class CROM : CMemoryBusDevice
    {
        public UInt16 ROMSize { get; set; }
        public Object pData { get; set; }

        public byte Read()
        {
            if (pData.GetType() == typeof(BASIC1))
            {
                return 0xff;// ((BASIC1)pData[Address.W - StartsAt.W];
                //return ((BASIC1)pData).bytes[Address.W - StartsAt.W];
            }
            else if (pData.GetType() == typeof(BASIC2))
            {
                return ((BASIC2)pData).bytes[Address.W - StartsAt.W];
            }
            else if (pData.GetType() == typeof(BASIC3))
            {
                return ((BASIC3)pData).bytes[Address.W - StartsAt.W];
            }
            else if (pData.GetType() == typeof(BASIC4))
            {
                return ((BASIC4)pData).bytes[Address.W - StartsAt.W];
            }
            else if (pData.GetType() == typeof(MONITOR))
            {
                return ((MONITOR)pData).bytes[Address.W - StartsAt.W];
            }
            //else if (pData.GetType() == typeof())
            //{
            //    return (()pData).bytes[Address.W - StartsAt.W];
            //}
            return 0xff;
        }

        public bool Write(byte InData)
        {
            return false;
        }
    }
}
