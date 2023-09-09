using NAudio.Dsp;
using NAudio.Wave;
using Raylib_cs;
using System.Runtime.InteropServices;
using static Raylib_cs.Raylib;
using System.Linq;
using System.Text.RegularExpressions;

namespace Musializer.Models
{
    public class AudioProcessor
    {
        private const int N = 1 << 14;
        WasapiLoopbackCapture loopbackCapture;

        private float[] inRaw;
        private float[] inWin;
        private Complex[] outRaw;
        private float[] outLog;
        private float[] outSmooth;
        private int frequenceCount;

        public float[] OutLog { get => outLog; set => outLog = value; }
        public float[] OutSmooth { get => outSmooth; set => outSmooth = value; }
        public int FrequenceCount { get => frequenceCount; set => frequenceCount = value; }

        public AudioProcessor()
        {
            inRaw = new float[N];
            inWin = new float[N];
            outRaw = new Complex[N];
            outLog = new float[N];
            outSmooth = new float[N];

            loopbackCapture = new WasapiLoopbackCapture();
            loopbackCapture.WaveFormat = new WaveFormat(44100, 16, 2);
            loopbackCapture.DataAvailable += AudioDataCallback;
            loopbackCapture.StartRecording();
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

        public void FFT(int stride, int n)
        {
            if (n < 0) throw new Exception("Number of samples cannot be smaller than 0");
            if (n == 1)
                outRaw[0] = new Complex();
        }
        public void Update()
        {

        }

        public void Close()
        {
            loopbackCapture.StopRecording();
            loopbackCapture.Dispose();
        }
    }
}
