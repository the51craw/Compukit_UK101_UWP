using System;
using System.Collections.Generic;
using System.Text;

namespace Compukit_UK101_UWP
{
    public class CMemoryBus
    {
        public const Int32 DEVICES_MAX = 11;
        public byte DeviceIndex;
        public CMemoryBusDevice[] Device = new CMemoryBusDevice[DEVICES_MAX];
        public Address Address { get; set; }
        public MONITOR Monitor;
        public BASIC1 Basic1;
        public BASIC2 Basic2;
        public BASIC3 Basic3;
        public BASIC4 Basic4;
        public CRAM RAM;
        public CVDU VDU;
        public CKeyboard Keyboard;
        public CACIA ACIA;
        public ROM8000 ROM8000;
        public MicToMidi MicToMidi;

        private MainPage mainPage;

        public CMemoryBus(MainPage mainPage)
        {
            this.mainPage = mainPage;
            int i;

            // Clear all device pointers:
            for (i = 0; i < DEVICES_MAX; i++)
            {
                Device[i] = null;
            }

            i = 0;

            Monitor = new MONITOR(Address);
            Monitor.StartsAt.W = 0xF800;
            Monitor.EndsAt.W = 0xFFFF;
            Device[0] = Monitor;

            ACIA = new CACIA(mainPage);
            ACIA.StartsAt.W = 0xF000;
            ACIA.EndsAt.W = 0xF0FF;
            Device[1] = ACIA;

            Keyboard = new CKeyboard();
            Keyboard.StartsAt.W = 0xDF00;
            Keyboard.EndsAt.W = 0xDF00;
            Device[2] = Keyboard;

            VDU = new CVDU();
            VDU.StartsAt.W = 0xD000;
            VDU.EndsAt.W = 0xD7FF;
            Device[3] = VDU;

            Basic4 = new BASIC4(Address);
            Basic4.StartsAt.W = 0xB800;
            Basic4.EndsAt.W = 0xBFFF;
            Device[4] = Basic4;

            Basic3 = new BASIC3(Address);
            Basic3.StartsAt.W = 0xB000;
            Basic3.EndsAt.W = 0xB7FF;
            Device[5] = Basic3;

            Basic2 = new BASIC2(Address);
            Basic2.StartsAt.W = 0xA800;
            Basic2.EndsAt.W = 0xAFFF;
            Device[6] = Basic2;

            Basic1 = new BASIC1(Address);
            Basic1.StartsAt.W = 0xA000;
            Basic1.EndsAt.W = 0xA7FF;
            Device[7] = Basic1;

            ROM8000 = new ROM8000(Address);
            ROM8000.StartsAt.W = 0x8000;
            ROM8000.EndsAt.W = 0x8FFF;
            Device[8] = ROM8000;

            MicToMidi = new MicToMidi(mainPage);
            MicToMidi.StartsAt.W = 0x6001;
            MicToMidi.EndsAt.W = 0x6001;
            Device[9] = MicToMidi;

            RAM = new CRAM();
            //RAM.SetRamSize(0x8000);
            RAM.SetRamSize(0x2000);
            Device[10] = RAM;
        }

        public void SetAddress(Address address)
        {
            Address = new Address(address);
            DeviceIndex = AddressToDeviceindex(Address);
            if (DeviceIndex < 11)
            {
                Device[DeviceIndex].SetAddress(Address);
            }
        }

        private byte AddressToDeviceindex(Address address)
        {
            if (Address.W >= 0xf800)
            {
                return 0;
            }
            else if (Address.W >= 0xf000)
            {
                return 1;
            }
            else if (Address.W >= 0xdf00)
            {
                return 2;
            }
            else if (Address.W >= 0xd000)
            {
                return 3;
            }
            else if (Address.W >= 0xb800)
            {
                return 4;
            }
            else if (Address.W >= 0xb000)
            {
                return 5;
            }
            else if (Address.W >= 0xa800)
            {
                return 6;
            }
            else if (Address.W >= 0xa000)
            {
                return 7;
            }
            else if (Address.W >= 0x8000)
            {
                return 8;
            }
            else if (Address.W >= 0x6001)
            {
                return 9;
            }
            else if (Address.W <= RAM.EndsAt.W)
            {
                return 10;
            }
            else
            {
                return 11;
            }
        }

        public void Write(byte Data)
        {
            switch (DeviceIndex)
            {
                case 0:
                    Monitor.Write(Data);
                    break;
                case 1:
                    ACIA.Write(Data);
                    break;
                case 2:
                    Keyboard.Write(Data);
                    break;
                case 3:
                    VDU.Write(Data);
                    break;
                case 4:
                    Basic4.Write(Data);
                    break;
                case 5:
                    Basic3.Write(Data);
                    RAM.Write(Data);
                    break;
                case 6:
                    Basic2.Write(Data);
                    break;
                case 7:
                    Basic1.Write(Data);
                    break;
                case 8:
                    ROM8000.Write(Data);
                    break;
                case 9:
                    MicToMidi.Write(Data);
                    break;
                case 10:
                    RAM.Write(Data);
                    break;
            }
        }

        public byte Read()
        {
            // Unavailable address spaces return H as data in the real hardware:
            byte OutData = (byte)Address.H;

            switch (DeviceIndex)
            {
                case 0:
                    //OutData = Monitor.pData[Address.W - Monitor.StartsAt.W];
                    OutData = Monitor.Read();
                    break;
                case 1:
                    OutData = ACIA.Read();
                    break;
                case 2:
                    OutData = Keyboard.Read();
                    break;
                case 3:
                    OutData = VDU.Read();
                    break;
                case 4:
                    OutData = Basic4.Read();
                    break;
                case 5:
                    OutData = Basic3.Read();
                    break;
                case 6:
                    OutData = Basic2.Read();
                    break;
                case 7:
                    OutData = Basic1.Read();
                    break;
                case 8:
                    OutData = ROM8000.Read();
                    break;
                case 9:
                    OutData = MicToMidi.Read();
                    break;
                case 10:
                    OutData = RAM.Read();
                    break;
            }

            return OutData;
        }
    }
}
