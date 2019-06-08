using System;
using System.Collections.Generic;
using System.Text;

namespace Compukit_UK101_UWP
{
    public class CKeyboard : CMemoryBusDevice
    {
        //private byte[][] Matrix;  //[8][8];
        public byte[] Keystates; //[8];
        public KeyboardMatrix Matrix;
    	public CKeyboard()
        {
            //// Setup Matrix:
            //int row, col;
            //Matrix = new byte[8][];
            Matrix = new KeyboardMatrix();

            //for (row = 0; row < 8; row++)
            //{
            //    Matrix[row] = new byte[8];

            //    for (col = 0; col < 8; col++)
            //    {
            //        // Matrix[7 - row][7 - col] = binFile.pData[row * 8 + col];
            //        Matrix[row][col] = keyboardMatrix.bytes[row * 8 + col];
            //    }
            //}
            Keystates = new byte[8];
            Reset();
        }

        public void Reset()
        {
            // Initiate Keystates:
            Keystates[0] = 0xFF;
            Keystates[1] = 0xFF;
            Keystates[2] = 0xFF;
            Keystates[3] = 0xFF;
            Keystates[4] = 0xFF;
            Keystates[5] = 0xFF;
            Keystates[6] = 0xFF;
            Keystates[7] = 0xFF;
        }

        public void PressKey(byte Key)
        {
            //byte temp1, temp2;

            // Add key in Keystates.
            // Find the position of this keycap in the Matrix:
            UInt16 row = 0;
            UInt16 col = 0; ;
            bool found = false;

            while (row < 8 && !found)
            {
                col = 0;
                while (col < 8 && !found)
                {
                    if (Matrix.Bytes[row][col] == Key)
                    {
                        found = true;
                    }
                    else
                    {
                        col++;
                    }
                }
                if (!found)
                {
                    row++;
                }
            }
            if (found)
            {
                //temp1 = Keystates[row];
                // Reset corresponding bit to indicate key down:
                Keystates[row] = (byte)(Keystates[row] ^ (0x80 >> (col))); // E.g. 1110 1111 ^ 0000 0100 = 1110 1011
                //temp2 = Keystates[row];
            }
        }

        public void ReleaseKey(byte Key)
        {
            // Remove key from Keystates.
            // Find the position of this keycap in the Matrix:
            UInt16 row = 0;
            UInt16 col = 0;
            bool found = false;
            while (row < 8 && !found)
            {
                col = 0;
                while (col < 8 && !found)
                {
                    if (Matrix.Bytes[row][col] == Key)
                    {
                        found = true;
                    }
                    else
                    {
                        col++;
                    }
                }
                if (!found)
                {
                    row++;
                }
            }
            if (found)
            {
                // Set corresponding bit to indicate key up:
                Keystates[row] = (byte)(Keystates[row] | (0x80 >> col)); // E.g. 1110 1011 | 0000 0100 = 1110 1111
            }
        }

        public byte Read()
        {
            // Inverted bits for row selection.
            // Row 0 = 254 (1111 1110), row 1 = 253 (1111 1101)... row 7 = 127 (0111 1111).
            // Loop through the bits and test selected rows. 
            // Reset corresponding bits in outdata:
            byte OutData = 0xFF;
            for (int i = 0; i < 8; i++)
            {
                // Is the row selected? (Bit at location 'i' is zero.)
                if ((Data & (0x80 >> i)) == 0) // 1101 1011 & 0000 1000 = 0000 1000 (no), 1101 1011 & 0000 0100 = 0000 0000(yes)
                {
                    // Mask 0's from corresponding Keystate onto OutData:
                    OutData = (byte)(OutData & Keystates[i]);
                }
            }

            return OutData;
        }

        public bool Write(byte InData)
        {
            // Inverted bits for row selection.
            // Row 0 = 254 (1111 1110), row 1 = 253 (1111 1101)... row 7 = 127 (0111 1111).
            // We need to store which row is selected (only one can be selected at a time).
            Data = InData;

            return true;
        }
    }
}

