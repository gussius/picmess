﻿// The following is code from an OpenTK tutorial.

using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;
//using OpenTK.Math;
 
namespace LearnShader
{
    public class HelloGL3: GameWindow
    {
        #region string vertexShaderSource
        string vertexShaderSource = @"
            

            #version 140
 
            // object space to camera space transformation
            uniform mat4 modelview_matrix; 
           
            attribute vec4 ambient_occlusion; 
            varying float ambient_occlusion_term;


            // camera space to clip coordinates
            uniform mat4 projection_matrix;
 
            // incoming vertex position
            in vec3 vertex_position;
 
            // incoming vertex normal
            in vec3 vertex_normal;
 
            // transformed vertex normal
            out vec3 normal;
 
            void main(void)
            {
              //not a proper transformation if modelview_matrix involves non-uniform scaling
              normal = ( modelview_matrix * vec4( vertex_normal, 0 ) ).xyz;

              // transforming the incoming vertex position
              gl_Position = projection_matrix * modelview_matrix * vec4( vertex_position, 1 );
                            
            }";
        #endregion

        #region string fragmentShaderSource
        string fragmentShaderSource = @"
            #version 140
 
            precision highp float;
 
            const vec3 ambient = vec3( 0.1, 0.1, 0.1 );
            const vec3 lightVecNormalized = normalize( vec3( 0.5, 0.5, 2 ) );
            const vec3 lightColor = vec3( 1.0, 0.8, 0.2 );
 
            in vec3 normal;
 
            out vec4 out_frag_color;
 
            void main(void)
            {
              float diffuse = clamp( dot( lightVecNormalized, normalize( normal ) ), 0.0, 1.0 );
              out_frag_color = vec4( ambient + diffuse * lightColor, 1.0 );
            }";
        #endregion

        int vertexShaderHandle,
            fragmentShaderHandle,
            shaderProgramHandle,
            modelviewMatrixLocation,
            projectionMatrixLocation,
            positionVboHandle,
            normalVboHandle,
            indicesVboHandle,
            VboID;
        int triangleCount = 0;
        double timeCount = 0;
 
        Matrix4 projectionMatrix, modelviewMatrix;

        Vertex[] vertexArray;
        uint[] indexArray;

        Vector3[] positionVboData = new Vector3[]{
            new Vector3(-1.0f, -1.0f,  1.0f),
            new Vector3( 1.0f, -1.0f,  1.0f),
            new Vector3( 1.0f,  1.0f,  1.0f),
            new Vector3(-1.0f,  1.0f,  1.0f),
            new Vector3(-1.0f, -1.0f, -1.0f),
            new Vector3( 1.0f, -1.0f, -1.0f), 
            new Vector3( 1.0f,  1.0f, -1.0f),
            new Vector3(-1.0f,  1.0f, -1.0f) }; 
 
        uint[] indicesVboData = new uint[]{
                // front face
                0, 1, 2, 2, 3, 0,
                // top face
                3, 2, 6, 6, 7, 3,
                // back face
                7, 6, 5, 5, 4, 7,
                // left face
                4, 0, 3, 3, 7, 4,
                // bottom face
                0, 1, 5, 5, 4, 0,
                // right face
                1, 5, 6, 6, 2, 1, }; 

        public HelloGL3() : base( 640, 480, new GraphicsMode( new ColorFormat( 8, 8, 8, 8 ), 16 ), "OpenGL 3.1 Example", 0,
            DisplayDevice.Default, 3, 1, GraphicsContextFlags.Debug )
        {
            CreateShaders(); 
            CreateProgram();
            GL.UseProgram( shaderProgramHandle );
 
            QueryMatrixLocations();
 
            float widthToHeight = ClientSize.Width / ( float )ClientSize.Height;
            SetProjectionMatrix( Matrix4.Perspective( 1.3f, widthToHeight, 1, 20 ) );
 
            SetModelviewMatrix( Matrix4.RotateX( 0.5f ) * Matrix4.CreateTranslation( 0, 0, -4 ) );

            LoadObjData();
            LoadVertices();
            LoadIndexer();
 
            // Other state
            GL.Enable( EnableCap.DepthTest );
            GL.ClearColor( 0, 0.1f, 0.4f, 1 );
        }

        private void LoadObjData()
        {
            int v, vn, vt, f;
            VNPair[] objIndexBuffer;
            int[] indexBuffer;
            VNPair[] vertexBuffer;
            Vector3[] objVertexBuffer;
            Vector3[] objNormalBuffer;

            Regex vertexRegex = new Regex("(?<xcoord>-?\\d*\\.\\d{4}) (?<ycoord>-?\\d*\\.\\d{4}) (?<zcoord>-?\\d*\\.\\d{4})");
            Regex facesRegex = new Regex("(?<a>\\d*)/\\d*/(?<d>\\d*) (?<b>\\d*)/\\d*/(?<e>\\d*) (?<c>\\d*)/\\d*/(?<f>\\d*)");

            using (StreamReader tr = new StreamReader("c:\\temp\\torus.obj"))
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
                Console.WriteLine("v = {0}\nf= {1}", v, f);

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

            foreach (Vertex vertex in vertexArray)
            {
                Console.WriteLine(vertex);
            }

            foreach (uint member in indexArray)
            {
                Console.Write("{0} ", member);
            }

            
        }

        private void CreateShaders()
        {
            vertexShaderHandle = GL.CreateShader( ShaderType.VertexShader );
            fragmentShaderHandle = GL.CreateShader( ShaderType.FragmentShader );
 
            GL.ShaderSource( vertexShaderHandle, vertexShaderSource );
            GL.ShaderSource( fragmentShaderHandle, fragmentShaderSource );
 
            GL.CompileShader( vertexShaderHandle );
            GL.CompileShader( fragmentShaderHandle );
        }

