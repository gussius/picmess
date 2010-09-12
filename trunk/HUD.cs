using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using System.Drawing.Drawing2D;

namespace LearnShader
{
    class FullScreenQuad
    {
        // Fields
        Bitmap textBmp;
        int textTexture;
        Shader fsQuadShader;
        Mesh fsQuadMesh;
        string name = "fsQuad";
        string sourceFile = @"C:\Temp\quad.obj";
        int sampler2DLocation;
        string consoleOutput;

        // Constructors
        public FullScreenQuad(int width, int height)
        {
            textBmp = (Bitmap)Bitmap.FromFile("c:\\temp\\pic7.png");
            GL.GenTextures(1, out textTexture);
            GL.BindTexture(TextureTarget.Texture2D, textTexture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, textBmp.Width, textBmp.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);

            fsQuadShader = Shader.CreateShader("fsquad.vert", "fsquad.frag", name);
            fsQuadMesh = Mesh.CreateMesh(sourceFile, name);
            GL.EnableVertexAttribArray(0);
            GL.BindAttribLocation(fsQuadShader.ShaderID, 0, "vertex_position");
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, 0);

            sampler2DLocation = GL.GetUniformLocation(fsQuadShader.ShaderID, "hud");
        }

        // Properties

        // Methods
        public void AddText(string text)
        {
            consoleOutput = consoleOutput + text;
        }
        public void DrawText()
        {           
            using (Graphics gfx = Graphics.FromImage(textBmp))
            {
                gfx.CompositingMode = CompositingMode.SourceOver;
                gfx.CompositingQuality = CompositingQuality.HighQuality;
                gfx.SmoothingMode = SmoothingMode.HighQuality;
                gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                gfx.DrawString(consoleOutput, new Font("Agency FB", 20, FontStyle.Regular), new SolidBrush(Color.Black), new PointF(25.0f, 40.0f));
                gfx.DrawString(Fps.FPS.ToString(), new Font("Agency FB", 20, FontStyle.Regular), new SolidBrush(Color.Black), new Point(580, 168));
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
            fsQuadShader.Bind();
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.BindTexture(TextureTarget.Texture2D, textTexture);
            GL.Uniform1(sampler2DLocation, 0);
            fsQuadMesh.Draw();
            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Blend);
        }
    }
}
