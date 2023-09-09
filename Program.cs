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
            Raylib.SetTargetFPS(60);

            while (!Raylib.WindowShouldClose())
            {
                Raylib.BeginDrawing();
                Raylib.EndDrawing();
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_M))
                    CAPTURE_MODE = !CAPTURE_MODE;

                if (Raylib.IsFileDropped())
                {

                }

                if (CAPTURE_MODE)
                {

                }
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_ENTER))
                    break;
            }

            Raylib.CloseWindow();
        }
    }
}