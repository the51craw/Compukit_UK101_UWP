using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
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
using Windows.Storage;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.Data.Pdf;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.ObjectModel;
using Windows.UI.ViewManagement;

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

        public MIDI Midi { get; set; }

        public Editor Editor { get; set; }

        public Boolean handleControlEvents { get; set; }

        private Boolean numLock = false;
        private CoreVirtualKeyStates keystate;

        public ObservableCollection<BitmapImage> HistoryPages
        {
            get;
            set;
        } = new ObservableCollection<BitmapImage>();

        public ObservableCollection<BitmapImage> TheProjectPages
        {
            get;
            set;
        } = new ObservableCollection<BitmapImage>();

        public ObservableCollection<BitmapImage> OperationPages
        {
            get;
            set;
        } = new ObservableCollection<BitmapImage>();

        public ObservableCollection<BitmapImage> ComposerPages
        {
            get;
            set;
        } = new ObservableCollection<BitmapImage>();

        public ObservableCollection<BitmapImage> UK101Pages
        {
            get;
            set;
        } = new ObservableCollection<BitmapImage>();

        public ObservableCollection<BitmapImage> CegmonPages
        {
            get;
            set;
        } = new ObservableCollection<BitmapImage>();

        public ObservableCollection<BitmapImage> LicensePages
        {
            get;
            set;
        } = new ObservableCollection<BitmapImage>();

        public MainPage()
        {
            this.InitializeComponent();
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(1200, 900));
            handleControlEvents = false;
            Init();
            InitDocuments();
        }

        private void Init()
        {
            CSignetic6502 = new CSignetic6502(mainPage);
            CClock = new CClock(this);
            CSignetic6502.MemoryBus.VDU.InitCVDU(this, gridScreen);
            cbSelectNumberOfLines.SelectedIndex = 1;
            keystate = Window.Current.CoreWindow.GetKeyState(VirtualKey.NumberKeyLock);
            numLock = (keystate & CoreVirtualKeyStates.Locked) != 0;
            keystate = Window.Current.CoreWindow.GetKeyState(VirtualKey.CapitalLock);
            capsLock = (keystate & CoreVirtualKeyStates.Locked) != 0;
            Midi = new MIDI(this);
            SetPage(0);
            btnHistory.Background = new SolidColorBrush(Color.FromArgb(255, 160, 160, 64));
            btnTheProject.Background = new SolidColorBrush(Color.FromArgb(255, 160, 160, 160));
            btnOperation.Background = new SolidColorBrush(Color.FromArgb(255, 160, 160, 160));
            btnManuals.Background = new SolidColorBrush(Color.FromArgb(255, 160, 160, 160));
            btnCompukitUK101.Background = new SolidColorBrush(Color.FromArgb(255, 160, 160, 64));
            btnCegmon.Background = new SolidColorBrush(Color.FromArgb(255, 160, 160, 160));
            cbSelectACIAUsage.SelectedIndex = 0;
            Editor = new Editor(this);
            handleControlEvents = true;
        }

        private void InitDocuments()
        {
            LoadDocument("History", HistoryPages);
            LoadDocument("TheProject", TheProjectPages);
            LoadDocument("Operation", OperationPages);
            LoadDocument("Composer", ComposerPages);
            LoadDocument("CompukitManual", UK101Pages);
            LoadDocument("CegmonManual", CegmonPages);
            LoadDocument("License", LicensePages);
        }

        private async void LoadDocument(String Name, ObservableCollection<BitmapImage> Pages)
        {
            StorageFile f = await
                StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Documents/" + Name + ".pdf"));
            PdfDocument pdfDoc = await PdfDocument.LoadFromFileAsync(f);

            Pages.Clear();

            for (uint i = 0; i < pdfDoc.PageCount; i++)
            {
                BitmapImage image = new BitmapImage();

                var page = pdfDoc.GetPage(i);

                using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                {
                    await page.RenderToStreamAsync(stream);
                    await image.SetSourceAsync(stream);
                }

                Pages.Add(image);
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Page switching handlers
        //////////////////////////////////////////////////////////////////////////////////////////////

        private void btnEmulator_Click(object sender, RoutedEventArgs e)
        {
            SetPage(0);
        }

        private void btnBasicFiles_Click(object sender, RoutedEventArgs e)
        {
            SetPage(1);
        }

        private void btnComposerEdit_Click(object sender, RoutedEventArgs e)
        {
            SetPage(2);
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            SetPage(3);
        }

        private void License_Click(object sender, RoutedEventArgs e)
        {
            SetPage(4);
        }

        private void SetPage(Int32 page)
        {
            btnEmulator.Background = new SolidColorBrush(Color.FromArgb(255, 160, 160, 160));
            cbSelectNumberOfLines.Background = new SolidColorBrush(Color.FromArgb(255, 160, 160, 160));
            cbSelectMIDIInputDevice.Background = new SolidColorBrush(Color.FromArgb(255, 160, 160, 160));
            cbSelectMIDIOutputDevice.Background = new SolidColorBrush(Color.FromArgb(255, 160, 160, 160));
            cbSelectACIAUsage.Background = new SolidColorBrush(Color.FromArgb(255, 160, 160, 160));
            btnBasicFiles.Background = new SolidColorBrush(Color.FromArgb(255, 160, 160, 160));
            btnComposerEdit.Background = new SolidColorBrush(Color.FromArgb(255, 160, 160, 160));
            btnHelp.Background = new SolidColorBrush(Color.FromArgb(255, 160, 160, 160));
            btnLicense.Background = new SolidColorBrush(Color.FromArgb(255, 160, 160, 160));
            gridScreen.Visibility = Visibility.Collapsed;
            gridBasicFiles.Visibility = Visibility.Collapsed;
            gridEdit.Visibility = Visibility.Collapsed;
            gridHelp.Visibility = Visibility.Collapsed;
            gridLicense.Visibility = Visibility.Collapsed;

            switch (page)
            {
                case 0:
                    cbSelectNumberOfLines.Background = new SolidColorBrush(Color.FromArgb(255, 160, 160, 64));
                    cbSelectMIDIInputDevice.Background = new SolidColorBrush(Color.FromArgb(255, 160, 160, 64));
                    cbSelectMIDIOutputDevice.Background = new SolidColorBrush(Color.FromArgb(255, 160, 160, 64));
                    cbSelectACIAUsage.Background = new SolidColorBrush(Color.FromArgb(255, 160, 160, 64));
                    btnEmulator.Background = new SolidColorBrush(Color.FromArgb(255, 160, 160, 64));
                    gridScreen.Visibility = Visibility.Visible;
                    break;
                case 1:
                    btnBasicFiles.Background = new SolidColorBrush(Color.FromArgb(255, 160, 160, 64));
                    gridBasicFiles.Visibility = Visibility.Visible;
                    break;
                case 2:
                    btnComposerEdit.Background = new SolidColorBrush(Color.FromArgb(255, 160, 160, 64));
                    gridEdit.Visibility = Visibility.Visible;
                    break;
                case 3:
                    btnHelp.Background = new SolidColorBrush(Color.FromArgb(255, 160, 160, 64));
                    gridHelp.Visibility = Visibility.Visible;
                    break;
                case 4:
                    gridLicense.Visibility = Visibility.Visible;
                    btnLicense.Background = new SolidColorBrush(Color.FromArgb(255, 160, 160, 64));
                    break;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Main page handlers (except page swithing)
        //////////////////////////////////////////////////////////////////////////////////////////////

        private void CbSelectNumberOfLines_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (handleControlEvents)
            {
                CClock.Hold = true;
                CClock.Timer.Stop();
                CSignetic6502.MemoryBus.VDU.ClearScreen();
                CSignetic6502.MemoryBus.VDU.NumberOfLines = 
                    (byte)(16 + 16 * cbSelectNumberOfLines.SelectedIndex);
                CSignetic6502.Reset();
                CClock.Hold = false;
                CClock.Timer.Start();
            }
            btnEmulator.Focus(FocusState.Programmatic);
        }

        private async void CbSelectACIAUsage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cbSelectACIAUsage.SelectedIndex)
            {
                case 1:
                    CSignetic6502.MemoryBus.ACIA.Mode = CACIA.IO_MODE_6820_TAPE;
                    break;
                case 2:
                    CSignetic6502.MemoryBus.ACIA.Mode = CACIA.IO_MODE_6820_MIDI;
                    if (!Midi.MidiIsReady())
                    {
                        Midi.ResetMidiInput();
                        Midi.ResetMidiOutput();
                        await Midi.MakeMidiDeviceLists();

                        foreach (string device in Midi.MidiInputDevices)
                        {
                            cbSelectMIDIInputDevice.Items.Add("In: " + device);
                        }

                        if (cbSelectMIDIInputDevice.Items.Count() > 0)
                        {
                            cbSelectMIDIInputDevice.SelectedIndex = 0;
                        }

                        foreach (string device in Midi.MidiOutputDevices)
                        {
                            cbSelectMIDIOutputDevice.Items.Add("Out: " + device);
                        }

                        if (cbSelectMIDIOutputDevice.Items.Count() > 0)
                        {
                            cbSelectMIDIOutputDevice.SelectedIndex = 0;
                        }
                    }
                    break;
                case 3: // Serial
                    CSignetic6502.MemoryBus.ACIA.Mode = CACIA.IO_MODE_6820_SERIAL;
                    break;
                case 4: // Load file
                    await LoadAFile();
                    CSignetic6502.MemoryBus.ACIA.Mode = CACIA.IO_MODE_6820_FILE;
                    cbSelectACIAUsage.SelectedIndex = 0;
                    break;
                case 5: // Save file
                    await SaveAFile();
                    CSignetic6502.MemoryBus.ACIA.Mode = CACIA.IO_MODE_6820_FILE;
                    cbSelectACIAUsage.SelectedIndex = 0;
                    break;
                case 6: // Close file
                    CloseAFile();
                    CSignetic6502.MemoryBus.ACIA.Mode = CACIA.IO_MODE_6820_FILE;
                    cbSelectACIAUsage.SelectedIndex = 0;
                    break;
            }
            btnEmulator.Focus(FocusState.Programmatic);
        }

        private async Task LoadAFile()
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.FileTypeFilter.Add(".bas");
            fileOpenPicker.FileTypeFilter.Add(".asm");
            StorageFile CurrentFile = await fileOpenPicker.PickSingleFileAsync();
            if (CurrentFile != null)
            {
                IRandomAccessStreamWithContentType x = await CurrentFile.OpenReadAsync();
                CSignetic6502.MemoryBus.ACIA.FileInputStream = x.AsStreamForRead();
                CSignetic6502.MemoryBus.ACIA.FileInputStreamLength =
                    CSignetic6502.MemoryBus.ACIA.FileInputStream.Seek(0, SeekOrigin.End);
                CSignetic6502.MemoryBus.ACIA.FileInputStream.Seek(0, SeekOrigin.Begin);
            }
        }

        private async Task SaveAFile()
        {
            FileSavePicker fileSavePicker = new FileSavePicker();
            fileSavePicker.FileTypeChoices.Add("Basic files", new List<string>() { ".bas" });
            fileSavePicker.FileTypeChoices.Add("Assembler files", new List<string>() { ".asm" });
            StorageFile x = await fileSavePicker.PickSaveFileAsync();
            CSignetic6502.MemoryBus.ACIA.FileOutputStream = await x.OpenStreamForWriteAsync();
            CSignetic6502.MemoryBus.ACIA.Mode = CACIA.IO_MODE_6820_FILE;
        }

        private void CloseAFile()
        {
            CSignetic6502.MemoryBus.ACIA.FileOutputStream.Flush();
            CSignetic6502.MemoryBus.ACIA.FileOutputStream.Close();
            CSignetic6502.MemoryBus.ACIA.FileOutputStream.Dispose();
            CSignetic6502.MemoryBus.ACIA.FileOutputStream = null;
        }

        private async void CbSelectMIDIInputDevice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Midi.ResetMidiInput();
            if (cbSelectMIDIInputDevice != null && cbSelectMIDIInputDevice.SelectedIndex > -1)
            {
                await Midi.InitInput(((string)cbSelectMIDIInputDevice.SelectedItem).Replace("In: ", ""));
                btnEmulator.Focus(FocusState.Programmatic);
            }
        }

        private async void CbSelectMIDIOutputDevice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Midi.ResetMidiOutput();
            if (cbSelectMIDIOutputDevice != null && cbSelectMIDIOutputDevice.SelectedIndex > -1)
            {
                await Midi.InitOutput(((string)cbSelectMIDIOutputDevice.SelectedItem).Replace("Out: ", ""));
                btnEmulator.Focus(FocusState.Programmatic);
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Emulator page event handlers
        //////////////////////////////////////////////////////////////////////////////////////////////

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

        private void GridScreen_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            btnEmulator.Focus(FocusState.Programmatic);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Basic files event handlers
        //////////////////////////////////////////////////////////////////////////////////////////////

        private void BtnAssemblerInstructions_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.AssemblerInstructions;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }


        private void BtnAssembler_01_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.Assembler_01;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnAsteroidShoot_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.AsteroidShoot;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnBestFitPolynomial16_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.BestFitPolynomial16;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnBestFitPolynomial32_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.BestFitPolynomial32;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnBiorythms_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.Biorythms;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnBreakout_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.Breakout;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnChessboard16_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.Chessboard16;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnChessboard32_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.Chessboard32;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnDockingASpaceship_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.DockingASpaceship;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnDogfight_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.Dogfight;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnDrawPictures_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.DrawPictures;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnDungeonAdventure_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.DungeonAdventure;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnFourierSeries_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.FourierSeries;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnGolf16_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.Golf16;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnGolf32_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.Golf32;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnGraphicAid_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.GraphicAid;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnGraphPlotter_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.GraphPlotter;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnGunfight_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.Gunfight;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnHangman_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.Hangman;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnHexapawn_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.Hexapawn;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnKing_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.King;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnLife_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.Life;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnMastermind_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.Mastermind;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnMaze_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.Maze;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnMoonLanding_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.MoonLanding;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnMugwump_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.Mugwump;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnNim_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.Nim;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnNoughtsAndCrosses_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.NoughtsAndCrosses;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnOnScreenEditor_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.OnScreenEditor;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnRealTimeStarTrek_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.RealTimeStarTrek;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnRealTimeStarTrekInstructions_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.RealTimeStarTrekInstructions;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnRobotChase_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.RobotChase;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnSolvingSimultaneousEquations_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.SolvingSimultaneousEquations;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnSpaceInvaders_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.SpaceInvaders;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnSpaceWar_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.SpaceWar;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnStarTrek_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.StarTrek;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnStarTrek3_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.StarTrek3;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnStarTrek32_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.StarTrek32;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnStarTrek4_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.StarTrek4;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnStarTrekInstructions_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.StarTrekInstructions;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnStarTrekInstructions1_32_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.StarTrekInstructions1_32;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnStarTrekInstructions2_32_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.StarTrekInstructions2_32;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnSurround_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.Surround;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnTankBattle_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.TankBattle;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }

        private void BtnTheTowerOfBrahma32_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.Lines = CSignetic6502.MemoryBus.ACIA.basicProg.TheTowerOfBrahma32;
            CSignetic6502.MemoryBus.ACIA.line = 0;
        }
        //////////////////////////////////////////////////////////////////////////////////////////////
        // ACIA and MIDI PAGE event handlers and functions
        //////////////////////////////////////////////////////////////////////////////////////////////

        private void BtnNewPart_Click(object sender, RoutedEventArgs e)
        {
            Part part = new Part(this);
            Editor.parts.Add(part);
            AddPart(part);
        }

        public void AddPart(Part part)
        {
            part.Name = "Part_" + (cbPart.Items.Count() + 1).ToString();
            cbPart.Items.Add(part.Name);
            cbPart.SelectedIndex = cbPart.Items.Count() - 1;
        }

        private void TbPartName_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                Int32 position = cbPart.SelectedIndex;
                String name = (String)cbPart.SelectedItem;
                cbPart.Items.RemoveAt(position);
                cbPart.Items.Insert(position, tbPartName.Text);
                cbPart.SelectedIndex = position;
                Editor.ChangePartName(name, (String)cbPart.SelectedItem);
            } catch { }
        }

        private void TbPartContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                Editor.parts[cbPart.SelectedIndex].SourceCode = tbPartContent.Text;
            } catch { }
        }

        private void CbPart_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                tbPartName.Text = (String)cbPart.SelectedItem;
                tbPartContent.Text = Editor.parts[cbPart.SelectedIndex].SourceCode;
            } catch { }
        }

        private void BtnInsertPart_Click(object sender, RoutedEventArgs e)
        {

            tbEditorContent.Text = tbEditorContent.Text
                .Insert(tbEditorContent.SelectionStart, "\r\n" + tbPartName.Text + " " + tbPartContent.Text);
        }

        private void BtnReceiveFile_Click(object sender, RoutedEventArgs e)
        {
            Editor.AddPart(CSignetic6502.MemoryBus.ACIA.outStream.ToArray());
        }

        private void BtnSendFile_Click(object sender, RoutedEventArgs e)
        {
            if (Editor.Compile())
            {
                CSignetic6502.MemoryBus.ACIA.inStream =
                    new System.IO.MemoryStream(Editor.Song.ObjectCode);
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Helpers
        //////////////////////////////////////////////////////////////////////////////////////////////

        public String GetPartContent()
        {
            return tbPartContent.Text;
        }

        public String GetEditorContent()
        {
            return tbEditorContent.Text;
        }

        public async void MessageBox(String Message)
        {
            MessageDialog warning = new MessageDialog(Message);
            warning.Title = "Warning!";
            warning.Commands.Add(new UICommand { Label = "Ok", Id = 0 });
            var response = await warning.ShowAsync();
        }

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            SetHelpPage(0);
        }

        private void BtnTheProject_Click(object sender, RoutedEventArgs e)
        {
            SetHelpPage(1);
        }

        private void BtnOperation_Click(object sender, RoutedEventArgs e)
        {
            SetHelpPage(2);
        }

        private void BtnComposer_Click(object sender, RoutedEventArgs e)
        {
            SetHelpPage(3);
        }

        private void BtnManuals_Click(object sender, RoutedEventArgs e)
        {
            SetHelpPage(4);
        }

        private void SetHelpPage(Int32 page)
        {
            btnHistory.Background = new SolidColorBrush(Color.FromArgb(255, 160, 160, 160));
            btnTheProject.Background = new SolidColorBrush(Color.FromArgb(255, 160, 160, 160));
            btnOperation.Background = new SolidColorBrush(Color.FromArgb(255, 160, 160, 160));
            btnComposer.Background = new SolidColorBrush(Color.FromArgb(255, 160, 160, 160));
            btnManuals.Background = new SolidColorBrush(Color.FromArgb(255, 160, 160, 160));
            gridHistory.Visibility = Visibility.Collapsed;
            gridTheProject.Visibility = Visibility.Collapsed;
            gridOperation.Visibility = Visibility.Collapsed;
            gridComposer.Visibility = Visibility.Collapsed;
            gridManuals.Visibility = Visibility.Collapsed;

            switch (page)
            {
                case 0:
                    btnHistory.Background = new SolidColorBrush(Color.FromArgb(255, 160, 160, 64));
                    gridHistory.Visibility = Visibility.Visible;
                    break;
                case 1:
                    btnTheProject.Background = new SolidColorBrush(Color.FromArgb(255, 160, 160, 64));
                    gridTheProject.Visibility = Visibility.Visible;
                    break;
                case 2:
                    btnOperation.Background = new SolidColorBrush(Color.FromArgb(255, 160, 160, 64));
                    gridOperation.Visibility = Visibility.Visible;
                    break;
                case 3:
                    btnComposer.Background = new SolidColorBrush(Color.FromArgb(255, 160, 160, 64));
                    gridComposer.Visibility = Visibility.Visible;
                    break;
                case 4:
                    gridManuals.Visibility = Visibility.Visible;
                    btnManuals.Background = new SolidColorBrush(Color.FromArgb(255, 160, 160, 64));
                    break;
            }
        }

        private async void BtnCompukitUK101_Click(object sender, RoutedEventArgs e)
        {
            svUK101.Visibility = Visibility.Visible;
            svCegmon.Visibility = Visibility.Collapsed;
            btnCegmon.Background = new SolidColorBrush(Color.FromArgb(255, 160, 160, 160));
            btnCompukitUK101.Background = new SolidColorBrush(Color.FromArgb(255, 160, 160, 64));
            //StorageFile f = await
            //    StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Documents/CompukitManual.pdf"));
            //PdfDocument pdfDoc = await PdfDocument.LoadFromFileAsync(f);
            //Load(pdfDoc);
        }

        private async void BtnCegmon_Click(object sender, RoutedEventArgs e)
        {
            svUK101.Visibility = Visibility.Collapsed;
            svCegmon.Visibility = Visibility.Visible;
            btnCompukitUK101.Background = new SolidColorBrush(Color.FromArgb(255, 160, 160, 160));
            btnCegmon.Background = new SolidColorBrush(Color.FromArgb(255, 160, 160, 64));
            //StorageFile f = await
            //    StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Documents/CegmonManual.pdf"));
            //PdfDocument pdfDoc = await PdfDocument.LoadFromFileAsync(f);
            //Load(pdfDoc);
        }

        //private async void Load(PdfDocument pdfDoc)
        //{ 
        //    PdfPages.Clear();

        //    for (uint i = 0; i < pdfDoc.PageCount; i++)
        //    {
        //        BitmapImage image = new BitmapImage();

        //        var page = pdfDoc.GetPage(i);

        //        using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
        //        {
        //            await page.RenderToStreamAsync(stream);
        //            await image.SetSourceAsync(stream);
        //        }

        //        PdfPages.Add(image);
        //    }
        //}
    }
}
