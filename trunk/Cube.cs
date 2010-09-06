using System;
using System.IO;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;

namespace LearnShader
{
    public class Cube : ISelectable
    {
        // Static Fields
        private static Shader cubeShader;
        private static string name = "cube";
        private static FrameBufferManager fbManager;

        // Instance Fields
        private Color4 color;
        private Vector3 position;
        private Vector3 rotation;
        private Mesh cubeMesh;
        private string sourceFile;
        private int surfaceColorLocation;
        private Matrix4 modelviewMatrix;
        private int modelviewMatrixLocation;
        private int id;
        private bool isSelected;

        // Constructors
        static Cube()
        {
            cubeShader = Shader.CreateShader("cube.vert", "cube.frag", name);
        }
        public Cube(Vector3 position, Vector3 rotation, Color4 color)
        {
            this.position = position;
            this.rotation = rotation;

            this.registerCube();
            this.color = new Color4((byte)id, (byte)(id >> 8), (byte)(id >> 16), (byte)(id >> 24));

            sourceFile = @"C:\Temp\cube.obj";

            cubeShader.Bind();
            cubeMesh = Mesh.CreateMesh(sourceFile, name);
            fbManager = FrameBufferManager.Instance;

            GL.EnableVertexAttribArray(0);
            GL.BindAttribLocation(cubeShader.ShaderID, 0, "vertex_position");
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, 0);

            GL.EnableVertexAttribArray(1);
            GL.BindAttribLocation(cubeShader.ShaderID, 1, "vertex_normal");
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, Vector3.SizeInBytes);

            surfaceColorLocation = GL.GetUniformLocation(cubeShader.ShaderID, "surfaceColor");
            modelviewMatrixLocation = GL.GetUniformLocation(cubeShader.ShaderID, "modelview_matrix");
        }

        // Properties
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }
        public Vector3 Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }
        public Color4 Color
        {
            get { return color; }
            set { color = value; }
        }
        public static int ShaderID
        {
            get { return cubeShader.ShaderID; }
        }
        public int Id
        {
            get { return id; }
        }
        public bool IsSelected
        {
            get { return isSelected; }
        }

        // Methods
        private void registerCube()
        {
            id = PickRegister.Instance.GetId(this);
        }
        public void Draw()
        {
            cubeShader.Bind();
            modelviewMatrix = Matrix4.CreateRotationX(rotation.X) *
                              Matrix4.CreateRotationY(rotation.Y) *
                              Matrix4.CreateRotationZ(rotation.Z) *
                              Matrix4.CreateTranslation(position);
            GL.UniformMatrix4(modelviewMatrixLocation, false, ref modelviewMatrix);
            GL.Uniform4(surfaceColorLocation, color);
            cubeMesh.Draw();
        }
    }


    public interface ISelectable
    {
        int Id { get; }
        bool IsSelected { get; }
    }
}
