using Musializer.Models;
using Raylib_cs;

namespace Musializer
{
    internal class Program
    {
        static bool CAPTURE_MODE = false;
        public static void Main()
        {
            int factor = 60;
            Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
            Raylib.InitWindow(factor * 16, factor * 10, "Musializer");
            Visualizer visualizer = new Visualizer();
            Raylib.SetTargetFPS(60);

            while (!Raylib.WindowShouldClose())
            {

                if (Raylib.IsKeyPressed(KeyboardKey.KEY_M))
                {
                    CAPTURE_MODE = !CAPTURE_MODE;
                    visualizer.Init();
                }

                if (CAPTURE_MODE)
                {
                    visualizer.Visualize();
                }
                else
                    visualizer.RenderStartScreen();
            }
            visualizer.Dispose();
            Raylib.CloseWindow();
        }
    }
}