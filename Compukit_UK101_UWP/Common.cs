using System;
using System.Collections.Generic;
using System.Text;

namespace Compukit_UK101_UWP
{
    public class Address /*: ICloneable*/
    {
        public UInt16 W { get { return w; } set { w = value; l = (byte)(value % 256); h = (byte)(value / 256); } }
        public byte L { get { return l; } set { l = value; w = (UInt16)((UInt16)(w & (UInt16)0xff00) | (UInt16)value & 0x00ff); } }
        public byte H { get { return h; } set { h = value; w = (UInt16)((UInt16)(w & (UInt16)0x00ff) | (UInt16)((value & 0x00ff) << 8)); } }

        private UInt16 w;
        private byte l;
        private byte h;

        public Address()
        {
            this.w = 0x0000;
            this.l = 0x00;
            this.h = 0x00;
        }

        public Address(Address address)
        {
            this.w = address.w;
            this.l = address.l;
            this.h = address.h;
        }

        public static Address operator ++(Address address)
        {
            Address a = new Address(address);
            a.w = address.w;
            a.l = address.l;
            a.h = address.h;

            //if (a.h == 0)
            //{
            //    if (a.l == 0xff)
            //    {
            //        // a.h++; This is removed to emulate the zero page 'bug'.
            //        a.l = 0x00;
            //    }
            //    else
            //    {
            //        a.l++;
            //    }
            //}
            //else
            {
                if (a.l == 0xff)
                {
                    a.h++;
                    a.l = 0x00;
                }
                else
                {
                    a.l++;
                }
            }
            a.w = (UInt16)(a.h * 256 + a.l);
            return a;
        }

        public static Address operator +(Address address1, Address address2)
        {
            Address a = new Address();
            a.w = (UInt16)(address1.w + address2.w);
            a.l = (byte)(a.w % 256);
            a.h = (byte)(a.w / 256);
            return a;
        }

        public static Address operator -(Address address1, Address address2)
        {
            Address a = new Address();
            a.w = (UInt16)(address1.w - address2.w);
            a.l = (byte)(a.w % 256);
            a.h = (byte)(a.w / 256);
            return a;
        }

        //public object Clone()
        //{
        //    Address address = new Address();
        //    address.w = w;
        //    address.l = l;
        //    address.h = h;
        //    return address;
        //}
    }

    class Common
    {
        //union _Address
        //{
        //    WORD W;

        //struct _B
        //{
        //    UCHAR L;
        //    UCHAR H;
        //}
        //B;
    };

//#define uAddress union _Address
}

