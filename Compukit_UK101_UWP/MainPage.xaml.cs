using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Compukit_UK101_UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public CSignetic6502 CSignetic6502 { get; set; }
        public CClock CClock;
        private static MainPage mainPage { get; set; }
        public Boolean capsLock { get; set; }

        private Boolean numLock = false;
        CoreVirtualKeyStates keystate;
        public MainPage()
        {
            this.InitializeComponent();
            mainPage = this;
            Init();
        }
        private void Init()
        {
            CSignetic6502 = new CSignetic6502(mainPage);
            CSignetic6502.MemoryBus.VDU.InitCVDU(this, gridScreen);
            CClock = new CClock(this);
            keystate = Window.Current.CoreWindow.GetKeyState(VirtualKey.NumberKeyLock);
            numLock = (keystate & CoreVirtualKeyStates.Locked) != 0;
            keystate = Window.Current.CoreWindow.GetKeyState(VirtualKey.CapitalLock);
            capsLock = (keystate & CoreVirtualKeyStates.Locked) != 0;
        }

        private void btnEmulator_Click(object sender, RoutedEventArgs e)
        {
            btnEmulator.Background = new SolidColorBrush(Color.FromArgb(255, 64, 255, 32));
            btnFile.Background = new SolidColorBrush(Color.FromArgb(255, 128, 128, 128));
            btnHelp.Background = new SolidColorBrush(Color.FromArgb(255, 128, 128, 128));
            btnLicense.Background = new SolidColorBrush(Color.FromArgb(255, 128, 128, 128));
            gridScreen.Visibility = Visibility.Visible;
            gridFile.Visibility = Visibility.Collapsed;
            gridHelp.Visibility = Visibility.Collapsed;
            gridLicense.Visibility = Visibility.Collapsed;
        }

        private void btnFile_Click(object sender, RoutedEventArgs e)
        {
            btnEmulator.IsFocusEngaged = false;
            btnEmulator.Background = new SolidColorBrush(Color.FromArgb(255, 128, 128, 128));
            btnFile.Background = new SolidColorBrush(Color.FromArgb(255, 64, 255, 32));
            btnHelp.Background = new SolidColorBrush(Color.FromArgb(255, 128, 128, 128));
            btnLicense.Background = new SolidColorBrush(Color.FromArgb(255, 128, 128, 128));
            gridScreen.Visibility = Visibility.Collapsed;
            gridFile.Visibility = Visibility.Visible;
            gridHelp.Visibility = Visibility.Collapsed;
            gridLicense.Visibility = Visibility.Collapsed;
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            btnEmulator.Background = new SolidColorBrush(Color.FromArgb(255, 128, 128, 128));
            btnFile.Background = new SolidColorBrush(Color.FromArgb(255, 128, 128, 128));
            btnHelp.Background = new SolidColorBrush(Color.FromArgb(255, 64, 255, 32));
            btnLicense.Background = new SolidColorBrush(Color.FromArgb(255, 128, 128, 128));
            gridScreen.Visibility = Visibility.Collapsed;
            gridFile.Visibility = Visibility.Collapsed;
            gridHelp.Visibility = Visibility.Visible;
            gridLicense.Visibility = Visibility.Collapsed;
        }

        private void License_Click(object sender, RoutedEventArgs e)
        {
            btnEmulator.Background = new SolidColorBrush(Color.FromArgb(255, 128, 128, 128));
            btnFile.Background = new SolidColorBrush(Color.FromArgb(255, 128, 128, 128));
            btnHelp.Background = new SolidColorBrush(Color.FromArgb(255, 128, 128, 128));
            btnLicense.Background = new SolidColorBrush(Color.FromArgb(255, 64, 255, 32));
            gridScreen.Visibility = Visibility.Collapsed;
            gridFile.Visibility = Visibility.Collapsed;
            gridHelp.Visibility = Visibility.Collapsed;
            gridLicense.Visibility = Visibility.Visible;
        }

        private void Page_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            // Do not repeat keys!
            if (!e.KeyStatus.WasKeyDown && mainPage != null)
            {
                // Skip disassemble on F12, will be done on key release:
                if (e.Key == VirtualKey.F12)
                {
                    return;
                }
                // Skip reset on Esc key, will be done on key release:
                else if (e.Key == VirtualKey.Escape)
                {
                    return;
                }

                // Scan codes:
                // Shift left  = 0x2a  send to UK101 as 0x12
                // Shift right = 0x36  send to UK101 as 0x10
                // Caps lock   = 0x3a  send to UK101 as 0x14
                // Ctrl left   = 0x1d  send to UK101 as 0x11
                // Ctrl right  = 0x1d  send to UK101 as 0x11
                // Arrow up    = 0x48  send to UK101 as 0xba
                // Enter       = 0x1c  send to UK101 as 0x0d
                if (e.KeyStatus.ScanCode == 0x2a)
                {
                    mainPage.CSignetic6502.MemoryBus.Keyboard.PressKey((byte)0x12);
                }
                else if (e.KeyStatus.ScanCode == 0x36)
                {
                    mainPage.CSignetic6502.MemoryBus.Keyboard.PressKey((byte)0x10);
                }
                else if (e.KeyStatus.ScanCode == 0x3a)
                {
                    mainPage.CSignetic6502.MemoryBus.Keyboard.PressKey((byte)0x14);
                }
                else if (e.KeyStatus.ScanCode == 0x1d)
                {
                    mainPage.CSignetic6502.MemoryBus.Keyboard.PressKey((byte)0x11);
                }
                else if (e.KeyStatus.ScanCode == 0x48)
                {
                    mainPage.CSignetic6502.MemoryBus.Keyboard.PressKey((byte)0xba);
                }
                else if (e.KeyStatus.ScanCode == 0x1c)
                {
                    mainPage.CSignetic6502.MemoryBus.Keyboard.PressKey((byte)0x0d);
                }
                else
                {
                    mainPage.CSignetic6502.MemoryBus.Keyboard.PressKey((byte)e.Key);
                }

                SetCapsLock();
            }
        }

        private void Page_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            // Disassemble on F12:
            if (e.Key == VirtualKey.F12)
            {
                mainPage.CClock.Timer.Stop();
                mainPage.CSignetic6502.Reset();
                mainPage.CSignetic6502.Dissassemble();
            }
            // Reset on Esc key:
            else if (e.Key == VirtualKey.Escape)
            {
                mainPage.CClock.Timer.Stop();
                mainPage.CSignetic6502.Reset();
                mainPage.CClock.Timer.Start();
            }
            if (/*e.KeyStatus.WasKeyDown && */mainPage != null)
            {

                // Scan codes:
                // Shift left  = 0x2a  send to UK101 as 0x12
                // Shift right = 0x36  send to UK101 as 0x10
                // Caps lock   = 0x3a  send to UK101 as 0x14
                // Ctrl left   = 0x1d  send to UK101 as 0x11
                // Ctrl right  = 0x1d  send to UK101 as 0x11
                // Arrow up    = 0x48  send to UK101 as 0xba
                // Enter       = 0x1c  send to UK101 as 0x0d
                if (e.KeyStatus.ScanCode == 0x2a)
                {
                    mainPage.CSignetic6502.MemoryBus.Keyboard.ReleaseKey((byte)0x12);
                }
                else if (e.KeyStatus.ScanCode == 0x36)
                {
                    mainPage.CSignetic6502.MemoryBus.Keyboard.ReleaseKey((byte)0x10);
                }
                else if (e.KeyStatus.ScanCode == 0x3a)
                {
                    mainPage.CSignetic6502.MemoryBus.Keyboard.ReleaseKey((byte)0x14);
                }
                else if (e.KeyStatus.ScanCode == 0x1d)
                {
                    mainPage.CSignetic6502.MemoryBus.Keyboard.ReleaseKey((byte)0x11);
                }
                else if (e.KeyStatus.ScanCode == 0x48)
                {
                    mainPage.CSignetic6502.MemoryBus.Keyboard.ReleaseKey((byte)0xba);
                }
                else if (e.KeyStatus.ScanCode == 0x1c)
                {
                    mainPage.CSignetic6502.MemoryBus.Keyboard.ReleaseKey((byte)0x0d);
                }
                else
                {
                    mainPage.CSignetic6502.MemoryBus.Keyboard.ReleaseKey((byte)e.Key);
                    //mainPage.CSignetic6502.MemoryBus.Keyboard.ReleaseKey((byte)e.KeyStatus.ScanCode);
                }

                SetCapsLock();
            }
        }

        private void SetCapsLock()
        {
            keystate = Window.Current.CoreWindow.GetKeyState(VirtualKey.CapitalLock);
            capsLock = (keystate & CoreVirtualKeyStates.Locked) != 0;
            if (capsLock)
            {
                mainPage.CSignetic6502.MemoryBus.Keyboard.Keystates[7] &= 0xfe;
            }
            else
            {
                mainPage.CSignetic6502.MemoryBus.Keyboard.Keystates[7] |= 0x01;
            }
        }

        private void BtnEmulator_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.KeyStatus.ScanCode == 0x1c || e.KeyStatus.ScanCode == 0x39)
            {
                Page_KeyDown(sender, e);
            }
        }

        private void BtnEmulator_PreviewKeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.KeyStatus.ScanCode == 0x1c || e.KeyStatus.ScanCode == 0x39)
            {
                Page_KeyUp(sender, e);
            }
        }
    }
}
