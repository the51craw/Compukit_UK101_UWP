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
        public byte[] pData;
        public byte pCharData;
        public bool Changed;
        public Grid gridScreen;
        public MainPage mainPage;

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
                    //image.HeightRequest = 500;
                    //image.WidthRequest = 500;
                    Grid.SetColumn(image, col);
                    Grid.SetRow(image, row);
                    gridScreen.Children.Insert(row * 64 + col, image);
                }
            }
        }

        public Boolean Write(byte InData)
        {
            if (Selected)
            {
                pData[Address.W - StartsAt.W] = InData;
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
                //image.HeightRequest = 500;
                //image.WidthRequest = 500;
                Int32 position = Address.W - StartsAt.W;
                Grid.SetColumn(image, position % 64);
                Grid.SetRow(image, position / 64);
                gridScreen.Children.RemoveAt(position);
                gridScreen.Children.Insert(position, image);
                image = null;
                return true;
            }
            else
            {
                return false;
            }
        }

        public byte Read()
        {
            if (Selected)
            {
                return pData[Address.W - StartsAt.W];
            }
            else
            {
                return 0xFF;
            }
        }
    }
}
