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
        private Bitmap textBmp, consoleBackground;
        private const TextureUnit backgroundTU = TextureUnit.Texture0;
        private const TextureUnit foregroundTU = TextureUnit.Texture1;
        private const int background = 0;
        private const int foreground = 1;
        private int[] consoleTexture = new int[2];
        private Shader fsQuadShader;
        private Mesh fsQuadMesh;
        private string name = "fsQuad";
        private string sourceFile = "quad.obj";
        private int foregroundSamplerLocation;
        private int backgroundSamplerLocation;
        private int startTimeLocation, currentTimeLocation;
        private int currentTime;
        private int retractedLocation;
        private int retracted = 0;
        private string consoleOutput;
        private List<string> consoleLines = new List<string>();
        int startLine, endLine, scrollLine;
        const int maxLines = 50;

        // Constructors
        static FullScreenQuad()
        {
            console = new FullScreenQuad(640, 480);
        }
        private FullScreenQuad(int width, int height)
        {
            // Load resources from assembly
            Assembly assembly;
            Stream imageStream;
            assembly = Assembly.GetExecutingAssembly();
            imageStream = assembly.GetManifestResourceStream("LearnShader.Textures.console.png");

            // Create a black bitmap for the forground text
            textBmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // Create a background bitmap from a provided png file.
            consoleBackground = (Bitmap)Bitmap.FromStream(imageStream);

            // Generate 2 texture IDs
            GL.GenTextures(2, consoleTexture);

            // Bind and initialise the background texture
            GL.ActiveTexture(backgroundTU);
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, consoleTexture[background]);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, consoleBackground.Width, consoleBackground.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);

            // Upload the bitmap data to the background texture
            BitmapData data = consoleBackground.LockBits(new Rectangle(0, 0, consoleBackground.Width, consoleBackground.Height), ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                PixelType.UnsignedByte, data.Scan0);
            consoleBackground.UnlockBits(data);

            // Bind and initialise the foreground texture
            GL.ActiveTexture(foregroundTU);
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, consoleTexture[foreground]);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, textBmp.Width, textBmp.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);

            // Initialise the shader and mesh and bind appropriate vertex data to shader
            fsQuadShader = Shader.CreateShader("fsquad.vert", "fsquad.frag", name);
            fsQuadMesh = Mesh.CreateMesh(sourceFile, name);
            GL.EnableVertexAttribArray(0);
            GL.BindAttribLocation(fsQuadShader.ShaderID, 0, "vertex_position");
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, 0);

            // Get locations for uniforms variable to pass to shader
            backgroundSamplerLocation = GL.GetUniformLocation(fsQuadShader.ShaderID, "background");
            foregroundSamplerLocation = GL.GetUniformLocation(fsQuadShader.ShaderID, "foreground");
            startTimeLocation = GL.GetUniformLocation(fsQuadShader.ShaderID, "startTime");
            currentTimeLocation = GL.GetUniformLocation(fsQuadShader.ShaderID, "currentTime");
            retractedLocation = GL.GetUniformLocation(fsQuadShader.ShaderID, "retracted");

            // Set initial uniforms
            fsQuadShader.Bind();
            GL.Uniform1(retractedLocation, retracted);

            //Clear memory and display empty strings
            this.DrawText();
            
        }

        // Properties
        public static FullScreenQuad Console
        {
            get { return console; }
        }

        // Methods
        public void AddText(string text)
        {
            if (consoleLines.Count >= maxLines)
                consoleLines.RemoveAt(0);
            consoleLines.Add(text);
            scrollLine = 0;

            this.DrawText();
        }
        public void DrawText()
        {
            Font consoleFont = new Font("Arial Narrow", 12, FontStyle.Bold);
            Brush consoleBrush = new SolidBrush(Color.White);

            consoleOutput = "";

            if ((startLine = consoleLines.Count - 5 + scrollLine) < 0)
            {
                startLine = 0;
                if (consoleLines.Count - startLine < 5)
                    endLine = consoleLines.Count;
            }
            else
                endLine = startLine + 5;
            if (endLine > consoleLines.Count)
                endLine = consoleLines.Count;

            for (int i = startLine; i < endLine; i++)
            {
                consoleOutput = consoleOutput + consoleLines[i] + "\n";
            }

            using (Graphics gfx = Graphics.FromImage(textBmp))
            {
                gfx.Clear(Color.Transparent);
                gfx.CompositingMode = CompositingMode.SourceOver;
                gfx.CompositingQuality = CompositingQuality.HighQuality;
                gfx.SmoothingMode = SmoothingMode.AntiAlias;
                gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                gfx.DrawString(consoleOutput, consoleFont, consoleBrush, new PointF(25.0f, 5.0f));
            }
            GL.ActiveTexture(foregroundTU);
            BitmapData data = textBmp.LockBits(new Rectangle(0, 0, textBmp.Width, textBmp.Height), ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, textBmp.Width, textBmp.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                PixelType.UnsignedByte, data.Scan0);
            textBmp.UnlockBits(data);
        }
        public void Draw()
        {
            fsQuadShader.Bind();
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Uniform1(backgroundSamplerLocation, 0);
            GL.Uniform1(foregroundSamplerLocation, 1);
            currentTime = Environment.TickCount;
            GL.Uniform1(currentTimeLocation, currentTime);
            fsQuadMesh.Draw();
            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Blend);
        }
        public void Toggle()
        {
            if (retracted == 0)
            {
                retracted = 1;
                currentTime = Environment.TickCount;
                GL.Uniform1(retractedLocation, retracted);
                GL.Uniform1(startTimeLocation, currentTime);
            }
            else
            {
                retracted = 0;
                currentTime = Environment.TickCount;
                GL.Uniform1(retractedLocation, retracted);
                GL.Uniform1(startTimeLocation, currentTime);
            }
        }
        public void Scroll(int lines)
        {
            if ((startLine == 0) && (lines > 0))
                lines = 0;
            
            if ((scrollLine = scrollLine - lines) > 0)
                scrollLine = 0;
            DrawText();            
        }
        public void DisplayHelp()
        {
            AddText("-- Toggle the console by pressing the '~' key.");
            AddText("-- Select and unselect cubes by clicking on them with the left mouse button.");
            AddText("-- To toggle the selection buffer view, press 'Ctrl-B'.");
            AddText("-- Press 'F1' to view these instructions.");
            AddText("-- To exit program, press 'Esc'");
        }
        public void Update(int mousePosition, int mouseWheelDelta)
        {
            if (mousePosition <= 110)
               console.Scroll(mouseWheelDelta);
        }
    }
}
