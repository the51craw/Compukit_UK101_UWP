using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Compukit_UK101_UWP
{
    // Signetic6502.cpp

    // The processor is one of two objects created by the UK101.
    // The other one is the Clock.
    // The processor creates the memory bus object, and the memory-
    // bus object creates the devices connected to them.

    public class CSignetic6502
    {
        private Boolean DebugEnabled = true;
        Address PC_Debug = new Address();
        Int32 AddressMode_Debug = 0;
        String hexChars = "0123456789abcdef";
        String DebugString = "";
        byte[] debugBytes = null;

        // Processor status register AND masks.
        // if(mask & P) flag set else flag reset.
        // To set a flag: P = P | mask
        // To reset a flag: P = P & ~mask
        public byte FLAG_6502_CARRY = 0x01;
        public byte FLAG_6502_ZERO = 0x02;
        public byte FLAG_6502_IRQ_DISABLE = 0x04;
        public byte FLAG_6502_DECIMAL_MODE = 0x08;
        public byte FLAG_6502_BRK_COMMAND = 0x10;
        public byte FLAG_6502_OVERFLOW = 0x40;
        public byte FLAG_6502_NEGATIVE = 0x80;

        // Addressing modes defined:
        public const UInt16 ADDRESS_MODE_6502_IMPLIED = 0x0001;
        public const UInt16 ADDRESS_MODE_6502_ACCUMULATOR = 0x0002;
        public const UInt16 ADDRESS_MODE_6502_ABSOLUTE = 0x0004;
        public const UInt16 ADDRESS_MODE_6502_ZERO_PAGE = 0x0008;
        public const UInt16 ADDRESS_MODE_6502_IMMEDIATE = 0x0010;
        public const UInt16 ADDRESS_MODE_6502_ABS_X = 0x0020;
        public const UInt16 ADDRESS_MODE_6502_ABS_Y = 0x0040;
        public const UInt16 ADDRESS_MODE_6502_IND_X = 0x0080;
        public const UInt16 ADDRESS_MODE_6502_IND_Y = 0x0100;
        public const UInt16 ADDRESS_MODE_6502_Z_PAGE_X = 0x0200;
        public const UInt16 ADDRESS_MODE_6502_RELATIVE = 0x0400;
        public const UInt16 ADDRESS_MODE_6502_INDIRECT = 0x0800;
        public const UInt16 ADDRESS_MODE_6502_Z_PAGE_Y = 0x1000;

        // Registers:
        Address PC;    // Program counter
        byte A;        // Accumulator
        byte X;        // X index
        byte Y;        // Y index
        byte S;        // Stack pointer (Always page 1!)
        byte P;        // Processor status register
        byte siTest;
        byte ucTest;

        public CMemoryBus MemoryBus;
        Address AddressInEffect;
        bool LogFlag;
        bool LogEnter;

        private MainPage mainPage;

        public CSignetic6502(MainPage mainPage)
        {
            this.mainPage = mainPage;
            AddressInEffect = new Address();
            MemoryBus = new CMemoryBus(mainPage);
            LogFlag = false;
            LogEnter = false;
            PC = new Address();
        }

        private void DebugLine(String PC, String OpCode, String AddressMode, byte[] Bytes, String AddressInEffect)
        {
            //if (DebugEnabled) Debug.WriteLine(PC + " " + OpCode + " " + AddressMode + " " + AddressInEffect);
        }

        public void Reset()
        {
            // The reset vector is located at 0xFFFC - 0xFFFD.
            // Load into PC:
            Address adr = new Address();
            Address AddressInEffect = new Address();
            adr.W = 0xFFFC;
            MemoryBus.SetAddress(adr);
            PC.L = MemoryBus.Read();
            adr.W = 0xFFFD;
            MemoryBus.SetAddress(adr);
            PC.H = MemoryBus.Read();

            // Reset the keyboard:
            MemoryBus.Keyboard.Reset();

            // Initiate stack pointer:
            //S = 0xFF;
            S = 0x40; // Stack is 0x0100 - 0x0140. Allocation starts at top. H is always 0x01.

            // Initiate processor status:
            P = 0x00;

            X = 0x00;
            Y = 0x00;
        }

        public void Dissassemble()
        {
            byte addressBytes;
            for (UInt16 i = 0; i < MemoryBus.ROM8000.pData.Length; i++)
            {
                PC = new Address();
                PC.W = (UInt16)(0x8000 + i);

                SingleStep();
                addressBytes = 0;
                switch (AddressMode_Debug)
                {
                    case ADDRESS_MODE_6502_ABSOLUTE:
                        addressBytes = 2;
                        break;
                    case ADDRESS_MODE_6502_ZERO_PAGE:
                        addressBytes = 1;
                        break;
                    case ADDRESS_MODE_6502_IMMEDIATE:
                        addressBytes = 1;
                        break;
                    case ADDRESS_MODE_6502_ABS_X:
                        addressBytes = 2;
                        break;
                    case ADDRESS_MODE_6502_ABS_Y:
                        addressBytes = 2;
                        break;
                    case ADDRESS_MODE_6502_IND_X:
                        addressBytes = 1;
                        break;
                    case ADDRESS_MODE_6502_IND_Y:
                        addressBytes = 1;
                        break;
                    case ADDRESS_MODE_6502_Z_PAGE_X:
                        addressBytes = 1;
                        break;
                    case ADDRESS_MODE_6502_RELATIVE:
                        addressBytes = 1;
                        break;
                    case ADDRESS_MODE_6502_INDIRECT:
                        addressBytes = 2;
                        break;
                    case ADDRESS_MODE_6502_Z_PAGE_Y:
                        addressBytes = 1;
                        break;
                    case ADDRESS_MODE_6502_IMPLIED:
                        addressBytes = 0;
                        break;
                }

                String addr = "";
                for (byte ab = 0; ab < addressBytes; ab++)
                {
                    addr += "" + hexChars[MemoryBus.ROM8000.pData[i + ab + 1] / 16] + hexChars[MemoryBus.ROM8000.pData[i + ab + 1] % 16] + "\t";
                }
                i += addressBytes;
                Debug.WriteLine(DebugString + " " + addr);
            }
        }

        public int SingleStep()
        {
            int ClockCycles = 0;
            byte OpCode;

            //if (DebugEnabled){PC_Debug = new Address(PC);debugBytes = null;}

            // Fetch next instruction:
            MemoryBus.SetAddress(PC);
            OpCode = MemoryBus.Read();

            // Decode and branch to handler:
            // Add the clock cycles to the trip meter:
            switch (OpCode)
            {
                // ADC
                case 0x6D:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tADC ABSOLUTE";
                    ADC(ADDRESS_MODE_6502_ABSOLUTE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABSOLUTE;
                    ClockCycles = 4;
                    break;

                case 0x65:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tADC ZERO_PAGE";
                    ADC(ADDRESS_MODE_6502_ZERO_PAGE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ZERO_PAGE;
                    ClockCycles = 3;
                    break;

                case 0x69:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tADC IMMEDIATE";
                    ADC(ADDRESS_MODE_6502_IMMEDIATE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IMMEDIATE;
                    ClockCycles = 2;
                    break;

                case 0x7D:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tADC ABS_X";
                    ADC(ADDRESS_MODE_6502_ABS_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABS_X;
                    ClockCycles = 4;
                    break;

                case 0x79:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tADC ABS_Y";
                    ADC(ADDRESS_MODE_6502_ABS_Y);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABS_Y;
                    ClockCycles = 4;
                    break;

                case 0x61:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tADC IND_X";
                    ADC(ADDRESS_MODE_6502_IND_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IND_X;
                    ClockCycles = 6;
                    break;

                case 0x71:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tADC IND_Y";
                    ADC(ADDRESS_MODE_6502_IND_Y);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IND_Y;
                    ClockCycles = 5;
                    break;

                case 0x75:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tADC(MODE_6502_Z_PAGE_X";
                    ADC(ADDRESS_MODE_6502_Z_PAGE_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_Z_PAGE_X;
                    ClockCycles = 4;
                    break;

                // AND
                case 0x2D:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tAND ABSOLUTE";
                    AND(ADDRESS_MODE_6502_ABSOLUTE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABSOLUTE;
                    ClockCycles = 4;
                    break;

                case 0x25:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tAND ZERO_PAGE";
                    AND(ADDRESS_MODE_6502_ZERO_PAGE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ZERO_PAGE;
                    ClockCycles = 3;
                    break;

                case 0x29:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tAND IMMEDIATE";
                    AND(ADDRESS_MODE_6502_IMMEDIATE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IMMEDIATE;
                    ClockCycles = 2;
                    break;

                case 0x3D:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tAND ABS_X";
                    AND(ADDRESS_MODE_6502_ABS_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABS_X;
                    ClockCycles = 4;
                    break;

                case 0x39:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tAND ABS_Y";
                    AND(ADDRESS_MODE_6502_ABS_Y);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABS_Y;
                    ClockCycles = 4;
                    break;

                case 0x21:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tAND IND_X";
                    AND(ADDRESS_MODE_6502_IND_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IND_X;
                    ClockCycles = 6;
                    break;

                case 0x31:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tAND IND_Y";
                    AND(ADDRESS_MODE_6502_IND_Y);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IND_Y;
                    ClockCycles = 5;
                    break;

                case 0x35:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tAND Z_PAGE_X";
                    AND(ADDRESS_MODE_6502_Z_PAGE_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_Z_PAGE_X;
                    ClockCycles = 4;
                    break;

                // ASL
                case 0x0A:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tASL ACCUMULATOR";
                    ASL(ADDRESS_MODE_6502_ACCUMULATOR);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ACCUMULATOR;
                    ClockCycles = 2;
                    break;

                case 0x0E:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tASL ABSOLUTE";
                    ASL(ADDRESS_MODE_6502_ABSOLUTE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABSOLUTE;
                    ClockCycles = 6;
                    break;

                case 0x06:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tASL ZERO_PAGE";
                    ASL(ADDRESS_MODE_6502_ZERO_PAGE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ZERO_PAGE;
                    ClockCycles = 5;
                    break;

                case 0x1E:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tASL ABS_X";
                    ASL(ADDRESS_MODE_6502_ABS_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABS_X;
                    ClockCycles = 7;
                    break;

                case 0x16:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tASL Z_PAGE_X";
                    ASL(ADDRESS_MODE_6502_Z_PAGE_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_Z_PAGE_X;
                    ClockCycles = 6;
                    break;

                // BCC
                case 0x90:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tBCC RELATIVE";
                    BCC(ADDRESS_MODE_6502_RELATIVE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_RELATIVE;
                    ClockCycles = 2;
                    break;

                // BCS
                case 0xB0:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tBCS RELATIVE";
                    BCS(ADDRESS_MODE_6502_RELATIVE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_RELATIVE;
                    ClockCycles = 2;
                    break;

                // BEQ
                case 0xF0:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tBEQ RELATIVE";
                    BEQ(ADDRESS_MODE_6502_RELATIVE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_RELATIVE;
                    ClockCycles = 2;
                    break;

                // BIT
                case 0x2C:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tBIT ABSOLUTE";
                    BIT(ADDRESS_MODE_6502_ABSOLUTE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABSOLUTE;
                    ClockCycles = 4;
                    break;

                case 0x24:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tBIT ZERO_PAGE";
                    BIT(ADDRESS_MODE_6502_ZERO_PAGE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ZERO_PAGE;
                    ClockCycles = 3;
                    break;

                // BMI
                case 0x30:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tBMI RELATIVE";
                    BMI(ADDRESS_MODE_6502_RELATIVE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_RELATIVE;
                    ClockCycles = 2;
                    break;

                // BNE
                case 0xD0:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tBNE RELATIVE";
                    BNE(ADDRESS_MODE_6502_RELATIVE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_RELATIVE;
                    ClockCycles = 2;
                    break;

                // BPL
                case 0x10:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tBPL RELATIVE";
                    BPL(ADDRESS_MODE_6502_RELATIVE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_RELATIVE;
                    ClockCycles = 2;
                    break;

                // BRK
                case 0x00:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tBRK IMPLIED";
                    BRK(ADDRESS_MODE_6502_IMPLIED);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IMPLIED;
                    ClockCycles = 7;
                    break;

                // BVC
                case 0x50:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tBVC RELATIVE";
                    BVC(ADDRESS_MODE_6502_RELATIVE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_RELATIVE;
                    ClockCycles = 2;
                    break;

                // BVS
                case 0x70:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tBVS RELATIVE";
                    BVS(ADDRESS_MODE_6502_RELATIVE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_RELATIVE;
                    ClockCycles = 2;
                    break;

                // CLC
                case 0x18:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tCLC IMPLIED";
                    CLC(ADDRESS_MODE_6502_IMPLIED);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IMPLIED;
                    ClockCycles = 2;
                    break;

                // CLD
                case 0xD8:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tCLD IMPLIED";
                    CLD(ADDRESS_MODE_6502_IMPLIED);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IMPLIED;
                    ClockCycles = 2;
                    break;

                // CLI
                case 0x58:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tCLI IMPLIED";
                    CLI(ADDRESS_MODE_6502_IMPLIED);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IMPLIED;
                    ClockCycles = 2;
                    break;

                // CLV
                case 0xB8:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tCLV IMPLIED";
                    CLV(ADDRESS_MODE_6502_IMPLIED);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IMPLIED;
                    ClockCycles = 2;
                    break;

                // CMP
                case 0xCD:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tCMP ABSOLUTE";
                    CMP(ADDRESS_MODE_6502_ABSOLUTE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABSOLUTE;
                    ClockCycles = 4;
                    break;

                case 0xC5:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tCMP ZERO_PAGE";
                    CMP(ADDRESS_MODE_6502_ZERO_PAGE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ZERO_PAGE;
                    ClockCycles = 3;
                    break;

                case 0xC9:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tCMP IMMEDIATE";
                    CMP(ADDRESS_MODE_6502_IMMEDIATE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IMMEDIATE;
                    ClockCycles = 2;
                    break;

                case 0xDD:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tCMP ABS_X";
                    CMP(ADDRESS_MODE_6502_ABS_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABS_X;
                    ClockCycles = 4;
                    break;

                case 0xD9:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tCMP ABS_Y";
                    CMP(ADDRESS_MODE_6502_ABS_Y);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABS_Y;
                    ClockCycles = 4;
                    break;

                case 0xC1:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tCMP IND_X";
                    CMP(ADDRESS_MODE_6502_IND_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IND_X;
                    ClockCycles = 6;
                    break;

                case 0xD1:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tCMP IND_Y";
                    CMP(ADDRESS_MODE_6502_IND_Y);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IND_Y;
                    ClockCycles = 5;
                    break;

                case 0xD5:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tCMP Z_PAGE_X";
                    CMP(ADDRESS_MODE_6502_Z_PAGE_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_Z_PAGE_X;
                    ClockCycles = 4;
                    break;

                // CPX
                case 0xEC:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tCPX ABSOLUTE";
                    CPX(ADDRESS_MODE_6502_ABSOLUTE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABSOLUTE;
                    ClockCycles = 4;
                    break;

                case 0xE4:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tCPX ZERO_PAGE";
                    CPX(ADDRESS_MODE_6502_ZERO_PAGE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ZERO_PAGE;
                    ClockCycles = 3;
                    break;

                case 0xE0:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tCPX IMMEDIATE";
                    CPX(ADDRESS_MODE_6502_IMMEDIATE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IMMEDIATE;
                    ClockCycles = 2;
                    break;

                // CPY
                case 0xCC:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tCPY ABSOLUTE";
                    CPY(ADDRESS_MODE_6502_ABSOLUTE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABSOLUTE;
                    ClockCycles = 4;
                    break;

                case 0xC4:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tCPY ZERO_PAGE";
                    CPY(ADDRESS_MODE_6502_ZERO_PAGE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ZERO_PAGE;
                    ClockCycles = 3;
                    break;

                case 0xC0:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tCPY IMMEDIATE";
                    CPY(ADDRESS_MODE_6502_IMMEDIATE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IMMEDIATE;
                    ClockCycles = 2;
                    break;

                // DEC
                case 0xCE:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tDEC ABSOLUTE";
                    DEC(ADDRESS_MODE_6502_ABSOLUTE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABSOLUTE;
                    ClockCycles = 6;
                    break;

                case 0xC6:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tDEC ZERO_PAGE";
                    DEC(ADDRESS_MODE_6502_ZERO_PAGE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ZERO_PAGE;
                    ClockCycles = 5;
                    break;

                case 0xDE:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tDEC ABS_X";
                    DEC(ADDRESS_MODE_6502_ABS_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABS_X;
                    ClockCycles = 7;
                    break;

                case 0xD6:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tDEC Z_PAGE_X";
                    DEC(ADDRESS_MODE_6502_Z_PAGE_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_Z_PAGE_X;
                    ClockCycles = 6;
                    break;

                // DEX
                case 0xCA:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tDEX IMPLIED";
                    DEX(ADDRESS_MODE_6502_IMPLIED);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IMPLIED;
                    ClockCycles = 2;
                    break;

                // DEY
                case 0x88:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tDEY IMPLIED";
                    DEY(ADDRESS_MODE_6502_IMPLIED);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IMPLIED;
                    ClockCycles = 2;
                    break;

                // EOR
                case 0x4D:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tEOR ABSOLUTE";
                    EOR(ADDRESS_MODE_6502_ABSOLUTE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABSOLUTE;
                    ClockCycles = 4;
                    break;

                case 0x45:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tEOR ZERO_PAGE";
                    EOR(ADDRESS_MODE_6502_ZERO_PAGE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ZERO_PAGE;
                    ClockCycles = 3;
                    break;

                case 0x49:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tEOR IMMEDIATE";
                    EOR(ADDRESS_MODE_6502_IMMEDIATE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IMMEDIATE;
                    ClockCycles = 2;
                    break;

                case 0x5D:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tEOR ABS_X";
                    EOR(ADDRESS_MODE_6502_ABS_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABS_X;
                    ClockCycles = 4;
                    break;

                case 0x59:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tEOR ABS_Y";
                    EOR(ADDRESS_MODE_6502_ABS_Y);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABS_Y;
                    ClockCycles = 4;
                    break;

                case 0x41:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tEOR IND_X";
                    EOR(ADDRESS_MODE_6502_IND_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IND_X;
                    ClockCycles = 6;
                    break;

                case 0x51:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tEOR IND_Y";
                    EOR(ADDRESS_MODE_6502_IND_Y);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IND_Y;
                    ClockCycles = 5;
                    break;

                case 0x55:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tEOR Z_PAGE_X";
                    EOR(ADDRESS_MODE_6502_Z_PAGE_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_Z_PAGE_X;
                    ClockCycles = 4;
                    break;

                // INC
                case 0xEE:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tINC ABSOLUTE";
                    INC(ADDRESS_MODE_6502_ABSOLUTE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABSOLUTE;
                    ClockCycles = 6;
                    break;

                case 0xE6:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tINC ZERO_PAGE";
                    INC(ADDRESS_MODE_6502_ZERO_PAGE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ZERO_PAGE;
                    ClockCycles = 5;
                    break;

                case 0xFE:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tINC ABS_X";
                    INC(ADDRESS_MODE_6502_ABS_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABS_X;
                    ClockCycles = 7;
                    break;

                case 0xF6:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tINC Z_PAGE_X";
                    INC(ADDRESS_MODE_6502_Z_PAGE_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_Z_PAGE_X;
                    ClockCycles = 6;
                    break;

                // INX
                case 0xE8:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tINX IMPLIED";
                    INX(ADDRESS_MODE_6502_IMPLIED);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IMPLIED;
                    ClockCycles = 2;
                    break;

                // INY
                case 0xC8:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tINY IMPLIED";
                    INY(ADDRESS_MODE_6502_IMPLIED);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IMPLIED;
                    ClockCycles = 2;
                    break;

                // JMP
                case 0x4C:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tJMP ABSOLUTE";
                    JMP(ADDRESS_MODE_6502_ABSOLUTE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABSOLUTE;
                    ClockCycles = 3;
                    break;

                case 0x6C:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tJMP INDIRECT";
                    JMP(ADDRESS_MODE_6502_INDIRECT);
                    //AddressMode_Debug = ADDRESS_MODE_6502_INDIRECT;
                    ClockCycles = 5;
                    break;

                // JSR
                case 0x20:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tJSR ABSOLUTE";
                    JSR(ADDRESS_MODE_6502_ABSOLUTE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABSOLUTE;
                    ClockCycles = 6;
                    break;

                // LDA
                case 0xAD:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tLDA ABSOLUTE";
                    LDA(ADDRESS_MODE_6502_ABSOLUTE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABSOLUTE;
                    ClockCycles = 4;
                    break;

                case 0xA5:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tLDA ZERO_PAGE";
                    LDA(ADDRESS_MODE_6502_ZERO_PAGE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ZERO_PAGE;
                    ClockCycles = 3;
                    break;

                case 0xA9:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tLDA IMMEDIATE";
                    LDA(ADDRESS_MODE_6502_IMMEDIATE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IMMEDIATE;
                    ClockCycles = 2;
                    break;

                case 0xBD:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tLDA ABS_X";
                    LDA(ADDRESS_MODE_6502_ABS_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABS_X;
                    ClockCycles = 4;
                    break;

                case 0xB9:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tLDA ABS_Y";
                    LDA(ADDRESS_MODE_6502_ABS_Y);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABS_Y;
                    ClockCycles = 4;
                    break;

                case 0xA1:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tLDA IND_X";
                    LDA(ADDRESS_MODE_6502_IND_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IND_X;
                    ClockCycles = 6;
                    break;

                case 0xB1:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tLDA IND_Y";
                    LDA(ADDRESS_MODE_6502_IND_Y);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IND_Y;
                    ClockCycles = 5;
                    break;

                case 0xB5:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tLDA Z_PAGE_X";
                    LDA(ADDRESS_MODE_6502_Z_PAGE_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_Z_PAGE_X;
                    ClockCycles = 4;
                    break;

                // LDX
                case 0xAE:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tLDX ABSOLUTE";
                    LDX(ADDRESS_MODE_6502_ABSOLUTE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABSOLUTE;
                    ClockCycles = 4;
                    break;

                case 0xA6:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tLDX ZERO_PAGE";
                    LDX(ADDRESS_MODE_6502_ZERO_PAGE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ZERO_PAGE;
                    ClockCycles = 3;
                    break;

                case 0xA2:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tLDX IMMEDIATE";
                    LDX(ADDRESS_MODE_6502_IMMEDIATE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IMMEDIATE;
                    ClockCycles = 2;
                    break;

                case 0xBE:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tLDX ABS_Y";
                    LDX(ADDRESS_MODE_6502_ABS_Y);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABS_Y;
                    ClockCycles = 4;
                    break;

                case 0xB6:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tLDX Z_PAGE_Y";
                    LDX(ADDRESS_MODE_6502_Z_PAGE_Y);
                    //AddressMode_Debug = ADDRESS_MODE_6502_Z_PAGE_Y;
                    ClockCycles = 4;
                    break;

                // LDY
                case 0xAC:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tLDY ABSOLUTE";
                    LDY(ADDRESS_MODE_6502_ABSOLUTE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABSOLUTE;
                    ClockCycles = 4;
                    break;

                case 0xA4:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tLDY ZERO_PAGE";
                    LDY(ADDRESS_MODE_6502_ZERO_PAGE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ZERO_PAGE;
                    ClockCycles = 3;
                    break;

                case 0xA0:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tLDY IMMEDIATE";
                    LDY(ADDRESS_MODE_6502_IMMEDIATE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IMMEDIATE;
                    ClockCycles = 2;
                    break;

                case 0xBC:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tLDY ABS_X";
                    LDY(ADDRESS_MODE_6502_ABS_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABS_X;
                    ClockCycles = 4;
                    break;

                case 0xB4:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tLDY Z_PAGE_X";
                    LDY(ADDRESS_MODE_6502_Z_PAGE_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_Z_PAGE_X;
                    ClockCycles = 4;
                    break;

                // LSR
                case 0x4A:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tLSR ACCUMULATOR";
                    LSR(ADDRESS_MODE_6502_ACCUMULATOR);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ACCUMULATOR;
                    ClockCycles = 2;
                    break;

                case 0x4E:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tLSR ABSOLUTE";
                    LSR(ADDRESS_MODE_6502_ABSOLUTE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABSOLUTE;
                    ClockCycles = 6;
                    break;

                case 0x46:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tLSR ZERO_PAGE";
                    LSR(ADDRESS_MODE_6502_ZERO_PAGE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ZERO_PAGE;
                    ClockCycles = 5;
                    break;

                case 0x5E:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tLSR ABS_X";
                    LSR(ADDRESS_MODE_6502_ABS_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABS_X;
                    ClockCycles = 7;
                    break;

                case 0x56:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tLSR Z_PAGE_X";
                    LSR(ADDRESS_MODE_6502_Z_PAGE_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_Z_PAGE_X;
                    ClockCycles = 6;
                    break;

                // NOP
                case 0xEA:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tNOP IMPLIED";
                    NOP(ADDRESS_MODE_6502_IMPLIED);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IMPLIED;
                    ClockCycles = 2;
                    break;

                // ORA
                case 0x0D:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tORA ABSOLUTE";
                    ORA(ADDRESS_MODE_6502_ABSOLUTE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABSOLUTE;
                    ClockCycles = 4;
                    break;

                case 0x05:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tORA ZERO_PAGE";
                    ORA(ADDRESS_MODE_6502_ZERO_PAGE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ZERO_PAGE;
                    ClockCycles = 3;
                    break;

                case 0x09:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tORA IMMEDIATE";
                    ORA(ADDRESS_MODE_6502_IMMEDIATE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IMMEDIATE;
                    ClockCycles = 2;
                    break;

                case 0x1D:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tORA ABS_X";
                    ORA(ADDRESS_MODE_6502_ABS_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABS_X;
                    ClockCycles = 4;
                    break;

                case 0x19:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tORA ABS_Y";
                    ORA(ADDRESS_MODE_6502_ABS_Y);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABS_Y;
                    ClockCycles = 4;
                    break;

                case 0x01:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tORA IND_X";
                    ORA(ADDRESS_MODE_6502_IND_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IND_X;
                    ClockCycles = 6;
                    break;

                case 0x11:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tORA IND_Y";
                    ORA(ADDRESS_MODE_6502_IND_Y);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IND_Y;
                    ClockCycles = 5;
                    break;

                case 0x15:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tORA Z_PAGE_X";
                    ORA(ADDRESS_MODE_6502_Z_PAGE_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_Z_PAGE_X;
                    ClockCycles = 4;
                    break;

                // PHA
                case 0x48:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tPHA IMPLIED";
                    PHA(ADDRESS_MODE_6502_IMPLIED);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IMPLIED;
                    ClockCycles = 3;
                    break;

                // PHP
                case 0x08:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tPHP IMPLIED";
                    PHP(ADDRESS_MODE_6502_IMPLIED);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IMPLIED;
                    ClockCycles = 3;
                    break;

                // PLA
                case 0x68:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tPLA IMPLIED";
                    PLA(ADDRESS_MODE_6502_IMPLIED);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IMPLIED;
                    ClockCycles = 4;
                    break;

                // PLP
                case 0x28:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tPLP IMPLIED";
                    PLP(ADDRESS_MODE_6502_IMPLIED);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IMPLIED;
                    ClockCycles = 4;
                    break;

                // ROL
                case 0x2A:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tROL ACCUMULATOR";
                    ROL(ADDRESS_MODE_6502_ACCUMULATOR);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ACCUMULATOR;
                    ClockCycles = 2;
                    break;

                case 0x2E:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tROL ABSOLUTE";
                    ROL(ADDRESS_MODE_6502_ABSOLUTE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABSOLUTE;
                    ClockCycles = 6;
                    break;

                case 0x26:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tROL ZERO_PAGE";
                    ROL(ADDRESS_MODE_6502_ZERO_PAGE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ZERO_PAGE;
                    ClockCycles = 5;
                    break;

                case 0x3E:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tROL ABS_X";
                    ROL(ADDRESS_MODE_6502_ABS_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABS_X;
                    ClockCycles = 7;
                    break;

                case 0x36:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tROL Z_PAGE_X";
                    ROL(ADDRESS_MODE_6502_Z_PAGE_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_Z_PAGE_X;
                    ClockCycles = 6;
                    break;

                // ROR
                case 0x6A:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tROR ACCUMULATOR";
                    ROR(ADDRESS_MODE_6502_ACCUMULATOR);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ACCUMULATOR;
                    ClockCycles = 2;
                    break;

                case 0x6E:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tROR ABSOLUTE";
                    ROR(ADDRESS_MODE_6502_ABSOLUTE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABSOLUTE;
                    ClockCycles = 6;
                    break;

                case 0x66:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tROR ZERO_PAGE";
                    ROR(ADDRESS_MODE_6502_ZERO_PAGE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ZERO_PAGE;
                    ClockCycles = 5;
                    break;

                case 0x7E:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tROR ABS_X";
                    ROR(ADDRESS_MODE_6502_ABS_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABS_X;
                    ClockCycles = 7;
                    break;

                case 0x76:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tROR Z_PAGE_X";
                    ROR(ADDRESS_MODE_6502_Z_PAGE_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_Z_PAGE_X;
                    ClockCycles = 6;
                    break;

                // RTI
                case 0x40:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tRTI IMPLIED";
                    RTI(ADDRESS_MODE_6502_IMPLIED);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IMPLIED;
                    ClockCycles = 6;
                    break;

                // RTS
                case 0x60:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tRTS IMPLIED";
                    RTS(ADDRESS_MODE_6502_IMPLIED);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IMPLIED;
                    ClockCycles = 6;
                    break;

                // SBC
                case 0xED:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tSBC ABSOLUTE";
                    SBC(ADDRESS_MODE_6502_ABSOLUTE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABSOLUTE;
                    ClockCycles = 4;
                    break;

                case 0xE5:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tSBC ZERO_PAGE";
                    SBC(ADDRESS_MODE_6502_ZERO_PAGE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ZERO_PAGE;
                    ClockCycles = 3;
                    break;

                case 0xE9:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tSBC IMMEDIATE";
                    SBC(ADDRESS_MODE_6502_IMMEDIATE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IMMEDIATE;
                    ClockCycles = 2;
                    break;

                case 0xFD:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tSBC ABS_X";
                    SBC(ADDRESS_MODE_6502_ABS_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABS_X;
                    ClockCycles = 4;
                    break;

                case 0xF9:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tSBC ABS_Y";
                    SBC(ADDRESS_MODE_6502_ABS_Y);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABS_Y;
                    ClockCycles = 4;
                    break;

                case 0xE1:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tSBC IND_X";
                    SBC(ADDRESS_MODE_6502_IND_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IND_X;
                    ClockCycles = 6;
                    break;

                case 0xF1:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tSBC IND_Y";
                    SBC(ADDRESS_MODE_6502_IND_Y);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IND_Y;
                    ClockCycles = 5;
                    break;

                case 0xF5:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tSBC Z_PAGE_X";
                    SBC(ADDRESS_MODE_6502_Z_PAGE_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_Z_PAGE_X;
                    ClockCycles = 4;
                    break;

                // SEC
                case 0x38:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tSEC IMPLIED";
                    SEC(ADDRESS_MODE_6502_IMPLIED);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IMPLIED;
                    ClockCycles = 2;
                    break;

                // SED
                case 0xF8:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tSED IMPLIED";
                    SED(ADDRESS_MODE_6502_IMPLIED);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IMPLIED;
                    ClockCycles = 2;
                    break;

                // SEI
                case 0x78:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tSEI IMPLIED";
                    SEI(ADDRESS_MODE_6502_IMPLIED);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IMPLIED;
                    ClockCycles = 2;
                    break;

                // STA
                case 0x8D:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tSTA ABSOLUTE";
                    STA(ADDRESS_MODE_6502_ABSOLUTE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABSOLUTE;
                    ClockCycles = 4;
                    break;

                case 0x85:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tSTA ZERO_PAGE";
                    STA(ADDRESS_MODE_6502_ZERO_PAGE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ZERO_PAGE;
                    ClockCycles = 2;
                    break;

                case 0x9D:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tSTA ABS_X";
                    STA(ADDRESS_MODE_6502_ABS_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABS_X;
                    ClockCycles = 5;
                    break;

                case 0x99:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tSTA ABS_Y";
                    STA(ADDRESS_MODE_6502_ABS_Y);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABS_Y;
                    ClockCycles = 5;
                    break;

                case 0x81:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tSTA IND_X";
                    STA(ADDRESS_MODE_6502_IND_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IND_X;
                    ClockCycles = 6;
                    break;

                case 0x91:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tSTA IND_Y";
                    STA(ADDRESS_MODE_6502_IND_Y);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IND_Y;
                    ClockCycles = 6;
                    break;

                case 0x95:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tSTA Z_PAGE_X";
                    STA(ADDRESS_MODE_6502_Z_PAGE_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_Z_PAGE_X;
                    ClockCycles = 4;
                    break;

                // STX
                case 0x8E:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tSTX ABSOLUTE";
                    STX(ADDRESS_MODE_6502_ABSOLUTE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABSOLUTE;
                    ClockCycles = 4;
                    break;

                case 0x86:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tSTX ZERO_PAGE";
                    STX(ADDRESS_MODE_6502_ZERO_PAGE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ZERO_PAGE;
                    ClockCycles = 2;
                    break;

                case 0x96:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tSTX Z_PAGE_Y";
                    STX(ADDRESS_MODE_6502_Z_PAGE_Y);
                    //AddressMode_Debug = ADDRESS_MODE_6502_Z_PAGE_Y;
                    ClockCycles = 4;
                    break;

                // STY
                case 0x8C:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tSTY ABSOLUTE";
                    STY(ADDRESS_MODE_6502_ABSOLUTE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ABSOLUTE;
                    ClockCycles = 4;
                    break;

                case 0x84:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tSTY ZERO_PAGE";
                    STY(ADDRESS_MODE_6502_ZERO_PAGE);
                    //AddressMode_Debug = ADDRESS_MODE_6502_ZERO_PAGE;
                    ClockCycles = 2;
                    break;

                case 0x94:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tSTY Z_PAGE_X";
                    STY(ADDRESS_MODE_6502_Z_PAGE_X);
                    //AddressMode_Debug = ADDRESS_MODE_6502_Z_PAGE_X;
                    ClockCycles = 4;
                    break;

                // TAX
                case 0xAA:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tTAX IMPLIED";
                    TAX(ADDRESS_MODE_6502_IMPLIED);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IMPLIED;
                    ClockCycles = 2;
                    break;

                // TAY
                case 0xA8:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tTAY IMPLIED";
                    TAY(ADDRESS_MODE_6502_IMPLIED);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IMPLIED;
                    ClockCycles = 2;
                    break;

                // TSX
                case 0xBA:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tTSX IMPLIED";
                    TSX(ADDRESS_MODE_6502_IMPLIED);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IMPLIED;
                    ClockCycles = 2;
                    break;

                // TXA
                case 0x8A:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tTXA IMPLIED";
                    TXA(ADDRESS_MODE_6502_IMPLIED);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IMPLIED;
                    ClockCycles = 2;
                    break;

                // TXS
                case 0x9A:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tTXS IMPLIED";
                    TXS(ADDRESS_MODE_6502_IMPLIED);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IMPLIED;
                    ClockCycles = 2;
                    break;

                // TYA
                case 0x98:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "\tTYA IMPLIED";
                    TYA(ADDRESS_MODE_6502_IMPLIED);
                    //AddressMode_Debug = ADDRESS_MODE_6502_IMPLIED;
                    ClockCycles = 2;
                    break;

                // If there was no recognizable opcode the processor
                // wildly runs on:
                default:
                    //if (DebugEnabled) DebugString = "" + hexChars[PC.H / 16] + hexChars[PC.H % 16] + hexChars[PC.L / 16] + hexChars[PC.L % 16] + "\t" + hexChars[OpCode / 16] + hexChars[OpCode % 16] + "??? ";
                    PC.W++;
                    break;
            }

            ////if (DebugEnabled && debugBytes != null){for (Int32 b = debugBytes.Length - 1; b > -1;  b--){DebugString += "\t" + hexChars[debugBytes[b] / 16] + hexChars[debugBytes[b] % 16];}}
            ////if (DebugEnabled) Debug.WriteLine(DebugString);

            return ClockCycles;
        }

        //////////////////////////////////////////////////////////////////////////
        // Opcode handlers 
        //////////////////////////////////////////////////////////////////////////
        /*
        Carry Flag 
        The carry flag is set if the last operation caused an overflow from bit 
        7 of the result or an underflow from bit 0. This condition is set during 
        arithmetic, comparison and during logical shifts. It can be explicitly 
        set using the 'Set Carry Flag' (SEC) instruction and cleared with 'Clear 
        Carry Flag' (CLC).

        Zero Flag 
        The zero flag is set if the result of the last operation was zero.

        Interrupt Disable 
        The interrupt disable flag is set if the program has executed a 'Set 
        Interrupt Disable' (SEI) instruction. While this flag is set the 
        processor will not respond to interrupts from devices until it is 
        cleared by a 'Clear Interrupt Disable' (CLI) instruction.

        Decimal Mode 
        While the decimal mode flag is set the processor will obey the rules of 
        Binary Coded Decimal (BCD) arithmetic during addition and subtraction. 
        The flag can be explicitly set using 'Set Decimal Flag' (SED) and 
        cleared with 'Clear Decimal Flag' (CLD).

        Break Command 
        The break command bit is set when a BRK instruction has been executed 
        and an interrupt has been generated to process it.

        Overflow Flag 
        The overflow flag is set during arithmetic operations if the result 
        has yielded an invalid 2's complement result (e.g. adding to positive 
        numbers and ending up with a negative result: 64 + 64 => -128). It is 
        determined by looking at the carry between bits 6 and 7 and between 
        bit 7 and the carry flag.

        Negative Flag 
        The negative flag is set if the result of the last operation had bit 
        7 set to a one.

        As instructions are executed a set of processor flags are set or clear 
        to record the results of the operation. This flags and some additional 
        control flags are held in a special status register. Each flag has a 
        single bit within the register.

        Instructions exist to test the values of the various bits, to set or 
        clear some of them and to push or pull the entire set to or from the 
        stack.
        */

        public void ADC(int AddressMode)
        {
            /*
              ADC               Add memory to accumulator with carry                ADC

              Add the data located at the effective address specified by the operand to the contents of the
            accumulator; add one to the result if the carry flag is set, and store the final result in the accumulator.
            The 65x processors have no add instruction that does not involve the carry. To avoid adding the carry
            flag to the result, you must either be sure that it is already clear, or you must explicitly clear it (using CLC)
            prior to executing the ADC instruction.
            In a multi-precision (multi-word) addition, the carry should be cleared before the low-order words are
            added; the addition of the low word will generate a new carry flag value based on the addition. This new value
            in the carry flag is added into the next (middle-order or high-order) addition; each intermediate result will
            correctly reflect the carry from the previous addition.
            d flag clear: Binary addition is performed.
            d flag set: Binary coded decimal (BCD) addition is performed.
            8-bit accumulator (all processors): Data added from memory is eight-bit.

            Flags Affected: n v - - - - z c
            n Set if most significant bit of result is set; else cleared.
            v Set if signed overflow; cleared if valid signed result.
            z Set if result is zero; else cleared.
            c Set if unsigned overflow; cleared if valid unsigned result.

              Operation:  A + M + C -> A, C                         N Z C I D V
                                                                    / / / _ _ /
                                            (Ref: 2.2.1)
              +----------------+-----------------------+---------+---------+----------+
              | Addressing Mode| Assembly Language Form| OP CODE |No. Bytes|No. Cycles|
              +----------------+-----------------------+---------+---------+----------+
              |  Immediate     |   ADC #Oper           |    69   |    2    |    2     |
              |  Zero Page     |   ADC Oper            |    65   |    2    |    3     |
              |  Zero Page,X   |   ADC Oper,X          |    75   |    2    |    4     |
              |  Absolute      |   ADC Oper            |    60   |    3    |    4     |
              |  Absolute,X    |   ADC Oper,X          |    70   |    3    |    4*    |
              |  Absolute,Y    |   ADC Oper,Y          |    79   |    3    |    4*    |
              |  (Indirect,X)  |   ADC (Oper,X)        |    61   |    2    |    6     |
              |  (Indirect),Y  |   ADC (Oper),Y        |    71   |    2    |    5*    |
              +----------------+-----------------------+---------+---------+----------+
              * Add 1 if page boundary is crossed.

            When the addition result is 0 to 255, the carry is cleared. 
            When the addition result is greater than 255, the carry is set. 

            V indicates whether the result of an addition or subraction is 
            outside the range -128 to 127

            */

            // Get effective address in AddressInEffect:
            FetchAddress(AddressMode);

            // Store bit 7 (sign) for later evaluation:
            //	byte sign = 0x80 & A;

            // Calculate the result:
            MemoryBus.SetAddress(AddressInEffect);

            byte M = MemoryBus.Read();

            // Decimal mode:
            if (IsSet(FLAG_6502_DECIMAL_MODE))
            {
                // Decimal add operation.
                // Split operands into two nibbles, and use as 
                // two digits. A carry is added to the least
                // significant nibble (lsn). A carry from lsn 
                // may occur, and is added to the msn. If a
                // carry results from adding msn:s, set carry
                // flag, else reset it. Note, a nibble may only
                // contain values from 0 to 9.

                // Save MSB:
                byte msb = (byte)(A & 0x80);

                // Split operands into nibbles:
                byte AL = (byte)(A & 0x0F);
                byte AH = (byte)((A & 0xF0) >> 4);
                byte ML = (byte)(M & 0x0F);
                byte MH = (byte)((M & 0xF0) >> 4);

                // Add lsn:
                AL += ML;
                if (IsSet(FLAG_6502_CARRY))
                {
                    AL++;
                    ResetFlag(FLAG_6502_CARRY);
                }

                // Remember internal carry:
                bool carry = AL > 9 ? true : false;

                // Create nibble-compliant versions of AH and MH to do the
                // nessecary two's complement operations for determining 
                // the overflow state. A nibble holds values between -8 and +7.
                // Look at msn components and determine the resulting overflow flag:
                byte cAH = (byte)(AH > 7 ? AH - 16 : AH);
                byte cMH = (byte)(MH > 7 ? MH - 16 : MH);
                byte tCarry = (byte)(carry ? 1 : 0);
                //byte tNewOverflow = (A + M + tCarry > 7) || (A + M + tCarry < -8) ? 1 : 0;

                // Add msn:
                AH += MH;
                if (carry)
                {
                    AH++;
                    AL -= 10;
                }
                /*
                        // Set overflow flag:
                        if(tNewOverflow)
                        {
                            SetFlag(FLAG_6502_OVERFLOW);
                        }
                        else
                        {
                            ResetFlag(FLAG_6502_OVERFLOW);
                        }
                */
                // Set carry flag:
                if (AH > 9)
                {
                    SetFlag(FLAG_6502_CARRY);
                    AH -= 10;
                }
                else
                {
                    ResetFlag(FLAG_6502_CARRY);
                }

                // Assemble resulting nibbles into Atmp;
                byte Atmp = (byte)((AH << 4) | AL);

                // Store result:
                A = Atmp;
            }
            else
            {
                // Normal add operation:
                // Fetch current carry:
                byte tCarry = (byte)(IsSet(FLAG_6502_CARRY) ? 0x01 : 0x00);

                // Overflow will be set if result is larger than 127:
                //		byte tNewOverflow = (A + M + tCarry > (int)127)?1:0;
                // Overflow will be set if result is utside range -128 - +127:
                int iA = (byte)A, iM = (byte)M, iTemp;
                iTemp = iA + iM;
                iTemp = iTemp + 1;
                byte tNewOverflow = (byte)(((byte)A + (byte)M + tCarry > (int)127) || ((byte)A + (byte)M + tCarry < (int)-128) ? 1 : 0);

                // Carry will be set if result is larger than 255:
                //		byte tNewCarry = (A + M + tCarry > (int)255)?1:0;
                // Carry will be set if result is utside range 0 - 255:
                byte tNewCarry = (byte)(((int)A + (int)M + tCarry > (int)255) || ((int)A + (int)M + tCarry < (int)0) ? 1 : 0);

                // Do the add:
                A = (byte)(A + M + tCarry);

                // Set new carry flag:
                if (tNewCarry > 0)
                {
                    SetFlag(FLAG_6502_CARRY);
                }
                else
                {
                    ResetFlag(FLAG_6502_CARRY);
                }

                // Set new overflow flag:
                if (tNewOverflow > 0)
                {
                    SetFlag(FLAG_6502_OVERFLOW);
                }
                else
                {
                    ResetFlag(FLAG_6502_OVERFLOW);
                }

                // Set negative flag:
                if ((0x80 & A) > 0)
                {
                    SetFlag(FLAG_6502_NEGATIVE);
                }
                else
                {
                    ResetFlag(FLAG_6502_NEGATIVE);
                }

                // Set zero flag:
                if (A == 0)
                {
                    SetFlag(FLAG_6502_ZERO);
                }
                else
                {
                    ResetFlag(FLAG_6502_ZERO);
                }
            }

            // Move to next instruction:
            PC.W++;
        }

        public void AND(int AddressMode)
        {
            /*
              AND                  "AND" memory with accumulator                    AND

            Bitwise logical AND the data located at the effective address specified by the operand with the contents
            of the accumulator. Each bit in the accumulator is ANDed with the corresponding bit in memory, with the
            result being stored in the respective accumulator bit.
            That is, a 1 or logical true results in given bit being true only if both elements of the respective bits
            being ANDed are 1s, or logically true.
            8-bit accumulator (all processors): Data ANDed from memory is eight-bit.

            Flags Affected: n - - - - - z -
            n Set if most significant bit of result is set; else cleared.
            z Set if result is zero; else cleared.

              Operation:  A /\ M -> A                               N Z C I D V
                                                                    / / _ _ _ _
                                           (Ref: 2.2.3.0)
              +----------------+-----------------------+---------+---------+----------+
              | Addressing Mode| Assembly Language Form| OP CODE |No. Bytes|No. Cycles|
              +----------------+-----------------------+---------+---------+----------+
              |  Immediate     |   AND #Oper           |    29   |    2    |    2     |
              |  Zero Page     |   AND Oper            |    25   |    2    |    3     |
              |  Zero Page,X   |   AND Oper,X          |    35   |    2    |    4     |
              |  Absolute      |   AND Oper            |    2D   |    3    |    4     |
              |  Absolute,X    |   AND Oper,X          |    3D   |    3    |    4*    |
              |  Absolute,Y    |   AND Oper,Y          |    39   |    3    |    4*    |
              |  (Indirect,X)  |   AND (Oper,X)        |    21   |    2    |    6     |
              |  (Indirect,Y)  |   AND (Oper),Y        |    31   |    2    |    5     |
              +----------------+-----------------------+---------+---------+----------+
              * Add 1 if page boundary is crossed.

            */

            // Get effective address in AddressInEffect:
            FetchAddress(AddressMode);

            // Calculate the result:
            MemoryBus.SetAddress(AddressInEffect);
            A = (byte)(A & MemoryBus.Read());

            // Set flags affected.
            // Negative:
            if ((0x80 & A) > 0)
            {
                SetFlag(FLAG_6502_NEGATIVE);
            }
            else
            {
                ResetFlag(FLAG_6502_NEGATIVE);
            }

            // Zero:
            if (A == 0)
            {
                SetFlag(FLAG_6502_ZERO);
            }
            else
            {
                ResetFlag(FLAG_6502_ZERO);
            }

            // Move to next instruction:
            PC.W++;
        }

        public void ASL(int AddressMode)
        {
            /*
              ASL          ASL Shift Left One Bit (Memory or Accumulator)           ASL

              Shift the contents of the location specified by the operand left one bit. That is, bit one takes on the
            value originally found in bit zero, bit two takes the value originally in bit one, and so on; the leftmost bit (bit 7
            on the 6502 and 65C02 or if m = 1 on the 65802/65816, or bit 15 if m = 0) is transferred into the carry flag; the
            rightmost bit, bit zero, is cleared. The arithmetic result of the operation is an unsigned multiplication by two.
            8-bit accumulator/memory (all processors): Data shifted is eight bits.

            Flags Affected:n - - - - - z c
            n Set if most significant bit of result is set; else cleared.
            z Set if result is zero; else cleared.
            c High bit becomes carry: set if high bit was set; cleared if high bit was zero.

                               +-+-+-+-+-+-+-+-+
              Operation:  C <- |7|6|5|4|3|2|1|0| <- 0
                               +-+-+-+-+-+-+-+-+                    N Z C I D V
                                                                    / / / _ _ _
                                             (Ref: 10.2)
              +----------------+-----------------------+---------+---------+----------+
              | Addressing Mode| Assembly Language Form| OP CODE |No. Bytes|No. Cycles|
              +----------------+-----------------------+---------+---------+----------+
              |  Accumulator   |   ASL A               |    0A   |    1    |    2     |
              |  Zero Page     |   ASL Oper            |    06   |    2    |    5     |
              |  Zero Page,X   |   ASL Oper,X          |    16   |    2    |    6     |
              |  Absolute      |   ASL Oper            |    0E   |    3    |    6     |
              |  Absolute, X   |   ASL Oper,X          |    1E   |    3    |    7     |
              +----------------+-----------------------+---------+---------+----------+
            */

            if (AddressMode == ADDRESS_MODE_6502_ACCUMULATOR)
            {
                // Store bit 7 (sign) for later evaluation:
                byte msb = (byte)(0x80 & A);

                // Shift left:
                A = (byte)(A << 1);

                // Set flags affected.
                // Carry:
                if (msb > 0)
                {
                    SetFlag(FLAG_6502_CARRY);
                }
                else
                {
                    ResetFlag(FLAG_6502_CARRY);
                }

                // Negative:
                if ((0x80 & A) > 0)
                {
                    SetFlag(FLAG_6502_NEGATIVE);
                }
                else
                {
                    ResetFlag(FLAG_6502_NEGATIVE);
                }

                // Zero:
                if (A == 0)
                {
                    SetFlag(FLAG_6502_ZERO);
                }
                else
                {
                    ResetFlag(FLAG_6502_ZERO);
                }
            }
            else
            {
                // Get effective address in AddressInEffect:
                FetchAddress(AddressMode);

                // Calculate the result:
                MemoryBus.SetAddress(AddressInEffect);
                byte M = MemoryBus.Read();

                // Store bit 7 (sign) for later evaluation:
                byte msb = (byte)(0x80 & M);

                // Shift left:
                M = (byte)(M << 1);

                // Store result:
                MemoryBus.Write(M);

                // Set flags affected.
                // Carry:
                if (msb > 0)
                {
                    SetFlag(FLAG_6502_CARRY);
                }
                else
                {
                    ResetFlag(FLAG_6502_CARRY);
                }

                // Negative:
                if ((0x80 & M) > 0)
                {
                    SetFlag(FLAG_6502_NEGATIVE);
                }
                else
                {
                    ResetFlag(FLAG_6502_NEGATIVE);
                }

                // Zero:
                if (M == 0)
                {
                    SetFlag(FLAG_6502_ZERO);
                }
                else
                {
                    ResetFlag(FLAG_6502_ZERO);
                }
            }

            // Move to next instruction:
            PC.W++;
        }

        public void BCC(int AddressMode)
        {
            /*
              BCC                     BCC Branch on Carry Clear                     BCC

              The carry flag in the P status register is tested. If it is clear, a branch is taken; if it is set, the instruction
            immediately following the two-byte BCC instruction is executed.
            If the branch is taken, a one-byte signed displacement, fetched from the second byte of the instruction,
            is sign-extended to sixteen bits and added to the program counter. Once the branch address has been calculated,
            the result is loaded into the program counter, transferring control to that location.
            The allowable range of the displacement is - 128 to + 127 (from the instruction immediately following
            the branch).
            BCC may be used in several ways: to test the result of a shift into the carry; to determine if the result of
            a comparison is either less than (in which case a branch will be taken), or greater than or equal (which causes
            control to fall through the branch instruction); or to determine if further operations are needed in multi-precision
            arithmetic.
            Because the BCC instruction causes a branch to be taken after a comparison or subtraction if the
            accumulator is less than the memory operand (since the carry flag will always be cleared as a result), many
            assemblers allow an alternate mnemonic for the BCC instruction: BLT, or Branch if Less Than.
            Flags Affected : - - - - - - - -

                                                                    N Z C I D V
              Operation:  Branch on C = 0                           _ _ _ _ _ _
                                           (Ref: 4.1.1.3)
              +----------------+-----------------------+---------+---------+----------+
              | Addressing Mode| Assembly Language Form| OP CODE |No. Bytes|No. Cycles|
              +----------------+-----------------------+---------+---------+----------+
              |  Relative      |   BCC Oper            |    90   |    2    |    2*    |
              +----------------+-----------------------+---------+---------+----------+
              * Add 1 if branch occurs to same page.
              * Add 2 if branch occurs to different page.
            */

            // Point out next byte, the branch offset:
            PC.W++;

            // Test carry flag:
            if (!IsSet(FLAG_6502_CARRY))
            {
                // Fetch branch offset:
                MemoryBus.SetAddress(PC);
                Address tmp = new Address();
                tmp.H = 0;
                tmp.L = MemoryBus.Read();

                // Prepare branch:
                if ((tmp.L & 0x80) == 0x00)
                {
                    PC += tmp;
                }
                else
                {
                    tmp.L = (byte)(0x00 - tmp.L);
                    PC -= tmp;
                }
            }

            // Move to next instruction:
            PC.W++;
        }

        public void BCS(int AddressMode)
        {
            /*
              BCS                      BCS Branch on carry set                      BCS

            The carry flag in the P status register is tested. If it is set, a branch is taken; if it is clear, the instruction
            immediately following the two-byte BCS instruction is executed.
            If the branch is taken, a one-byte signed displacement, fetched from the second byte of the instruction,
            is sign-extended to sixteen bits and added to the program counter. Once the branch address has been calculated,
            the result is loaded into the program counter, transferring control to that location.
            The allowable range of the displacement is - 128 to + 127 (from the instruction immediately following
            the branch).
            BCS is used in several ways: to test the result of a shift into the carry; to determine if the result of a
            comparison is either greater than or equal (which causes the branch to be taken) or less than; or to determine if
            further operations are needed in multi-precision arithmetic operations.
            Because the BCS instruction causes a branch to be taken after a comparison or subtraction if the
            accumulator is greater than or equal to the memory operand (since the carry flag will always be set as a result),
            many assemblers allow an alternate mnemonic for the BCS instruction: BGE or Branch if Greater or Equal.
            Flags Affected: - - - - - - - -

              Operation:  Branch on C = 1                           N Z C I D V
                                                                    _ _ _ _ _ _
                                           (Ref: 4.1.1.4)
              +----------------+-----------------------+---------+---------+----------+
              | Addressing Mode| Assembly Language Form| OP CODE |No. Bytes|No. Cycles|
              +----------------+-----------------------+---------+---------+----------+
              |  Relative      |   BCS Oper            |    B0   |    2    |    2*    |
              +----------------+-----------------------+---------+---------+----------+
              * Add 1 if branch occurs to same  page.
              * Add 2 if branch occurs to next  page.
            */

            // Point at next byte:
            PC.W++;

            // Test carry flag:
            if (IsSet(FLAG_6502_CARRY))
            {
                // Fetch next byte:
                MemoryBus.SetAddress(PC);
                Address tmp = new Address();
                tmp.H = 0;
                tmp.L = MemoryBus.Read();

                // Prepare branch:
                if ((tmp.L & 0x80) == 0x00)
                {
                    PC += tmp;
                }
                else
                {
                    tmp.L = (byte)(0x00 - tmp.L);
                    PC -= tmp;
                }
            }

            // Move to next instruction:
            PC.W++;
        }

        public void BEQ(int AddressMode)
        {
            /*
              BEQ                    BEQ Branch on result zero                      BEQ

              The zero flag in the P status register is tested. If it is set, meaning that the last value tested (which
            affected the zero flag) was zero, a branch is taken; if it is clear, meaning the value tested was non-zero, the
            instruction immediately following the two-byte BEQ instruction is executed.
            If the branch is taken, a one-byte signed displacement, fetched from the second byte of the instruction,
            is sign-extended to sixteen bits and added to the program counter. Once the branch address has been calculated,
            the result is loaded into the program counter, transferring control to that location.
            The allowable range of the displacement is - 128 to + 127 (from the instruction immediately following
            the branch).
            BEQ may be used in several ways: to determine if the result of a comparison is zero (the two values
            compared are equal), for example, or if a value just loaded, pulled, shifted, incremented or decremented is zero;
            or to determine if further operations are needed in multi-precision arithmetic operations. Because testing for
            equality to zero does not require a previous comparison with zero, it is generally most efficient for loop counters
            to count downwards, existing when zero is reached.
            Flags Affected: - - - - - - - -

                                                                    N Z C I D V
              Operation:  Branch on Z = 1                           _ _ _ _ _ _
                                           (Ref: 4.1.1.5)
              +----------------+-----------------------+---------+---------+----------+
              | Addressing Mode| Assembly Language Form| OP CODE |No. Bytes|No. Cycles|
              +----------------+-----------------------+---------+---------+----------+
              |  Relative      |   BEQ Oper            |    F0   |    2    |    2*    |
              +----------------+-----------------------+---------+---------+----------+
              * Add 1 if branch occurs to same  page.
              * Add 2 if branch occurs to next  page.
            */

            // Point at next byte:
            PC.W++;

            // Test zero flag:
            if (IsSet(FLAG_6502_ZERO))
            {
                // Fetch next byte:
                MemoryBus.SetAddress(PC);
                Address tmp = new Address();
                tmp.H = 0;
                tmp.L = MemoryBus.Read();

                // Prepare branch:
                if ((tmp.L & 0x80) == 0x00)
                {
                    PC += tmp;
                }
                else
                {
                    tmp.L = (byte)(0x00 - tmp.L);
                    PC -= tmp;
                }
            }

            // Move to next instruction:
            PC.W++;
        }

        public void BIT(int AddressMode)
        {
            /*
              BIT             BIT Test bits in memory with accumulator              BIT

            BIT sets the P status register flags based on the result of two different operations, making it a dualpurpose
            instruction:
            First, it sets or clears the n flag to reflect the value of the high bit of the data located at the effective
            address specified by the operand, and sets or clears the v flag to reflect the contents of the next-to-highest bit of
            the data addressed.
            Second, it logically ANDs the data located at the effective address with the contents of the accumulator;
            it changes neither value, but sets the z flag if the result is zero, or clears it if the result is non-zero.
            BIT is usually used immediately preceding a conditional branch instruction: to test a memory value's
            highest or next-to-highest bits; with a mask in the accumulator, to test any bits of the memory operand; or with a
            constant as the mask (using immediate addressing) or a mask in memory, to test any bits in the accumulator.
            All of these tests are non-destructive of the data in the accumulator or in memory. When the BIT instruction is
            used with the immediate addressing mode, the n and v flags are unaffected.
            8-bit accumulator/memory (all processors): Data in memory is eight-bit; bit 7 is moved into the n
            flag; bit 6 is moved into the v flag.
            16-bit accumulator/memory (65802/65816 only, m = 0): Data in memory is sixteen-bit: the low-order
            eight bits are located at the effective address; the high-order eight bits are located at the effective address plus
            one. Bit 15 is moved into the n flag; bit 14 is moved into the v flag.

            Flags Affected: n v - - - - z - (Other than immediate addressing)
            - - - - - - z - (Immediate addressing only)
            n Takes value of most significant bit of memory data.
            v Takes value of next-to-highest bit of memory data.
            z Set if logical AND of memory and accumulator is zero; else cleared.

              Operation:  A /\ M, M7 -> N, M6 -> V

              Bit 6 and 7 are transferred to the status register.   N Z C I D V
              If the result of A /\ M is zero then Z = 1, otherwise M7/ _ _ _ M6
              Z = 0
                                           (Ref: 4.2.1.1)
              +----------------+-----------------------+---------+---------+----------+
              | Addressing Mode| Assembly Language Form| OP CODE |No. Bytes|No. Cycles|
              +----------------+-----------------------+---------+---------+----------+
              |  Zero Page     |   BIT Oper            |    24   |    2    |    3     |
              |  Absolute      |   BIT Oper            |    2C   |    3    |    4     |
              +----------------+-----------------------+---------+---------+----------+
            */

            // Get effective address in AddressInEffect:
            FetchAddress(AddressMode);

            // Calculate the result:
            MemoryBus.SetAddress(AddressInEffect);
            byte Byte = MemoryBus.Read();
            //	Byte = Byte & A;

            // Does not apply to negative and overflow flag in immediate mode:
            if ((P & ADDRESS_MODE_6502_IMMEDIATE) == 0)
            {
                // Transfer bit seven to minus flag:
                if ((Byte & 0x80) > 0)
                {
                    SetFlag(FLAG_6502_NEGATIVE);
                }
                else
                {
                    ResetFlag(FLAG_6502_NEGATIVE);
                }

                // Transfer bit six to overflow flag:
                if ((Byte & 0x40) > 0)
                {
                    SetFlag(FLAG_6502_OVERFLOW);
                }
                else
                {
                    ResetFlag(FLAG_6502_OVERFLOW);
                }
            }

            Byte = (byte)(Byte & A);

            // Set zero flag accordingly:
            if (Byte == 0)
            {
                SetFlag(FLAG_6502_ZERO);
            }
            else
            {
                ResetFlag(FLAG_6502_ZERO);
            }

            // Move to next instruction:
            PC.W++;
        }

        public void BMI(int AddressMode)
        {
            /*
              BMI                    BMI Branch on result minus                     BMI

            The negative flag in the P status register is tested. If it is set, the high bit of the value which most
            recently affected the n flag was set, and a branch is taken. A number with its high bit set may be interpreted as
            a negative two's-complement numbers, so this instruction tests, among other things, for the sign of two'scomplement
            numbers. If the negative flag is clear, the high bit of the value which most recently affected the
            flag was clear, or, in the two's-complement system, was a positive number, and the instruction immediately
            following the two-byte BMI instruction is executed.
            If the branch is taken, a one-byte signed displacement, fetched from the second byte of the instruction,
            is sign-extended to sixteen bits and added to the program counter. Once the branch address has been calculated,
            the result is loaded into the program counter, transferring control to that location.
            The allowable range of the displacement is - 128 to + 127 (from the instruction immediately following
            the branch).
            BMI is primarily used to either determine, in two's-complement arithmetic, if a value is negative or, in
            logic situations, if the high bit of the value is set. It can also be used when looping down through zero (the loop
            counter must have a positive initial value) to determine if zero has been passed and to effect an exit from the
            loop.
            Flags Affected: - - - - - - - -

              Operation:  Branch on N = 1                           N Z C I D V
                                                                    _ _ _ _ _ _
                                           (Ref: 4.1.1.1)
              +----------------+-----------------------+---------+---------+----------+
              | Addressing Mode| Assembly Language Form| OP CODE |No. Bytes|No. Cycles|
              +----------------+-----------------------+---------+---------+----------+
              |  Relative      |   BMI Oper            |    30   |    2    |    2*    |
              +----------------+-----------------------+---------+---------+----------+
              * Add 1 if branch occurs to same page.
              * Add 1 if branch occurs to different page.
            */

            // Point at next byte:
            PC.W++;

            // Test minus flag:
            if (IsSet(FLAG_6502_NEGATIVE))
            {
                // Fetch next byte:
                MemoryBus.SetAddress(PC);
                Address tmp = new Address();
                tmp.H = 0;
                tmp.L = MemoryBus.Read();

                // Prepare branch:
                if ((tmp.L & 0x80) == 0x00)
                {
                    PC += tmp;
                }
                else
                {
                    tmp.L = (byte)(0x00 - tmp.L);
                    PC -= tmp;
                }
            }

            // Move to next instruction:
            PC.W++;
        }

        public void BNE(int AddressMode)
        {
            /*
              BNE                   BNE Branch on result not zero                   BNE

            The zero flag in the P status register is tested. If it is clear (meaning the value just tested is non-zero), a
            branch is taken; if it is set (meaning the value tested is zero), the instruction immediately following the two-byte
            BNE instruction is executed.
            If the branch is taken, a one-byte signed displacement, fetched from the second byte of the instruction,
            is sign-extended to sixteen bits and added to the program counter. Once the branch address has been calculated,
            the result is loaded into the program counter, transferring control to that location.
            The allowable range of the displacement is - 128 to + 127 (from the instruction immediately following
            the branch).
            BNE may be used in several ways: to determine if the result of a comparison is non-zero (the two
            values compared are not equal), for example, or if the value just loaded or pulled from the stack is non-zero, or
            to determine if further operations are needed in multi-precision arithmetic operations.
            Flags Affected: - - - - - - - -

              Operation:  Branch on Z = 0                           N Z C I D V
                                                                    _ _ _ _ _ _
                                           (Ref: 4.1.1.6)
              +----------------+-----------------------+---------+---------+----------+
              | Addressing Mode| Assembly Language Form| OP CODE |No. Bytes|No. Cycles|
              +----------------+-----------------------+---------+---------+----------+
              |  Relative      |   BMI Oper            |    D0   |    2    |    2*    |
              +----------------+-----------------------+---------+---------+----------+
              * Add 1 if branch occurs to same page.
              * Add 2 if branch occurs to different page.
            */

            // Point at next byte:
            PC.W++;

            // Test zero flag:
            if (!IsSet(FLAG_6502_ZERO))
            {
                // Fetch next byte:
                MemoryBus.SetAddress(PC);
                Address tmp = new Address();
                tmp.H = 0;
                tmp.L = MemoryBus.Read();

                // Prepare branch:
                if ((tmp.L & 0x80) == 0x00)
                {
                    PC += tmp;
                }
                else
                {
                    tmp.L = (byte)(0x00 - tmp.L);
                    PC -= tmp;
                }
            }

            // Move to next instruction:
            PC.W++;
        }

        public void BPL(int AddressMode)
        {
            /*
              BPL                     BPL Branch on result plus                     BPL

            The negative flag in the P status register is tested. If it is clear - meaning that the last value which
            affected the zero flag had it's high bit clear - a branch is taken. In the two's complement system, values with
            their high bit clear are interpreted as positive numbers. If the flag is set, meaning the high bit of the last value
            was set, the branch is not taken; it is a two's-complement negative number, and the instruction immediately
            following the two-byte BPL instruction is executed.
            If the branch is taken, a one-byte signed displacement, fetched from the second byte of the instruction,
            is sign-extended to sixteen bits and added to the program counter. Once the branch address has been calculated,
            the result is loaded into the program counter, transferring control to that location.
            The allowable range of the displacement is - 128 to + 127 (from the instruction immediately following
            the branch).
            BPL is used primarily to determine, in two's-complement arithmetic, if a value is positive or not or, in
            logic situations, if the high bit of the value is clear.
            Flags Affected: - - - - - - - -

              Operation:  Branch on N = 0                           N Z C I D V
                                                                    _ _ _ _ _ _
                                           (Ref: 4.1.1.2)
              +----------------+-----------------------+---------+---------+----------+
              | Addressing Mode| Assembly Language Form| OP CODE |No. Bytes|No. Cycles|
              +----------------+-----------------------+---------+---------+----------+
              |  Relative      |   BPL Oper            |    10   |    2    |    2*    |
              +----------------+-----------------------+---------+---------+----------+
              * Add 1 if branch occurs to same page.
              * Add 2 if branch occurs to different page.
            */

            // Point at next byte:
            PC.W++;

            // Test minus flag:
            if (!IsSet(FLAG_6502_NEGATIVE))
            {
                // Fetch next byte:
                MemoryBus.SetAddress(PC);
                Address tmp = new Address();
                tmp.H = 0;
                tmp.L = MemoryBus.Read();

                // Prepare branch:
                if ((tmp.L & 0x80) == 0x00)
                {
                    PC += tmp;
                }
                else
                {
                    tmp.L = (byte)(0x00 - tmp.L);
                    PC -= tmp;
                }
            }

            // Move to next instruction:
            PC.W++;
        }

        public void BRK(int AddressMode)
        {
            /*
              BRK                          BRK Force Break                          BRK

            Force a software interrupt. BRK is unaffected by the i interrupt disable flag.
            Although BRK is a one-byte instruction, the program counter (which is pushed onto the stack by the
            instruction) is incremented by two; this lets you follow the break instruction with a one-byte signature byte
            indicating which break caused the interrupt. Even if a signature byte is not needed, either the byte following the
            BRK instruction must be padded with some value or the break-handling routine must decrement the return
            address on the stack to let an RTI (return from interrupt) instruction executed correctly.
            6502, 65C02, and Emulation Mode (e = 1): The program counter is incremented by two, then pushed
            onto the stack; the status register, with the b break flag set, is pushed onto the stack; the interrupt disable flag is
            set; and the program counter is loaded from the interrupt vector at $FFFE-FFFF. It is up to the interrupt
            handling routine at this address to check the b flag in the stacked status register to determine if the interrupt was
            caused by a software interrupt (BRK) or by a hardware IRQ, which shares BRK vector but pushes the status
            register onto the stack with the b break flag clear. For example,
            0000 68   PLA copy status from
            0001 48   PHA top of stack
            0002 2910 AND #$10 check BRK bit
            0004 D007 BNE ISBRK branch if set
            65802/65816 Native Mode (e = 0): The program counter bank register is pushed onto the stack; the
            program counter is incremented by two and pushed onto the stack; the status register is pushed onto the stack;
            the interrupt disable flag is set; the program bank register is cleared to zero; and the program counter is loaded
            from the break vector at $00FFE6-00FFE7.
            6502: The d decimal flag is not modified after a break is executed.
            Flags Affected: - - - b - i - - (6502)
            - - - b d i - - (65C02, 65802/65816 emulation mode e = 1)
            - - - - d i - - (65802/65816 native mode e = 0)
            b b in the P register value pushed onto the stack is set.
            d d is reset to 0, for binary arithmetic.
            i The interrupt disable flag is set, disabling hardware IRQ interrupts.

              Operation:  Forced Interrupt PC + 2 toS P toS         N Z C I D V
                                                                    _ _ _ 1 _ _
                                             (Ref: 9.11)
              +----------------+-----------------------+---------+---------+----------+
              | Addressing Mode| Assembly Language Form| OP CODE |No. Bytes|No. Cycles|
              +----------------+-----------------------+---------+---------+----------+
              |  Implied       |   BRK                 |    00   |    1    |    7     |
              +----------------+-----------------------+---------+---------+----------+
              1. A BRK command cannot be masked by setting I.
            */

            // Move to next instruction:
            PC.W++;
            PC.W++;

            // Push program counter on stack:
            Push(PC.H);
            Push(PC.L);

            // Set the break flag:
            SetFlag(FLAG_6502_BRK_COMMAND);

            // Push processor status on stack:
            Push(P);

            // Set the IRQ disable flag:
            SetFlag(FLAG_6502_IRQ_DISABLE);

            // The interrupt vector is located at 0xFFFE - 0xFFFF.
            // Fetch interrupt vector and jump:
            Address adr = new Address();
            adr.W = 0xFFFE;
            MemoryBus.SetAddress(adr);
            PC.L = MemoryBus.Read();
            adr.W = 0xFFFF;
            MemoryBus.SetAddress(adr);
            PC.H = MemoryBus.Read();
        }

        public void BVC(int AddressMode)
        {
            /*
              BVC                   BVC Branch on overflow clear                    BVC

            The overflow flag in the P status register is tested. If it is clear, a branch is taken; if it is set, the
            instruction immediately following the two-byte BVC instruction is executed.
            If the branch is taken, a one-byte signed displacement, fetched from the second byte of the instruction,
            is sign-extended to sixteen bits and added to the program counter. Once the branch address has been calculated,
            the result is loaded into the program counter, transferring control to that location.
            The allowable range of the displacement is - 128 to + 127 (from the instruction immediately following
            the branch).
            The overflow flag is altered by only four instructions on the 6502 and 65C02 - addition, subtraction, the
            CLV clear-the-flag instruction, and the BIT bit-testing instruction. In addition, all the flags are restored from
            the stack by the PLP and RTI instructions. On the 65802/65816, however, the SEP and REP instructions can
            also modify the v flag.
            BVC is used almost exclusively to check that a two's-complement arithmetic calculation has not
            overflowed, much as the carry is used to determine if an unsigned arithmetic calculation has overflowed. (Note,
            however, that the compare instructions do not affect the overflow flag.) You can also use BVC to test the
            second - highest bit in a value by using it after the BIT instruction, which moves the second - highest bit of the
            tested value into the v flag.
            The overflow flag can also be set by the Set Overflow hardware signal on the 6502, 65C02, and 65802;
            on many systems, however, there is no connection to this pin.
            Flags Affected: - - - - - - - -

              Operation:  Branch on V = 0                           N Z C I D V
                                                                    _ _ _ _ _ _
                                           (Ref: 4.1.1.8)
              +----------------+-----------------------+---------+---------+----------+
              | Addressing Mode| Assembly Language Form| OP CODE |No. Bytes|No. Cycles|
              +----------------+-----------------------+---------+---------+----------+
              |  Relative      |   BVC Oper            |    50   |    2    |    2*    |
              +----------------+-----------------------+---------+---------+----------+
              * Add 1 if branch occurs to same page.
              * Add 2 if branch occurs to different page.
            */

            // Point at next byte:
            PC.W++;

            // Test overflow flag:
            if (!IsSet(FLAG_6502_OVERFLOW))
            {
                // Fetch next byte:
                MemoryBus.SetAddress(PC);
                Address tmp = new Address();
                tmp.H = 0;
                tmp.L = MemoryBus.Read();

                // Prepare branch:
                if ((tmp.L & 0x80) == 0x00)
                {
                    PC += tmp;
                }
                else
                {
                    tmp.L = (byte)(0x00 - tmp.L);
                    PC -= tmp;
                }
            }

            // Move to next instruction:
            PC.W++;
        }

        public void BVS(int AddressMode)
        {
            /*
              BVS                    BVS Branch on overflow set                     BVS

            The overflow flag in the P status register is tested. If it is set, a branch is taken; if it is clear, the
            instruction immediately following the two-byte BVS instruction is executed.
            If the branch is taken, a one-byte signed displacement, fetched from the second byte of the instruction,
            is sign-extended to sixteen bits and added to the program counter. Once the branch address has been calculated,
            the result is loaded into the program counter, transferring control to that location.
            The allowable range of the displacement is - 128 to + 127 (from the instruction immediately following
            the branch).
            The overflow flag is altered by only four instructions on the 6502 and 65C02 - addition, subtraction, the
            CLV clear-the-flag instruction and the BIT bit-testing instructions. In addition, all the flags are restored from
            the stack by the PLP and RTI instruction. On the 65802/65816, the SEP and REP instructions can also modify
            the v flag.
            BVS is used almost exclusively to determine if a two's-complement arithmetic calculation has
            overflowed, much as the carry is used to determine if an unsigned arithmetic calculation has overflowed. (Note,
            however, that the compare instructions do not affect the overflow flag.) You can also use BVS to test the
            second-highest bit in a value by using it after the BLT instruction, which moves the second-highest bit of the
            tested value into the v flag.
            The overflow flag can also be set by the Set Overflow hardware signal on the 6502, 65C02, and 65802;
            on many systems, however, there is no hardware connection to this signal.
            Flags Affected: - - - - - - - -

              Operation:  Branch on V = 1                           N Z C I D V
                                                                    _ _ _ _ _ _
                                           (Ref: 4.1.1.7)
              +----------------+-----------------------+---------+---------+----------+
              | Addressing Mode| Assembly Language Form| OP CODE |No. Bytes|No. Cycles|
              +----------------+-----------------------+---------+---------+----------+
              |  Relative      |   BVS Oper            |    70   |    2    |    2*    |
              +----------------+-----------------------+---------+---------+----------+
              * Add 1 if branch occurs to same page.
              * Add 2 if branch occurs to different page.
            */

            // Point at next byte:
            PC.W++;

            // Test overflow flag:
            if (IsSet(FLAG_6502_OVERFLOW))
            {
                // Fetch next byte:
                MemoryBus.SetAddress(PC);
                Address tmp = new Address();
                tmp.H = 0;
                tmp.L = MemoryBus.Read();

                // Prepare branch:
                if ((tmp.L & 0x80) == 0x00)
                {
                    PC += tmp;
                }
                else
                {
                    tmp.L = (byte)(0x00 - tmp.L);
                    PC -= tmp;
                }
            }

            // Move to next instruction:
            PC.W++;
        }

        public void CLC(int AddressMode)
        {
            /*
              CLC                       CLC Clear carry flag                        CLC

            Clear the carry flag in the status register.
            CLC is used prior to addition (using the 65x's ADC instruction) to keep the carry flag from affecting
            the result; prior to a BCC (branch on carry clear) instruction on the 6502 to force a branch-always; and prior to
            an XCE (exchange carry flag with emulation bit) instruction to put the 65802 or 65816 into native mode.
            Flags Affected: - - - - - - - c
            c carry flag cleared always.

              Operation:  0 -> C                                    N Z C I D V
                                                                    _ _ 0 _ _ _
                                            (Ref: 3.0.2)
              +----------------+-----------------------+---------+---------+----------+
              | Addressing Mode| Assembly Language Form| OP CODE |No. Bytes|No. Cycles|
              +----------------+-----------------------+---------+---------+----------+
              |  Implied       |   CLC                 |    18   |    1    |    2     |
              +----------------+-----------------------+---------+---------+----------+
            */

            // Clear the flag:
            ResetFlag(FLAG_6502_CARRY);

            // Move to next instruction:
            PC.W++;
        }

        public void CLD(int AddressMode)
        {
            /*
              CLD                      CLD Clear decimal mode                       CLD

            Clear the decimal mode flag in the status register.
            CLD is used to shift 65x processors back into binary mode from decimal mode, so that the ADC and
            SBC instructions will correctly operate on binary rather than BCD data.
            Flags Affected: - - - - d - - -
            d decimal mode flag cleared always.

              Operation:  0 -> D                                    N A C I D V
                                                                    _ _ _ _ 0 _
                                            (Ref: 3.3.2)
              +----------------+-----------------------+---------+---------+----------+
              | Addressing Mode| Assembly Language Form| OP CODE |No. Bytes|No. Cycles|
              +----------------+-----------------------+---------+---------+----------+
              |  Implied       |   CLD                 |    D8   |    1    |    2     |
              +----------------+-----------------------+---------+---------+----------+
            */

            // Clear the flag:
            ResetFlag(FLAG_6502_DECIMAL_MODE);

            // Move to next instruction:
            PC.W++;
        }

        public void CLI(int AddressMode)
        {
            /*
              CLI                  CLI Clear interrupt disable bit                  CLI

            Clear the interrupt disable flag in the status register.
            CLI is used to re-enable hardware interrupt (IRQ) processing. (When the i bit is set, hardware
            interrupts are ignored.) The processor itself sets the i flag when it begins servicing an interrupt, so interrupt
            handling routines must re-enable interrupts with CLI if the interrupt-service routine is designed to service
            interrupts that occur while a previous interrupt is still being handled; otherwise, the RTI instruction will restore
            a clear i flag from the stack, and CLI is not necessary. CLI is also used to re-enable interrupts if they have
            been disabled during execution of time-critical or other code which cannot be interrupted.
            Flags Affected: - - - - - i - -
            i interrupt disable flag cleared always.

              Operation: 0 -> I                                     N Z C I D V
                                                                    _ _ _ 0 _ _
                                            (Ref: 3.2.2)
              +----------------+-----------------------+---------+---------+----------+
              | Addressing Mode| Assembly Language Form| OP CODE |No. Bytes|No. Cycles|
              +----------------+-----------------------+---------+---------+----------+
              |  Implied       |   CLI                 |    58   |    1    |    2     |
              +----------------+-----------------------+---------+---------+----------+
            */

            // Clear the flag:
            ResetFlag(FLAG_6502_IRQ_DISABLE);

            // Move to next instruction:
            PC.W++;
        }

        public void CLV(int AddressMode)
        {
            /*
              CLV                      CLV Clear overflow flag                      CLV

            Clear the overflow flag in the status register.
            CLV is sometimes used prior to a BVC (branch on overflow clear) to force a branch-always on the
            6502. Unlike the other clear flag instructions, there is no complementary "set flag" instruction to set the
            overflow flag, although the overflow flag can be set by hardware via the Set Overflow input pin on the
            processor. This signal, however, is often unconnected. The 65802/65816 REP instruction can, of course, clear
            the overflow flag; on the 6502 and 65C02, a BIT instruction with a mask in memory that has bit 6 set can be
            used to set the overflow flag.
            Flags Affected: - v - - - - - -
            v overflow flag cleared always.

              Operation: 0 -> V                                     N Z C I D V
                                                                    _ _ _ _ _ 0
                                            (Ref: 3.6.1)
              +----------------+-----------------------+---------+---------+----------+
              | Addressing Mode| Assembly Language Form| OP CODE |No. Bytes|No. Cycles|
              +----------------+-----------------------+---------+---------+----------+
              |  Implied       |   CLV                 |    B8   |    1    |    2     |
              +----------------+-----------------------+---------+---------+----------+
            */

            // Clear the flag:
            ResetFlag(FLAG_6502_OVERFLOW);

            // Move to next instruction:
            PC.W++;
        }

        public void CMP(int AddressMode)
        {
            /*
              CMP                CMP Compare memory and accumulator                 CMP

            Subtract the data located at the effective address specified by the operand from the contents of the
            accumulator, setting the carry, zero, and negative flags based on the result, but without altering the contents of
            either the memory location or the accumulator. That is, the result is not saved. The comparison is of unsigned
            binary values only.
            The CMP instruction differs from the SBC instruction in several ways. First, the result is not saved.
            Second, the value in the carry prior to the operation is irrelevant to the operation; that is, the carry does not have
            to be set prior to a compare as it is with 65x subtractions. Third, the compare instruction does not set the
            overflow flag, so it cannot be used for signed comparisons. Although decimal mode does not affect the CMP
            instruction, decimal comparisons are effective, since the equivalent binary values maintain the same magnitude
            relationships as the decimal values have, for example, $99 > $04 just as 99 > 4.
            The primary use for the compare instruction is to set the flags so that a conditional branch can then be
            executed.
            8-bit accumulator (all processors): Data compared is eight-bit.
            16-bit accumulator (65802/65816 only, m = 0): Data compared is sixteen-bit: the low-order eight bits
            of the data in memory are located at the effective address; the high-order eight bits are located at the effective
            address plus one.
            Flags Affective: n - - - - - z c
            n Set if most significant bit of result is set; else cleared.
            z Set if result is zero; else cleared.
            c Set if no borrow required (accumulator value higher or same);
            cleared if borrow required (accumulator value lower).

              Operation:  A - M                                     N Z C I D V
                                                                    / / / _ _ _
                                            (Ref: 4.2.1)
              +----------------+-----------------------+---------+---------+----------+
              | Addressing Mode| Assembly Language Form| OP CODE |No. Bytes|No. Cycles|
              +----------------+-----------------------+---------+---------+----------+
              |  Immediate     |   CMP #Oper           |    C9   |    2    |    2     |
              |  Zero Page     |   CMP Oper            |    C5   |    2    |    3     |
              |  Zero Page,X   |   CMP Oper,X          |    D5   |    2    |    4     |
              |  Absolute      |   CMP Oper            |    CD   |    3    |    4     |
              |  Absolute,X    |   CMP Oper,X          |    DD   |    3    |    4*    |
              |  Absolute,Y    |   CMP Oper,Y          |    D9   |    3    |    4*    |
              |  (Indirect,X)  |   CMP (Oper,X)        |    C1   |    2    |    6     |
              |  (Indirect),Y  |   CMP (Oper),Y        |    D1   |    2    |    5*    |
              +----------------+-----------------------+---------+---------+----------+
              * Add 1 if page boundary is crossed.

              In subtraction operations the carry flag is cleared - set to 0 - if a 
              borrow is required, set to 1 - if no borrow is required.
              CMP CPX CPY and SBC are subtraction operations. 

              When the subtraction result is 0 to 255, the carry is set. 
              When the subtraction result is less than 0, the carry is cleared. 


            Compare Accumulator with Memory CMP
            Subtract the data located at the effective address specified by the operand from the contents of the
            accumulator, setting the carry, zero, and negative flags based on the result, but without altering the contents of
            either the memory location or the accumulator. That is, the result is not saved. The comparison is of unsigned
            binary values only.
            The CMP instruction differs from the SBC instruction in several ways. First, the result is not saved.
            Second, the value in the carry prior to the operation is irrelevant to the operation; that is, the carry does not have
            to be set prior to a compare as it is with 65x subtractions. Third, the compare instruction does not set the
            overflow flag, so it cannot be used for signed comparisons. Although decimal mode does not affect the CMP
            instruction, decimal comparisons are effective, since the equivalent binary values maintain the same magnitude
            relationships as the decimal values have, for example, $99 > $04 just as 99 > 4.
            The primary use for the compare instruction is to set the flags so that a conditional branch can then be
            executed.
            8-bit accumulator (all processors): Data compared is eight-bit.
            Flags Affective: n — — — — — z c
            n Set if most significant bit of result is set; else cleared.
            z Set if result is zero; else cleared.
            c Set if no borrow required (accumulator value higher or same);
            cleared if borrow required (accumulator value lower).
            */

            // Get effective address in AddressInEffect:
            FetchAddress(AddressMode);

            // Fetch memory data:
            MemoryBus.SetAddress(AddressInEffect);
            byte M = MemoryBus.Read();

            // Carry will be reset if result is lower than 0x00:
            byte tNewCarry = (byte)(((int)A < (int)M) ? 0 : 1);

            // Do the subtract:
            byte tA = (byte)(A - M);

            // Set new carry flag:
            if (tNewCarry > 0)
            {
                SetFlag(FLAG_6502_CARRY);
            }
            else
            {
                ResetFlag(FLAG_6502_CARRY);
            }

            // Set negative flag:
            if ((0x80 & tA) > 0)
            {
                SetFlag(FLAG_6502_NEGATIVE);
            }
            else
            {
                ResetFlag(FLAG_6502_NEGATIVE);
            }

            // Set zero flag:
            if (tA == 0)
            {
                SetFlag(FLAG_6502_ZERO);
            }
            else
            {
                ResetFlag(FLAG_6502_ZERO);
            }

            // Move to next instruction:
            PC.W++;
        }

        public void CPX(int AddressMode)
        {
            /*
            Subtract the data located at the effective address specified by the operand from the contents of the X
            register, setting the carry, zero, and negative flags based on the result, but without altering the contents of either
            the memory location or the register. The result is not saved. The comparison is of unsigned values only (except
            for signed comparison for equality).
            The primary use for the CPX instruction is to test the value of the X index register against loop
            boundaries, setting the flags so that a conditional branch can be executed.
            8-bit index registers (all processors): Data compared is eight-bit.
            16-bit index registers (65802/65816 only, x = 0): Data compared is sixteen-bit: the low-order eight
            bits of the data in memory are located at the effective address; the high-order eight bits are located at the
            effective address plus one.
            Flags Affected: n - - - - - z c
            n Set if most significant bit of result is set; else cleared.
            z Set if result is zero; else cleared.
            c Set if no borrow required (X register value higher or same);
            cleared if borrow required (X register value lower).

              In subtraction operations the carry flag is cleared - set to 0 - if a 
              borrow is required, set to 1 - if no borrow is required.
              CMP CPX CPY and SBC are subtraction operations. 
            */

            // Get effective address in AddressInEffect:
            FetchAddress(AddressMode);

            // Fetch memory data:
            MemoryBus.SetAddress(AddressInEffect);
            byte M = MemoryBus.Read();

            // Carry will be reset if result is lower than 0x00:
            byte tNewCarry = (byte)(((int)X < (int)M) ? 0 : 1);

            // Do the subtract:
            byte tX = (byte)(X - M);

            // Set new carry flag:
            if (tNewCarry > 0)
            {
                SetFlag(FLAG_6502_CARRY);
            }
            else
            {
                ResetFlag(FLAG_6502_CARRY);
            }

            // Set negative flag:
            if ((0x80 & tX) > 0)
            {
                SetFlag(FLAG_6502_NEGATIVE);
            }
            else
            {
                ResetFlag(FLAG_6502_NEGATIVE);
            }

            // Set zero flag:
            if (tX == 0)
            {
                SetFlag(FLAG_6502_ZERO);
            }
            else
            {
                ResetFlag(FLAG_6502_ZERO);
            }

            // Move to next instruction:
            PC.W++;
        }

        public void CPY(int AddressMode)
        {
            /*
            Subtract the data located at the effective address specified by the operand from the contents of the Y
            register, setting the carry, zero, and negative flags based on the result, but without altering the contents of either
            the memory location or the register. The comparison is of unsigned values only (expect for signed comparison
            for equality).
            The primary use for the CPY instruction is to test the value of the Y index register against loop
            boundaries, setting the flags so that a conditional branch can be executed.
            8-bit index registers (all processors): Data compared is eight-bit.
            16-bit index registers (65802/65816 only, x = 0): Data compared is sixteen-bit: the low-order eight
            bits of the data in memory is located at the effective address; the high-order eight bits are located at the effective
            address plus one.
            Flags Affected: n - - - - - z c
            n Set if most significant bit of result is set; else cleared.
            z Set if result is zero; else cleared.
            c Set if no borrow required (Y register value higher or same);
            cleared if borrow required (Y register value lower).

              In subtraction operations the carry flag is cleared - set to 0 - if a 
              borrow is required, set to 1 - if no borrow is required.
              CMP CPX CPY and SBC are subtraction operations. 
            */

            // Get effective address in AddressInEffect:
            FetchAddress(AddressMode);

            // Fetch memory data:
            MemoryBus.SetAddress(AddressInEffect);
            byte M = MemoryBus.Read();

            // Carry will be RESET if result is lower than 0x00:
            byte tNewCarry = (byte)(((int)Y < (int)M) ? 0 : 1); // Todo: ??? Why int?

            // Do the subtract:
            byte tY = (byte)(Y - M);

            // Set new carry flag:
            if (tNewCarry > 0)
            {
                SetFlag(FLAG_6502_CARRY);
            }
            else
            {
                ResetFlag(FLAG_6502_CARRY);
            }

            // Set negative flag:
            if ((0x80 & tY) > 0)
            {
                SetFlag(FLAG_6502_NEGATIVE);
            }
            else
            {
                ResetFlag(FLAG_6502_NEGATIVE);
            }

            // Set zero flag:
            if (tY == 0)
            {
                SetFlag(FLAG_6502_ZERO);
            }
            else
            {
                ResetFlag(FLAG_6502_ZERO);
            }

            // Move to next instruction:
            PC.W++;
        }

        public void DEC(int AddressMode)
        {
            /*
            Decrement DEC
            Decrement by one the contents of the location specified by the operand (subtract one from the value).
            Unlike subtracting a one using the SBC instruction, the decrement instruction is neither affected by nor
            affected the carry flag. You can test for wraparound only by testing after every decrement to see if the value is
            zero or negative. On the other hand, you don't need to set the carry before decrementing.
            DEC is unaffected by the setting of the d (decimal) flag.
            8-bit accumulator/memory (all processors): Data decremented is eight-bit.
            16-bit accumulator/memory (65802/65816 only, m = 0): Data Decremented is sixteen-bit: if in
            memory, the low-order eight bits are located at the effective address; the high-order eight bits are located at the
            effective address plus one.
            Flags Affected: n - - - - - z -
            n Set if most significant bit of result is set; else cleared.
            z Set if result is zero; else cleared.
            */

            // Get effective address in AddressInEffect:
            FetchAddress(AddressMode);

            // Fetch memory data:
            MemoryBus.SetAddress(AddressInEffect);
            byte M = MemoryBus.Read();

            // Decrement:
            M--;

            // Store result:
            MemoryBus.Write(M);

            // Set flags affected.
            // Negative:
            if ((0x80 & M) > 0)
            {
                SetFlag(FLAG_6502_NEGATIVE);
            }
            else
            {
                ResetFlag(FLAG_6502_NEGATIVE);
            }

            // Zero:
            if (M == 0)
            {
                SetFlag(FLAG_6502_ZERO);
            }
            else
            {
                ResetFlag(FLAG_6502_ZERO);
            }

            // Move to next instruction:
            PC.W++;
        }

        public void DEX(int AddressMode)
        {
            /*
            Decrement Index Register X DEX
            Decrement by one the contents of index register X (subtract one from the value). This is a special
            purpose, implied addressing form of the DEC instruction.
            Unlike using SBC to subtract a one from the value, the DEX instruction does not affect the carry flag;
            you can test for wraparound only by testing after every decrement to see if the value is zero or negative. On the
            other hand, you don't need to set carry before decrementing.
            DEX is unaffected by the setting of the d (decimal) flag.
            8-bit index registers (all processors): Data decremented is eight-bit.
            16-bit index registers (65802/65816 only, x = 0): Data decremented is sixteen-bit.
            Flags Affected: n - - - - - z -
            n Set if most significant bit of result is set; else cleared.
            z Set if result is zero; else cleared.
            */

            // Decrement:
            X--;

            // Set flags affected.
            // Negative:
            if ((0x80 & X) > 0)
            {
                SetFlag(FLAG_6502_NEGATIVE);
            }
            else
            {
                ResetFlag(FLAG_6502_NEGATIVE);
            }

            // Zero:
            if (X == 0)
            {
                SetFlag(FLAG_6502_ZERO);
            }
            else
            {
                ResetFlag(FLAG_6502_ZERO);
            }

            // Move to next instruction:
            PC.W++;
        }

        public void DEY(int AddressMode)
        {
            /*
            Decrement Index Register Y DEY
            Decrement by one the contents of index register Y (subtract one from the value). This is a special
            purpose, implied addressing form of the DEC instruction.
            Unlike using SBC to subtract a one from the value, the DEY instruction does not affect the carry flag;
            you can test for wraparound only by testing after every decrement to see if the value is zero or negative. On the
            other hand, you don't need to set the carry before decrementing.
            DEY is unaffected by the setting of the d (decimal) flag.
            8-bit index registers (all processors): Data decremented is eight-bit.
            16-bit index registers (65802/65816 only, x = 0): Data decremented is sixteen-bit.
            Flags Affected: n - - - - - z -
            n Set if most significant bit of result is set; else cleared.
            z Set if result is zero; else clear.
            */

            // Decrement:
            Y--;

            // Set flags affected.
            // Negative:
            if ((0x80 & Y) > 0)
            {
                SetFlag(FLAG_6502_NEGATIVE);
            }
            else
            {
                ResetFlag(FLAG_6502_NEGATIVE);
            }

            // Zero:
            if (Y == 0)
            {
                SetFlag(FLAG_6502_ZERO);
            }
            else
            {
                ResetFlag(FLAG_6502_ZERO);
            }

            // Move to next instruction:
            PC.W++;
        }

        public void EOR(int AddressMode)
        {
            /*
            Exclusive-OR Accumulator with Memory EOR
            Bitwise logical Exclusive-OR the data located at the effective address specified by the operand with the
            contents of the accumulator. Each bit in the accumulator is exclusive-ORed with the corresponding bit in
            memory, and the result is stored into the same accumulator bit.
            The truth table for the logical exclusive-OR operation is:
            Second Operand
            0 1
            First Operand
            0 0 1
            1 1 0
            Figure 18-5Exclusive OR Truth Table
            A 1 or logical true results only if the two elements of the Exclusive-OR operation are different.
            8-bit accumulator (all processors): Data exclusive-ORed from memory is eight-bit.
            16-bit accumulator (65802/65816 only, m = 0): Data exclusive-ORed from memory is sixteen-bit: the
            low-order eight bits are located at the effective address; the high-order eight bits are located at the effective
            address plus one.
            Flags Affected: n - - - - - z -
            n Set if most significant bit of result is set; else cleared.
            z Set if result is zero; else cleared.
            */

            // Get effective address in AddressInEffect:
            FetchAddress(AddressMode);

            // Calculate the result:
            MemoryBus.SetAddress(AddressInEffect);
            A = (byte)(A ^ MemoryBus.Read());

            // Set flags affected.
            // Negative:
            if ((0x80 & A) > 0)
            {
                SetFlag(FLAG_6502_NEGATIVE);
            }
            else
            {
                ResetFlag(FLAG_6502_NEGATIVE);
            }

            // Zero:
            if (A == 0)
            {
                SetFlag(FLAG_6502_ZERO);
            }
            else
            {
                ResetFlag(FLAG_6502_ZERO);
            }

            // Move to next instruction:
            PC.W++;
        }

        public void INC(int AddressMode)
        {
            /*
            Increment INC
            Increment by one the contents of the location specified by the operand (add one to the value).
            Unlike adding a one with the ADC instruction, however, the increment instruction is neither affected by
            nor affects the carry flag. You can test for wraparound only by testing after every increment to see if the result
            is zero or positive. On the other hand, you don't have to clear the carry before incrementing.
            The INC instruction is unaffected by the d (decimal) flag.
            8-bit accumulator/memory (all processors): Data incremented is eight-bit.
            16-bit accumulator/memory (65802/65816 only, m=0): Data incremented is sixteen-bit: if in
            memory, the low-order eight bits are located at the effective address; the high-order eight-bits are located at the
            effective address plus one.
            Flags Affected: n - - - - - z -
            n Set if most significant bit of result is set; else cleared.
            z Set if result is zero; else cleared.
            */

            // Get effective address in AddressInEffect:
            FetchAddress(AddressMode);

            // Fetch memory data:
            MemoryBus.SetAddress(AddressInEffect);
            byte M = MemoryBus.Read();

            // Inrement:
            M++;

            // Store result:
            MemoryBus.Write(M);

            // Set flags affected.
            // Negative:
            if ((0x80 & M) > 0)
            {
                SetFlag(FLAG_6502_NEGATIVE);
            }
            else
            {
                ResetFlag(FLAG_6502_NEGATIVE);
            }

            // Zero:
            if (M == 0)
            {
                SetFlag(FLAG_6502_ZERO);
            }
            else
            {
                ResetFlag(FLAG_6502_ZERO);
            }

            // Move to next instruction:
            PC.W++;
        }

        public void INX(int AddressMode)
        {
            /*
            Increment Index Register X INX
            Increment by one the contents of index register X (add one to the value). This is a special purpose,
            implied addressing form of the INC instruction.
            Unlike using ADC to add a one to the value, the INX instruction does not affect the carry flag. You can
            execute it without first clearing the carry. But you can test for wraparound only by testing after every increment
            to see if the result is zero or positive. The INX instruction is unaffected by the d (decimal) flag.
            8-bit index registers (all processors): Data incremented is eight-bit.
            16-bit index registers (65802/65816 only, x = 0): Data incremented is sixteen-bit.
            Flags Affected: n - - - - - z -
            n Set if most significant bit of result is set; else cleared.
            z Set if result is zero; else cleared.
            */

            // Increment:
            X++;

            // Set flags affected.
            // Negative:
            if ((0x80 & X) > 0)
            {
                SetFlag(FLAG_6502_NEGATIVE);
            }
            else
            {
                ResetFlag(FLAG_6502_NEGATIVE);
            }

            // Zero:
            if (X == 0)
            {
                SetFlag(FLAG_6502_ZERO);
            }
            else
            {
                ResetFlag(FLAG_6502_ZERO);
            }

            // Move to next instruction:
            PC.W++;
        }

        public void INY(int AddressMode)
        {
            /*
            Increment Index Register Y INY
            Increment by one the contents of index register Y (add one to the value). This is a special purpose,
            implied addressing form of the INC instruction.
            Unlike using ADC to add one to the value, the INY instruction does not affect the carry flag. You can
            execute it without first clearing the carry. But you can test for wraparound only by testing after every increment
            to see if the value is zero or positive. The INY instruction is unaffected by the d (decimal) flag.
            8-bit index registers (all processors): Data incremented is eight-bit.
            16-bit index registers (65802/65816 only, x = 0): Data incremented is sixteen-bit.
            Flags Affected: n - - - - - z -
            n Set if most significant bit of result is set; else cleared.
            z Set if result is zero; else cleared.
            */

            // Increment:
            Y++;

            // Set flags affected.
            // Negative:
            if ((0x80 & Y) > 0)
            {
                SetFlag(FLAG_6502_NEGATIVE);
            }
            else
            {
                ResetFlag(FLAG_6502_NEGATIVE);
            }

            // Zero:
            if (Y == 0)
            {
                SetFlag(FLAG_6502_ZERO);
            }
            else
            {
                ResetFlag(FLAG_6502_ZERO);
            }

            // Move to next instruction:
            PC.W++;
        }

        public void JMP(int AddressMode)
        {
            /*
            Jump JMP
            Transfer control to the address specified by the operand field.
            The program counter is loaded with the target address. If a long JMP is executed, the program counter
            bank is loaded from the third byte of the target address specified by the operand.
            NOTE: 6502 has a 'bug' when performing an indirect jump. If low address is 0xFF the high address will
            NOT be incremented. 
            Flags Affected: - - - - - - - -
            */

            // Get effective address in AddressInEffect:
            FetchAddress(AddressMode);

            PC = new Address(AddressInEffect);
        }

        public void JSR(int AddressMode)
        {
            /*
            Jump to Subroutine JSR
            Transfer control to the subroutine at the location specified by the operand, after first pushing onto the
            stack, as a return address, the current program counter value, that is, the address of the last instruction byte (the
            third byte of a three-byte instruction, the fourth byte of a four-byte instruction), not the address of the next
            instruction.
            If an absolute operand is coded and is less than or equal to $FFFF, absolute addressing is assumed by
            the assembler; if the value is greater than $FFFF, absolute long addressing is used.
            If long addressing is used, the current program counter bank is pushed onto the stack first. Next - or
            first in the more normal case of intra-bank addressing - the high order byte of the return address is pushed,
            followed by the low order byte. This leaves it on the stack in standard 65x order (lowest byte at the lowest
            address, highest byte at the highest address). After the return address is pushed, the stack pointer points to the
            next available location (next lower byte) on the stack. Finally, the program counter (and, in the case of long
            addressing, the program counter bank register) is loaded with the values specified by the operand, and control is
            transferred to the target location.
            Flags Affected: - - - - - - - -
            */

            // Get effective address in AddressInEffect:
            FetchAddress(AddressMode);

            /*
                // Since we are to push last address onto the
                // stack, we must increment PC first:
                PC.W++;
            */
            // Push program counter on stack:
            Push(PC.H);
            Push(PC.L);

            // Set new PC:
            PC = new Address(AddressInEffect);
        }

        public void LDA(int AddressMode)
        {
            /*
            Load Accumulator from Memory LDA
            Load the accumulator with the data located at the effective address specified by the operand.
            8-bit accumulator (all processors): Data is eight-bit
            16-bit accumulator (65802/65816 only, m = 0): Data is sixteen-bit; the low-order eight bits are
            located at the effective address; the high-order eight bits are located at the effective address plus one.
            Flags Affected: n - - - - - z -
            n Set if most significant bit of loaded value is set; else cleared.
            z Set if value loaded is zero; else cleared.
            */

            // Get effective address in AddressInEffect:
            FetchAddress(AddressMode);

            // Calculate the result:
            MemoryBus.SetAddress(AddressInEffect);
            A = MemoryBus.Read();

            // Set flags affected.
            // Negative:
            if ((0x80 & A) > 0)
            {
                SetFlag(FLAG_6502_NEGATIVE);
            }
            else
            {
                ResetFlag(FLAG_6502_NEGATIVE);
            }

            // Zero:
            if (A == 0)
            {
                SetFlag(FLAG_6502_ZERO);
            }
            else
            {
                ResetFlag(FLAG_6502_ZERO);
            }

            // Move to next instruction:
            PC.W++;
        }

        public void LDX(int AddressMode)
        {
            /*
            Load Index Register X from Memory LDX
            Load index register X with the data located at the effective address specific by the operand.
            8-bit index registers (all processors): Data is eight-bit.
            16-bit index registers (65802/65816 only, x = 0): Data is sixteen-bit: the low-order eight bits are
            located at the effective address; the high-order eight bits are located at the effective address plus one.
            Flags Affected: n - - - - - z -
            n Set if most significant bit of loaded value is set; else cleared.
            z Set if value loaded is zero; else cleared.
            */

            // Get effective address in AddressInEffect:
            FetchAddress(AddressMode);

            // Calculate the result:
            MemoryBus.SetAddress(AddressInEffect);
            X = MemoryBus.Read();

            // Set flags affected.
            // Negative:
            if ((0x80 & X) > 0)
            {
                SetFlag(FLAG_6502_NEGATIVE);
            }
            else
            {
                ResetFlag(FLAG_6502_NEGATIVE);
            }

            // Zero:
            if (X == 0)
            {
                SetFlag(FLAG_6502_ZERO);
            }
            else
            {
                ResetFlag(FLAG_6502_ZERO);
            }

            // Move to next instruction:
            PC.W++;
        }

        public void LDY(int AddressMode)
        {
            /*
            Load Index Register Y from Memory LDY
            Load index register Y with the data located at the effective address specified by the operand.
            8-bit index registers (all processors): Data is eight-bit.
            16-bit index registers (65802/65816 only, x = 0): Data is sixteen-bit: the low-order eight bits are
            located at the effective address; the high-order eight bits are located at the effective address plus one.
            Flags Affected: n - - - - - z -
            n Set if most significant bit of loaded value is set; else cleared.
            z Set if value loaded is zero; else cleared.
            */

            // Get effective address in AddressInEffect:
            FetchAddress(AddressMode);

            // Calculate the result:
            MemoryBus.SetAddress(AddressInEffect);
            Y = MemoryBus.Read();

            // Set flags affected.
            // Negative:
            if ((0x80 & Y) > 0)
            {
                SetFlag(FLAG_6502_NEGATIVE);
            }
            else
            {
                ResetFlag(FLAG_6502_NEGATIVE);
            }

            // Zero:
            if (Y == 0)
            {
                SetFlag(FLAG_6502_ZERO);
            }
            else
            {
                ResetFlag(FLAG_6502_ZERO);
            }

            // Move to next instruction:
            PC.W++;
        }

        public void LSR(int AddressMode)
        {
            /*
            Logical Shift Memory or Accumulator Right LSR
            Logical shift the contents of the location specified by the operand right one bit. That is, bit zero takes
            on the value originally found in bit one, bit one takes the value originally found in bit two, and so on; the
            leftmost bit (bit 7 if the m memory select flag is one when the instruction is executed or bit 15 if it is zero) is
            cleared; the rightmost bit, bit zero, is transferred to the carry flag. This is the arithmetic equivalent of unsigned
            division by two.
            0 1 0 1 1 0 0 1 1
            X
            Carry Flag
            Figure 18-6 LSR
            8-bit accumulator/memory (all processors): Data shifted is eight-bit.
            16-bit accumulator/memory (65802/65816 only, m = 0): Data shifted is sixteen-bit: if in memory, the
            low-order eight bits are located at the effective address; the high-order eight bits are located at the effective
            address plus one.
            Flags Affected: n - - - - - z c
            n Cleared.
            z Set if result is zero; else cleared.
            c Low bit becomes carry: set if low bit was set; cleared if low bit was zero.
            */
            if (AddressMode == ADDRESS_MODE_6502_ACCUMULATOR)
            {
                // Store bit 1 for later evaluation:
                byte lsb = (byte)(0x01 & A);

                // Shift right:
                A = (byte)(A >> 1);

                // Set flags affected.
                // Carry:
                if (lsb > 0)
                {
                    SetFlag(FLAG_6502_CARRY);
                }
                else
                {
                    ResetFlag(FLAG_6502_CARRY);
                }

                // Negative:
                ResetFlag(FLAG_6502_NEGATIVE);

                // Zero:
                if (A == 0)
                {
                    SetFlag(FLAG_6502_ZERO);
                }
                else
                {
                    ResetFlag(FLAG_6502_ZERO);
                }
            }
            else
            {
                // Get effective address in AddressInEffect:
                FetchAddress(AddressMode);

                // Calculate the result:
                MemoryBus.SetAddress(AddressInEffect);
                byte M = MemoryBus.Read();

                // Store bit 7 (sign) for later evaluation:
                byte lsb = (byte)(0x01 & M);

                // Shift right:
                M = (byte)(M >> 1);

                // Store result:
                MemoryBus.Write(M);

                // Set flags affected.
                // Carry:
                if (lsb > 0)
                {
                    SetFlag(FLAG_6502_CARRY);
                }
                else
                {
                    ResetFlag(FLAG_6502_CARRY);
                }

                // Negative:
                ResetFlag(FLAG_6502_NEGATIVE);

                // Zero:
                if (M == 0)
                {
                    SetFlag(FLAG_6502_ZERO);
                }
                else
                {
                    ResetFlag(FLAG_6502_ZERO);
                }
            }

            // Move to next instruction:
            PC.W++;
        }

        public void NOP(int AddressMode)
        {
            /*
            No Operation NOP
            Executing a NOP takes no action; it has no effect on any 65x registers or memory, except the program
            counter, which is incremented once to point to the next instruction.
            Its primary uses are during debugging, where it is used to "patch out" unwanted code, or as a placeholder,
            included in the assembler source, where you anticipate you may have to "patch in" instructions, and
            want to leave a "hole" for the patch.
            NOP may also be used to expand timing loops - each NOP instruction takes two cycles to execute, so
            adding one or more may help fine tune a timing loop.
            Flags Affected: - - - - - - - -
            */

            // Move to next instruction:
            PC.W++;
        }

        public void ORA(int AddressMode)
        {
            /*
            OR Accumulator with Memory ORA
            Bitwise logical OR the data located at the effective address specified by the operand with the contents
            of the accumulator. Each bit in the accumulator is ORed with the corresponding bit in memory. The result is
            stored into the same accumulator bit.
            The truth table for the logical OR operation is:
            Second Operand
            0 1
            First Operand
            0 0 1
            1 1 1
            Figure 18-7 Logical OR Truth Table
            A 1 or logical true results if either of the two operands of the OR operation is true.
            8-bit accumulator (all processors): Data ORed from memory is eight-bit.
            16-bit accumulator (65802/65816 only, m=0): Data ORed from memory is sixteen-bit: the low-order
            eight bits are located at the effective address; the high-order eight bits are located at the effective address plus
            one.
            Flags Affected: n - - - - - z -
            n Set if most significant bit of result is set; else cleared.
            z Set if result is zero; else cleared.
            */

            // Get effective address in AddressInEffect:
            FetchAddress(AddressMode);

            // Calculate the result:
            MemoryBus.SetAddress(AddressInEffect);
            A = (byte)(A | MemoryBus.Read());

            // Set flags affected.
            // Negative:
            if ((0x80 & A) > 0)
            {
                SetFlag(FLAG_6502_NEGATIVE);
            }
            else
            {
                ResetFlag(FLAG_6502_NEGATIVE);
            }

            // Zero:
            if (A == 0)
            {
                SetFlag(FLAG_6502_ZERO);
            }
            else
            {
                ResetFlag(FLAG_6502_ZERO);
            }

            // Move to next instruction:
            PC.W++;
        }

        public void PHA(int AddressMode)
        {
            /*
            Push Accumulator PHA
            Push the accumulator onto the stack. The accumulator itself is unchanged.
            8-bit accumulator (all processors): The single byte contents of the accumulator are pushed -
            they are stored to the location pointed to by the stack pointer and the stack pointer is decremented.
            16-bit accumulator (65802/65816 only, m = 0): Both accumulator bytes are pushed. The
            high byte is pushed first, then the low byte. The stack point now points to the next available stack
            location, directly below the last byte pushed.
            Flags Affected: - - - - - - - -
            */

            // Push A to stack:
            Push(A);

            // Move to next instruction:
            PC.W++;
        }

        public void PHP(int AddressMode)
        {
            /*
            Push Processor Status Register PHP
            Push the contents of the processor status register P onto the stack.
            Since the status register is always an eight-bit register, this is always an eight-bit operation, regardless
            of the settings of the m and x mode select flags on the 65802/65816. The status register contents are not
            changed by the operation. The stack pointer now points to the next available stack location, directly below the
            byte pushed.
            This provides the means for saving either the current mode settings or a particular set of status flags so
            they may be restored or in some other way used later.
            Note, however, that the e bit (the 6502 emulation mode flag on the 65802/65816) is not pushed onto the
            stack or otherwise accessed or saved. The only access to the e flag is via the XCE instruction.
            Flags Affected: - - - - - - - -
            */

            // Push P to stack:
            Push(P);

            // Move to next instruction:
            PC.W++;
        }

        public void PLA(int AddressMode)
        {
            /*
            Pull Accumulator PLA
            Pull the value on the top of the stack into the accumulator. The previous contents of the accumulator
            are destroyed.
            8-bit accumulator (all processors): The stack pointer is first incremented. Then the byte pointed to
            by the stack pointer is loaded into the accumulator.
            16-bit accumulator (65802/65816 only, m = 0): Both accumulator bytes are pulled. The
            accumulator’s low byte is pulled first, then the high byte is pulled.
            Note that unlike some other microprocessors, the 65x pull instructions set the negative and zero flags.
            Flags Affected: n - - - - - z -
            n Set if most significant bit of pulled value is set; else cleared.
            z Set if value pulled is zero; else cleared.
            */

            // Pull A from stack:
            A = Pull();

            // Set flags affected.
            // Negative:
            if ((0x80 & A) > 0)
            {
                SetFlag(FLAG_6502_NEGATIVE);
            }
            else
            {
                ResetFlag(FLAG_6502_NEGATIVE);
            }

            // Zero:
            if (A == 0)
            {
                SetFlag(FLAG_6502_ZERO);
            }
            else
            {
                ResetFlag(FLAG_6502_ZERO);
            }

            // Move to next instruction:
            PC.W++;
        }

        public void PLP(int AddressMode)
        {
            /*
            Pull Status Flags PLP
            Pull the eight-bit value on the top of the stack into the processor status register P, switching the status
            byte to that value.
            Since the status register is an eight-bit register, only one byte is pulled from the stack, regardless of the
            settings of the m and x mode select flags on the 65802/65816. The stack pointer is first incremented. Then the
            byte pointed to by the stack pointer is loaded into the status register.
            This provides the means for restoring either previous mode settings or a particular set of status flags that
            reflect the result of a previous operation.
            Note, however, that the e flag-the 6502 emulation mode flag on the 65802/65816-is not on the stack so
            cannot be pulled from it. The only means of setting the e flag is the XCE instruction.
            Flags Affected: n v - b d i z c (6502, 65C02,
            65802/65816 emulation mode e=1)
            n v m x d i z c (65802/65816 native mode e=0)
            All flags are replaced by the values in the byte pulled from the stack.
            */

            // Pull P from stack:
            P = Pull();

            // Move to next instruction:
            PC.W++;
        }

        public void ROL(int AddressMode)
        {
            /*
            Rotate Memory or Accumulator Left ROL
            Rotate the contents of the location specified by the operand left one bit. Bit one takes on the
            value originally found in bit zero, bit two takes the value originally in bit one, and so on; the rightmost
            bit, bit zero, takes the value in the carry flag; the leftmost bit (bit 7 on the 6502 and 65C02 or if m = 1
            on the 65802/65816, or bit 15 if m = 0) is transferred into the carry flag.
            1 0 1 1 0 0 1 1
            X
            Carry Flag
            Figure 18-8 ROL
            8-bit accumulator/memory (all processors): Data rotated is eight bits, plus carry.
            16-bit accumulator/memory (65802/65816 only, m=0): Data rotated is sixteen bits, plus carry: if in
            memory, the low-order eight bits are located at the effective address; the high eight bits are located at the
            effective address plus one.
            Flags Affected: n - - - - - z c
            n Set if most significant bit of result is set; else cleared.
            z Set if result is zero; else cleared.
            c High bit becomes carry: set if high bit was set; cleared if high bit
            was clear.
            */
            if (AddressMode == ADDRESS_MODE_6502_ACCUMULATOR)
            {
                // Store bit msb for later evaluation:
                byte msb = (byte)(0x80 & A);

                // Shift left:
                A = (byte)(A << 1);

                // Insert carry at lsb:
                if (IsSet(FLAG_6502_CARRY))
                {
                    A = (byte)(A | 0x01);
                }

                // Set flags affected.
                // Carry:
                if (msb > 0)
                {
                    SetFlag(FLAG_6502_CARRY);
                }
                else
                {
                    ResetFlag(FLAG_6502_CARRY);
                }

                // Negative:
                if ((A & 0x80) > 0)
                {
                    SetFlag(FLAG_6502_NEGATIVE);
                }
                else
                {
                    ResetFlag(FLAG_6502_NEGATIVE);
                }

                // Zero:
                if (A == 0)
                {
                    SetFlag(FLAG_6502_ZERO);
                }
                else
                {
                    ResetFlag(FLAG_6502_ZERO);
                }
            }
            else
            {
                // Get effective address in AddressInEffect:
                FetchAddress(AddressMode);

                // Calculate the result:
                MemoryBus.SetAddress(AddressInEffect);
                byte M = MemoryBus.Read();

                // Store bit msb for later evaluation:
                byte msb = (byte)(0x80 & M);

                // Shift left:
                M = (byte)(M << 1);

                // Insert carry at lsb:
                if (IsSet(FLAG_6502_CARRY))
                {
                    M = (byte)(M | 0x01);
                }

                // Store result:
                MemoryBus.Write(M);

                // Set flags affected.
                // Carry:
                if (msb > 0)
                {
                    SetFlag(FLAG_6502_CARRY);
                }
                else
                {
                    ResetFlag(FLAG_6502_CARRY);
                }

                // Negative:
                if ((M & 0x80) > 0)
                {
                    SetFlag(FLAG_6502_NEGATIVE);
                }
                else
                {
                    ResetFlag(FLAG_6502_NEGATIVE);
                }

                // Zero:
                if (M == 0)
                {
                    SetFlag(FLAG_6502_ZERO);
                }
                else
                {
                    ResetFlag(FLAG_6502_ZERO);
                }
            }

            // Move to next instruction:
            PC.W++;
        }

        public void ROR(int AddressMode)
        {
            /*
            Rotate Memory or Accumulator Right ROR
            Rotate the contents of the location specified by the operand right one bit. Bit zero takes on the value
            originally found in bit one, bit one takes the value originally in bit two, and so on; the leftmost bit (bit 7 on the
            6502 and 65C02 or if m = 1 on the 65802/65816, or bit 15 if m = 0) takes the value in the carry flag; the
            rightmost bit, bit zero, is transferred into the carry flag.
            1 0 1 1 0 0 1 1
            X
            Figure 18-9 ROR
            8-bit accumulator/memory (all processors): Data rotated is eight bits, plus carry.
            16-bit accumulator/memory (65802/65816 only, m=0): Data rotated is sixteen bits, plus carry: if in
            memory, the low-order eight bits are located at the effective address; the high-order eight bits are located at the
            effective address plus one.
            Flags Affected: n - - - - - z c
            n Set if most significant bit of result is set; else cleared.
            z Set if result is zero; else cleared.
            c Low bit becomes carry: set if low bit was set; cleared if low
            bit was clear.
            */
            if (AddressMode == ADDRESS_MODE_6502_ACCUMULATOR)
            {
                // Store bit 0 for later evaluation:
                byte lsb = (byte)(0x01 & A);

                // Shift right:
                A = (byte)(A >> 1);

                // Insert carry at msb:
                if (IsSet(FLAG_6502_CARRY))
                {
                    A = (byte)(A | 0x80);
                }

                // Set flags affected.
                // Carry:
                if (lsb > 0)
                {
                    SetFlag(FLAG_6502_CARRY);
                }
                else
                {
                    ResetFlag(FLAG_6502_CARRY);
                }

                // Negative:
                if ((A & 0x80) > 0)
                {
                    SetFlag(FLAG_6502_NEGATIVE);
                }
                else
                {
                    ResetFlag(FLAG_6502_NEGATIVE);
                }

                // Zero:
                if (A == 0)
                {
                    SetFlag(FLAG_6502_ZERO);
                }
                else
                {
                    ResetFlag(FLAG_6502_ZERO);
                }
            }
            else
            {
                // Get effective address in AddressInEffect:
                FetchAddress(AddressMode);

                // Calculate the result:
                MemoryBus.SetAddress(AddressInEffect);
                byte M = MemoryBus.Read();

                // Store bit 0 for later evaluation:
                byte lsb = (byte)(0x01 & M);

                // Shift right:
                M = (byte)(M >> 1);

                // Insert carry at msb:
                if (IsSet(FLAG_6502_CARRY))
                {
                    M = (byte)(M | 0x80);
                }

                // Store result:
                MemoryBus.Write(M);

                // Set flags affected.
                // Carry:
                if (lsb > 0)
                {
                    SetFlag(FLAG_6502_CARRY);
                }
                else
                {
                    ResetFlag(FLAG_6502_CARRY);
                }

                // Negative:
                if ((M & 0x80) > 0)
                {
                    SetFlag(FLAG_6502_NEGATIVE);
                }
                else
                {
                    ResetFlag(FLAG_6502_NEGATIVE);
                }

                // Zero:
                if (M == 0)
                {
                    SetFlag(FLAG_6502_ZERO);
                }
                else
                {
                    ResetFlag(FLAG_6502_ZERO);
                }
            }

            // Move to next instruction:
            PC.W++;
        }

        public void RTI(int AddressMode)
        {
            /*
            Return from Interrupt RTI
            Pull the status register and the program counter from the stack. If the 65802/65816 is set to
            native mode (e = 0), also pull the program bank register from the stack.
            RTI pulls values off the stack in the reverse order they were pushed onto it by hardware or
            software interrupts. The RTI instruction, however, has no way of knowing whether the values pulled
            off the stack into the status register and the program counter are valid - or even, for that matter, that an
            interrupt has ever occurred. It blindly pulls the first three (or four) bytes off the top of the stack and
            stores them into the various registers.
            Unlike the RTS instruction, the program counter address pulled off the stack is the exact
            address to return to; the value on the stack is the value loaded into the program counter. It does not
            need to be incremented as a subroutine's return address does.
            Pulling the status register gives the status flags the values they had immediately prior to the
            start of interrupt-processing.
            One extra byte is pulled in the 65802/65816 native mode than in emulation mode, the same
            extra byte that is pushed by interrupts in native mode, the program bank register. It is therefore
            essential that the return from interrupt be executed in the same mode (emulation or native) as the
            original interrupt.
            6502, 65C02, and Emulation Mode (e = 1): The status register is pulled from the stack, then
            the program counter is pulled from the stack (three bytes are pulled).
            65802/65816 Native Mode (e = 0): The status register is pulled from the stack, then the
            program counter is pulled from the stack, then the program bank register is pulled from the stack (four
            bytes are pulled).
            Stack
            (Stack Pointer After) Old Status Register
            Return Address Bank
            Return Address High
            Return Address Low
            Stack Pointer Before
            Bank 0
            Figure 18-10Native Mode Stack before RTI.
            Flags Affected: n v - - d i z c (6502, 65C02,
            65802/65816 emulation mode e = 1)
            n v m x d i z c (65802/65816 native mode e = 0)
            All flags are restored to their values prior to interrupt (each flag takes
            the value of its corresponding bit in the stacked status byte, except that
            the Break flag is ignored).
            */

            // Fetch processor status from stack:
            // HBE Should we keep the break flag?
            P = Pull();

            // Fetch program counter from stack:
            PC.L = Pull();
            PC.H = Pull();
        }

        public void RTS(int AddressMode)
        {
            /*
            Return from Subroutine RTS
            Pull the program counter, incrementing the stacked, sixteen-bit value by one before loading the
            program counter with it.
            When a subroutine is called (via a jump to subroutine instruction), the current return address is
            pushed onto the stack. To return to the code following the subroutine call, a return instruction must be
            executed, which pulls the return address from the stack, increments it, and loads the program counter
            with it, transferring control to the instruction immediately following the jump to subroutine.
            Stack
            (Stack Pointer After) Return Address High
            Return Address Low
            Stack Pointer Before
            Bank 0
            Figure 18-12 Stack before RTS
            Flags Affected: - - - - - - - -
            */

            // Fetch program counter from stack:
            PC.L = Pull();
            PC.H = Pull();
            PC.W++;
        }

        public void SBC(int AddressMode)
        {
            /*
              In subtraction operations the carry flag is cleared - set to 0 - if a 
              borrow is required, set to 1 - if no borrow is required.
              CMP CPX CPY and SBC are subtraction operations. 

            Subtract with Borrow from Accumulator SBC
            Subtract the data located at the effective address specified by the operand from the contents of the
            accumulator; subtract one more if the carry flag is clear, and store the result in the accumulator.
            The 65x processors have no subtract instruction that does not involve the carry. To avoid subtracting
            the carry flag from the result, either you must be sure it is set or you must explicitly set it (using SEC) prior to
            executing the SBC instruction.
            In a multi-precision (multi-word) subtract, you set the carry before the low words are subtracted. The
            low word subtraction generates a new carry flag value based on the subtraction. The carry is set if no borrow
            was required and cleared if borrow was required. The complement of the new carry flag (one if the carry is
            clear) is subtracted during the next subtraction, and so on. Each result thus correctly reflects the borrow from
            the previous subtraction.
            Note that this use of the carry flag is the opposite of the way the borrow flag is used by some other
            processors, which clear (not set) the carry if no borrow was required.
            d flag clear: Binary subtraction is performed.
            d flag set: Binary coded decimal (BCD) subtraction is performed.
            8-bit accumulator (all processors): Data subtracted from memory is eight-bit.
            16-bit accumulator (65802/65816 only, m=0): Data subtracted from memory is sixteen-bit: the low
            eight bits is located at the effective address; the high eight bits is located at the effective address plus one.
            Flags Affected: n v - - - - z c
            n Set if most significant bit of result is set; else cleared.
            v Set if signed overflow; cleared if valid sign result.
            z Set if result is zero; else cleared.
            c Set if unsigned borrow not required; cleared if unsigned borrow.
            */

            // Get effective address in AddressInEffect:
            FetchAddress(AddressMode);

            // Store bit 7 (sign) for later evaluation:
            //	byte sign = 0x80 & A;

            // Calculate the address:
            MemoryBus.SetAddress(AddressInEffect);

            // Read effective data:
            byte M = MemoryBus.Read();

            //Här måste vi ta hänsyn till decimal mode!
            if (IsSet(FLAG_6502_DECIMAL_MODE))
            {
                // Decimal subtract operation.
                // Split operands into two nibbles, and use as 
                // two digits. A borrow is subtracted from the least
                // significant nibble (lsn). A borrow from lsn 
                // may occur, and is subtracted to the msn. If a
                // borrow results from adding msn:s, set carry
                // flag, else reset it. Note, a nibble may only
                // contain values from 0 to 9, and cannot be negative.

                // Note: the overflow flag is erratic. It is set when
                // bit 7 changes from 1 to 0 when subtracting a positive
                // number (tested with 1).
                // Therefore, save bit 7 for later test:
                byte msb = (byte)(A & 0x80);

                // Overflow will be set as if we did a normal subtract:
                byte tCarry = (byte)((P & FLAG_6502_CARRY) > 0 ? 0 : 1);
                byte tNewOverflow = (byte)((A - M - tCarry < (byte)0x80) ? 1 : 0);

                // Split operands into nibbles:
                byte AL = (byte)(A & 0x0F);
                byte AH = (byte)((A & 0xF0) >> 4);
                byte ML = (byte)(M & 0x0F);
                byte MH = (byte)((M & 0xF0) >> 4);

                // Subtract lsn.
                // Do we need to borrow?
                bool borrow = false;
                if (IsSet(FLAG_6502_CARRY))
                {
                    // Carry on means no borrow!
                    // Only if ML > AL we will need to borrow:
                    if (ML > AL)
                    {
                        borrow = true;
                    }
                }
                else
                {
                    // If ML >= AL we will need to borrow.
                    // (If equal, the borrow would make it 
                    // negative!
                    if (ML >= AL)
                    {
                        borrow = true;
                    }
                }
                if (borrow)
                {
                    AL += 10;
                    AH -= 1;
                }

                // Do the subtraction:
                AL -= ML;

                // And aslo decrement if no carry flag:
                if (!IsSet(FLAG_6502_CARRY))
                {
                    AL--;
                }

                // Subtract msn:
                // Do we need to borrow?
                if (MH > AH)
                {
                    // Reset carry flag to indicate a borrow:
                    ResetFlag(FLAG_6502_CARRY);

                    // And do the actual borrow:
                    AH += 10;
                }
                else
                {
                    // Set carry flag:
                    SetFlag(FLAG_6502_CARRY);
                }

                // Do the actual subtract:
                AH -= MH;

                // Assemble resulting nibbles into A;
                A = (byte)((AH << 4) | AL);
                /*
                        // Set overflow flag:
                        if(tNewOverflow)
                        {
                            SetFlag(FLAG_6502_OVERFLOW);
                        }
                        else
                        {
                            ResetFlag(FLAG_6502_OVERFLOW);
                        }
                */
            }
            else
            {
                // Normal subtract operation:
                // Fetch current carry:
                byte tCarry = (byte)(!IsSet(FLAG_6502_CARRY) ? 0x01 : 0x00);

                // Overflow will be set if result is lower than -128:
                //		byte tNewOverflow = (A - M - tCarry < -128)?1:0;
                // Overflow will be set if result is utside range -128 - +127:
                byte tNewOverflow = (byte)(((byte)A - (byte)M - tCarry > (int)127) || ((byte)A - (byte)M - tCarry < (int)-128) ? 1 : 0);

                // Carry will be RESET if result is lower than 0x00:
                //		byte tNewCarry = (A - M - tCarry < (byte)0x00)?0:1;
                // Carry will be RESET if result is utside range 0 - 255:
                byte tNewCarry = (byte)(((int)A - (int)M - tCarry > (int)255) || ((int)A - (int)M - tCarry < (int)0) ? 0 : 1);

                // Do the subtract:
                A = (byte)(A - M - tCarry);

                // Set new carry flag:
                if (tNewCarry > 0)
                {
                    SetFlag(FLAG_6502_CARRY);
                }
                else
                {
                    ResetFlag(FLAG_6502_CARRY);
                }

                // Set new overflow flag:
                if (tNewOverflow > 0)
                {
                    SetFlag(FLAG_6502_OVERFLOW);
                }
                else
                {
                    ResetFlag(FLAG_6502_OVERFLOW);
                }

                // Negative:
                if (!IsSet(FLAG_6502_DECIMAL_MODE) && ((0x80 & A) > 0))
                //if(!(P & FLAG_6502_DECIMAL_MODE) && (0x80 & A))
                {
                    SetFlag(FLAG_6502_NEGATIVE);
                }
                else
                {
                    ResetFlag(FLAG_6502_NEGATIVE);
                }

                // Zero:
                if (A == 0)
                {
                    SetFlag(FLAG_6502_ZERO);
                }
                else
                {
                    ResetFlag(FLAG_6502_ZERO);
                }
            }

            // Move to next instruction:
            PC.W++;
        }

        public void SEC(int AddressMode)
        {
            /*
            Set Carry Flag SEC
            Set the carry flag in the status register.
            SEC is used prior to subtraction (using the 65x's SBC instruction) to keep the carry flag from affecting
            the result, and prior to an XCE (exchange carry flag with emulation bit) instruction to put the 65802 or 65816
            into 6502 emulation mode.
            Flags Affected: - - - - - - - c
            c Carry flag set always.
            */

            // Set the flag:
            SetFlag(FLAG_6502_CARRY);

            // Move to next instruction:
            PC.W++;
        }

        public void SED(int AddressMode)
        {
            /*
            Set Decimal Mode Flag SED
            Set the decimal mode flag in the status register.
            SED is used to shift 65x processors into decimal mode from binary mode, so that the ADC and SBC
            instructions will operate correctly on the BCD data, performing automatic decimal adjustment.
            Flags Affected: - - - - d - - -
            d Decimal mode flag set always.
            */

            // Set the flag:
            SetFlag(FLAG_6502_DECIMAL_MODE);

            // Move to next instruction:
            PC.W++;
        }

        public void SEI(int AddressMode)
        {
            /*
            Set Interrupt Disable Flag SEI
            Set the interrupt disable flag in the status register.
            SEI is used to disable hardware interrupt processing. When the i bit is set, maskable hardware
            interrupts (IRQ') are ignored. The processor itself sets the i flag when it begins servicing an interrupt, so
            interrupt handling routines that are intended to be interruptable must reenable interrupts with CLI. If interrupts
            are to remain blocked during the interrupt service, exiting the routine via RTI will automatically restore the
            status register with the i flag clear, re-enabling interrupts.
            Flags Affected: - - - - - i - -
            i Interrupt disable flag set always.
            */

            // Set the flag:
            SetFlag(FLAG_6502_IRQ_DISABLE);

            // Move to next instruction:
            PC.W++;
        }

        public void STA(int AddressMode)
        {
            /*
            Store Accumulator to Memory STA
            Store the value in the accumulator to the effective address specified by the operand.
            8-bit accumulator (all processors): Value is eight-bit.
            16-bit accumulator (65802/65816 only, m=0): Value is sixteen-bit: the low-order eight bits are stored
            to the effective address; the high-order eight bits are stored to the effective address plus one.
            The 65x flags are unaffected by store instructions.
            Flags Affected: - - - - - - - -
            */

            // Get effective address in AddressInEffect:
            FetchAddress(AddressMode);

            // Store A at address:
            MemoryBus.SetAddress(AddressInEffect);
            MemoryBus.Write(A);

            // Move to next instruction:
            PC.W++;
        }

        public void STX(int AddressMode)
        {
            /*
            Store Index Register X to Memory STX
            Store the value in index register X to the effective address specified by the operand.
            8-bit index registers (all processors): Value is eight-bit.
            16-bit index registers (65802/65816 only, x = 0): Value is sixteen-bit: the low-order eight bits are
            stored to the effective address; the high-order eight bits are stored to the effective address plus one.
            The 65x flags are unaffected by store instructions.
            Flags Affected: - - - - - - - -
            */

            // Get effective address in AddressInEffect:
            FetchAddress(AddressMode);

            // Store A at address:
            MemoryBus.SetAddress(AddressInEffect);
            MemoryBus.Write(X);

            // Move to next instruction:
            PC.W++;
        }

        public void STY(int AddressMode)
        {
            /*
            Store Index Register Y to Memory STY
            Store the value in index register Y to the effective address specified by the operand.
            8-bit index registers (all processors): Value is eight-bit.
            16-bit index registers (65802/65816 only, x = 0): Value is sixteen-bit: the low-order eight bits are
            stored to the effective address; the high-order eight bits are stored to the effective address plus one.
            The 65x flags are unaffected by store instructions.
            Flags Affected: - - - - - - - -
            */

            // Get effective address in AddressInEffect:
            FetchAddress(AddressMode);

            // Store A at address:
            MemoryBus.SetAddress(AddressInEffect);
            MemoryBus.Write(Y);

            // Move to next instruction:
            PC.W++;
        }

        public void TAX(int AddressMode)
        {
            /*
            Transfer Accumulator to Index Register X TAX
            Transfer the value in the accumulator to index register X. If the registers are different sizes, the nature
            of the transfer is determined by the destination register. The value in the accumulator is not changed by the
            operation.
            8-bit accumulator, 8-bit index registers (all processors): Value transferred is eight-bit.
            8-bit accumulator, 16-bit index registers (65802/65816 only, m = 1, x = 0): Value transferred is
            sixteen-bit; the eight-bit A accumulator becomes the low byte of the index register; the hidden eight-bit B
            accumulator becomes the high byte of the index register.
            16-bit accumulator, 8-bit index registers (65802/65816 only, m=0, x=1): Value transferred to the
            eight-bit index register is eight-bit, the low byte of the accumulator.
            16-bit accumulator, 16-bit index registers (65802/65816 only, m=0, x=0): Value transferred to the
            sixteen-bit index register is sixteen-bit, the full sixteen-bit accumulator.
            Flags Affected: n - - - - - z -
            n Set if most significant bit of transferred value is set; else cleared.
            z Set if value transferred is zero; else cleared.
            */

            // Transfer A to X:
            X = A;

            // Negative:
            if ((0x80 & X) > 0)
            {
                SetFlag(FLAG_6502_NEGATIVE);
            }
            else
            {
                ResetFlag(FLAG_6502_NEGATIVE);
            }

            // Zero:
            if (X == 0)
            {
                SetFlag(FLAG_6502_ZERO);
            }
            else
            {
                ResetFlag(FLAG_6502_ZERO);
            }

            // Move to next instruction:
            PC.W++;
        }

        public void TAY(int AddressMode)
        {
            /*
            Transfer Accumulator to Index Register Y TAY
            Transfer the value in the accumulator to index register Y. If the registers are different sizes, the nature
            of the transfer is determined by the destination register. The value in the accumulator is not changed by the
            operation.
            8-bit accumulator, 8-bit index registers (all processors): Value transferred is eight-bit.
            8-bit accumulator, 16-bit index registers (65802/65816 only, m = 1, x = 0): Value transferred is
            sixteen-bit; the eight-bit A accumulator becomes the low byte of the index register; the hidden eight-bit B
            accumulator becomes the high byte of the index register.
            16-bit accumulator, 8-bit index registers (65802/65816 only, m=0, x=1): Value transferred to the
            eight-bit index register is eight-bit, the low byte of the accumulator.
            16-bit accumulator, 16-bit index registers (65802/65816 only, m=0, x=0): Value transferred to the
            sixteen-bit index register is sixteen-bit, the full sixteen-bit accumulator.
            Flags Affected: n - - - - - z -
            n Set if most significant bit of transferred value is set; else cleared.
            z Set if value transferred is zero; else cleared.
            */

            // Transfer A to Y:
            Y = A;

            // Negative:
            if ((0x80 & Y) > 0)
            {
                SetFlag(FLAG_6502_NEGATIVE);
            }
            else
            {
                ResetFlag(FLAG_6502_NEGATIVE);
            }

            // Zero:
            if (Y == 0)
            {
                SetFlag(FLAG_6502_ZERO);
            }
            else
            {
                ResetFlag(FLAG_6502_ZERO);
            }

            // Move to next instruction:
            PC.W++;
        }

        public void TSX(int AddressMode)
        {
            /*
            Transfer Stack Pointer to Index Register X TSX
            Transfer the value in the stack pointer S to index register X. The stack pointer's value is not changed
            by the operation.
            8-bit index registers (all processors): Only the low byte of the value in the stack pointer is transferred
            to the X register. In the 6502, the 65C02, and the 6502 emulation mode, the stack pointer and the index
            registers are only a single byte each, so the byte in the stack pointer is transferred to the eight-bit X register. In
            65802/65816 native mode, the stack pointer is sixteen bits, so its most significant byte is not transferred if the
            index registers are in eight-bit mode.
            16-bit index registers (65802/65816 only, x=0): The full sixteen-bit value in the stack pointer is
            transferred to the X register.
            Flags Affected: n - - - - - z -
            n Set if most significant bit of transferred value is set; else cleared.
            z Set if value transferred is zero; else cleared.
            */

            // Transfer S to X:
            X = S;

            // Negative:
            if ((0x80 & X) > 0)
            {
                SetFlag(FLAG_6502_NEGATIVE);
            }
            else
            {
                ResetFlag(FLAG_6502_NEGATIVE);
            }

            // Zero:
            if (X == 0)
            {
                SetFlag(FLAG_6502_ZERO);
            }
            else
            {
                ResetFlag(FLAG_6502_ZERO);
            }

            // Move to next instruction:
            PC.W++;
        }

        public void TXA(int AddressMode)
        {
            /*
            Transfer Index Register X to Accumulator TXA
            Transfer the value in index register X to the accumulator. If the registers are different sizes, the nature
            of the transfer is determined by the destination (the accumulator). The value in the index register is not changed
            by the operation.
            8-bit index registers, 8-bit accumulator (all processors): Value transferred is eight-bit.
            16-bit index registers, 8-bit accumulator (65802/65816 only, x=0, m=1): Value transferred to the
            eight-bit accumulator is eight-bit, the low byte of the index register; the hidden eight-bit accumulator B is not
            affected by the transfer.
            8-bit index registers, 16-bit accumulator (65802/65816 only, x=1, m=0): The eight-bit index register
            becomes of the low byte of the accumulator; the high accumulator byte is zeroed.
            16-bit index registers, 16-bit accumulator (65802/65816 only, x=0, m=0): Value transferred to the
            sixteen-bit accumulator is sixteen-bit, the full sixteen-bit index register.
            Flags Affected: n - - - - - z -
            n Set if most significant bit of transferred value is set; else cleared.
            z Set if value transferred is zero; else cleared.
            */

            // Transfer X to A:
            A = X;

            // Negative:
            if ((0x80 & A) > 0)
            {
                SetFlag(FLAG_6502_NEGATIVE);
            }
            else
            {
                ResetFlag(FLAG_6502_NEGATIVE);
            }

            // Zero:
            if (A == 0)
            {
                SetFlag(FLAG_6502_ZERO);
            }
            else
            {
                ResetFlag(FLAG_6502_ZERO);
            }

            // Move to next instruction:
            PC.W++;
        }

        public void TXS(int AddressMode)
        {
            /*
            Transfer Index Register X to Stack Pointer TXS
            Transfer the value in index register X to the stack pointer, S. The index register's value is not changed
            by the operation.
            TXS, along with TCS, are the only two instructions for changing the value in the stack pointer. The
            two are also the only two transfer instructions that do not alter the flags.
            6502, 65C02, and 6502 emulation mode (65802/65816, e=1): The stack pointer is only eight bits (it is
            concatenated to a high byte of one, confining the stack to page one), and the index registers are only eight bits.
            The byte in X is transferred to the eight-bit stack pointer.
            8-bit index registers (65802/65816 native mode, x=1): The stack pointer is sixteen bits but the index
            registers are only eight bits. A copy of the byte in X is transferred to the low stack pointer byte and the high
            stack pointer byte is zeroed.
            16-bit index registers (65802/65816 native mode, x=0): The full sixteen-bit value in X is transferred
            to the sixteen-bit stack pointer.
            Flags Affected: - - - - - - - -
            */

            // Transfer X to S:
            S = X;

            // Move to next instruction:
            PC.W++;
        }

        public void TYA(int AddressMode)
        {
            /*
            Transfer Index Register Y to Accumulator TYA
            Transfer the value in index register Y to the accumulator. If the registers are different sizes, the nature
            of the transfer is determined by the destination (the accumulator). The value in the index register is not changed
            by the operation.
            8-bit index registers, 8-bit accumulator (all processors): Value transferred is eight-bit.
            16-bit index registers, 8-bit accumulator (65802/65816 only, x=0, m=1): Value transferred to the
            eight-bit accumulator is eight-bit, the low byte of the index register; the hidden eight-bit accumulator B is not
            affected by the transfer.
            8-bit index registers, 16-bit accumulator (65802/65816 only, x=1, m=0): The eight-bit index register
            becomes of the low byte of the accumulator; the high accumulator byte is zeroed.
            16-bit index registers, 16-bit accumulator (65802/65816 only, x=0, m=0): Value transferred to the
            sixteen-bit accumulator is sixteen-bit, the full sixteen-bit index register.
            Flags Affected: n - - - - - z -
            n Set if most significant bit of transferred value is set; else cleared.
            z Set if value transferred is zero; else cleared.
            */

            // Transfer Y to A:
            A = Y;

            // Negative:
            if ((0x80 & A) > 0)
            {
                SetFlag(FLAG_6502_NEGATIVE);
            }
            else
            {
                ResetFlag(FLAG_6502_NEGATIVE);
            }

            // Zero:
            if (A == 0)
            {
                SetFlag(FLAG_6502_ZERO);
            }
            else
            {
                ResetFlag(FLAG_6502_ZERO);
            }

            // Move to next instruction:
            PC.W++;
        }

        //////////////////////////////////////////////////////////////////////////
        // Addressing modes 
        //////////////////////////////////////////////////////////////////////////

        public void FetchAddress(int AddressMode)
        {
            switch (AddressMode)
            {
                case ADDRESS_MODE_6502_ABSOLUTE:
                    Fetch_ABSOLUTE();
                    break;
                case ADDRESS_MODE_6502_ZERO_PAGE:
                    Fetch_ZERO_PAGE();
                    break;
                case ADDRESS_MODE_6502_IMMEDIATE:
                    Fetch_IMMEDIATE();
                    break;
                case ADDRESS_MODE_6502_ABS_X:
                    Fetch_ABS_X();
                    break;
                case ADDRESS_MODE_6502_ABS_Y:
                    Fetch_ABS_Y();
                    break;
                case ADDRESS_MODE_6502_IND_X:
                    Fetch_IND_X();
                    break;
                case ADDRESS_MODE_6502_IND_Y:
                    Fetch_IND_Y();
                    break;
                case ADDRESS_MODE_6502_Z_PAGE_X:
                    Fetch_Z_PAGE_X();
                    break;
                case ADDRESS_MODE_6502_RELATIVE:
                    Fetch_RELATIVE();
                    break;
                case ADDRESS_MODE_6502_INDIRECT:
                    Fetch_INDIRECT();
                    break;
                case ADDRESS_MODE_6502_Z_PAGE_Y:
                    Fetch_Z_PAGE_Y();
                    break;
            }
        }

        public void Fetch_ABSOLUTE()
        {//
            PC.W++;
            MemoryBus.SetAddress(PC);
            AddressInEffect.L = MemoryBus.Read();
            PC.W++;
            MemoryBus.SetAddress(PC);
            AddressInEffect.H = MemoryBus.Read();

            //if (DebugEnabled) CreateDebugBytes(2);
            //if (DebugEnabled) SetDebugByte(0, AddressInEffect.L);
            //if (DebugEnabled) SetDebugByte(0, AddressInEffect.H);
        }

        public void Fetch_ZERO_PAGE()
        {//
            PC.W++;
            MemoryBus.SetAddress(PC);
            AddressInEffect.L = MemoryBus.Read();
            AddressInEffect.H = 0x00;

            //if (DebugEnabled) CreateDebugBytes(1);
            //if (DebugEnabled) SetDebugByte(0, AddressInEffect.L);
        }

        public void Fetch_IMMEDIATE()
        {//
            PC.W++;
            AddressInEffect = new Address(PC);

            //if (DebugEnabled) CreateDebugBytes(1);
            //if (DebugEnabled) SetDebugByte(0, MemoryBus.Read());
        }

        public void Fetch_ABS_X()
        {//
            PC.W++;
            MemoryBus.SetAddress(PC);
            AddressInEffect.L = MemoryBus.Read();
            PC.W++;
            MemoryBus.SetAddress(PC);
            AddressInEffect.H = MemoryBus.Read();
            AddressInEffect.W += X;
            //	AddressInEffect.L += X;

            //if (DebugEnabled) CreateDebugBytes(2);
            //if (DebugEnabled) SetDebugByte(0, AddressInEffect.L);
            //if (DebugEnabled) SetDebugByte(0, AddressInEffect.H);
        }

        public void Fetch_ABS_Y()
        {//
            PC.W++;
            MemoryBus.SetAddress(PC);
            AddressInEffect.L = MemoryBus.Read();
            PC.W++;
            MemoryBus.SetAddress(PC);
            AddressInEffect.H = MemoryBus.Read();
            AddressInEffect.W += Y;
            //	AddressInEffect.L += Y;

            //if (DebugEnabled) CreateDebugBytes(2);
            //if (DebugEnabled) SetDebugByte(0, AddressInEffect.L);
            //if (DebugEnabled) SetDebugByte(0, AddressInEffect.H);
        }

        public void Fetch_IND_X()
        {//
         // Fetch address in zero page, as indicated by immediate 
         // byte, and store in a temporary address, incremented by
         // the offset in X, with wrap-around:
            Address temp = new Address();
            PC.W++;
            MemoryBus.SetAddress(PC);
            temp.L = (byte)(X + MemoryBus.Read());
            temp.H = 0x00;

            //if (DebugEnabled) CreateDebugBytes(1);
            //if (DebugEnabled) SetDebugByte(0, MemoryBus.Read());

            // Fetch address in effect from the data in zero page as
            // appointed by the temporary address:
            MemoryBus.SetAddress(temp);
            AddressInEffect.L = MemoryBus.Read();
            temp.L++;
            MemoryBus.SetAddress(temp);
            AddressInEffect.H = MemoryBus.Read();
        }

        public void Fetch_IND_Y()
        {//
         // Fetch address in zero page, as indicated by immediate 
         // byte, and store in a temporary address:
            Address temp = new Address();
            PC.W++;
            MemoryBus.SetAddress(PC);
            temp.L = MemoryBus.Read();
            temp.H = 0x00;

            //if (DebugEnabled) CreateDebugBytes(1);
            //if (DebugEnabled) SetDebugByte(0, MemoryBus.Read());

            // Fetch address in effect from the data in zero page as
            // appointed by the temporary address:
            MemoryBus.SetAddress(temp);
            AddressInEffect.L = MemoryBus.Read();
            temp.L++;
            MemoryBus.SetAddress(temp);
            AddressInEffect.H = MemoryBus.Read();

            // Add index Y to form the actual address:
            AddressInEffect.W += Y;
            // Or, if wrap-around is to be in effect:
            //	AddressInEffect.L  += Y;
        }

        public void Fetch_Z_PAGE_X()
        {//
            PC.W++;
            MemoryBus.SetAddress(PC);
            AddressInEffect.L = (byte)(X + MemoryBus.Read());
            AddressInEffect.H = 0x00;
        }

        public void Fetch_RELATIVE()
        {//
            PC.W++;
            int temp;
            MemoryBus.SetAddress(PC);
            temp = MemoryBus.Read();

            //if (DebugEnabled) CreateDebugBytes(1);
            //if (DebugEnabled) SetDebugByte(0, (byte)temp);
            
            if (temp > 127)
            {
                temp -= 256;
            }
            AddressInEffect.W = (UInt16)(PC.W + temp);
        }

        public void Fetch_INDIRECT()
        {//
         /*
         There’s hardware bug on the 6502 that causes jump indirect, with an operand which ends in $FF (such
         as $11FF), to bomb; the new high program counter value is taken incorrectly from $1100, not the correct $1200.
         */
            Address temp = new Address();

            // Fetch address of indirect address stored:
            PC.W++;
            MemoryBus.SetAddress(PC);
            temp.L = MemoryBus.Read();
            PC.W++;
            MemoryBus.SetAddress(PC);
            temp.H = MemoryBus.Read();
            MemoryBus.SetAddress(temp);

            //if (DebugEnabled) CreateDebugBytes(2);
            //if (DebugEnabled) SetDebugByte(0, temp.L);
            //if (DebugEnabled) SetDebugByte(0, temp.H);

            // Fetch address at that location:
            AddressInEffect.L = MemoryBus.Read();
            temp.W++;
            if (temp.L == 0x00) temp.H--; // The 'bug' described 17 lines above.
            MemoryBus.SetAddress(temp);
            AddressInEffect.H = MemoryBus.Read();
        }

        public void Fetch_Z_PAGE_Y()
        {//
            PC.W++;
            MemoryBus.SetAddress(PC);
            AddressInEffect.H = 0x00;
            //AddressInEffect.W = (UInt16)(Y + MemoryBus.Read());
            AddressInEffect.L = (byte)(Y + MemoryBus.Read());
            //if (DebugEnabled) CreateDebugBytes(1);
            //if (DebugEnabled) SetDebugByte(0, MemoryBus.Read());
        }

        private void CreateDebugBytes(byte count)
        {
            debugBytes = new byte[count];
        }

        private void SetDebugByte(byte n, byte b)
        {
            debugBytes[n] = b;
        }

        //////////////////////////////////////////////////////////////////////////
        // Set/reset flags
        //////////////////////////////////////////////////////////////////////////

        public void Set_VCZN_Flags(bool UseDecimalFlag)
        {
            // Set Overflow flag:
            if (siTest > 127 || siTest < -127) // That is, if sign has changed...
            {
                SetFlag(FLAG_6502_OVERFLOW);
            }
            else
            {
                ResetFlag(FLAG_6502_OVERFLOW);
            }

            // Set Carry, Zero and Negative flags:
            Set_CZN_Flags(UseDecimalFlag);
        }
        public void Set_CZN_Flags(bool UseDecimalFlag)
        {
            // Set Carry flag: // HBE: This is wrong! Use most significate nibble > 7 or < -8.
            if (UseDecimalFlag && IsSet(FLAG_6502_DECIMAL_MODE))
            //if(UseDecimalFlag && (P & FLAG_6502_DECIMAL_MODE))
            {
                if (siTest > 99 || siTest < -99)
                {
                    SetFlag(FLAG_6502_CARRY);
                }
                else
                {
                    ResetFlag(FLAG_6502_CARRY);
                }
            }
            else
            {

                if (siTest > 255 || siTest < -255)
                {
                    SetFlag(FLAG_6502_CARRY);
                }
                else
                {
                    ResetFlag(FLAG_6502_CARRY);
                }
            }

            // Set Zero and Negative flag:
            Set_ZN_Flags();
        }

        public void Set_ZN_Flags()
        {
            // Set Zero flag:
            if (ucTest == 0)
            {
                SetFlag(FLAG_6502_ZERO);
            }
            else
            {
                ResetFlag(FLAG_6502_ZERO);
            }

            // Set Negative flag:
            if ((0x80 & ucTest) > 0)
            {
                SetFlag(FLAG_6502_NEGATIVE);
            }
            else
            {
                ResetFlag(FLAG_6502_NEGATIVE);
            }
        }


        // Processor status register AND masks.
        // if(mask & P) flag set else flag reset.
        // To set a flag: P = P | mask
        // To reset a flag: P = P & ~mask
        // FLAG_6502_CARRY 1
        // FLAG_6502_ZERO 2
        // FLAG_6502_IRQ_DISABLE 4
        // FLAG_6502_DECIMAL_MODE 8
        // FLAG_6502_BRK_COMMAND 16
        // FLAG_6502_OVERFLOW 64
        // FLAG_6502_NEGATIVE 128

        public void SetFlag(byte Flag)
        {
            P = (byte)(P | Flag);
        }

        public void ResetFlag(byte Flag)
        {
            P = (byte)(P & ~Flag);
        }

        bool IsSet(byte Flag)
        {
            return ((P & Flag) == Flag) ? true : false;
        }

        //////////////////////////////////////////////////////////////////////////
        // Push/pull stack
        //////////////////////////////////////////////////////////////////////////

        public void Push(byte Byte)
        {
            // Obtain address to current stack location:
            Address adr = new Address();
            adr.L = S;
            adr.H = 0x01; // Stack always in page 1!

            // Write databyte to stack:
            MemoryBus.SetAddress(adr);
            MemoryBus.Write(Byte);
            //MemoryBus.RAM.pData[adr.W] = Byte;

            // Decrement stack pointer:
            S -= 1;
        }

        public byte Pull()
        {
            // Increment stack pointer:
            S += 1;

            // Obtain address to current stack location:
            Address adr = new Address();
            adr.L = S;
            adr.H = 0x01; // Stack always in page 1!

            // Fetch data from stack:
            MemoryBus.SetAddress(adr);
            return MemoryBus.Read();
            //return MemoryBus.RAM.pData[adr.W];
        }
    }
}
