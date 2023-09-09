using NAudio.Dsp;
using NAudio.Wave;
using Raylib_cs;
using System.Runtime.InteropServices;
using static Raylib_cs.Raylib;
using System.Linq;

namespace Musializer.Models
{
    public class AudioProcessor
    {
        private const int N = 1 << 14;
        private float[] inRaw;
        private float[] inWin;
        private Complex[] outRaw;
        private float[] outLog;
        private float[] outSmooth;
        private uint channels = 2;
        private int frequenceCount;
        private WaveInEvent waveIn;

        public float[] OutLog { get => outLog; set => outLog = value; }
        public float[] OutSmooth { get => outSmooth; set => outSmooth = value; }
        public int FrequenceCount { get => frequenceCount; set => frequenceCount = value; }

        public AudioProcessor()
        {
            inRaw = new float[N];
            inWin = new float[N];
            outRaw = new Complex[N];
            OutLog = new float[N];
            OutSmooth = new float[N];

            waveIn = new WaveInEvent();
            waveIn.DeviceNumber = 0;
            waveIn.WaveFormat = new WaveFormat(44100, 16, 2);
            waveIn.DataAvailable += AudioDataCallback;
            waveIn.StartRecording();
        }

        void AudioDataCallback(object sender, WaveInEventArgs e)
        {
            byte[] audioData = e.Buffer;
            int frames = e.BytesRecorded;

            int elementsToMove = (N - frames);
            float[] tempArray = new float[elementsToMove];
            Array.Copy(inRaw, frames, inRaw, 0, elementsToMove);
            Array.Copy(tempArray, inRaw, elementsToMove);
            
            for (int i = 0; i < frames / 2; ++i)
            {
                // converting the data to float samples 
                short sample = BitConverter.ToInt16(audioData, i * 2);
                float sampleFloat = sample / 32768f; //normalizing the values to [-1, 1]
                inRaw[i] = sampleFloat;
            }
        }

        public void Update()
        {

        }

        public void Close()
        {
           waveIn.StopRecording();     
        }
    }
}
