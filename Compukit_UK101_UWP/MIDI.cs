using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Midi;
using Windows.UI.Xaml.Controls;
using Windows.UI.Core;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Compukit_UK101_UWP
{
    public class MIDI
    {
        //public CoreDispatcher coreDispatcher;
        public IMidiOutPort midiOutPort;
        public MidiInPort midiInPort;
        public byte DeviceID = 0x10;
        public byte MidiOutPortChannel { get; set; }
        public byte MidiInPortChannel { get; set; }
        public Int32 MidiOutPortSelectedIndex { get; set; }
        public Int32 MidiInPortSelectedIndex { get; set; }
        public Boolean VenderDriverPresent = false;
        public List<String> MidiInputDevices { get; set; }
        public List<String> MidiOutputDevices { get; set; }

        private MainPage mainPage;

        public MIDI(MainPage mainPage)
        {
            this.mainPage = mainPage;
        }

        // Constructor using a combobox for full device watch:
        //public MIDI(MainPage mainPage, ComboBox OutputDeviceSelector, ComboBox InputDeviceSelector, byte MidiOutPortChannel, byte MidiInPortChannel)
        //{
        //    this.mainPage = mainPage;
        //    //coreDispatcher.ProcessEvents(CoreProcessEventsOption.ProcessAllIfPresent);
        //    //midiOutputDeviceWatcher.StartWatcher();
        //    //midiInputDeviceWatcher.StartWatcher();
        //    this.MidiOutPortChannel = MidiOutPortChannel;
        //    this.MidiInPortChannel = MidiInPortChannel;
        //    //Init("INTEGRA-7");
        //    //Init("MIDI");
        //}

        ~MIDI()
        {
            try
            {
                midiOutPort.Dispose();
                midiInPort.Dispose();
                midiOutPort = null;
                midiInPort = null;
            } catch { }
        }

        public void ResetMidiInput()
        {
            try
            {
                if (midiInPort != null)
                {
                    midiInPort.MessageReceived -= MidiInPort_MessageReceived;
                    midiInPort.Dispose();
                    midiInPort = null;
                    GC.Collect();
                }
            }
            catch { }
        }

        public void ResetMidiOutput()
        {
            try
            {
                if (midiOutPort != null)
                {
                    midiOutPort.Dispose();
                    midiOutPort = null;
                    GC.Collect();
                }
            }
            catch { }
        }

        public async Task InitInput(String inputDeviceName)
        {
            DeviceInformationCollection midiInputDevices = await DeviceInformation.FindAllAsync(MidiInPort.GetDeviceSelector());
            DeviceInformation midiInDevInfo = null;

            foreach (DeviceInformation device in midiInputDevices)
            {
                if (device.Name.Contains(inputDeviceName) && !device.Name.Contains("CTRL"))
                {
                    midiInDevInfo = device;
                    break;
                }
            }

            if (midiInDevInfo != null)
            {
                midiInPort = await MidiInPort.FromIdAsync(midiInDevInfo.Id);
            }

            if (midiInPort == null)
            {
                System.Diagnostics.Debug.WriteLine("Unable to create MidiInPort from input device");
            }
            else
            {
                midiInPort.MessageReceived += MidiInPort_MessageReceived;
            }
        }

        public async Task InitOutput(String outputDeviceName)
        {
            DeviceInformationCollection midiOutputDevices = await DeviceInformation.FindAllAsync(MidiOutPort.GetDeviceSelector());
            DeviceInformation midiOutDevInfo = null;

            foreach (DeviceInformation device in midiOutputDevices)
            {
                if (device.Name.Contains(outputDeviceName) && !device.Name.Contains("CTRL"))
                {
                    midiOutDevInfo = device;
                    break;
                }
            }

            if (midiOutDevInfo != null)
            {
                midiOutPort = await MidiOutPort.FromIdAsync(midiOutDevInfo.Id);
            }

            if (midiOutPort == null)
            {
                System.Diagnostics.Debug.WriteLine("Unable to create MidiOutPort from output device");
            }
        }

        private void MidiInPort_MessageReceived(MidiInPort sender, MidiMessageReceivedEventArgs args)
        {
            IMidiMessage receivedMidiMessage = args.Message;
            byte[] rawData = receivedMidiMessage.RawData.ToArray();
            mainPage.CSignetic6502.MemoryBus.ACIA.MidiInPort_MessageReceived(rawData);
        }

        public List<String> GetMidiDeviceList()
        {
            return MidiInputDevices;
        }

        public async Task MakeMidiDeviceLists()
        {
            DeviceInformationCollection midiInputDevices = await DeviceInformation.FindAllAsync(MidiInPort.GetDeviceSelector());
            MidiInputDevices = new List<String>();
            foreach (DeviceInformation device in midiInputDevices)
            {
                if (device.Name.Contains("["))
                {
                    MidiInputDevices.Add(device.Name.Remove(device.Name.IndexOf('[')));
                }
                else
                {
                    MidiInputDevices.Add(device.Name);
                }
            }
            DeviceInformationCollection midiOutputDevices = await DeviceInformation.FindAllAsync(MidiOutPort.GetDeviceSelector());
            MidiOutputDevices = new List<String>();
            foreach (DeviceInformation device in midiOutputDevices)
            {
                if (device.Name.Contains("["))
                {
                    MidiOutputDevices.Add(device.Name.Remove(device.Name.IndexOf('[')));
                }
                else
                {
                    MidiOutputDevices.Add(device.Name);
                }
            }
        }

        public Boolean MidiIsReady()
        {
            return midiInPort != null && midiOutPort != null;
        }

        public async Task CheckForVenderDriver()
        {
            DeviceInformationCollection collection = await DeviceInformation.FindAllAsync();
            foreach (DeviceInformation dev in collection)
            {
                if (dev.Name.ToLower().Contains("integra")
                    && dev.Id.ToLower().Contains("wave")
                    && dev.IsEnabled)
                {
                    VenderDriverPresent = true;
                    break;
                }
            }
        }

        public Boolean VenderDriverDetected()
        {
            return VenderDriverPresent;
        }

        public void NoteOn(byte currentChannel, byte noteNumber, byte velocity)
        {
            if (midiOutPort != null)
            {
                IMidiMessage midiMessageToSend = new MidiNoteOnMessage(currentChannel, noteNumber, velocity);
                midiOutPort.SendMessage(midiMessageToSend);
            }
        }

        public void NoteOff(byte currentChannel, byte noteNumber)
        {
            if (midiOutPort != null)
            {
                IMidiMessage midiMessageToSend = new MidiNoteOnMessage(currentChannel, noteNumber, 0);
                midiOutPort.SendMessage(midiMessageToSend);
            }
        }

        public void SendControlChange(byte channel, byte controller, byte value)
        {
            if (midiOutPort != null)
            {
                IMidiMessage midiMessageToSend = new MidiControlChangeMessage(channel, controller, value);
                midiOutPort.SendMessage(midiMessageToSend);
            }
        }

        public void SetVolume(byte currentChannel, byte volume)
        {
            if (midiOutPort != null)
            {
                IMidiMessage midiMessageToSend = new MidiControlChangeMessage(currentChannel, 0x07, volume);
                midiOutPort.SendMessage(midiMessageToSend);
            }
        }

        public void AllNotesOff(byte currentChannel)
        {
            if (midiOutPort != null)
            {
                IMidiMessage midiMessageToSend = new MidiControlChangeMessage(currentChannel, 0x78, 0);
                midiOutPort.SendMessage(midiMessageToSend);
            }
        }

        public void ProgramChange(byte currentChannel, String smsb, String slsb, String spc)
        {
            try
            {
                MidiControlChangeMessage controlChangeMsb = new MidiControlChangeMessage(currentChannel, 0x00, (byte)(UInt16.Parse(smsb)));
                MidiControlChangeMessage controlChangeLsb = new MidiControlChangeMessage(currentChannel, 0x20, (byte)(UInt16.Parse(slsb)));
                MidiProgramChangeMessage programChange = new MidiProgramChangeMessage(currentChannel, (byte)(UInt16.Parse(spc) - 1));
                midiOutPort.SendMessage(controlChangeMsb);
                midiOutPort.SendMessage(controlChangeLsb);
                midiOutPort.SendMessage(programChange);
            }
            catch { }
        }

        public void ProgramChange(byte currentChannel, byte msb, byte lsb, byte pc)
        {
            try
            {
                MidiControlChangeMessage controlChangeMsb = new MidiControlChangeMessage(currentChannel, 0x00, msb);
                MidiControlChangeMessage controlChangeLsb = new MidiControlChangeMessage(currentChannel, 0x20, lsb);
                MidiProgramChangeMessage programChange = new MidiProgramChangeMessage(currentChannel, (byte)(pc - 1));
                midiOutPort.SendMessage(controlChangeMsb);
                midiOutPort.SendMessage(controlChangeLsb);
                midiOutPort.SendMessage(programChange);
            }
            catch { }
        }

        public void SendSystemExclusive(byte[] bytes)
        {
            IBuffer buffer = bytes.AsBuffer();
            midiOutPort.SendBuffer(buffer);
            //MidiSystemExclusiveMessage midiMessageToSend = new MidiSystemExclusiveMessage(buffer);
            //midiOutPort.SendMessage(midiMessageToSend);
            //Task task = Task.Run(() =>
            //{
            //    midiOutPort.SendMessage(midiMessageToSend);
            //});
            //task = null;
            //GC.Collect();
        }

        public byte[] SystemExclusiveRQ1Message(byte[] Address, byte[] Length)
        {
            byte[] result = new byte[17];
            result[0] = 0xf0; // Start of exclusive message
            result[1] = 0x41; // Roland
            result[2] = DeviceID; // Device Id is 17 according to settings in INTEGRA-7 (Menu -> System -> MIDI, 1 = 0x00 ... 17 = 0x10)
            result[3] = 0x00;
            result[4] = 0x00;
            result[5] = 0x64; // INTEGRA-7
            result[6] = 0x11; // Command (DT1)
            result[7] = Address[0];
            result[8] = Address[1];
            result[9] = Address[2];
            result[10] = Address[3];
            result[11] = Length[0];
            result[12] = Length[1];
            result[13] = Length[2];
            result[14] = Length[3];
            result[15] = 0x00; // Filled out by CheckSum but present here to avoid confusion about index 15 missing.
            result[16] = 0xf7; // End of sysex
            CheckSum(ref result);
            return (result);
        }

        public byte[] SystemExclusiveDT1Message(byte[] Address, byte[] DataToTransmit)
        {
            Int32 length = 13 + DataToTransmit.Length;
            byte[] result = new byte[length]; 
            result[0] = 0xf0; // Start of exclusive message
            result[1] = 0x41; // Roland
            result[2] = DeviceID; // Device Id is 17 according to settings in INTEGRA-7 (Menu -> System -> MIDI, 1 = 0x00 ... 17 = 0x10)
            result[3] = 0x00;
            result[4] = 0x00;
            result[5] = 0x64; // INTEGRA-7
            result[6] = 0x12; // Command (DT1)
            result[7] = Address[0];
            result[8] = Address[1];
            result[9] = Address[2];
            result[10] = Address[3];
            for (Int32 i = 0; i < DataToTransmit.Length; i++)
            {
                result[i + 11] = DataToTransmit[i];
            }
            result[12 + DataToTransmit.Length] = 0xf7; // End of sysex
            CheckSum(ref result);
            return (result);
        }

        public void CheckSum(ref byte[] bytes)
        {
            byte chksum = 0;
            for (Int32 i = 7; i < bytes.Length - 2; i++)
            {
                chksum += bytes[i];
            }
            bytes[bytes.Length - 2] = (byte)((0x80 - (chksum & 0x7f)) & 0x7f);
        }
    }
}
