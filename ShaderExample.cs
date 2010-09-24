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
        Cube[] cubeArray;
        Random randomNumber;
        Vector3 randomVector;
        Vector3 randomRotation;
        FrameBufferManager fbManager;
        bool showingSelectBuffer = false;
        FullScreenQuad FSQuad;
        bool selectDrag = false;
        Vector2 mousePosition;
        Selection selection;
        Vector3 selectionOffset;

        // Constructors
        public Game()
            : base(640, 480, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 16, 0, 8),
                   "OpenGL 3.1 Example", 0, DisplayDevice.Default, 3, 1, GraphicsContextFlags.Debug)
        {
            // Initialise fields
            mousePosition = new Vector2();

            // Setup window view.
            float widthToHeight = ClientSize.Width / (float)ClientSize.Height;
            Shader.SetProjectionMatrix(Matrix4.CreatePerspectiveFieldOfView(0.25f, widthToHeight, 60, 120));
            Shader.SetLightPosition(new Vector3(3.0f, 4.0f, 5.0f));
            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(0.2f, 0.2f, 0.2f, 1);

            // Register a button down event.
            Mouse.ButtonDown += new EventHandler<OpenTK.Input.MouseButtonEventArgs>(MouseButtonDown);
            Mouse.ButtonUp += new EventHandler<OpenTK.Input.MouseButtonEventArgs>(MouseButtonUp);
            Keyboard.KeyDown += new EventHandler<KeyboardKeyEventArgs>(KeyboardKeyDown);


            //  ------- Sandbox -------

            // Initialise objects to be drawn in the scene.
            FSQuad = new FullScreenQuad(ClientSize.Width, ClientSize.Height);
            randomNumber = new Random();
            cubeArray = new Cube[25];

            // Populate the Cube array with Cubes at random locations and rotations.
            for (int i = 0; i < 25; i++)
            {
                randomVector = new Vector3(randomNumber.Next(-5, 5), randomNumber.Next(-5, 5), randomNumber.Next(-120, -10));
                randomRotation = new Vector3(randomNumber.Next(0, 314) / 100, randomNumber.Next(0, 314) / 100, 0);
                cubeArray[i] = new Cube(randomVector, randomRotation, new Color4(0.4f, 0.5f, 0.0f, 1.0f));
            }

            // Create references to the manager classes singletons.
            fbManager = FrameBufferManager.Instance;

            // Add introduction text
            ResetConsole();
        }

        // Methods
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            Fps.GetFps(e.Time);
            mousePosition = new Vector2(Mouse.X, Mouse.Y);

            if (selectDrag == true)
            {
                if (selection.Selected != null)
                {
                    selectionOffset = selection.Selected.Position - selection.SurfaceCoordinate;
                    Vector3 newPosition = unProject(mousePosition.X, mousePosition.Y); // Possible innacuracy here
                    newPosition.Z = selection.Selected.Position.Z;
                    selection.Selected.Position = newPosition;
                }
            }
            foreach (Cube sample in cubeArray)
            {
                if (selection.Selected != null)
                    if (sample.Id == selection.Selected.Id)
                    {
                        // Do nothing
                    }
                    else
                        sample.Rotation = sample.Rotation + new Vector3(0.01f, 0.02f, 0.0f);
                else
                    sample.Rotation = sample.Rotation + new Vector3(0.01f, 0.02f, 0.0f);
            }

            if (Keyboard[OpenTK.Input.Key.Escape])
                Exit();
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            DrawScene(RenderState.Render);
            if (showingSelectBuffer)
            {
                fbManager.ReadFBO(RenderState.Select);
                GL.BlitFramebuffer(0, 0, Width, Height, 0, 0, Width, Height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
            }
            else
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
        private void ResetConsole()
        {
            FSQuad.AddText("-- Toggle the console by pressing the 'Tilde' key.");
            FSQuad.AddText("-- Select and unselect cubes by clicking on them with the left mouse button.");
            FSQuad.AddText("-- To toggle the selection buffer view, press 'Ctrl-B'.");
            FSQuad.AddText("-- Press F1 to view these instructions.");
            FSQuad.AddText("-- To exit program, press 'Esc'");
        }
        private void MouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Left)
            {
                if (selection.Selected != null)
                    selection.Selected.IsSelected = false;
                selection = PickColor(e.X, e.Y);
                selectDrag = true;
            }
        }
        private void MouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            selectDrag = false;
        }
        private void KeyboardKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            #region 'Tilde'
            if (e.Key == Key.Tilde)
                FSQuad.Toggle();
            #endregion
            #region 'Ctrl-B'
            if (e.Key == Key.B)
            {
                if (Keyboard[Key.LControl])
                {
                    if (showingSelectBuffer == true)
                    {
                        showingSelectBuffer = false;
                        FSQuad.AddText("Selection Buffer was displayed.");
                    }
                    else
                        showingSelectBuffer = true;
                }
            }
            #endregion
            #region 'F1'
            if (e.Key == Key.F1)
                ResetConsole();
            #endregion
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
        private Selection PickColor(int x, int y)
        {
            // TODO: Tidy this method up

            DrawScene(RenderState.Select);
            fbManager.ReadFBO(RenderState.Select);
            Byte4 pixel = new Byte4();
            GL.ReadPixels(x, this.Height - y, 1, 1, PixelFormat.Rgba, PixelType.UnsignedByte, ref pixel);
            Cube selectedCube = (Cube)PickRegister.Instance.LookupSelectable((int)pixel.ToUInt32());
            if (selectedCube != null)
            {
                if (selectedCube.IsSelected == true)
                {
                    selectedCube.IsSelected = false;
                    FSQuad.AddText("Cube Id #" + selectedCube.Id + " unselected");
                }
                else
                {
                    selectedCube.IsSelected = true;
                    FSQuad.AddText("Cube Id #" + selectedCube.Id + " selected");
                }
            }

            Vector3 worldCoordinate = unProject(x, y);
            Selection selection = new Selection(selectedCube, worldCoordinate);

            return selection;
        }
        private Vector3 unProject(float screenX, float screenY)
        {
            fbManager.ReadFBO(RenderState.Select);
            screenY = this.Height - screenY;
            float zDepth = new float();
            GL.ReadPixels((int)screenX, (int)screenY, 1, 1, PixelFormat.DepthComponent, PixelType.Float, ref zDepth); 
            float xDeviceCoord = screenX / (float)this.Width * 2.0f - 1.0f;
            float yDeviceCoord = screenY / (float)this.Height * 2.0f - 1.0f;
            float zDeviceCoord = zDepth * 2.0f - 1.0f;
            Matrix4 projectionMatrix = Shader.ProjectionMatrix;
            Vector4 deviceCoorinates = new Vector4(xDeviceCoord, yDeviceCoord, zDeviceCoord, 1.0f);
            Vector4 worldCoordinates = Vector4.Transform(deviceCoorinates, Matrix4.Invert(projectionMatrix));
            worldCoordinates = worldCoordinates / worldCoordinates.W;
            Vector3 outCoordinate = new Vector3(worldCoordinates.X, worldCoordinates.Y, worldCoordinates.Z);

            return outCoordinate;
        }
    }

    public struct Selection
    {
        // Private Fields
        private ISelectable selected;
        private Vector3 surfaceCoordinate;

        // Constructors
        public Selection(ISelectable selected, Vector3 surfaceCoordinate)
        {
            this.selected = selected;
            this.surfaceCoordinate = surfaceCoordinate;
        }

        // Properties
        public ISelectable Selected { get { return selected; } }
        public Vector3 SurfaceCoordinate { get { return surfaceCoordinate; } }
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