        private void CreateProgram()
        {
            shaderProgramHandle = GL.CreateProgram();
 
            GL.AttachShader( shaderProgramHandle, vertexShaderHandle );
            GL.AttachShader( shaderProgramHandle, fragmentShaderHandle );
 
            GL.LinkProgram( shaderProgramHandle );
 
            string programInfoLog;
            GL.GetProgramInfoLog( shaderProgramHandle, out programInfoLog );
            Debug.WriteLine( programInfoLog );
        }

         private void QueryMatrixLocations()
         {
             projectionMatrixLocation = GL.GetUniformLocation(shaderProgramHandle, "projection_matrix");
             modelviewMatrixLocation = GL.GetUniformLocation(shaderProgramHandle, "modelview_matrix");
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

         private void LoadVertexPositions()
         {
             GL.GenBuffers(1, out positionVboHandle);
             GL.BindBuffer(BufferTarget.ArrayBuffer, positionVboHandle);
             GL.BufferData<Vector3>(BufferTarget.ArrayBuffer,
                 new IntPtr(positionVboData.Length * Vector3.SizeInBytes),
                 positionVboData, BufferUsageHint.StaticDraw);

             GL.EnableVertexAttribArray(0);
             GL.BindAttribLocation(shaderProgramHandle, 0, "vertex_position");
             GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, 0);
         }

         private void LoadVertexNormals()
         {
             GL.GenBuffers(1, out normalVboHandle);
             GL.BindBuffer(BufferTarget.ArrayBuffer, normalVboHandle);
             GL.BufferData<Vector3>(BufferTarget.ArrayBuffer,
                 new IntPtr(positionVboData.Length * Vector3.SizeInBytes),
                 positionVboData, BufferUsageHint.StaticDraw);

             GL.EnableVertexAttribArray(1);
             GL.BindAttribLocation(shaderProgramHandle, 1, "vertex_normal");
             GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, 0);
         }

         private void LoadVertices()
         {
             GL.GenBuffers(1, out VboID);
             GL.BindBuffer(BufferTarget.ArrayBuffer, VboID);
             GL.BufferData<Vertex>(BufferTarget.ArrayBuffer,
                 new IntPtr(vertexArray.Length * Vertex.SizeInBytes),
                 vertexArray, BufferUsageHint.StaticDraw);

             GL.EnableVertexAttribArray(0);
             GL.BindAttribLocation(shaderProgramHandle, 0, "vertex_position");
             GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, 0);

             GL.EnableVertexAttribArray(1);
             GL.BindAttribLocation(shaderProgramHandle, 1, "vertex_normal");
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

         protected override void OnUpdateFrame(FrameEventArgs e)
         {
             SetModelviewMatrix(Matrix4.RotateY((float)e.Time/2) * modelviewMatrix);

             if (Keyboard[OpenTK.Input.Key.Escape])
                 Exit();
         }

         protected override void OnRenderFrame(FrameEventArgs e)
         {
             GL.Viewport(0, 0, Width, Height);
             GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

             GL.DrawElements(BeginMode.Triangles, indexArray.Length,
                 DrawElementsType.UnsignedInt, IntPtr.Zero);

             GL.Flush();
             SwapBuffers();
         }

         protected override void OnResize(EventArgs e)
         {
             float widthToHeight = ClientSize.Width / (float)ClientSize.Height;
             SetProjectionMatrix(Matrix4.Perspective(1.3f, widthToHeight, 1, 20));
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

/* Reference to a running basic triangle code
namespace LearnShader
{
    class Game : GameWindow
	{
		/// <summary>Creates a 800x600 window with the specified title.</summary>
	    public Game()
	        : base(800, 600, GraphicsMode.Default, "OpenTK QBlender Example")
		{
	            VSync = VSyncMode.On;
		}
	
	    /// <summary>Load resources here.</summary>
	    /// <param name="e">Not used.</param>
	    protected override void OnLoad(EventArgs e)
	    {
	        base.OnLoad(e);

			GL.ClearColor(0.1f, 0.2f, 0.5f, 0.0f);
            GL.Enable(EnableCap.DepthTest);
        }

	
        /// <summary>
        /// Called when your window is resized. Set your viewport here. It is also
        /// a good place to set up your projection matrix (which probably changes
        /// along when the aspect ratio of your window).
        /// </summary>
        /// <param name="e">Not used.</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Width / (float)Height, 1.0f, 64.0f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
        }

	
        /// <summary>
        /// Called when it is time to setup the next frame. Add you game logic here.
        /// </summary>
        /// <param name="e">Contains timing information for framerate independent logic.</param>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (Keyboard[Key.Escape])
                Exit();
        }

		
        /// <summary>
        /// Called when it is time to render the next frame. Add your rendering code here.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			
			//Matrix4 modelview = Matrix4.LookAt(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY);
            //GL.MatrixMode(MatrixMode.Modelview);
           // GL.LoadMatrix(ref modelview);
            GL.Begin(BeginMode.Triangles);
                GL.Color3(0.2f, 0.6f, 0.9f);
                GL.Vertex3(-1, -1, -4);
                GL.Color3(0.0f, 0.6f, 0.0f);
                GL.Vertex3(0, 1, -4);
                GL.Color3(0.8f, 0.6f, 0.2f);
                GL.Vertex3(1, -1, -4);
            GL.End();
			
            SwapBuffers();
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
		{
            // The 'using' idiom guarantees proper resource cleanup.
            // We request 30 UpdateFrame events per second, and unlimited
            // RenderFrame events (as fast as the computer can handle).
            using (Game game = new Game())
			{
                game.Run(30.0);
			}
		}
		
		
		
	}
}
*/

