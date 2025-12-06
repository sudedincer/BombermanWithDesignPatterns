using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Bomberman.UI.Rendering
{
    public static class TextureGenerator
    {
        public static Texture2D GenerateColoredSquare(GraphicsDevice device, int size, Color color)
        {
            Texture2D tex = new Texture2D(device, size, size);
            Color[] data = new Color[size * size];

            for (int i = 0; i < data.Length; i++)
                data[i] = color;

            tex.SetData(data);
            return tex;
        }

        public static Texture2D GenerateSandTexture(GraphicsDevice device, int size)
        {
            Texture2D tex = new Texture2D(device, size, size);
            Color[] data = new Color[size * size];
            Random rng = new Random();

            for (int i = 0; i < data.Length; i++)
            {
                byte variation = (byte)rng.Next(0, 20);
                data[i] = new Color(
                    (byte)(194 + variation),
                    (byte)(178 + variation),
                    (byte)(128 + variation));
            }

            tex.SetData(data);
            return tex;
        }

        public static Texture2D GenerateGrassTexture(GraphicsDevice device, int size)
        {
            Texture2D tex = new Texture2D(device, size, size);
            Color[] data = new Color[size * size];
            Random rng = new Random();

            for (int i = 0; i < data.Length; i++)
            {
                byte g = (byte)rng.Next(120, 170);
                byte g2 = (byte)Math.Clamp(g + rng.Next(-10, 10), 0, 255);
                data[i] = new Color(30, g2, 30);
            }

            tex.SetData(data);
            return tex;
        }

        public static Texture2D GenerateAsphaltTexture(GraphicsDevice device, int size)
        {
            Texture2D tex = new Texture2D(device, size, size);
            Color[] data = new Color[size * size];
            Random rng = new Random();

            for (int i = 0; i < data.Length; i++)
            {
                byte gray = (byte)rng.Next(40, 70);
                data[i] = new Color(gray, gray, gray);
            }

            tex.SetData(data);
            return tex;
        }

        public static Texture2D GenerateStoneTexture(GraphicsDevice device, int size, Color baseColor, Color noiseColor)
        {
            Texture2D tex = new Texture2D(device, size, size);
            Color[] data = new Color[size * size];
            Random rng = new Random();

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float noise = (float)rng.NextDouble() * 0.25f;
                    Color mix = Color.Lerp(baseColor, noiseColor, noise);
                    data[y * size + x] = mix;
                }
            }

            tex.SetData(data);
            return tex;
        }

        public static Texture2D GenerateCrackedTexture(GraphicsDevice device, int size, Color color1, Color color2)
        {
            Texture2D tex = new Texture2D(device, size, size);
            Color[] data = new Color[size * size];
            Random rng = new Random();

            // arka plan
            for (int i = 0; i < data.Length; i++)
            {
                float t = (float)rng.NextDouble() * 0.5f;
                data[i] = Color.Lerp(color1, color2, t);
            }

            // dikey çatlaklar
            for (int i = 0; i < 4; i++)
            {
                int x = rng.Next(0, size);
                for (int y = 0; y < size; y++)
                {
                    data[y * size + x] = Color.Black;
                }
            }

            tex.SetData(data);
            return tex;
        }

        public static Texture2D GenerateMetalTexture(GraphicsDevice device, int size, Color baseColor, Color shineColor)
        {
            Texture2D tex = new Texture2D(device, size, size);
            Color[] data = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float shine = (float)Math.Sin(x * 0.3f) * 0.2f + 0.4f;
                    data[y * size + x] = Color.Lerp(baseColor, shineColor, shine);
                }
            }

            tex.SetData(data);
            return tex;
        }

        public static Texture2D GenerateBombTexture(GraphicsDevice device, int size, Color color)
        {
            Texture2D tex = new Texture2D(device, size, size);
            Color[] data = new Color[size * size];

            for (int i = 0; i < data.Length; i++)
                data[i] = color;

            tex.SetData(data);
            return tex;
        }
    }
}