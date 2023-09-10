using NAudio.Dsp;
using NAudio.Wave;
using static Raylib_cs.Raylib;

namespace Musializer.Models
{
    public class AudioProcessor
    {
        private const int N = 1 << 14;

        //this value provides a nice result 
        private int smoothness = 8;
        private int smearness = 6;
        WasapiLoopbackCapture loopbackCapture;

        private WaveBuffer rawData;

        private Complex[] fftData;
        private int fftIndex;
        private float[] outLog;
        private float[] outSmooth;
        private float[] outSmear;
        private int frequencesCount;

        public int FrequenceCount { get => frequencesCount; set => frequencesCount = value; }
        public float[] OutSmooth { get => outSmooth; set => outSmooth = value; }
        public float[] OutSmear { get => outSmear; set => outSmear = value; }

        public AudioProcessor()
        {
            fftData = new Complex[N];
            outLog = new float[N];
            outSmooth = new float[N];
            outSmear = new float[N];
            rawData = new WaveBuffer(0);

            loopbackCapture = new WasapiLoopbackCapture();
            //loopbackCapture.WaveFormat = new WaveFormat(44100, 16, 2);
            loopbackCapture.DataAvailable += AudioDataCallback;
            loopbackCapture.StartRecording();
        }

        void AudioDataCallback(object sender, WaveInEventArgs e)
        {
            rawData = new WaveBuffer(e.Buffer);
        }

        private void PerformFFTOnRawData()
        {
            int samples = rawData.FloatBuffer.Length / 8;

            for (int i = 0; i < samples; i++)
            {
                if (fftIndex >= N) fftIndex = 0;
                fftData[fftIndex].X = (float)(rawData.FloatBuffer[i] * FastFourierTransform.HannWindow(fftIndex, N));
                fftData[fftIndex].Y = 0;
                fftIndex++;
            }
            int m = (int)Math.Log(N, 2.0);
            FastFourierTransform.FFT(true, m, fftData);
        }

        private void ComputeNormalizedLogarithmicAmplitudes()
        {
            float step = 1.06f;
            float lowf = 1.0f;
            float maxAmp = 1.0f;
            float scaleFactor = 10000f;
            frequencesCount = 0;

            for (float f = lowf; (int)f < N; f = (float)Math.Ceiling(f * step))
            {
                float f1 = (float)Math.Ceiling(f * step);
                float a = 0.0f;

                for (int q = (int)f; q < N / 2 && q < (int)f1; ++q)
                {
                    float b = Magnitude(fftData[q]);
                    if (b > a) a = b;
                }
                a *= scaleFactor;
                if (maxAmp < a)
                    maxAmp = a ;
                //scale the frequence by scaleFactor
                outLog[frequencesCount++] = a;
            }

            //normalize the values to [0, 1] range
            for (int i = 0; i < frequencesCount; i++)
            {
                outLog[i] /= maxAmp;
                //Console.WriteLine($"{outLog[i]}\n");
            }
        }

        private float Magnitude(Complex z)
        {
            var res = (float)Math.Sqrt(z.X * z.X + z.Y * z.Y);
            return res;
        }

        private void SmoothAmplitudes(int smoothness)
        {
            float dt = GetFrameTime();
            for (int i = 0; i < frequencesCount; i++)
            {
                outSmooth[i] += (outLog[i] - OutSmooth[i]) * dt * smoothness;
                outSmear[i] += (OutSmooth[i] - outSmear[i]) * dt * smearness;
            }
        }

        public void Update()
        {
            PerformFFTOnRawData();
            ComputeNormalizedLogarithmicAmplitudes();
            SmoothAmplitudes(smoothness);
        }

        public void Close()
        {
            loopbackCapture.StopRecording();
            loopbackCapture.Dispose();
        }
    }
}
