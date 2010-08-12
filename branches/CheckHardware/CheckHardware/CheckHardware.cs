using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace CheckHardware
{
    public class Game : GameWindow
    {
        public Game()
            : base(640, 480, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 16, 0, 8), "OpenGL 3.1 Example", 0,
                DisplayDevice.Default, 3, 1, GraphicsContextFlags.Debug)
        {

            string GLVersion = GL.GetString(StringName.Version);
            Console.WriteLine("OpenGL version = {0}", GLVersion);

            string shaderVersion = GL.GetString(StringName.ShadingLanguageVersion);
            Console.WriteLine("Shader Version = {0}", shaderVersion);

            string renderer = GL.GetString(StringName.Renderer);
            Console.WriteLine("Renderer = {0}", renderer);

            string vendor = GL.GetString(StringName.Vendor);
            Console.WriteLine("Vendor = {0}", vendor);

            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(0.2f, 0.2f, 0.2f, 1);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (Keyboard[OpenTK.Input.Key.Escape])
            {
                Exit();
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Flush();
            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            // Do nothing here.
        }
    }

    public class Program
    {
        [STAThread]
        public static void Main()
        {
            using (Game win = new Game())
            {
                win.Run();
            }
        }
    }

}
