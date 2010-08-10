// The following is code from an OpenTK tutorial.

using System;
using System.IO;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace LearnShader
{
    public class Game: GameWindow
    {
        Cube cube;
        int     modelviewMatrixLocation;
        int     projectionMatrixLocation;
        int     lightPositionLocation;
        Matrix4 projectionMatrix;
        Matrix4 modelviewMatrix;
        Vector3 lightPosition;

        public Game() : base( 640, 480, new GraphicsMode( new ColorFormat( 8, 8, 8, 8 ), 16, 0, 8), "OpenGL 3.1 Example", 0,
            DisplayDevice.Default, 3, 1, GraphicsContextFlags.Debug )
        {
            cube = new Cube(new Vector3(0, 0, 0), new Vector3(0.4f, 0.5f, 0.0f));
            
            QueryMatrixLocations();
 
            float widthToHeight = ClientSize.Width / ( float )ClientSize.Height;
            SetProjectionMatrix(Matrix4.CreatePerspectiveFieldOfView(0.5f, widthToHeight, 1, 30));
            SetModelviewMatrix( Matrix4.CreateRotationX( 0.5f ) * Matrix4.CreateTranslation( 0, -8, -100 ) );
            SetLightPosition( new Vector3(3.0f, 4.0f, 5.0f) );

            GL.Enable( EnableCap.DepthTest );
            GL.ClearColor( 0.2f, 0.2f, 0.2f, 1 );
        }

        private void QueryMatrixLocations()
         {
             projectionMatrixLocation = GL.GetUniformLocation(cube.ShaderID, "projection_matrix");
             modelviewMatrixLocation = GL.GetUniformLocation(cube.ShaderID, "modelview_matrix");
             lightPositionLocation = GL.GetUniformLocation(cube.ShaderID, "lightPosition");
         }

        private void SetModelviewMatrix(Matrix4 matrix)
         {
             modelviewMatrix = matrix;
             GL.UniformMatrix4(modelviewMatrixLocation, false, ref modelviewMatrix);
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
            SetModelviewMatrix(Matrix4.CreateRotationY((float)e.Time/2) * modelviewMatrix);

            if (Keyboard[OpenTK.Input.Key.Escape])
            {
                Exit();
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            cube.Draw();
            GL.Flush();
            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            float widthToHeight = ClientSize.Width / (float)ClientSize.Height;
            SetProjectionMatrix(Matrix4.CreatePerspectiveFieldOfView(0.5f, widthToHeight, 1, 150));
        }
}

    public class Program
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
