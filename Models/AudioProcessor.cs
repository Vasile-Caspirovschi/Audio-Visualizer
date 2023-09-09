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
        private Plugin plugin;
        
        public AudioProcessor(Plugin plugin)
        {
            this.plugin = plugin;
            Raylib.InitAudioDevice();
            inRaw = new float[N];
            inWin = new float[N];
            outRaw = new Complex[N];
            outLog = new float[N];
            outSmooth = new float[N];
        }

        struct Frame
        {
            public float left;
            public float right;
        }

        void Callback(IntPtr bufferData, int frames)
        {
            Frame fs = Marshal.PtrToStructure<Frame>(bufferData);
            

            int elementsToMove = (N - frames);
            float[] tempArray = new float[elementsToMove];
            Array.Copy(inRaw, frames, inRaw, 0, elementsToMove);
            Array.Copy(tempArray, inRaw, elementsToMove);

            for (int i = 0; i < frames; ++i)
            {
                inRaw[N - frames + i] = fs.left;
            }
        }

        void LoadAndPlayDroppedMusic()
        {
            FilePathList droppedFiles = LoadDroppedFiles();
            if (droppedFiles.count == 0)
                return;
            if (IsMusicReady(plugin.Music))
            {
                StopMusicStream(plugin.Music);
                UnloadMusicStream(plugin.Music);
            }
            string filePath;
            unsafe
            {
                IntPtr path = (IntPtr)droppedFiles.paths[0];
                filePath = new string(Marshal.PtrToStringAuto(path));
            }

            plugin.Music = LoadMusicStream(filePath);
            if (IsMusicReady(plugin.Music))
            {
                plugin.Error = false;
                plugin.Volume = 0.2f;
                SetMusicVolume(plugin.Music, plugin.Volume);
                PlayMusicStream(plugin.Music);
                AudioStreamProcessor audioProcessor = (IntPtr bufferData, int frames) =>
                {
                    Callback(bufferData, frames);
                };
                AttachAudioStreamProcessor(plugin.Music, )

            }
        }
        public void UpdatePlugin()
        {

        }

        public void ClosePlugin()
        {
            StopMusicStream(plugin.Music);
          
        }
    }
}
