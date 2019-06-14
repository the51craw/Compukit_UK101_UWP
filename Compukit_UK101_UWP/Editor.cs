using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compukit_UK101_UWP
{
    public class Editor
    {
        public void AddPart(byte[] bytes)
        {
            Part part = new Part();
            part.ObjectCode = bytes;
            part.SourceCode = part.Decompile();
        }
    }

    public class Part
    {
        public byte[] ObjectCode { get; set; }
        public string SourceCode { get; set; }
        public string Name { get; set; }

        private string[] notes = new string[]
        {
            "C0 ","C#0 ","D0 ","D#0 ","E0 ","Fb0 ","F0 ","G0 ","G#0 ","A0 ","Bb0 ","B0 ",
            "C1 ","C#1 ","D1 ","D#1 ","E1 ","Fb1 ","F1 ","G1 ","G#1 ","A1 ","Bb1 ","B1 ",
            "C2 ","C#2 ","D2 ","D#2 ","E2 ","Fb2 ","F2 ","G2 ","G#2 ","A2 ","Bb2 ","B2 ",
            "C3 ","C#3 ","D3 ","D#3 ","E3 ","Fb3 ","F3 ","G3 ","G#3 ","A3 ","Bb3 ","B3 ",
            "C4 ","C#4 ","D4 ","D#4 ","E4 ","Fb4 ","F4 ","G4 ","G#4 ","A4 ","Bb4 ","B4 ","C5 "
        };

        public Part()
        {
            SourceCode = "";
        }

        public void Add(byte[] o)
        {
            ObjectCode = o;
        }

        /**
         * Object code holds note on/off events and pauses in a compressed form.
         * Bit 6 is '0' for note event and '1' for pauses.
         * Key on has bit 7 set to '1'. Bit 1 to 6 is note number.
         * Key off has bit 7 set to '0'. Bit 1 to 6 is note number + 0x3e.
         */
        public string Decompile()
        {
            string result = "";
            if (ObjectCode != null && ObjectCode.Length > 0)
            {
                for (int i = 0; i < ObjectCode.Length; i++)
                {
                    if ((ObjectCode[i] & 0xc0) == 0xc0)
                    {
                        result += AddPause(i);
                    }
                    else
                    {
                        if ((ObjectCode[i] & 0x80) == 0x80)
                        {
                            // Note on
                            result += AddNoteOn((byte)(ObjectCode[i] & 0x80));
                        }
                        else
                        {
                            result += AddNoteOff((byte)(ObjectCode[i] & 0x80));
                        }
                    }
                }
            }
            return result;
        }

        private String AddPause(Int32 i)
        {
            byte b;
            UInt16 ticks;
            UInt32 pauseVal;
            String s = "";

            // Pause, strip of bit 7 and 6:
            b = (byte)(ObjectCode[i] & 0x3f);
            // 0x0e is 1/4 note, what about the rest?
            // Assuming that it is relative, one 1/4 note
            // is 14 'ticks' and a 'tick' is 1/56 note.
            // However, MIDI has 96 ticks per 1/4 note,
            // so let's convert to MIDI ticks:
            ticks = (UInt16)(6 * (b + 2));

            // Also note that multiple pauses might be
            // present, so look ahead for more while b = 0xff:
            pauseVal = ticks;
            while (((byte)(ObjectCode[i + 1]) & 0xc0) == 0xc0)
            {
                i++;
                b = (byte)(b & 0x3f);
                ticks = (UInt16)(6 * (b + 2));
                pauseVal += (UInt32)ticks;
            }
            while (pauseVal > 1)
            {
                if (pauseVal >= 384)
                {
                    pauseVal -= 384;
                    s += "P1 ";
                }
                else if (pauseVal >= 288)
                {
                    pauseVal -= 288;
                    s += "P2. ";
                }
                else if (pauseVal >= 192)
                {
                    pauseVal -= 192;
                    s += "P2 ";
                }
                else if (pauseVal >= 128)
                {
                    pauseVal -= 128;
                    s += "P2T ";
                }
                else if (pauseVal >= 144)
                {
                    pauseVal -= 144;
                    s += "P4. ";
                }
                else if (pauseVal >= 96)
                {
                    pauseVal -= 96;
                    s += "P4 ";
                }
                else if (pauseVal >= 72)
                {
                    pauseVal -= 72;
                    s += "P8. ";
                }
                else if (pauseVal >= 64)
                {
                    pauseVal -= 64;
                    s += "P4T ";
                }
                else if (pauseVal >= 48)
                {
                    pauseVal -= 48;
                    s += "P8 ";
                }
                else if (pauseVal >= 36)
                {
                    pauseVal -= 36;
                    s += "P16. ";
                }
                else if (pauseVal >= 32)
                {
                    pauseVal -= 32;
                    s += "P8T ";
                }
                else if (pauseVal >= 24)
                {
                    pauseVal -= 24;
                    s += "P16 ";
                }
                else if (pauseVal >= 18)
                {
                    pauseVal -= 18;
                    s += "P32. ";
                }
                else if (pauseVal >= 16)
                {
                    pauseVal -= 16;
                    s += "P16T ";
                }
                else if (pauseVal >= 12)
                {
                    pauseVal -= 12;
                    s += "P32 ";
                }
                else if (pauseVal >= 9)
                {
                    pauseVal -= 9;
                    s += "P64. ";
                }
                else if (pauseVal >= 8)
                {
                    pauseVal -= 8;
                    s += "P32T ";
                }
                else if (pauseVal >= 6)
                {
                    pauseVal -= 6;
                    s += "P64 ";
                }
                else if (pauseVal >= 4)
                {
                    pauseVal -= 4;
                    s += "P64T ";
                }
                else if (pauseVal >= 3)
                {
                    pauseVal -= 3;
                    s += "P128 ";
                }
                else if (pauseVal >= 2)
                {
                    pauseVal -= 2;
                    s += "P128T ";
                }
                else if (pauseVal >= 1)
                {
                    pauseVal -= 1;
                    s += "P256T ";
                }
            }
            return s;
        }

        private String AddNoteOn(byte b)
        {
            // Note 0 = C0, 1 = C#0 etc
            return notes[b];
        }

        private String AddNoteOff(byte b)
        {
            try
            {
                b -= 0x3e;
                return notes[b];
            }
            catch
            {
                return "Byte in error: " + b.ToString() + " ";
            }
        }
    }
}
