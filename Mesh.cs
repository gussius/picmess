using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace LearnShader
{
    class Mesh
    {
        // Static Fields
        private static Dictionary<string, Mesh> meshRegister = new Dictionary<string, Mesh>();

        // Instance Fields
        private uint[] indexArray;
        private Vertex[] vertexArray;
        private int VboID;
        private int indicesVboHandle;

        // Constructors
        public static Mesh CreateMesh(string fileName, string meshName)
        {
            if (meshRegister.ContainsKey(meshName))
                return meshRegister[meshName];

            return new Mesh(fileName, meshName);
        }
        private Mesh(string fileName, string meshName)
        {
            meshRegister.Add(meshName, this);
            LoadObjFile(fileName);
            LoadVertices();
            LoadIndexer();
        }

        // Methods
        private void LoadIndexer()
        {
            GL.GenBuffers(1, out indicesVboHandle);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indicesVboHandle);
            GL.BufferData<uint>(BufferTarget.ElementArrayBuffer,
                new IntPtr(indexArray.Length * Vector3.SizeInBytes),
                indexArray, BufferUsageHint.StaticDraw);
        }
        public void LoadVertices()
        {
            GL.GenBuffers(1, out VboID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VboID);
            GL.BufferData<Vertex>(BufferTarget.ArrayBuffer,
                new IntPtr(vertexArray.Length * Vertex.SizeInBytes),
                vertexArray, BufferUsageHint.StaticDraw);
        }
        public void Draw()
        {
            GL.DrawElements(BeginMode.Triangles, indexArray.Length,
                            DrawElementsType.UnsignedInt, IntPtr.Zero);
        }
        public void LoadObjFile(string objFileName)
        {
            int v, vn, vt, f;
            VNPair[] objIndexBuffer;
            int[] indexBuffer;
            VNPair[] vertexBuffer;
            Vector3[] objVertexBuffer;
            Vector3[] objNormalBuffer;

            Regex vertexRegex = new Regex("(?<xcoord>-?\\d*\\.\\d{4}) (?<ycoord>-?\\d*\\.\\d{4}) (?<zcoord>-?\\d*\\.\\d{4})");
            Regex facesRegex = new Regex("(?<a>\\d*)/\\d*/(?<d>\\d*) (?<b>\\d*)/\\d*/(?<e>\\d*) (?<c>\\d*)/\\d*/(?<f>\\d*)");

            Assembly assembly;
            StreamReader objStreamReader;
            assembly = Assembly.GetExecutingAssembly();

            using (objStreamReader = new StreamReader(assembly.GetManifestResourceStream("LearnShader.Geometry." + objFileName)))
            {
                Console.WriteLine("--Loading {0}\n", objFileName);

                // initialise the array counters
                v = 0; vn = 0; vt = 0; f = 0;

                string line;
                Match vertexMatch, faceMatch;

                #region First pass for counting lines to ascertain array lengths
                while (objStreamReader.Peek() > -1)
                {
                    line = objStreamReader.ReadLine();

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
                objStreamReader.BaseStream.Seek(0, SeekOrigin.Begin);
                objStreamReader.DiscardBufferedData();

                int vCount = 0;
                int vnCount = 0;
                int fnCount = 0;
                float x, y, z;

                #region Second pass for loading data into arrays
                while (objStreamReader.Peek() > -1)
                {
                    line = objStreamReader.ReadLine();
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

                objStreamReader.Close();
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
    }


    [Serializable]
    struct Vertex : ISerializable
    {
        // Static Fields
        private static readonly int sizeInBytes = 24;

        // Instance Fields
        private Vector3 position;
        private Vector3 normal;

        // Constructors
        public Vertex(Vector3 position, Vector3 normal)
        {
            this.position = position;
            this.normal = normal;
        }
        public Vertex(SerializationInfo info, StreamingContext context)
        {
            position = (Vector3)info.GetValue("position", typeof(Vector3));
            normal = (Vector3)info.GetValue("normal", typeof(Vector3));
        }

        // Properties
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }
        public Vector3 Normal
        {
            get { return normal; }
            set { normal = value; }
        }
        public static int SizeInBytes
        {
            get { return sizeInBytes; }
        }

        // Methods
        public override string ToString()
        {
            return "Position: {" + position.X + ", " + position.Y + ", " + position.Z + "}\n" +
                   "Normal:   {" + normal.X + ", " + normal.Y + ", " + normal.Z + "}";
        }
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("position", position);
            info.AddValue("normal", normal);
        }
    }


    class VNPair
    {
        // Fields
        private int position;
        private int normal;

        // Constructors
        public VNPair(int position, int normal)
        {
            this.position = position;
            this.normal = normal;
        }
        public VNPair()
        {
            this.position = 0;
            this.normal = 0;
        }

        // Properties
        public int P
        {
            get { return position; }
            set { position = value; }
        }
        public int N
        {
            get { return normal; }
            set { normal = value; }
        }

        // Operators
        public static bool operator ==(VNPair pair1, VNPair pair2)
        {
            if (((object)pair1 == null) && ((object)pair2 == null))
                return true;
            if (((object)pair1 != null) && ((object)pair2 == null))
                return false;
            if (((object)pair1 == null) && ((object)pair2 != null))
                return false;

            if ((pair1.P == pair2.P) && (pair1.N == pair2.N))
            {
                return true;
            }
            return false;
        }
        public static bool operator !=(VNPair pair1, VNPair pair2)
        {
            if ((pair1.P != pair2.P) || (pair1.N != pair2.N))
            {
                return true;
            }
            return false;
        }

        // Methods
        public override string ToString()
        {
            return "{" + this.P + ", " + this.N + "}";
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }


    struct Byte4
    {
        // Fields
        public byte R, G, B, A;

        // Constructors
        public Byte4(byte[] input)
        {
            R = input[0];
            G = input[1];
            B = input[2];
            A = input[3];
        }

        // Methods
        public uint ToUInt32()
        {
            byte[] temp = new byte[] { this.R, this.G, this.B, this.A };
            return BitConverter.ToUInt32(temp, 0);
        }
        public override string ToString()
        {
            return "{R, G, B, A} = { " + this.R + ", " + this.G + ", " + this.B + ", " + this.A + " }";
        }
    }
}
