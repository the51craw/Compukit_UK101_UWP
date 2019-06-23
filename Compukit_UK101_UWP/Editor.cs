using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compukit_UK101_UWP
{
    /**
     * About the COLON (:) marker:
     * The code must start with a colon (: 0x3a) to make Composer start accept events.
     * The code should end with a colon (: 0x3a) to make Composer leave load mode.
     * If the code does not end with a colon, the Composer will not stop loading. Instead
     * the user has to reset the UK101, enter Composer again, and can then play the music.
     * Edit will add colon at start and end of the object code.
     * 
     * About events:
     * Events are note on, note off and pauses. In the source they are separated
     * by spaces, and syntax is:
     * Note on:  a + sign, the note name and octave, e.g. +G#2.
     * Note off: a - sign, the note name and octave, e.g. -G#2.
     * Pause:    a P and an inverted pause value, e.g. P4 for a quarter note pause. There
     *           are no triplets and no dotted values, only 1, 2, 4, 8, 16 and 32.
     * 
     * About parts names and song:
     * Each part has a name. The default name is always 'Part' plus a sequence number. 
     * The user can change the name in the textbox next to the parts combobox. 
     * The name must not contain any white space nor control characters.
     * The name preceeds the events in the editor textbox.
     * After the parts, last in editor textbox, is the song compiled.
     * The song starts with the word 'Song' and then follows by a list of part names.
     * Names must be unique, can not be 'Song' nor anything that can be confused with 
     * any of the events. NOTE! There is not check for uniqueness. The first occurence
     * will be used instead!
     * If two or more parts should be merged to play simultaneously, add a + sign 
     * before the names of the parts that are to be merged with the previous part.
     * E.g. Three parts to merge: Part1 +Part2 +Part3
     * 
     * About the edit boxes:
     * There are two large edit boxes. The top one is for editing the part that is
     * selected in the combobox. The bottom one is for editing the song.
     * Events and names are space separated, and newlines are ignored (treated as 
     * a separator).
     * The part edit box, the top one, may only contain events.
     * The song edit box, the bottom one, may only contain names.
     * 
     * About compilation.
     * All parts are first asked to compile themself, then the song is compiled.
     */
    public class Editor
    {
        public List<Part> parts { get; set; }
        public Song Song { get; set; }

        private MainPage mainPage;

        public Editor(MainPage mainPage)
        {
            this.mainPage = mainPage;
            parts = new List<Part>();
        }

        public void AddPart(byte[] bytes)
        {
            Part part = new Part(mainPage);
            part.ObjectCode = bytes;
            part.SourceCode = part.Decompile();
            parts.Add(part);
            mainPage.AddPart(part);
        }

        public void ChangePartName(String oldName, String newName)
        {
            Int32 index = FindPart(oldName);
            if (index > -1)
            {
                parts[index].Name = newName;
            }
        }

        public Boolean Compile()
        {
            // This compiles the parts and then the song.
            Song = new Song();

            String[] Songparts = mainPage.GetEditorContent().Replace("\r", " ").Replace("\n", " ").Split(' ');
            foreach (String partName in Songparts)
            {
                if (partName != " ")
                {
                    Int32 index = FindPart(partName.TrimStart('+'));
                    if (index < 0)
                    {
                        mainPage.MessageBox("There is no part by name " + partName.TrimStart('+') + "!");
                        return false;
                    }
                    else
                    {
                        if (partName.StartsWith('+') && Song.Parts.Count() > 0)
                        {
                            Song.Merge(parts[index]);
                        }
                        else
                        {
                            Song.Parts.Add(parts[index]);
                        }
                    }
                }
            }

            // Loop all parts in song and concatenate source code and object code:
            Song.SourceCode = "";
            Song.ObjectCode = new byte[] { 0x0d, 0x0a, 0x3a, 0x0d, 0x0a };
            foreach (Part p in Song.Parts)
            {
                p.Compile();
                Song.SourceCode += p.SourceCode + " ";
                Song.ObjectCode = (byte[])Song.ObjectCode.Concat(p.ObjectCode).ToArray();
            }
            Song.ObjectCode = (byte[])Song.ObjectCode
                .Concat(new byte[] { 0x45, 0x41, 0x45, 0x42, 0x0d, 0x0a, 0x3a, 0x0d, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }).ToArray();
            return true;
        }

        private Int32 FindPart(String name)
        {
            Int32 index = parts.Count() - 1;
            while (index > -1 && parts[index].Name != name)
            {
                index--;
            }
            return index;
        }
    }

    public class Song
    {
        public String SourceCode { get; set; }
        public byte[] ObjectCode { get; set; }
        public List<Part> Parts { get;set; }

        public Song()
        {
            Parts = new List<Part>();
        }

        public void Merge(Part part)
        {
            // Merge new part with last part in song:
            String[] parts1 = Parts[Parts.Count() - 1].SourceCode.Split(" ");
            String[] parts2 = part.SourceCode.Split(" ");
            Double time1 = 0;
            Double time2 = 0;
            Int32 index1 = 0;
            Int32 index2 = 0;
            String result = "";

            // Merge as long as both has more:
            while (index1 < parts1.Length && index2 < parts2.Length)
            {
                while (index1 < parts1.Length && index2 < parts2.Length)
                {
                    while (!(parts1[index1].StartsWith('P')))
                    {
                        result += parts1[index1++] + " ";
                    }
                    while (!(parts1[index1].StartsWith('P')))
                    {
                        result += parts2[index2++] + " ";
                    }
                    if (parts1[index1].StartsWith('P') && parts1[index1].StartsWith('P'))
                    {
                        try
                        {
                            Int32 p1 = Int32.Parse(parts1[index1].Remove(0, 1));
                            Int32 p2 = Int32.Parse(parts2[index2].Remove(0, 1));
                            if (p1 > p2)
                            {
                                // Step index1 forward and store pauses until pointing at a non pause
                                // event or time passes next non psuse event in parts1:
                                time1 += 1 / p1;
                                time2 += 1 / p2;
                                while (!(parts1[index1].StartsWith('P')))
                                {
                                    result += parts1[index1++] + " ";
                                    p1 = Int32.Parse(parts1[index1].Remove(0, 1));
                                    time1 += 1 / p1;
                                }
                            }
                            else if (p2 > p1)
                            {
                                // Step index1 forward and store pauses until pointing at a non pause
                                // event or time passes next non psuse event in parts1:
                                time1 += 1 / p1;
                                time2 += 1 / p2;
                                while (!(parts2[index2].StartsWith('P')))
                                {
                                    result += parts2[index2++] + " ";
                                    p2 = Int32.Parse(parts2[index2].Remove(0, 1));
                                    time2 += 1 / p2;
                                }
                            }
                            else
                            {
                                result += parts1[index1] + " ";
                            }
                        }
                        catch { }
                    }
                }

                // Add as long as parts1 still has more:
                while (index1 < parts1.Length)
                {
                    result += parts1[index1] + " ";
                }

                // Add as long as parts2 still has more:
                while (index2 < parts2.Length)
                {
                    result += parts2[index2] + " ";
                }
            }
            Parts[Parts.Count() - 1].SourceCode = result;
        }
    }

    public class Part
    {
        public byte[] ObjectCode { get; set; }
        public string SourceCode { get; set; }
        public string Name { get; set; }

        private List<byte> objectBytes;
        private MainPage mainPage;

        private string[] notes = new string[]
        {
            "C0","C#0","D0","Eb0","E0","F0","F#0","G0","G#0","A0","Bb0","B0",
            "C1","C#1","D1","Eb1","E1","F1","F#1","G1","G#1","A1","Bb1","B1",
            "C2","C#2","D2","Eb2","E2","F2","F#2","G2","G#2","A2","Bb2","B2",
            "C3","C#3","D3","Eb3","E3","F3","F#3","G3","G#3","A3","Bb3","B3",
            "C4","C#4","D4","Eb4","E4","F4","F#4","G4","G#4","A4","Bb4","B4","C5"
        };

        private byte[] noteOn = new byte[]
        {
            0x91, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98,
            0x99, 0x9a, 0x9b, 0x9c, 0x9d, 0x9e, 0x9f, 0x90,
            0xa1, 0xa2, 0xa3, 0xa4, 0xa5, 0xa6, 0xa7, 0xa8,
            0xa9, 0xaa, 0xab, 0xac, 0xad, 0xae, 0xaf, 0xa0,
            0xb1, 0xb2, 0xb3, 0xb4, 0xb5, 0xb6, 0xb7, 0xb8,
            0xb9, 0xba, 0xbb, 0xbc, 0xbd, 0xbe, 0xbf, 0xb0,
            0xc1, 0xc2, 0xc3, 0xc4, 0xc5, 0xc6, 0xc7, 0xc8,
            0xc9, 0xca, 0xcb, 0xcc, 0xcd
        };

        private byte[] noteOff = new byte[]
        {
            0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5a,
            0x5b, 0x5c, 0x5d, 0x5e, 0x5f, 0x50, 0x61, 0x62,
            0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6a,
            0x6b, 0x6c, 0x6d, 0x6e, 0x6f, 0x60, 0x71, 0x72,
            0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7a,
            0x7b, 0x7c, 0x7d, 0x7e, 0x7f, 0x70, 0x81, 0x82,
            0x83, 0x84, 0x85, 0x86, 0x87, 0x88, 0x89, 0x8a,
            0x8b, 0x8c, 0x8d, 0x8e, 0x8f
        };


        public Part(MainPage mainPage)
        {
            this.mainPage = mainPage;
            SourceCode = "";
            objectBytes = new List<byte>();
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
            byte b;
            UInt16 ticks;

            CombineBytes();

            if (objectBytes != null && objectBytes.Count() > 0)
            {
                int i = 0;
                while (i < objectBytes.Count())
                {
                    if (objectBytes[i] == 0x3a)
                    {
                        i++;
                    }
                    else
                    { 
                        if (objectBytes[i] > 0xce)
                        {
                            // One or more pauses
                            ticks = 0;
                            while (objectBytes[i] > 0xce)
                            {
                                ticks += ((byte)(objectBytes[i++] - 0xcf));
                            }
                            result += AddPause(ticks);
                        }
                        if (objectBytes[i] > 0x90)
                        {
                            // Note on
                            result += AddNoteOn((byte)(objectBytes[i] - 0x91));
                            i++;
                        }
                        else if (objectBytes[i] > 0x52)
                        {
                            // Note off
                            result += AddNoteOff((byte)(objectBytes[i] - 0x53));
                            i++;
                        }
                        else
                        {
                            i++;
                        }
                    }
                }
            }
            return result;
        }

        private void CombineBytes()
        {
            if (ObjectCode != null && ObjectCode.Length > 0)
            {
                for (int i = 0; i < ObjectCode.Length; i++)
                {
                    if (!(ObjectCode[i] == 0x0d || ObjectCode[i] == 0x0a))
                    {
                        if (ObjectCode[i] == 0x3a)
                        {
                            objectBytes.Add(ObjectCode[i]);
                        }
                        else
                        {
                            if ((ObjectCode[i] & 0xf0) == 0x40 && (ObjectCode[i + 1] & 0xf0) == 0x40)
                            {
                                // One byte is combined by two bytes with high nibble = 4.
                                // If we came here, we must also get the next byte and 
                                // put the low nibbles together into the byte in effect.
                                objectBytes.Add((byte)(((ObjectCode[i++] & 0x0f) << 4) | (ObjectCode[i] & 0x0f)));
                            }
                        }
                    }
                }
            }
        }

        private String AddPause(UInt16 pauseVal)
        {
            //byte b;
            //UInt32 pauseVal;
            String s = "";

            // Pause, strip of bit 7 and 6:
            // 0x0e is 1/4 note, what about the rest?
            // Assuming that it is relative, one 1/4 note
            // is 14 'ticks' and a 'tick' is 1/56 note.
            // However, MIDI has 96 ticks per 1/4 note,
            // so let's convert to MIDI ticks:
            //ticks = (UInt16)(6 * (b + 2));

            //// Also note that multiple pauses might be
            //// present, so look ahead for more while b = 0xff:
            //pauseVal = ticks;
            //while (((byte)(ObjectCode[i + 1]) & 0xc0) == 0xc0)
            //{
            //    i++;
            //    b = (byte)(b & 0x3f);
            //    ticks = (UInt16)(6 * (b + 2));
            //    pauseVal += (UInt32)ticks;
            //}

            // 0 - 32, 16 = 1/4 note
            /*
             * 1 = 1/64
             * 2 = 1/32
             * 4 = 1/16
             * 8 = 1/8
             * 16 = 1/4
             * 32 = 1/2
             */

            while (pauseVal > 1)
            {
                if (pauseVal >= 32)
                {
                    pauseVal -= 32;
                    s += "P2 ";
                }
                else if (pauseVal >= 16)
                {
                    pauseVal -= 16;
                    s += "P4 ";
                }
                else if (pauseVal >= 8)
                {
                    pauseVal -= 8;
                    s += "P8 ";
                }
                else if (pauseVal >= 4)
                {
                    pauseVal -= 4;
                    s += "P16 ";
                }
                else if (pauseVal >= 2)
                {
                    pauseVal -= 2;
                    s += "P32 ";
                }
                else if (pauseVal >= 1)
                {
                    pauseVal -= 1;
                    s += "P64 ";
                }
            }
            return s + " ";
        }

        private String AddNoteOn(byte b)
        {
            try
            {
                return "+" + notes[b] + " ";
            }
            catch
            {
                return "Byte in error: " + b.ToString() + " ";
            }
        }

        private String AddNoteOff(byte b)
        {
            try
            {
                return "-" + notes[b] + " ";
            }
            catch
            {
                return "Byte in error: " + b.ToString() + " ";
            }
        }

        public void Compile()
        {
            // This compiles this part
            List<byte> ObjectBytes = new List<byte>();
            String[] events = SourceCode.Split(' ');
            for (Int32 i = 0; i < events.Length; i++)
            {
                if (events[i].StartsWith('+'))
                {
                    String note = events[i].Remove(0, 1);
                    Int32 index = IndexOfNote(note);
                    if (index < 0)
                    {
                        mainPage.MessageBox("Error: " + events[i] + " not recognized!");
                    }
                    else
                    {
                        byte[] b = MakeObjectFromByte((byte)(noteOn[index]));
                        ObjectBytes.Add(b[0]);
                        ObjectBytes.Add(b[1]);
                    }
                }
                else if (events[i].StartsWith('-'))
                {
                    String note = events[i].Remove(0, 1);
                    Int32 index = IndexOfNote(note);
                    if (index < 0)
                    {
                        mainPage.MessageBox("Error: " + events[i] + " not recognized!");
                    }
                    else
                    {
                        byte[] b = MakeObjectFromByte((byte)(noteOff[index]));
                        ObjectBytes.Add(b[0]);
                        ObjectBytes.Add(b[1]);
                    }
                }
                else if (events[i].ToUpper().StartsWith('P'))
                {
                    String note = events[i].Remove(0, 1);
                    try
                    {
                        // 1/4 note shoud be 0xdf
                        // 1/4 note should be 0x10 before 0xcf bias
                        // 1/4  = 0x0f
                        // 1/8  = 0x08
                        // 1/16 = 0x04
                        // 1/32 = 0x02
                        // 1/64 = 0x01
                        Int32 pausvalue = Int32.Parse(note);
                        byte[] b = MakeObjectFromByte((byte)(pausvalue + 0xcf));
                        ObjectBytes.Add(b[0]);
                        ObjectBytes.Add(b[1]);
                    }
                    catch
                    {
                        mainPage.MessageBox("Error: " + events[i] + " not recognized!");
                    }
                }
                else if (events[i] != " " && events[i] != "" && events[i] != "\r" && events[i] != "\n")
                {
                    mainPage.MessageBox("Error: " + events[i] + " not recognized!");
                }
            }
            ObjectCode = ObjectBytes.ToArray();
        }

        private Int32 IndexOfNote(String note)
        {
            Int32 index = 0;
            try
            {
                while (notes[index] != note.ToUpper())
                {
                    index++;
                }
            }
            catch { }
            if (index >= notes.Length)
            {
                return -1;
            }
            else
            {
                return index;
            }
        }

        private byte[] MakeObjectFromByte(byte b)
        {
            byte[] bytes = new byte[2];
            bytes[0] = (byte)(0x40 | ((b & 0xf0) >> 4));
            bytes[1] = (byte)(0x40 | (b & 0x0f));
            return bytes;
        }
    }
}
