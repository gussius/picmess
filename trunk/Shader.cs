using System;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace LearnShader
{
    class Shader
	{
        /*TODO: Need to work in sone error control here.
         */
		private int shaderID;
		private int vertexShader;
		private int fragmentShader;

		public int ShaderID
		{
            get { return shaderID; }
		}

		public Shader(string vsFileName, string fsFileName)
		{
            string shaderVersion = GL.GetString(StringName.ShadingLanguageVersion);
            Console.WriteLine("--Shader Version = {0}", shaderVersion);

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
		}

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
				Console.Write("--Shader {0}, file {1} compile error: {2}\n", shader, fileName, outputBuffer);
            else
                Console.Write("--Shader {0}, file {1} Validated\n", shader, fileName);

		}

        static void ValidateProgram(int program)
		{
			string outputBuffer = string.Empty;
			outputBuffer = GL.GetProgramInfoLog(program);

			if (outputBuffer != string.Empty)
				Console.Write("--Program {0} linking error: {1}\n", program, outputBuffer);
            else
                Console.Write("--Program {0} Validated\n", program);

			GL.ValidateProgram(program);
			int status;
			GL.GetProgram(program, ProgramParameter.ValidateStatus, out status);

			if (status != 1)
				Console.WriteLine("--Error validating shader");
		}
	}
}
