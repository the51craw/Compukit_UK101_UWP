using System;
using System.Collections.Generic;
using System.Text;

namespace Compukit_UK101_UWP
{
    public class CMemoryBus
    {
        public const Int32 DEVICES_MAX = 12;
        public byte DeviceIndex;
        public CMemoryBusDevice[] Device = new CMemoryBusDevice[DEVICES_MAX];
        public ushort Address { get; set; }
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
        public NoDevice NoDevice;
        private MemoryMap memoryMap;
        
        private MainPage mainPage;

        public CMemoryBus(MainPage mainPage)
        {
            this.mainPage = mainPage;
            int i;

            memoryMap = new MemoryMap();

            // Clear all device pointers:
            for (i = 0; i < DEVICES_MAX; i++)
            {
                Device[i] = null;
            }

            i = 0;

            Monitor = new MONITOR(Address);
            Monitor.StartsAt = 0xF800;
            Monitor.EndsAt = 0xFFFF;
            Device[0] = Monitor;

            ACIA = new CACIA(mainPage);
            ACIA.StartsAt = 0xF000;
            ACIA.EndsAt = 0xF0FF;
            Device[1] = ACIA;

            Keyboard = new CKeyboard();
            Keyboard.StartsAt = 0xDF00;
            Keyboard.EndsAt = 0xDF00;
            Device[2] = Keyboard;

            VDU = new CVDU();
            VDU.StartsAt = 0xD000;
            VDU.EndsAt = 0xD7FF;
            Device[3] = VDU;

            Basic4 = new BASIC4(Address);
            Basic4.StartsAt = 0xB800;
            Basic4.EndsAt = 0xBFFF;
            Device[4] = Basic4;

            Basic3 = new BASIC3(Address);
            Basic3.StartsAt = 0xB000;
            Basic3.EndsAt = 0xB7FF;
            Device[5] = Basic3;

            Basic2 = new BASIC2(Address);
            Basic2.StartsAt = 0xA800;
            Basic2.EndsAt = 0xAFFF;
            Device[6] = Basic2;

            Basic1 = new BASIC1(Address);
            Basic1.StartsAt = 0xA000;
            Basic1.EndsAt = 0xA7FF;
            Device[7] = Basic1;

            ROM8000 = new ROM8000(Address);
            ROM8000.StartsAt = 0x8000;
            ROM8000.EndsAt = 0x8FFF;
            Device[8] = ROM8000;

            MicToMidi = new MicToMidi(mainPage);
            MicToMidi.StartsAt = 0x6001;
            MicToMidi.EndsAt = 0x6001;
            Device[9] = MicToMidi;

            RAM = new CRAM();
            //RAM.SetRamSize(0x8000);
            RAM.SetRamSize(0x2000);
            Device[10] = RAM;

            NoDevice = new NoDevice();
            Device[11] = NoDevice;
        }

        public void SetAddress(UInt16 Address)
        {
            DeviceIndex = memoryMap.Map[Address];
            Device[DeviceIndex].SetAddress(Address);
        }


        public void Write(byte Data)
        {
            Device[DeviceIndex].Write(Data);
        }

        public byte Read()
        {
            return Device[DeviceIndex].Read();
        }
    }
}
