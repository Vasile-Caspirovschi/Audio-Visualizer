using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Musializer.Models
{
    public class Plugin
    {
        Music music;
        Font font;
        bool error;
        float volume;

        public Plugin(Music music, Font font, bool error, float volume)
        {
            this.music = music;
            this.font = font;
            this.error = error;
            this.volume = volume;
        }

        public Music Music { get => music; set => music = value; }
        public Font Font { get => font; set => font = value; }
        public bool Error { get => error; set => error = value; }
        public float Volume { get => volume; set => volume = value; }
    }
}
