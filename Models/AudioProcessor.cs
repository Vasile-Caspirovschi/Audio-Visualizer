using NAudio.Wave;
using Raylib_cs;
using System.Runtime.InteropServices;
using static Raylib_cs.Raylib;
using System.Linq;
using System.Text.RegularExpressions;
using System.Numerics;
using NAudio.CoreAudioApi;

namespace Musializer.Models
{
    public class AudioProcessor
    {
        private const int N = 1 << 14;

        //this value provides a nice result 
        private int smoothness = 8;

        WasapiLoopbackCapture loopbackCapture;

        private float[] inRaw;
        private float[] inWin;
        private Complex[] outRaw;
        private float[] outLog;
        private float[] outSmooth;
        private int frequencesCount;

        public float[] OutSmooth { get => outSmooth; set => outSmooth = value; }
        public int FrequenceCount { get => frequencesCount; set => frequencesCount = value; }

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
                float samplefloat = sample / 32768f; //normalizing the values to [-1, 1]
                inRaw[i] = samplefloat;
            }
        }

        private void FFT(int indexIn, int stride, int indexOut, int n)
        {
            if (n < 0) throw new Exception("Number of samples cannot be smaller than 0");
            if (n == 1)
            {
                outRaw[indexOut] = new Complex(inRaw[indexIn],0.0f);
                return;
            }

            FFT(indexIn, stride*2, indexOut, n/2);
            FFT(indexIn + stride, stride * 2, indexOut + n / 2, n / 2);

            for (int i = 0; i < n/2; ++i)
            {
                float t = (float)i / n;
                Complex v = Complex.Exp(Complex.ImaginaryOne * Math.PI * t * -2) * outRaw[i + n/2];
                Complex e = outRaw[i];
                outRaw[i] = e + v;
                outRaw[i + n / 2] = e - v;
            }
        }

        private void ApplyHannWindowing()
        {
            for (int i = 0; i < N; i++)
            {
                float t = (float)i / N;
                float hann = 0.5f - 0.5f * MathF.Cos(2 * MathF.PI * t);
                inWin[i] = inRaw[i] * hann;
            }
        }

        private void ComputeNormalizedLogarithmicAmplitudes()
        {
            float step = 1.06f;
            float lowf = 1.0f;
            float maxAmp = 1.0f;

            for (float f = lowf; (int)f < N / 2; f = (float)Math.Ceiling(f * step))
            {
                float f1 = (float)Math.Ceiling(f * step);
                float a = 0.0f;

                for (int q = (int)f; q < N / 2 && q < (int)f1; ++q)
                {
                    float b = Amp(outRaw[q]); 
                    if (b > a) a = b;
                }
                if (maxAmp < a)
                    maxAmp = a; 
                outLog[frequencesCount++] = a;
            }

            for (int i = 0; i < frequencesCount; i++)
                outLog[i] /= maxAmp;
        }
        
        private void SmoothAmplitudes(int smoothness)
        {
            float dt = GetFrameTime();
            for (int i = 0; i < frequencesCount; i++)
            {
                outSmooth[i] += (outLog[i] - outSmooth[i]) * dt * smoothness;
            }
        }

        private float Amp(Complex z)
        {
            float a = Convert.ToSingle(z.Real);
            float b = Convert.ToSingle(z.Imaginary);
            return MathF.Log(a*a + b*b);
        }

        public void Update()
        {
            ApplyHannWindowing();
            FFT(0, 1, 0, N);
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
