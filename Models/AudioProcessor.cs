using NAudio.Dsp;
using NAudio.Wave;
using static Raylib_cs.Raylib;

namespace Musializer.Models
{
    public class AudioProcessor
    {
        private const int N = 1<<14;

        //this value provides a nice result 
        private int smoothness = 8;

        WasapiLoopbackCapture loopbackCapture;

        private float[] inRaw;
        private float[] inWin;
        private Complex[] outRaw;
        private float[] outLog;
        private float[] outSmooth;
        private int frequencesCount;
        private Complex[] Data;

        public float[] OutSmooth { get => outSmooth; set => outSmooth = value; }
        public int FrequenceCount { get => frequencesCount; set => frequencesCount = value; }

        public AudioProcessor()
        {
            inRaw = new float[N];
            inWin = new float[N];
            outRaw = new Complex[N];
            outLog = new float[N];
            outSmooth = new float[N];
            Data = new Complex[N];
            loopbackCapture = new WasapiLoopbackCapture();
            loopbackCapture.WaveFormat = new WaveFormat(44100, 16, 2);
            loopbackCapture.DataAvailable += AudioDataCallback;
            loopbackCapture.StartRecording();
        }

        void AudioDataCallback(object sender, WaveInEventArgs e)
        {
            //byte[] audioData = e.Buffer;
            //int frames = e.BytesRecorded;
            //if (frames > N)
            //    frames = N;
            //int elementsToMove = (N - frames);

            //float[] tempArray = new float[elementsToMove];
            //Array.Copy(inRaw, frames, inRaw, 0, elementsToMove);
            //Array.Copy(tempArray, inRaw, elementsToMove);

            //for (int i = 0; i < frames / 2; ++i)
            //{
            //    // converting the data to float samples 
            //    short sample = BitConverter.ToInt16(audioData, i * 2);
            //    float samplefloat = sample / 32768f; //normalizing the values to [-1, 1]
            //    Data[i] = new Complex();
            //    Data[i].X = samplefloat;
            //    Data[i].Y = 0;
            //    //Console.WriteLine($"{inRaw[i]}\n");
            //}
            // Assuming audioData is a float[] array to store the audio data
            float[] audioData = new float[e.BytesRecorded / sizeof(float)];

            // Convert the audio bytes to float samples
            Buffer.BlockCopy(e.Buffer, 0, audioData, 0, e.BytesRecorded);

            // Populate complexData for FFT analysis
            for (int i = 0; i < audioData.Length; i++)
            {
                Data[i] = new Complex();
                Data[i].X = audioData[i];
                Data[i].Y = 0;
            }

            NAudio.Dsp.FastFourierTransform.FFT(true, (int)Math.Sqrt(N), Data);
            Update();
        }

        //private void FFT(int indexIn, int stride, int indexOut, int n)
        //{
        //    //if (n == 0) throw new Exception("Number of samples cannot be less than or equal to 0");
        //    if (n == 1)
        //    {
        //        outRaw[indexOut] = new Complex(inRaw[indexIn], 0.0);
        //        return;
        //    }

        //    FFT(indexIn, stride * 2, indexOut, n / 2);
        //    FFT(indexIn + stride, stride * 2, indexOut + n / 2, n / 2);

        //    for (int i = 0; i < n/2; ++i)
        //    {
        //        float t = (float)i / n;

        //        Complex v = Complex.(-2.0 * Math.PI * Complex.ImaginaryOne * t) * outRaw[i + n / 2];
        //        Complex e = outRaw[i];
        //        outRaw[i] = e + v;
        //        outRaw[i + n / 2] = e - v;
        //    }
        //}


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
            frequencesCount = 0;

            for (float f = lowf; (int)f < N; f = (float)Math.Ceiling(f * step))
            {
                float f1 = (float)Math.Ceiling(f * step);
                float a = 0.0f;

                for (int q = (int)f; q < N / 2 && q < (int)f1; ++q)
                {
                    float b = Amp(Data[q]);
                    if (b > a) a = b;
                }
                if (maxAmp < a)
                    maxAmp = a;
                outLog[frequencesCount++] = a;
            }

            for (int i = 0; i < frequencesCount; i++)
            {
                outLog[i] /= maxAmp;
                Console.WriteLine($"{outLog[i]}\n");
            }
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
            //float a = Convert.ToSingle(z.X);
            //float b = Convert.ToSingle(z.Y);
            //var result = MathF.Log(a * a + b * b);
            //return result;
            float a = Convert.ToSingle(z.X);
            float b = Convert.ToSingle(z.Y);
            a = Math.Abs(a); b = Math.Abs(b);
            if (a < b) return b;
            return a;
        }

        public void Update()
        {
            ApplyHannWindowing();
            //FFT(0, 1, 0, N);
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
