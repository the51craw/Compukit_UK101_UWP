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
        public MainPage()
        {
            this.InitializeComponent();
            mainPage = this;
            handleControlEvents = false;
            Init();
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
            cbSelectACIAUsage.SelectedIndex = 0;
            Editor = new Editor(this);
            handleControlEvents = true;
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
            gridFile.Visibility = Visibility.Collapsed;
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
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.AssemblerInstructions;
        }


        private void BtnAssembler_01_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.Assembler_01;
        }

        private void BtnAsteroidShoot_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.AsteroidShoot;
        }

        private void BtnBestFitPolynomial16_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.BestFitPolynomial16;
        }

        private void BtnBestFitPolynomial32_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.BestFitPolynomial32;
        }

        private void BtnBiorythms_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.Biorythms;
        }

        private void BtnBreakout_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.Breakout;
        }

        private void BtnChessboard16_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.Chessboard16;
        }

        private void BtnChessboard32_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.Chessboard32;
        }

        private void BtnDockingASpaceship_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.DockingASpaceship;
        }

        private void BtnDogfight_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.Dogfight;
        }

        private void BtnDrawPictures_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.DrawPictures;
        }

        private void BtnDungeonAdventure_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.DungeonAdventure;
        }

        private void BtnFourierSeries_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.FourierSeries;
        }

        private void BtnGolf16_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.Golf16;
        }

        private void BtnGolf32_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.Golf32;
        }

        private void BtnGraphicAid_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.GraphicAid;
        }

        private void BtnGraphPlotter_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.GraphPlotter;
        }

        private void BtnGunfight_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.Gunfight;
        }

        private void BtnHangman_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.Hangman;
        }

        private void BtnHexapawn_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.Hexapawn;
        }

        private void BtnKing_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.King;
        }

        private void BtnLife_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.Life;
        }

        private void BtnMastermind_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.Mastermind;
        }

        private void BtnMaze_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.Maze;
        }

        private void BtnMoonLanding_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.MoonLanding;
        }

        private void BtnMugwump_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.Mugwump;
        }

        private void BtnNim_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.Nim;
        }

        private void BtnNoughtsAndCrosses_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.NoughtsAndCrosses;
        }

        private void BtnOnScreenEditor_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.OnScreenEditor;
        }

        private void BtnRealTimeStarTrek_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.RealTimeStarTrek;
        }

        private void BtnRealTimeStarTrekInstructions_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.RealTimeStarTrekInstructions;
        }

        private void BtnRobotChase_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.RobotChase;
        }

        private void BtnSolvingSimultaneousEquations_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.SolvingSimultaneousEquations;
        }

        private void BtnSpaceInvaders_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.SpaceInvaders;
        }

        private void BtnSpaceWar_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.SpaceWar;
        }

        private void BtnStarTrek_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.StarTrek;
        }

        private void BtnStarTrek3_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.StarTrek3;
        }

        private void BtnStarTrek32_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.StarTrek32;
        }

        private void BtnStarTrek4_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.StarTrek4;
        }

        private void BtnStarTrekInstructions_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.StarTrekInstructions;
        }

        private void BtnStarTrekInstructions1_32_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.StarTrekInstructions1_32;
        }

        private void BtnStarTrekInstructions2_32_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.StarTrekInstructions2_32;
        }

        private void BtnSurround_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.Surround;
        }

        private void BtnTankBattle_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.TankBattle;
        }

        private void BtnTheTowerOfBrahma32_Click(object sender, RoutedEventArgs e)
        {
            CSignetic6502.MemoryBus.ACIA.lines = CSignetic6502.MemoryBus.ACIA.basicProg.TheTowerOfBrahma32;
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
    }
}
