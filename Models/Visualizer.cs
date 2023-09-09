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

        void DrawCricles(int m, float cellWidth, float saturation, float value)
        {
            int h = GetRenderHeight();
            Texture2D texture = new Texture2D(){ id = Rlgl.rlGetTextureIdDefault(), height = 1, width = 1, mipmaps = 1, format = PixelFormat.PIXELFORMAT_UNCOMPRESSED_R8G8B8A8 };

            for (int i = 0; i < m; i++)
            {
                float hue = (float)i / m;
                float t = audioProcessor.OutSmooth[i];
                Color color = ColorFromHSV(hue * 360, saturation, value);
                Vector2 center = new Vector2(){ X = i * cellWidth + cellWidth / 2, Y = h - h * 2 / 3 * t };
                float radius = cellWidth * 5 * MathF.Sqrt(t);
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
            Texture2D texture = new Texture2D(){ id = Rlgl.rlGetTextureIdDefault(), height = 1, width = 1, mipmaps = 1, format = PixelFormat.PIXELFORMAT_UNCOMPRESSED_R8G8B8A8 };

            for (int i = 0; i < m; ++i)
            {
                float start = audioProcessor.OutSmear[i];
                float end = audioProcessor.OutSmooth[i];
                float hue = (float)i / m;
                Color color = ColorFromHSV(hue * 360, saturation, value);
                Vector2 startPos = new Vector2(){
                    
                      X = i*cellWidth + cellWidth/2,
                      Y = h - h*2/3*start,
                };
                Vector2 endPos = new Vector2(){
                     X = i*cellWidth + cellWidth/2,
                     Y = h - h*2/3*end,
                };
                float radius = cellWidth * MathF.Sqrt(end);
                Vector2 origin = new Vector2();
                if (endPos.Y >= startPos.Y)
                {
                    Rectangle dest = new Rectangle(){
	                    x = startPos.X - radius,
	                    y = startPos.Y,
	                    width = 2*radius,
	                    height = endPos.Y - startPos.Y
                    };
                    Rectangle source = new Rectangle(){ x = 0, y = 0, width = 1, height = 0.5f };
                    DrawTexturePro(texture, source, dest, origin, 0, color);
                }
                else
                {
                    Rectangle dest = new Rectangle() {
                        x = endPos.X - radius, 
                        y = endPos.Y, 
                        width = 2*radius, 
                        height = startPos.Y - endPos.Y};
                    Rectangle source = new Rectangle() { x = 0, y = 0.5f, width = 1, height = 0.5f};
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
                Vector2 startPos = new Vector2(){
                    X = i*cellWidth + cellWidth/2,
                    Y = h - h*2/3*t,
    };
                Vector2 endPos = new Vector2(){
                    X = i*cellWidth + cellWidth/2,
                    Y = h,
    };
                float thickness = cellWidth / 1.5f * MathF.Sqrt(t);
                DrawLineEx(startPos, endPos, thickness, color);
            }
        }

        public void Dispose()
        {
            audioProcessor.Close();
        }


    }
}
