using Raylib_cs;
using System.Numerics;
using System.Text;
using static Raylib_cs.Raylib;

namespace Musializer.Models
{
    public class Visualizer
    {
        const int FONT_SIZE = 35;
        AudioProcessor audioProcessor;

        public Visualizer()
        {
            audioProcessor = new AudioProcessor();
        }

        public void Visualize()
        {
            BeginDrawing();
            ClearBackground(GetColor(0x101010FF));
            EndDrawing();
        }

        public void RenderStartScreen()
        {
            BeginDrawing();
            ClearBackground(GetColor(0x101010FF));
            float w = GetRenderWidth();
            float h = GetRenderHeight();
            string message = "Press M to start sound capture";
            int width = MeasureText(message, (int)FONT_SIZE);
            DrawText(message, (int)w / 2 - width / 2, (int)h / 2 - FONT_SIZE / 2, FONT_SIZE, Color.WHITE);
            EndDrawing();
        }



        public void Dispose()
        {
            audioProcessor.Close();
        }


    }
}
