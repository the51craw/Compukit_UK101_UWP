using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Audio;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Media.MediaProperties;
using Windows.Media.Render;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Compukit_UK101_UWP
{
    // We are initializing a COM interface for use within the namespace
    // This interface allows access to memory at the byte level which we need to populate audio data that is generated
    [ComImport]
    [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]

    unsafe interface IMemoryBufferByteAccess
    {
        void GetBuffer(out byte* buffer, out uint capacity);
    }

    public class MicToMidi : CMemoryBusDevice
    {
        AudioDeviceInputNode MicToMidiInput { get; set; }

        private MainPage mainPage;
        private AudioGraph audioGraph;
        private AudioDeviceInputNode deviceInputNode;
        private AudioFrameOutputNode frameOutputNode;
        private DispatcherTimer timer;
        private DispatcherTimer outTimer;
        private Int32 periodLength;
        private Int32 periodLengthUK101;
        private Int32 readCount;


        public MicToMidi(MainPage mainPage)
        {
            this.mainPage = mainPage;
            Init();
        }

        private async void Init()
        {
            AudioGraphSettings settings = new AudioGraphSettings(AudioRenderCategory.Media);
            settings.QuantumSizeSelectionMode = QuantumSizeSelectionMode.LowestLatency;

            CreateAudioGraphResult result = await AudioGraph.CreateAsync(settings);

            if (result.Status != AudioGraphCreationStatus.Success)
            {
                mainPage.MessageBox("Could not create input device for Mic To MIDI!");
                return;
            }

            audioGraph = result.Graph;
            CreateAudioDeviceInputNodeResult deviceInputNodeResult = await audioGraph.CreateDeviceInputNodeAsync(MediaCategory.Other);

            if (deviceInputNodeResult.Status != AudioDeviceNodeCreationStatus.Success)
            {
                mainPage.MessageBox(String.Format("Audio Device Input unavailable because {0}", deviceInputNodeResult.Status.ToString()));
                return;
            }

            deviceInputNode = deviceInputNodeResult.DeviceInputNode;

            frameOutputNode = audioGraph.CreateFrameOutputNode();
            deviceInputNode.AddOutgoingConnection(frameOutputNode);
            //audioGraph.QuantumStarted += AudioGraph_QuantumStarted;

            audioGraph.Start();
            deviceInputNode.Start();
            frameOutputNode.Start();

            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1); // 1 ms
            timer.Tick += Timer_Tick;
            timer.Start();

            //outTimer = new DispatcherTimer();
            //outTimer.Interval = new TimeSpan(0, 0, 0, 0, 1); // 1 ms
            //outTimer.Tick += OutTimer_Tick;
            //outTimer.Start();
            readCount = 0;
            periodLength = 0;
        }

        unsafe private void Timer_Tick(object sender, object e)
        {
            AudioFrame audioFrame = frameOutputNode.GetFrame();
            using (AudioBuffer buffer = audioFrame.LockBuffer(AudioBufferAccessMode.Read))
            using (IMemoryBufferReference memoryBufferReference = buffer.CreateReference())
            {
                byte* dataInBytes;
                uint capacityInBytes;
                float* dataInFloat;

                ((IMemoryBufferByteAccess)memoryBufferReference).GetBuffer(out dataInBytes, out capacityInBytes);
                dataInFloat = (float*)dataInBytes;

                Int32 pulseOn = 0;
                Int32 pulseOff = 0;
                Boolean transitionUpFound = false;
                Boolean transitionDownFound = false;
                Int32 transitionCount = 0;
                Boolean high = dataInFloat[0] > 0;
                for (Int32 i = 0; i < capacityInBytes / 8; i++)
                {
                    if (dataInFloat[i] != 0)
                    {
                        if (dataInFloat[i] < -0.05) // If low
                        {
                            if (high) // If was high
                            {
                                transitionDownFound = true;
                                transitionCount++;
                            }
                            high = false;
                        }
                        else if(dataInFloat[i] > 0.05) // Is high
                        {
                            if (!high) // If was low
                            {
                                transitionUpFound = true;
                                transitionCount++;
                            }
                            high = true;
                        }

                        if (transitionCount > 2)
                        {
                            periodLength = pulseOn + pulseOff;
                            break;
                        }

                        if (high && transitionUpFound)
                        {
                            pulseOn++;
                        }
                        else if (!high && transitionDownFound)
                        {
                            pulseOff++;
                        }
                    }
                }
                dataInFloat = null;
                dataInBytes = null;
                memoryBufferReference.Dispose();
                buffer.Dispose();
                audioFrame.Dispose();
                periodLengthUK101 = (int)(periodLength / factor);
            }
            //audioFrame = frameOutputNode.GetFrame();
        }

        private void AudioGraph_QuantumStarted(AudioGraph sender, object args)
        {
            AudioFrame audioFrame = frameOutputNode.GetFrame();
        }

        public void Write(byte inData)
        {
        }

        // This timer controls what is read from MicToMidi.
        // Since timing is different in Windows compared to UK101
        // we must translate period time to number of expected reads
        // for the pitch in question.
        // Then count how many times UK101 reads MicToMidi and swap
        // output level accordingly to mimic the square wave produced
        // by my interface.
        // To be trimmed:
        const int max = 10000;
        const int min = 100;
        const double factor = 9.45; // 9.2 - 9.7; 

        private void OutTimer_Tick(object sender, object e)
        {
            //if (periodLength >= min && periodLength <= max)
            {
                periodLengthUK101 = (int)(periodLength / factor);

                periodLength = 0; // Maybe...
            }
        }

        Int32 cnt = 0;
        public byte Read()
        {
            cnt++;
            if (cnt > periodLengthUK101)
            {
                cnt = 0;
            }

            if (cnt > periodLengthUK101 / 2)
            {
                return 0x61;
            }
            else
            {
                return 0x60;
            }
        }
    }
}
