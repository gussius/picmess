using System;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Runtime.Serialization;

namespace LearnShader
{
    [Serializable]
    struct Vertex : ISerializable
    {
        private Vector3 position;
        private Vector3 normal;
        private static readonly int sizeInBytes = 24;

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

        public override string ToString()
        {
            return "Position: {" + position.X + ", " + position.Y + ", " + position.Z + "}\n" +
                   "Normal:   {" + normal.X + ", " + normal.Y + ", " + normal.Z + "}";
        }
        
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

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("position", position);
            info.AddValue("normal", normal);
        }
    }
    
    class VNPair
    {
        private int position;
        private int normal;

        public VNPair(int pos_ref, int norm_ref)
        {
            this.position = pos_ref;
            this.normal = norm_ref;
        }

        public VNPair()
        {
            this.position = 0;
            this.normal = 0;
        }

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
    }

    class Shader
	{
        //TODO: Need to work in sone error control here.

		private int shaderID;
		private int vertexShader;
		private int fragmentShader;

		public int ShaderID
		{
            get { return shaderID; }
		}

		public Shader(string vsFileName, string fsFileName)
		{
			vertexShader = GL.CreateShader(ShaderType.VertexShader);
			fragmentShader = GL.CreateShader(ShaderType.FragmentShader);

			string vsString = File.ReadAllText(vsFileName);
            GL.ShaderSource(vertexShader, vsString);
            
            string fsString = File.ReadAllText( fsFileName );
			GL.ShaderSource(fragmentShader, fsString);

            GL.CompileShader(vertexShader);
            ValidateShader(vertexShader, vsFileName);
            GL.CompileShader(fragmentShader);
            ValidateShader(fragmentShader, fsFileName);

            shaderID = GL.CreateProgram();
			GL.AttachShader(shaderID, vertexShader);
			GL.AttachShader(shaderID, fragmentShader);
			GL.LinkProgram(shaderID);
			ValidateProgram(shaderID);

			// TO DO: Check if program compiled correctly and output error message if need be.
            Shader.ValidateShader(ShaderID, "shader1.vert");
            Shader.ValidateShader(ShaderID, "shader1.frag");
            Shader.ValidateProgram(ShaderID);
		}

/*      Deconstructor ?? (Need this?)
		~Shader()
		{
			GL.DetachShader(shaderID, fragmentShader);
			GL.DetachShader(shaderID, vertexShader);

			GL.DeleteShader(fragmentShader);
			GL.DeleteShader(vertexShader);
			GL.DeleteProgram(shaderID);
		}
		*/
		
		public void Bind()
		{
			GL.UseProgram(shaderID);
		}

		public void UnBind()
		{
			GL.UseProgram(0);
		}

		static void ValidateShader(int shader, string fileName)
		{
			string outputBuffer = string.Empty;
			outputBuffer = GL.GetShaderInfoLog(shader);

			if (outputBuffer != string.Empty)
				Console.Write("Shader {0}, file {1} compile error: {2}\n", shader, fileName, outputBuffer);
		}

		static void ValidateProgram(int program)
		{
			string outputBuffer = string.Empty;
			outputBuffer = GL.GetProgramInfoLog(program);

			if (outputBuffer != string.Empty)
				Console.Write("Program {0} linking error: {1}\n", program, outputBuffer);

			GL.ValidateProgram(program);
			int status;
			GL.GetProgram(program, ProgramParameter.ValidateStatus, out status);

			if (status < 1)
				Console.WriteLine("Error validating shader");
		}
	}
}
