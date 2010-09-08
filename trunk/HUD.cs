using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;

namespace LearnShader
{
    class FullScreenQuad
    {
        // Fields
        Bitmap textBmp;
        int textTexture;
        float linePosY = 100;
        Shader fsQuadShader;

        // Constructors
        public FullScreenQuad(int width, int height)
        {
            textBmp = new Bitmap(width, height);

            textTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textTexture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, textBmp.Width, textBmp.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);

            fsQuadShader = Shader.CreateShader("fsquad.vert", "fsquad.frag", "fsquad");

        }

        // Properties

        // Methods
        public void AddText(string text)
        {
            using (Graphics gfx = Graphics.FromImage(textBmp))
            {
                linePosY -= 20;
                gfx.Clear(Color.Transparent);
                gfx.DrawString(text, new Font("Arial", 16), new SolidBrush(Color.Black), new PointF(10.0f, linePosY));

            }
        }
        public void uploadTexture(int width, int height)
        {
            BitmapData data = textBmp.LockBits(new Rectangle(0, 0, textBmp.Width, textBmp.Height), ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                PixelType.UnsignedByte, data.Scan0);
            textBmp.UnlockBits(data);


        }
        public void Draw()
        {
            

        }

    }
}
