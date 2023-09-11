//using NAudio.Dsp;
using NAudio.Wave;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace Musializer.Models
{
    public class AudioProcessor
    {
        private const int N = 1 << 14;

        //this values provides a nice result so i kept them
        private int smoothness = 8;
        private int smearness = 6;

        WasapiLoopbackCapture loopbackCapture;

        private WaveBuffer rawData;

        private Complex[] fftData;
        private int fftIndex;
        private float[] outLog;
        private float[] inRaw;
        private float[] outSmooth;
        private float[] outSmear;
        private int frequencesCount;

        public int FrequenceCount { get => frequencesCount; set => frequencesCount = value; }
        public float[] OutSmooth { get => outSmooth; set => outSmooth = value; }
        public float[] OutSmear { get => outSmear; set => outSmear = value; }

        public AudioProcessor()
        {
            fftData = new Complex[N];
            inRaw = new float[N];
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
            int frames = rawData.FloatBuffer.Length / 8;

            for (int i = 0; i < frames; i++)
            {
                if (fftIndex >= N) fftIndex = 0;
                inRaw[fftIndex++] = rawData.FloatBuffer[i];
            }
            //int m = (int)Math.Log(N, 2.0);
            //FastFourierTransform.FFT(true, m, fftData);
            FFT(0, 1, 0, N);
        }

        
        public void FFT(int indexIn, int stride, int indexOut, int n)
        {
            if (n <= 0)
            {
                throw new ArgumentException("n must be greater than 0");
            }
            if (n == 1)
            {
                fftData[indexOut] = new Complex(inRaw[indexIn],0);
                return;
            }
            FFT(indexIn, stride * 2, indexOut, n / 2);
            FFT(indexIn + stride, stride * 2, indexOut + n / 2, n / 2);
            for (int k = 0; k < n / 2; ++k)
            {
                double t = (double)k / n;
                Complex v = Complex.Exp(-2 * Complex.ImaginaryOne * Math.PI * t) * fftData[indexOut + k + n / 2];
                Complex e = fftData[ k + indexOut];
                fftData[k + indexOut] = e + v;
                fftData[k + indexOut + n / 2] = e - v;
            }
        }

        private void ComputeNormalizedLogarithmicAmplitudes()
        {
            float step = 1.06f;
            float lowf = 1.0f;
            float maxAmp = 1.0f;
            float scaleFactor = N;
            frequencesCount = 0;

            for (float f = lowf; (int)f < N / 2; f = (float)Math.Ceiling(f * step))
            {
                float f1 = (float)Math.Ceiling(f * step);
                //float a = Magnitude(fftData[(int)f]);
                float a = 0.0f;

                for (int q = (int)f; q < N / 2 && q < (int)f1; ++q)
                {
                    float b = (float)fftData[q].Magnitude;
                    if (b > a) a = b;
                }
                a *= scaleFactor;
                if (maxAmp < a)
                    maxAmp = a;
                //scale the frequence by scaleFactor
                outLog[frequencesCount++] = a;
            }

            //normalize the values to [0, 1] range and apply hann windowing
            for (int i = 0; i < frequencesCount; i++)
            {
                outLog[i] /= maxAmp;
                float t = (float)i / frequencesCount;
                float hann = 0.5f - 0.5f * MathF.Cos(2 * MathF.PI * t);
                outLog[i] = outLog[i] * hann;
                //scalling to the power 
                outLog[i] = MathF.Sqrt(outLog[i]) * 0.7f;
            }
        }

        //private float Magnitude(Complex z)
        //{
        //    var res = (float)Math.Sqrt(z.X * z.X + z.Y * z.Y);
        //    return res;
        //}

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
