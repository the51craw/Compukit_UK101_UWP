using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compukit_UK101_UWP
{
    class MemoryMap
    {
        public byte[] Map = new byte[0x10000];

        public MemoryMap()
        {
            for (Int32 Address = 0; Address < 0x10000; Address++)
            {
                if (Address >= 0xf800)
                {
                    Map[Address] = 0;
                }
                else if (Address >= 0xf000 && Address <= 0xf0ff)
                {
                    Map[Address] = 1;
                }
                else if (Address == 0xdf00)
                {
                    Map[Address] = 2;
                }
                else if (Address >= 0xd000 && Address <= 0xd7ff)
                {
                    Map[Address] = 3;
                }
                else if (Address >= 0xb800 && Address <= 0xbfff)
                {
                    Map[Address] = 4;
                }
                else if (Address >= 0xb000 && Address <= 0xb7ff)
                {
                    Map[Address] = 5;
                }
                else if (Address >= 0xa800 && Address <= 0xafff)
                {
                    Map[Address] = 6;
                }
                else if (Address >= 0xa000 && Address <= 0xa7ff)
                {
                    Map[Address] = 7;
                }
                else if (Address >= 0x8000 && Address <= 0x8fff)
                {
                    Map[Address] = 8;
                }
                else if (Address == 0x6001)
                {
                    Map[Address] = 9;
                }
                else if (Address < 0x2000)
                {
                    Map[Address] = 10;
                }
                else
                {
                    Map[Address] = 11;
                }

            }
        }
    }
}
