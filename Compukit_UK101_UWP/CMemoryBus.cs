using System;
using System.Collections.Generic;
using System.Text;

namespace Compukit_UK101_UWP
{
    public class CMemoryBus
    {
        public const Int32 DEVICES_MAX = 9;
        public byte DeviceIndex;
        public CMemoryBusDevice[] Device = new CMemoryBusDevice[DEVICES_MAX];
        public Address Address { get; set; }
        int selectedDevice;
        public MONITOR Monitor;
        public BASIC1 Basic1;
        public BASIC2 Basic2;
        public BASIC3 Basic3;
        public BASIC4 Basic4;
        public CRAM RAM;
        //public CCHARGEN CharGen;
        public CVDU VDU;
        public CKeyboard Keyboard;
        public CACIA ACIA;
        public ROM8000 ROM8000;

        public CMemoryBus()
        {
            int i;

            //Address = new Address();

            // Clear all device pointers:
            for (i = 0; i < DEVICES_MAX; i++)
            {
                Device[i] = null;
            }

            i = 0;

            // Read the system ROMs:
            Basic1 = new BASIC1(Address);
            Basic1.StartsAt.W = 0xA000;
            Basic1.EndsAt.W = 0xA7FF;
            Device[i++] = Basic1;

            Basic2 = new BASIC2(Address);
            Basic2.StartsAt.W = 0xA800;
            Basic2.EndsAt.W = 0xAFFF;
            Device[i++] = Basic2;

            Basic3 = new BASIC3(Address);
            Basic3.StartsAt.W = 0xB000;
            Basic3.EndsAt.W = 0xB7FF;
            Device[i++] = Basic3;

            Basic4 = new BASIC4(Address);
            Basic4.StartsAt.W = 0xB800;
            Basic4.EndsAt.W = 0xBFFF;
            Device[i++] = Basic4;

            Monitor = new MONITOR(Address);
            Monitor.StartsAt.W = 0xF800;
            Monitor.EndsAt.W = 0xFFFF;
            Device[i++] = Monitor;

            //CharGen.LoadRom("SysRoms\\CharGen.rom");
            //CharGen.StartsAt.W = 0x0000;
            //CharGen.EndsAt.W = 0x0000;
            //CharGen.Accessible = false;
            //Device[i++] = &CharGen;

            // Setup RAM:
            //	RAM.SetRamSize(0x7FFF);
            RAM = new CRAM();
            //RAM.SetRamSize(0x8000);
            RAM.SetRamSize(0x2000);
            Device[i++] = RAM;

            // Setup keyboard:
            //	Keyboard.StartsAt.W = 0xDC00;
            //	Keyboard.EndsAt.W = 0xDFFF;
            Keyboard = new CKeyboard();
            Keyboard.StartsAt.W = 0xDF00;
            Keyboard.EndsAt.W = 0xDF00;
            Device[i++] = Keyboard;

            // Setup VDU:
            VDU = new CVDU();
            VDU.StartsAt.W = 0xD000;
            VDU.EndsAt.W = 0xD7FF;
            //VDU.pCharData = CharGen.pData;
            Device[i++] = VDU;

            // Setup ACIA:
            //ACIA.StartsAt.W = 0xF000;
            ////	ACIA.EndsAt.W = 0xF0FF;
            //ACIA.EndsAt.W = 0xF001;
            //Device[i++] = &ACIA;

            // Look for ROMs in folder ROMs:
            ROM8000 = new ROM8000(Address);
            ROM8000.StartsAt.W = 0x8000;
            ROM8000.EndsAt.W = 0x8FFF;
            Device[i++] = ROM8000;
        }

        public void SetAddress(Address address)
        {
            Address = new Address(address);
            DeviceIndex = AddressToDeviceindex(Address);
            Device[DeviceIndex].SetAddress(Address);
            //for (Int32 i = 0; i < DEVICES_MAX; i++)
            //{
            //    Device[i].SetAddress(Address);
            //}
        }

        private byte AddressToDeviceindex(Address address)
        {
            if (Address.W >= 0xf800)
            {
                return 4;
            }
            else if (Address.W >= 0xdf00)
            {
                return 6;
            }
            else if (Address.W >= 0xd000)
            {
                return 7;
            }
            else if (Address.W >= 0xb800)
            {
                return 3;
            }
            else if (Address.W >= 0xb000)
            {
                return 2;
            }
            else if (Address.W >= 0xa800)
            {
                return 1;
            }
            else if (Address.W >= 0xa000)
            {
                return 0;
            }
            else if (Address.W >= 0x8000)
            {
                return 8;
            }
            else
            {
                return 5;
            }
        }

        public void Write(byte Data)
        {
            //for (Int32 i = 0; i < DEVICES_MAX; i++)
            //{
            //    if (Device[i].Selected)
            //    {
                    switch (DeviceIndex)
                    {
                        case 0:
                            Basic1.Write(Data);
                            break;
                        case 1:
                            Basic2.Write(Data);
                            break;
                        case 2:
                            Basic3.Write(Data);
                            break;
                        case 3:
                            Basic4.Write(Data);
                            break;
                        case 4:
                            Monitor.Write(Data);
                            break;
                        case 5:
                            RAM.Write(Data);
                            break;
                        case 6:
                            Keyboard.Write(Data);
                            break;
                        case 7:
                            VDU.Write(Data);
                            break;
                        case 8:
                            ROM8000.Write(Data);
                            break;
                    }
            //    }
            //}
        }

        public byte Read()
        {
            // Unavailable address spaces return H as data in the real hardware:
            byte OutData = (byte)Address.H;

            //for (Int32 i = 0; i < DEVICES_MAX; i++)
            //{
            //    if (Device[i].Selected)
            //    {
                    switch (DeviceIndex)
                    {
                        case 0:
                            OutData = Basic1.Read();
                            break;
                        case 1:
                            OutData = Basic2.Read();
                            break;
                        case 2:
                            OutData = Basic3.Read();
                            break;
                        case 3:
                            OutData = Basic4.Read();
                            break;
                        case 4:
                            //OutData = Monitor.pData[Address.W - Monitor.StartsAt.W];
                            OutData = Monitor.Read();
                            break;
                        case 5:
                            OutData = RAM.Read();
                            break;
                        case 6:
                            OutData = Keyboard.Read();
                            break;
                        case 7:
                            OutData = VDU.Read();
                            break;
                        case 8:
                            OutData = ROM8000.Read();
                            break;
                    }
            //    }
            //}

            return OutData;
        }
    }
}
