using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Compukit_UK101_UWP
{
    public class CVDU : CMemoryBusDevice
    {
        public bool inScene;
        public UInt16 RAMSize { get; set; }
        public byte NumberOfLines
        {
            set
            {
                if (numberOfLines != value)
                {
                    numberOfLines = value;
                    if (numberOfLines == 16)
                    {
                        gridScreen.RowDefinitions.Clear();
                        for (byte row = 0; row < 16; row++)
                        {
                            gridScreen.RowDefinitions.Add(new RowDefinition());
                            for (byte col = 0; col < 64; col++)
                            {
                                AddChar(row, col);
                            }
                        }
                    }
                    else
                    {
                        for (byte row = 16; row < 32; row++)
                        {
                            gridScreen.RowDefinitions.Add(new RowDefinition());
                            for (byte col = 0; col < 64; col++)
                            {
                                AddChar(row, col);
                            }
                        }

                    }
                    SetScreenSize();
                }
            }
        }

        private void AddChar(byte row, byte col)
        {
            Image image = new Image();
            Int32 charNumber = pData[col + row * 64];
            String stringNumber = (((MONITOR)mainPage.CSignetic6502.MemoryBus.Monitor).pData[charNumber]).ToString();
            while (stringNumber.Length < 3) stringNumber = "0" + stringNumber;
            String name = "char_" + stringNumber + ".png";
            image.Source = new BitmapImage(new System.Uri("ms-appx:///Images/" + name));
            image.Stretch = Stretch.Fill;
            image.Margin = new Thickness(0, 0, -1, -1); // One pixel overlap (after stretch) to avoid gaps.
            image.Opacity = 1;
            image.HorizontalAlignment = HorizontalAlignment.Stretch;
            image.VerticalAlignment = VerticalAlignment.Stretch;
            Grid.SetColumn(image, col);
            Grid.SetRow(image, row);
            gridScreen.Children.Insert(row * 64 + col, image);
        }

        // Window size in CEGMON is stored at 0x0222 - 0x0226 (546 - 550):
        // SWIDTH column width (-1)     0x0222 = 0x2f (47)
        // SLTOP low byte of top        0x0223 = 0x0c (12)
        // SHTOP high byte of top       0x0224 = 0xd0 (208)
        // SLBASE low byte of base      0x0225 = 0xcc (204)
        // SHBASE high byte of base     0x0226 = 0xd3 (211)
        // Number of lines in CEGMON is stored at 0X0223 (547) and 0x0225 (549)
        // 0x0c (12), 0xcc (204) = 32 lines
        // 0x0c (12), 0xa0 (160) = 16 lines
        // Cursor Y position in 0x0200 (512)
        // CEGMON will reinitialize thos at reset, so to make screen 16 lines 
        // we have to patch CEGMON:
        // 0xfbc0 (in CEGMON file byte 0x3c0 (960)) = 0xd7 for 32 lines and 0x37 for 16 lines.
        public void SetScreenSize()
        {
            if (numberOfLines == 16)
            {
                //mainPage.CSignetic6502.MemoryBus.Monitor.pData[0x3bc] = 0x2f;
                //mainPage.CSignetic6502.MemoryBus.Monitor.pData[0x3bd] = 0x4c;
                //mainPage.CSignetic6502.MemoryBus.Monitor.pData[0x3be] = 0xd0;
                mainPage.CSignetic6502.MemoryBus.Monitor.pData[0x3bf] = 0x8c;
                mainPage.CSignetic6502.MemoryBus.Monitor.pData[0x3c0] = 0xd3;
                //mainPage.CSignetic6502.MemoryBus.RAM.pData[0x0222] = 0x47;
                //mainPage.CSignetic6502.MemoryBus.RAM.pData[0x0223] = 0x0c;
                //mainPage.CSignetic6502.MemoryBus.RAM.pData[0x0224] = 0xd0;
                //mainPage.CSignetic6502.MemoryBus.RAM.pData[0x0225] = 0xcc;
                //mainPage.CSignetic6502.MemoryBus.RAM.pData[0x0226] = 0xd1;
            }
            else
            {
                //mainPage.CSignetic6502.MemoryBus.Monitor.pData[0x3bc] = 0x2f;
                //mainPage.CSignetic6502.MemoryBus.Monitor.pData[0x3bd] = 0x4c;
                //mainPage.CSignetic6502.MemoryBus.Monitor.pData[0x3be] = 0xd0;
                mainPage.CSignetic6502.MemoryBus.Monitor.pData[0x3bf] = 0x8c;
                mainPage.CSignetic6502.MemoryBus.Monitor.pData[0x3c0] = 0xd7;
                //mainPage.CSignetic6502.MemoryBus.RAM.pData[0x0222] = 0x47;
                //mainPage.CSignetic6502.MemoryBus.RAM.pData[0x0223] = 0x0c;
                //mainPage.CSignetic6502.MemoryBus.RAM.pData[0x0224] = 0xd0;
                //mainPage.CSignetic6502.MemoryBus.RAM.pData[0x0225] = 0xcc;
                //mainPage.CSignetic6502.MemoryBus.RAM.pData[0x0226] = 0xd3;
            }
        }

        public byte[] pData;
        public byte pCharData;
        public bool Changed;
        public Grid gridScreen;
        public MainPage mainPage;

        private byte numberOfLines;

        public CVDU()
        {
            inScene = false;
            pCharData = 0;
            RAMSize = 4096;
            pData = new byte[RAMSize];
        }

        public void InitCVDU(MainPage mainPage, Grid gridScreen)
        {
            this.gridScreen = gridScreen;
            this.mainPage = mainPage;

            numberOfLines = 32;

            gridScreen.HorizontalAlignment = HorizontalAlignment.Stretch;
            gridScreen.VerticalAlignment = VerticalAlignment.Stretch;
            gridScreen.RowDefinitions.Clear();
            gridScreen.ColumnDefinitions.Clear();
            gridScreen.ColumnSpacing = 0;
            gridScreen.RowSpacing = 0;

            for (Int32 row = 0; row < 32; row++)
            {
                gridScreen.RowDefinitions.Add(new RowDefinition());
            }

            for (Int32 col = 0; col < 64; col++)
            {
                gridScreen.ColumnDefinitions.Add(new ColumnDefinition());
            }

            Random random = new Random(43);
            byte[] garbage = new byte[32 * 64];
            random.NextBytes(garbage);
            for (Int32 row = 0; row < 32; row++)
            {
                for (Int32 col = 0; col < 64; col++)
                {
                    Image image = new Image();
                    Int32 charNumber = garbage[row * 16 + col];
                    String stringNumber = (((MONITOR)mainPage.CSignetic6502.MemoryBus.Monitor).pData[charNumber]).ToString();
                    while (stringNumber.Length < 3) stringNumber = "0" + stringNumber;
                    String name = "char_" + stringNumber + ".png";
                    image.Source = new BitmapImage(new System.Uri("ms-appx:///Images/" + name));
                    image.Stretch = Stretch.Fill;
                    image.Margin = new Thickness(0, 0, -1, -1); // One pixel overlap (after stretch) to avoid gaps.
                    image.Opacity = 1;
                    image.HorizontalAlignment = HorizontalAlignment.Stretch;
                    image.VerticalAlignment = VerticalAlignment.Stretch;
                    Grid.SetColumn(image, col);
                    Grid.SetRow(image, row);
                    gridScreen.Children.Insert(row * 64 + col, image);
                }
            }
        }

        public void ClearScreen()
        {
            for (Int32 row = 0; row < 32; row++)
            {
                for (Int32 col = 0; col < 64; col++)
                {
                    Image image = new Image();
                    image.Source = new BitmapImage(new System.Uri("ms-appx:///Images/char_032.png"));
                    image.Stretch = Stretch.Fill;
                    image.Margin = new Thickness(0, 0, -1, -1); // One pixel overlap (after stretch) to avoid gaps.
                    image.Opacity = 1;
                    image.HorizontalAlignment = HorizontalAlignment.Stretch;
                    image.VerticalAlignment = VerticalAlignment.Stretch;
                    Grid.SetColumn(image, col);
                    Grid.SetRow(image, row);
                    gridScreen.Children.RemoveAt(row * 64 + col);
                    gridScreen.Children.Insert(row * 64 + col, image);
                    pData[row * 64 + col] = 32;
                }
            }
        }

        public override void Write(byte InData)
        {
            pData[Address - StartsAt] = InData;
            Image image = new Image();
            String stringNumber = InData.ToString();
            while (stringNumber.Length < 3) stringNumber = "0" + stringNumber;
            String name = "char_" + stringNumber + ".png";
            image.Source = new BitmapImage(new System.Uri("ms-appx:///Images/" + name));
            image.Stretch = Stretch.Fill;
            image.Margin = new Thickness(0, 0, -1, -1); // One pixel overlap (after stretch) to avoid gaps.
            image.Opacity = 1;
            image.HorizontalAlignment = HorizontalAlignment.Stretch;
            image.VerticalAlignment = VerticalAlignment.Stretch;
            Int32 position = Address - StartsAt;
            Grid.SetColumn(image, position % 64);
            Grid.SetRow(image, position / 64);
            gridScreen.Children.RemoveAt(position);
            gridScreen.Children.Insert(position, image);
            image = null;
        }

        public override byte Read()
        {
            return pData[Address - StartsAt];
        }
    }
}
