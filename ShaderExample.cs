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
        Cube cube;
        public int projectionMatrixLocation;
        int lightPositionLocation;
        Matrix4 projectionMatrix;
        Vector3 lightPosition;
        Cube[] cubeArray;
        Random randomNumber;
        Vector3 randomVector;
        Vector3 randomRotation;

        public Game()
            : base(640, 480, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 16, 0, 8), "OpenGL 3.1 Example", 0,
                DisplayDevice.Default, 3, 1, GraphicsContextFlags.Debug)
        {
            cube = new Cube(new Vector3(0, 0, -20), new Vector3(0.0f, 0.0f, 0.25f*3.14f), new Color4(0.4f, 0.5f, 0.0f, 1.0f));
            randomNumber = new Random();
            cubeArray = new Cube[25];

            Mouse.ButtonDown += new EventHandler<OpenTK.Input.MouseButtonEventArgs>(MouseButtonDown);

            QueryMatrixLocations();

            float widthToHeight = ClientSize.Width / (float)ClientSize.Height;
            SetProjectionMatrix(Matrix4.CreatePerspectiveFieldOfView(0.25f, widthToHeight, 60, 120));
            SetLightPosition(new Vector3(3.0f, 4.0f, 5.0f));

            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(0.2f, 0.2f, 0.2f, 1);

            for (int i = 0; i < 25; i++)
            {
                randomVector = new Vector3(randomNumber.Next(-5, 5), randomNumber.Next(-5, 5), randomNumber.Next(-120, -10));
                randomRotation = new Vector3(randomNumber.Next(0, 314)/100, randomNumber.Next(0, 314)/100, 0);
                cubeArray[i] = new Cube(randomVector, randomRotation, new Color4(0.4f, 0.5f, 0.0f, 1.0f));
            }
        }

        public void MouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            Byte4 pixel = new Byte4();
            GL.ReadPixels(e.X, this.Height - e.Y, 1, 1, PixelFormat.Rgba, PixelType.UnsignedByte, ref pixel);
            Cube cubeRef;
            if ((cubeRef = (Cube)PickRegister.Instance.LookupSelectable((int)pixel.ToUInt32())) != null)
            {
                cubeRef.Color = new Color4(0.5f, 0.5f, 0.5f, 1.0f);
                Console.WriteLine("-- Picked Color Integer ID = {0}", pixel.ToUInt32());
            }
        }

        private void QueryMatrixLocations()
        {
            projectionMatrixLocation = GL.GetUniformLocation(cube.ShaderID, "projection_matrix");
            lightPositionLocation = GL.GetUniformLocation(cube.ShaderID, "lightPosition");
        }

        private void SetProjectionMatrix(Matrix4 matrix)
        {
            projectionMatrix = matrix;
            GL.UniformMatrix4(projectionMatrixLocation, false, ref projectionMatrix);
        }

        private void SetLightPosition(Vector3 light)
        {
            lightPosition = light;
            GL.Uniform3(lightPositionLocation, ref lightPosition);
        }

        /*      Load and Save methods
                private void SaveFile()
                {
                    using (Stream stream = File.Open("scene.gus", FileMode.Create))
                    {
                        BinaryFormatter bformatter = new BinaryFormatter();

                        Console.WriteLine("Writing Scene Information");
                        bformatter.Serialize(stream, vertexArray);
                        bformatter.Serialize(stream, indexArray);
                        stream.Close();
                    }
                }

                private void LoadFile(string filename)
                {
                    using (Stream stream = File.Open(filename, FileMode.Open))
                    {
                        BinaryFormatter bformatter = new BinaryFormatter();

                        Console.WriteLine("--Reading Scene Information from \"{0}\"", filename);
                        vertexArray = (Vertex[])bformatter.Deserialize(stream);
                        indexArray = (uint[])bformatter.Deserialize(stream);
                        stream.Close();
                    }
                }
                */

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
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            foreach (Cube sample in cubeArray)
            {
                sample.Draw();
            }

            GL.Flush();
            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            float widthToHeight = ClientSize.Width / (float)ClientSize.Height;
            SetProjectionMatrix(Matrix4.CreatePerspectiveFieldOfView(0.5f, widthToHeight, 1, 150));
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
