﻿using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace LearnShader
{
    public sealed class Shader
    {
        // Static Fields
        private static Dictionary<string, Shader> shaderRegister = new Dictionary<string, Shader>();
        private static Matrix4 projectionMatrix;
        private static Vector3 lightPosition;
    
        // Instance Fields
        private int shaderID;
        private int vertexShader;
        private int fragmentShader;
        private int projectionMatrixLocation;
        private int lightPositionLocation;

        // Constructors
        private Shader(string vsFileName, string fsFileName, string shaderName)
        {
            shaderRegister.Add(shaderName, this);

            string shaderVersion = GL.GetString(StringName.ShadingLanguageVersion);
            Console.WriteLine("--Shader Version = {0}", shaderVersion);

            vertexShader = GL.CreateShader(ShaderType.VertexShader);
            fragmentShader = GL.CreateShader(ShaderType.FragmentShader);

            string vsString = File.ReadAllText(vsFileName);
            GL.ShaderSource(vertexShader, vsString);

            string fsString = File.ReadAllText(fsFileName);
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

            this.Bind();

            projectionMatrixLocation = GL.GetUniformLocation(this.ShaderID, "projection_matrix");
            Console.WriteLine("projectionMatrixLocation = {0}", projectionMatrixLocation);

            lightPositionLocation = GL.GetUniformLocation(this.ShaderID, "lightPosition");
            Console.WriteLine("lightPositionLocation = {0}", lightPositionLocation);
            GL.UniformMatrix4(projectionMatrixLocation, false, ref projectionMatrix);
            GL.Uniform3(lightPositionLocation, ref lightPosition);
            this.UnBind();
        }

        // Properties
        public int ShaderID
        {
            get { return shaderID; }
        }

        // Static Methods
        public static Shader CreateShader(string vsFileName, string fsFileName, string shaderName)
        {
            if (shaderRegister.ContainsKey(shaderName))
                return shaderRegister[shaderName];

            return new Shader(vsFileName, fsFileName, shaderName);
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
        public static void SetProjectionMatrix(Matrix4 matrix)
        {
            projectionMatrix = matrix;
        }
        public static void SetLightPosition(Vector3 light)
        {
            lightPosition = light;
        }

        // Instance Methods
        public void Bind()
        {
            GL.UseProgram(shaderID);
        }
        public void UnBind()
        {
            GL.UseProgram(0);
        }
    }
}
