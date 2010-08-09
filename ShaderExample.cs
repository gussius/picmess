// The following is code from an OpenTK tutorial.

using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace LearnShader
{
    public class HelloGL3: GameWindow
    {
        //Cube testCube;
        Shader shader1;
        int modelviewMatrixLocation;
        int projectionMatrixLocation;
        int lightPositionLocation;
        int indicesVboHandle;
        int VboID;
        uint[] indexArray;
        Matrix4 projectionMatrix, modelviewMatrix;
        Vector3 lightPosition;
        Vertex[] vertexArray;

        public HelloGL3() : base( 640, 480, new GraphicsMode( new ColorFormat( 8, 8, 8, 8 ), 16, 0, 8), "OpenGL 3.1 Example", 0,
            DisplayDevice.Default, 3, 1, GraphicsContextFlags.Debug )
        {
            //testCube = new Cube(new Vector3(0, 0, 0), new Vector3(0.8f, 0.5f, 0.0f));
            shader1 = new Shader("cluster.vert", "cluster.frag");
            shader1.Bind();

            QueryMatrixLocations();
 
            float widthToHeight = ClientSize.Width / ( float )ClientSize.Height;
            SetProjectionMatrix(Matrix4.CreatePerspectiveFieldOfView(0.5f, widthToHeight, 1, 30));
            SetModelviewMatrix( Matrix4.CreateRotationX( 0.5f ) * Matrix4.CreateTranslation( 0, -8, -100 ) );
            SetLightPosition( new Vector3(3.0f, 4.0f, 5.0f) );

            LoadObjData(); //Load from obj file
            //LoadFile("scene.gus");
            //SaveFile(); //Save loaded obj data to binary ".gus" file.
            LoadVertices();
            //LoadIndexer();

            

            GL.Enable( EnableCap.DepthTest );
            GL.ClearColor( 0.2f, 0.2f, 0.2f, 1 );
        }

        private void LoadObjData()
        {
            int v, vn, vt, f;
            VNPair[] objIndexBuffer;
            int[] indexBuffer;
            VNPair[] vertexBuffer;
            Vector3[] objVertexBuffer;
            Vector3[] objNormalBuffer;
            string fileName = @"c:\temp\cluster.obj";

            Regex vertexRegex = new Regex("(?<xcoord>-?\\d*\\.\\d{4}) (?<ycoord>-?\\d*\\.\\d{4}) (?<zcoord>-?\\d*\\.\\d{4})");
            Regex facesRegex = new Regex("(?<a>\\d*)/\\d*/(?<d>\\d*) (?<b>\\d*)/\\d*/(?<e>\\d*) (?<c>\\d*)/\\d*/(?<f>\\d*)");

            Console.WriteLine("--Loading {0}", fileName);

            using (StreamReader tr = new StreamReader(fileName))
            {
                // initialise the array counters
                v = 0; vn = 0; vt = 0; f = 0;

                string line;
                Match vertexMatch, faceMatch;

                #region First pass for counting lines to ascertain array lengths
                while (tr.Peek() > -1)
                {
                    line = tr.ReadLine();

                    if (line.StartsWith("vn"))
                        vn++;
                    else if (line.StartsWith("vt"))
                        vt++;
                    else if (line.StartsWith("v"))
                        v++;
                    else if (line.StartsWith("f"))
                        f++;
                }
#endregion

                objVertexBuffer = new Vector3[v];
                vertexBuffer = new VNPair[f * 3];
                indexBuffer = new int[f * 3];
                objNormalBuffer = new Vector3[vn];
                objIndexBuffer = new VNPair[f * 3];

                // Reset filestream back to zero
                tr.BaseStream.Seek(0, SeekOrigin.Begin);
                tr.DiscardBufferedData();

                int vCount = 0;
                int vnCount = 0;
                int fnCount = 0;
                float x, y, z;
                
                #region Second pass for loading data into arrays
                while (tr.Peek() > -1)
                {
                    line = tr.ReadLine();
                    vertexMatch = vertexRegex.Match(line);
                    faceMatch = facesRegex.Match(line);

                    if (vertexMatch.Success)
                    {
                        x = (float)Convert.ToDecimal(vertexMatch.Groups["xcoord"].Value);
                        y = (float)Convert.ToDecimal(vertexMatch.Groups["ycoord"].Value);
                        z = (float)Convert.ToDecimal(vertexMatch.Groups["zcoord"].Value);

                        if (line.StartsWith("vn"))
                            objNormalBuffer[vnCount++] = new Vector3(x, y, z);

                        else if (line.StartsWith("v") && !line.StartsWith("vt"))
                            objVertexBuffer[vCount++] = new Vector3(x, y, z);
                    }

                    if (faceMatch.Success)
                    {
                        if (line.StartsWith("f"))
                        {
                            objIndexBuffer[fnCount++] = new VNPair(Convert.ToInt16(faceMatch.Groups["a"].Value) - 1,
                                                                   Convert.ToInt16(faceMatch.Groups["d"].Value) - 1);
                            objIndexBuffer[fnCount++] = new VNPair(Convert.ToInt16(faceMatch.Groups["b"].Value) - 1,
                                                                   Convert.ToInt16(faceMatch.Groups["e"].Value) - 1);
                            objIndexBuffer[fnCount++] = new VNPair(Convert.ToInt16(faceMatch.Groups["c"].Value) - 1,
                                                                   Convert.ToInt16(faceMatch.Groups["f"].Value) - 1);
                        }
                    }
                }
                #endregion
                
                tr.Close();
            }

            int vPair = 0;
            int iCounter = 0;

            #region Now process arrays to sort and duplicate vertices with multiple normals
            foreach (VNPair index in objIndexBuffer)
            {
                for (int i = 0; i <= vPair; i++)
                {
                        
                    if (vertexBuffer[i] == index)
                    {
                        indexBuffer[iCounter++] = i;
                        break;
                    }
                    else
                    {
                        if (i == vPair)
                        {
                            indexBuffer[iCounter++] = i;
                            vertexBuffer[vPair++] = index;
                            break;
                        }
                    }
                }
            }
#endregion

            #region Populate vertex array...
            vertexArray = new Vertex[vPair];
            indexArray = new uint[iCounter];

            for (int i = 0; i < vPair; i++)
            {
                vertexArray[i].Position = objVertexBuffer[vertexBuffer[i].P];
                vertexArray[i].Normal = objNormalBuffer[vertexBuffer[i].N];
            }

            for (int i = 0; i < iCounter; i++)
            {
                indexArray[i] = (uint)indexBuffer[i];
            }
            #endregion
        }

        private void QueryMatrixLocations()
         {
             projectionMatrixLocation = GL.GetUniformLocation(shader1.ShaderID, "projection_matrix");
             modelviewMatrixLocation = GL.GetUniformLocation(shader1.ShaderID, "modelview_matrix");
             lightPositionLocation = GL.GetUniformLocation(shader1.ShaderID, "lightPosition");
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

        private void LoadVertices()
        {
            GL.GenBuffers(1, out VboID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VboID);
            GL.BufferData<Vertex>(BufferTarget.ArrayBuffer,
                new IntPtr(vertexArray.Length * Vertex.SizeInBytes),
                vertexArray, BufferUsageHint.StaticDraw);

            LoadIndexer();

            GL.EnableVertexAttribArray(0);
            GL.BindAttribLocation(shader1.ShaderID, 0, "vertex_position");
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, 0);

            GL.EnableVertexAttribArray(1);
            GL.BindAttribLocation(shader1.ShaderID, 1, "vertex_normal");
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, Vector3.SizeInBytes);
        }

        private void LoadIndexer()
        {
            GL.GenBuffers(1, out indicesVboHandle);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indicesVboHandle);
            GL.BufferData<uint>(BufferTarget.ElementArrayBuffer,
                new IntPtr(indexArray.Length * Vector3.SizeInBytes),
                indexArray, BufferUsageHint.StaticDraw);
        }
/*        
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
                //SaveFile();
                Exit();
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.DrawElements(BeginMode.Triangles, indexArray.Length,
                DrawElementsType.UnsignedInt, IntPtr.Zero);

            //testCube.Draw();

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
            using (HelloGL3 win = new HelloGL3())
            {
                string version = GL.GetString(StringName.Version);
                if (version.StartsWith("3.1")) win.Run();
                else Debug.WriteLine("Requested OpenGL version not available.");
            }
        }
    }

}
