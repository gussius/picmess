using System;
using System.Drawing;
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
        Scene scene1;
        Random randomNumber;
        Vector3 randomVector;
        Vector3 randomRotation;
        FrameBufferManager fbManager;
        bool showingSelectBuffer = false;
        FullScreenQuad console;
        Selection singleSelection;

        // Constructors
        public Game()
            : base(640, 480, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 16, 0, 8),
                   "OpenGL 3.1 Example", 0, DisplayDevice.Default, 3, 1, GraphicsContextFlags.Debug)
        {
            // Initialise fields
            WindowBorder = WindowBorder.Fixed;
            singleSelection = new Selection();

            // Setup window view.
            float widthToHeight = ClientSize.Width / (float)ClientSize.Height;
            Shader.SetProjectionMatrix(Matrix4.CreatePerspectiveFieldOfView(0.25f, widthToHeight, 60, 120));
            Shader.SetLightPosition(new Vector3(3.0f, 4.0f, 5.0f));
            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(0.2f, 0.2f, 0.2f, 1);

            // Register a button down event.
            Mouse.ButtonDown += new EventHandler<OpenTK.Input.MouseButtonEventArgs>(MouseButtonDown);
            Mouse.ButtonUp += new EventHandler<OpenTK.Input.MouseButtonEventArgs>(MouseButtonUp);
            Mouse.WheelChanged += new EventHandler<MouseWheelEventArgs>(MouseWheelChanged);
            Keyboard.KeyDown += new EventHandler<KeyboardKeyEventArgs>(KeyboardKeyDown);


            //  ------- Sandbox -------

            // Initialise objects to be drawn in the scene.
            console = FullScreenQuad.Console;
            randomNumber = new Random();
            scene1 = new Scene();
            //cubeArray = new Cube[25];

            // Populate the Scene with Cubes at random locations and rotations.
            for (int i = 0; i < 25; i++)
            {
                randomVector = new Vector3(randomNumber.Next(-5, 5), randomNumber.Next(-5, 5), randomNumber.Next(-120, -10));
                randomRotation = new Vector3(randomNumber.Next(0, 314) / 100, randomNumber.Next(0, 314) / 100, 0);
                scene1.AddActor(new Cube(randomVector, randomRotation, new Color4(0.4f, 0.5f, 0.0f, 1.0f)));
            }

            // Create references to the manager classes singletons.
            fbManager = FrameBufferManager.Instance;

            // Add introduction text
            console.DisplayHelp();
        }

        // Methods
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            singleSelection.DragTo(Mouse.X, Mouse.Y);

            // Rotate all cubes except selected cube.
            foreach (ISelectable sample in scene1.ActorList)
            {
                if (singleSelection.Info.Selected == null)
                    sample.Rotation = sample.Rotation + new Vector3(0.01f, 0.02f, 0.0f);
                else
                    if (sample.Id !=  singleSelection.Info.Selected.Id)
                        sample.Rotation = sample.Rotation + new Vector3(0.01f, 0.02f, 0.0f);
            }
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            scene1.DrawScene(RenderState.Render);
            if (showingSelectBuffer)
            {
                fbManager.ReadFBO(RenderState.Select);
                GL.BlitFramebuffer(0, 0, Width, Height, 0, 0, Width, Height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
            }
            else
                console.Draw();

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

        // User Interface Methods
        private void MouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Left)
                singleSelection.Pick(scene1, e.X, e.Y);
        }
        private void MouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            singleSelection.DisableDrag(Mouse.X, Mouse.Y);
        }
        private void MouseWheelChanged(object sender, MouseWheelEventArgs e)
        {
            console.Update(e.Position.Y, e.Delta);
        }
        private void KeyboardKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            #region 'Tilde'
            if (e.Key == Key.Tilde)
                console.Toggle();
            #endregion
            #region 'Ctrl-B'
            if (e.Key == Key.B)
            {
                if (Keyboard[Key.LControl])
                {
                    if (showingSelectBuffer == true)
                    {
                        showingSelectBuffer = false;
                        console.AddText("Selection Buffer was displayed.");
                    }
                    else
                        showingSelectBuffer = true;
                }
            }
            #endregion
            #region 'F1'
            if (e.Key == Key.F1)
                console.DisplayHelp();
            #endregion
            #region 'Esc'
            if (e.Key == Key.Escape)
                Exit();
            #endregion
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
