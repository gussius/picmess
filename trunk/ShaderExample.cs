using System;
using System.IO;
using System.Diagnostics;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace LearnShader
{
    public class Game : GameWindow
    {
        // Fields
        Cube[] cubeArray;
        Random randomNumber;
        Vector3 randomVector;
        Vector3 randomRotation;
        FrameBufferManager fbManager;
        bool showingSelectBuffer = false;
        FullScreenQuad FSQuad;

        // Constructors
        public Game()
            : base(640, 480, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 16, 0, 8),
                   "OpenGL 3.1 Example", 0, DisplayDevice.Default, 3, 1, GraphicsContextFlags.Debug)
        {
            // Retrieve references to shader variables.


            // Setup window view.
            float widthToHeight = ClientSize.Width / (float)ClientSize.Height;
            Shader.SetProjectionMatrix(Matrix4.CreatePerspectiveFieldOfView(0.25f, widthToHeight, 60, 120));
            Shader.SetLightPosition(new Vector3(3.0f, 4.0f, 5.0f));
            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(0.2f, 0.2f, 0.2f, 1);

            // Register a mouse button down event.
            Mouse.ButtonDown += new EventHandler<OpenTK.Input.MouseButtonEventArgs>(MouseButtonDown);
            

            //  ------- Sandbox -------

            FSQuad = new FullScreenQuad(ClientSize.Width, ClientSize.Height);
            FSQuad.AddText("Hello World");
            FSQuad.AddText("Whats up my peeps, here is some more text to fill up the screen");
            
            FSQuad.uploadTexture(ClientSize.Width, ClientSize.Height);


            // Initialise objects to be drawn in the scene.
            
            randomNumber = new Random();
            cubeArray = new Cube[25];

            // Populate the Cube array with Cubes at random locations and rotations.
            for (int i = 0; i < 25; i++)
            {
                randomVector = new Vector3(randomNumber.Next(-5, 5), randomNumber.Next(-5, 5), randomNumber.Next(-120, -10));
                randomRotation = new Vector3(randomNumber.Next(0, 314)/100, randomNumber.Next(0, 314)/100, 0);
                cubeArray[i] = new Cube(randomVector, randomRotation, new Color4(0.4f, 0.5f, 0.0f, 1.0f));
            }

            // Create references to the manager classes singletons.
            fbManager = FrameBufferManager.Instance;
        }

        // Methods
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            foreach (Cube sample in cubeArray)
            {
                sample.Rotation = sample.Rotation + new Vector3(0.01f, 0.02f, 0.0f);
            }
            if (Keyboard[OpenTK.Input.Key.Escape])
            {
                Exit();
            }
            if (Keyboard[OpenTK.Input.Key.Z])
            {
                showingSelectBuffer = true;
            }
            if (Keyboard[OpenTK.Input.Key.A])
            {
                showingSelectBuffer = false;
            }
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            DrawScene(RenderState.Render);
            if (showingSelectBuffer)
            {
                fbManager.ReadFBO(RenderState.Select);
                GL.BlitFramebuffer(0, 0, Width, Height, 0, 0, Width, Height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
            }

            FSQuad.Draw();

            GL.Flush();
            SwapBuffers();
        }
        protected override void OnResize(EventArgs e)
        {
            float widthToHeight = ClientSize.Width / (float)ClientSize.Height;
            Shader.SetProjectionMatrix(Matrix4.CreatePerspectiveFieldOfView(0.5f, widthToHeight, 1, 150));
            GL.Viewport(0, 0, Width, Height);
            fbManager.UpdateSelectionViewport(Width, Height);
        }        
        private void MouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            PickColor(e.X, e.Y);
        }
        private void DrawScene(RenderState state)
        {
            fbManager.BindFBO(state);
            if (state == RenderState.Render)
                GL.ClearColor(0.2f, 0.2f, 0.2f, 1);
            else
                if (state == RenderState.Select)
                    GL.ClearColor(0.0f, 0.0f, 0.0f, 0);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            foreach (Cube sample in cubeArray)
            {
                sample.Draw();
            }
        }
        private ISelectable PickColor(int x, int y)
        {
            DrawScene(RenderState.Select);
            fbManager.ReadFBO(RenderState.Select);
            Byte4 pixel = new Byte4();
            GL.ReadPixels(x, this.Height - y, 1, 1, PixelFormat.Rgba, PixelType.UnsignedByte, ref pixel);
            Cube selected = (Cube)PickRegister.Instance.LookupSelectable((int)pixel.ToUInt32());
            if (selected != null)
            {
                if (selected.IsSelected == true)
                {
                    selected.IsSelected = false;
                    Console.WriteLine("Cube Id#{0} unselected", selected.Id);
                }
                else
                {
                    selected.IsSelected = true;
                    Console.WriteLine("Cube Id#{0} selected", selected.Id);
                }
            }
            return selected;
        }
    }


    public class EntryPoint
    {
        [STAThread]
        public static void Main()
        {
            using (Game win = new Game())
            {
                string version = GL.GetString(StringName.Version);
                if (version.StartsWith("3.1")) win.Run();
                else Debug.WriteLine("Requested OpenGL version not available.");
            }
        }
    }

}
