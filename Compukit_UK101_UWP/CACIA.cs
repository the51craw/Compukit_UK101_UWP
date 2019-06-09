using Compukit_UK101_UWP.Basic;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;

namespace Compukit_UK101_UWP
{
    public class CACIA : CMemoryBusDevice
    {
        // I/O modes:
        const byte IO_MODE_6820_FILE = 1; // Use filesystem
        const byte IO_MODE_6820_MIDI = 2; // Use a MIDI interface
        const byte IO_MODE_6820_TAPE = 4; // Use internal classes

        // Status register flags:
        const byte ACIA_STATUS_IRQ  = 0x80; // ACIA wishes to interrupt processor
        const byte ACIA_STATUS_PE   = 0x40; // A parity error has occurred
        const byte ACIA_STATUS_OVRN = 0x20; // Receiver overrun, character not read before next came in
        const byte ACIA_STATUS_FE = 0x10;   // A framing error has occurred
        const byte ACIA_STATUS_CTS = 0x08;  // Must be LOW for clear to send. A high also forces a low TDRE
        const byte ACIA_STATUS_DCD = 0x04;  // Is LOW when clear to send. Dos not reset until CPU read status AND data.
        const byte ACIA_STATUS_TDRE = 0x02; // Is HIGH when a transmit is finished. Goes low when writing new data to send.
        const byte ACIA_STATUS_RDRF = 0x01; // Is HIGH when a character has been read in and is ready to fetch.

        // Control register:
        const byte ACIA_CONTROL_ENABLE_IRQ = 0x80;              // A HIGH enables receive interrupt
        const byte ACIA_CONTROL_TC_MASK = 0x60;                 // To mask out transmit control
        const byte ACIA_CONTROL_TC_LOW_DISABLE = 0x00;          // RTS low, disable interrupts
        const byte ACIA_CONTROL_TC_LOW_ENABLE = 0x20;           // RTS low, enable interrupts
        const byte ACIA_CONTROL_TC_HIGH_DISABLE = 0x40;         // RTS high, disable interrupts
        const byte ACIA_CONTROL_TC_LOW_DISABLE_BREAK = 0x60;    // RTS low, disable interrupts
        const byte ACIA_CONTROL_PROTOCOL_MASK = 0x1C;           // Protocol mask, Bits, parity and stop bits
        const byte ACIA_CONTROL_DIVISION_MASK = 0x03;           // Counter division bits.
        const byte ACIA_CONTROL_MASTER_RESET = 0x03;            // Master reset command.

        public LoadAndRunTest LoadAndRunTest { get; set; }
        public string[] file = null;

        private DispatcherTimer timer;
        private byte IOMode;
        private byte ACIAStatus;
        private byte status;
        private Int16 line;
        private Int16 pos;
        private byte Command;
        private MainPage mainPage;

        public CACIA(MainPage mainPage)
        {
            this.mainPage = mainPage;
            LoadAndRunTest = new LoadAndRunTest();
            IOMode = IO_MODE_6820_TAPE;
	        ReadOnly = false;
	        ACIAStatus = 0x00;
            line = 0;
            pos = 0;
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 10);// 33);
            timer.Tick += Timer_Tick;
            SetFlag(ACIA_STATUS_RDRF);
        }

        public override SetAddress()
        {

        }

        public byte Read()
        {
            if (Selected && Address.L == 1)
            {
                timer.Start();
                ResetFlag(ACIA_STATUS_RDRF);
                if (line < LoadAndRunTest.lines.Length)
                {
                    if (pos > LoadAndRunTest.lines[line].Length - 1)
                    {
                        line++;
                        pos = 0;
                        GC.Collect();
                        return 0x0d;
                    }
                    else
                    {
                        byte b = (byte)LoadAndRunTest.lines[line][pos++];
                        return b;
                    }
                }
                else
                {
                    mainPage.CSignetic6502.MemoryBus.RAM.pData[0x0203] = 0x00;
                    return 0x00;
                }
            }
            else
            {
                if (line < LoadAndRunTest.lines.Length)
                {
                    return ACIAStatus;
                }
                else
                {
                    return 0;
                }
            }
        }

        public Boolean Write(byte InData)
        {
            if (Selected && Address.L == 1)
            {
            }
            else
            {
                ACIAStatus = InData;
                Command = InData;

                // Reset IRQ if disabled by current command:
                if ((Command & ACIA_CONTROL_ENABLE_IRQ) == 0x00
                    || (Command & ACIA_CONTROL_TC_MASK) != ACIA_CONTROL_TC_LOW_ENABLE
                    || (Command & ACIA_CONTROL_DIVISION_MASK) == ACIA_CONTROL_MASTER_RESET)
                {
                    // Reset because Rx or Tx IRQ disabled or MasterReset issued:
                    ResetFlag(ACIA_STATUS_IRQ);
                }

                // Set RTS (not implemented on communications side yet.
                if ((Command & ACIA_CONTROL_TC_MASK) == ACIA_CONTROL_TC_HIGH_DISABLE)
                {
                    // Set RTS:
                }
                else
                {
                    // Reset RTS:
                }
            }
            return true;
        }


        // Clock for simulating speed and IRQ:
        private void Timer_Tick(object sender, object e)
        {
            SetFlag(ACIA_STATUS_RDRF);
            timer.Stop();
            SetFlag(ACIA_STATUS_RDRF);
            if ((Command & ACIA_CONTROL_ENABLE_IRQ) != 0x00)
            {
                SetFlag(ACIA_STATUS_IRQ);
            }
            //switch (Id)
            //{
            //    case TIMER_ACIA_READ_READY:
            //        // It is time to make new data available, and if enabled, set the IRQ:
            //        SetFlag(ACIA_STATUS_RDRF);
            //        if (Command & ACIA_CONTROL_ENABLE_IRQ)
            //        {
            //            SetFlag(ACIA_STATUS_IRQ);
            //        }
            //        break;

            //    case TIMER_ACIA_WRITE_READY:
            //        // ACIA is finished sending a byte. If enabled, also set the IRQ:
            //        SetFlag(ACIA_STATUS_TDRE);
            //        if (Command & ACIA_CONTROL_ENABLE_IRQ)
            //        {
            //            SetFlag(ACIA_STATUS_IRQ);
            //        }
            //        break;
            //}
            //	KillTimer(hWnd, Id);
        }

        ///////////////////////////////////////////////////////////////////////////////////
        // Helpers:
        ///////////////////////////////////////////////////////////////////////////////////

        // Set a flag:
        void SetFlag(byte Flag)
        {
            ACIAStatus = (byte)(ACIAStatus | Flag);
        }

        // Reset a flag:
        void ResetFlag(byte Flag)
        {
            ACIAStatus = (byte)(ACIAStatus & ~Flag);
        }
    }
}
