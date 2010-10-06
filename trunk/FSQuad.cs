using System;
using System.IO;
using System.Reflection;
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
        private static FullScreenQuad console;
        private Bitmap image;
        private const TextureUnit texUnit = TextureUnit.Texture0;
        private int texture;
        private Shader fsQuadShader;
        private Mesh fsQuadMesh;
        private string name = "fsQuad";
        private string sourceFile = "quad.obj";
        private int samplerLocation;

        // Constructors
        static FullScreenQuad()
        {
            console = new FullScreenQuad(640, 480);
        }
        private FullScreenQuad(int width, int height)
        {
            // Create a bitmap from a provided png file.
            image = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // Generate texture ID
            GL.GenTextures(1, out texture);

            // Bind and initialise the texture
            GL.ActiveTexture(texUnit);
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);

            // Upload the bitmap data to the texture
            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                PixelType.UnsignedByte, data.Scan0);
            image.UnlockBits(data);
            
            // Initialise the shader and mesh and bind appropriate vertex data to shader
            fsQuadShader = Shader.CreateShader("fsquad.vert", "fsquad.frag", name);
            fsQuadMesh = Mesh.CreateMesh(sourceFile, name);
            GL.EnableVertexAttribArray(0);
            GL.BindAttribLocation(fsQuadShader.ShaderID, 0, "vertex_position");
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, 0);

            // Get locations for uniforms variable to pass to shader
            samplerLocation = GL.GetUniformLocation(fsQuadShader.ShaderID, "background");

            // Set initial uniforms
            fsQuadShader.Bind();
        }

        // Properties
        public static FullScreenQuad Console
        {
            get { return console; }
        }

        // Methods
        public void Draw()
        {
            fsQuadShader.Bind();
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Uniform1(samplerLocation, 0);
            fsQuadMesh.Draw();
            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Blend);
        }
    }
}
