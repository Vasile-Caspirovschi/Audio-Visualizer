using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace Musializer.Models
{
    public class Visualizer
    {
        const int FONT_SIZE = 35;
        private float maxHeight;
        //this variable is responsible for visualization to not fall down bellow a specific minHeight
        AudioProcessor audioProcessor;
        Shader circle;
        Shader smear;

        public Visualizer()
        {
           
            circle = LoadShader(null, "../../../Shaders/circle.fs");
            smear = LoadShader(null, "../../../Shaders/smear.fs");
        }

        public void Init()
        {
            audioProcessor = new AudioProcessor();
        }

        public void Visualize()
        {
            maxHeight = GetRenderWidth() * 2 / 4;
            audioProcessor.Update();
            BeginDrawing();
            ClearBackground(GetColor(0x101010FF));
            int m = audioProcessor.FrequenceCount;
            float cellWidth = (float)GetRenderWidth() / m;
            float saturation = 0.8f;
            float value = 0.8f;

            DrawBars(m, cellWidth, saturation, value);
            BeginShaderMode(smear);
            DrawCircleTrails(m, cellWidth, saturation, value);
            EndShaderMode();
            BeginShaderMode(circle);
            DrawCricles(m, cellWidth, saturation, value);
            EndShaderMode();
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

        void DrawCricles(int m, float cellWidth, float saturation, float value)
        {
            int h = GetRenderHeight();
            Texture2D texture = new Texture2D() { id = Rlgl.rlGetTextureIdDefault(), height = 1, width = 1, mipmaps = 1, format = PixelFormat.PIXELFORMAT_UNCOMPRESSED_R8G8B8A8 };

            for (int i = 0; i < m; i++)
            {
                float hue = (float)i / m;
                float t = Convert.ToSingle(audioProcessor.OutSmooth[i]);
                Color color = ColorFromHSV(hue * 360, saturation, value);
                Vector2 center = new Vector2() { X = i * cellWidth + cellWidth / 2, Y = h - maxHeight * t  };
                float radius = cellWidth * 5 * MathF.Sqrt(t) ;
                Vector2 position = new Vector2()
                {
                    X = center.X - radius,
                    Y = center.Y - radius
                };
                DrawTextureEx(texture, position, 0, 2 * radius, color);
            }
        }

        void DrawCircleTrails(int m, float cellWidth, float saturation, float value)
        {
            float h = GetRenderHeight();
            Texture2D texture = new Texture2D() { id = Rlgl.rlGetTextureIdDefault(), height = 1, width = 1, mipmaps = 1, format = PixelFormat.PIXELFORMAT_UNCOMPRESSED_R8G8B8A8 };

            for (int i = 0; i < m; ++i)
            {
                float start = audioProcessor.OutSmear[i];
                float end = audioProcessor.OutSmooth[i];
                float hue = (float)i / m;
                Color color = ColorFromHSV(hue * 360, saturation, value);
                Vector2 startPos = new Vector2()
                {
                    X = i * cellWidth + cellWidth / 2,
                    Y = h - maxHeight * start,
                };
                Vector2 endPos = new Vector2()
                {
                    X = i * cellWidth + cellWidth / 2,
                    Y = h - maxHeight * end,
                };
                float radius = cellWidth * MathF.Sqrt(end);
                Vector2 origin = new Vector2();
                if (endPos.Y >= startPos.Y)
                {
                    Rectangle dest = new Rectangle()
                    {
                        x = startPos.X - radius,
                        y = startPos.Y ,
                        width = 2 * radius,
                        height = endPos.Y - startPos.Y 
                    };
                    Rectangle source = new Rectangle() { x = 0, y = 0, width = 1, height = 0.5f };
                    DrawTexturePro(texture, source, dest, origin, 0, color);
                }
                else
                {
                    Rectangle dest = new Rectangle()
                    {
                        x = endPos.X - radius,
                        y = endPos.Y ,
                        width = 2 * radius,
                        height = startPos.Y - endPos.Y 
                    };
                    Rectangle source = new Rectangle() { x = 0, y = 0.5f, width = 1, height = 0.5f };
                    DrawTexturePro(texture, source, dest, origin, 0, color);
                }
            }
        }

        void DrawBars(int m, float cellWidth, float saturation, float value)
        {
            int h = GetRenderHeight();
            for (int i = 0; i < m; ++i)
            {
                float hue = (float)i / m;
                float t = audioProcessor.OutSmooth[i];
                Color color = ColorFromHSV(hue * 360, saturation, value);
                Vector2 startPos = new Vector2()
                {
                    X = i * cellWidth + cellWidth / 2,
                    Y = h - maxHeight * t,
                };
                Vector2 endPos = new Vector2()
                {
                    X = i * cellWidth + cellWidth / 2,
                    Y = h,
                };
                float thickness = cellWidth / 1.5f * MathF.Sqrt(t);
                DrawLineEx(startPos, endPos, thickness, color);
            }
        }

        public void Dispose()
        {
            if (audioProcessor is not null)
                audioProcessor.Close();
        }


    }
}
