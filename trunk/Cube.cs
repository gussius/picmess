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
        private static Shader selectionShader;
        private static string name = "cube";
        private static FrameBufferManager fbManager;
        private static int surfaceColorLocation;
        private static int modelviewMatrixLocation;
        private static int selectColorLocation;
        private static int selectModelviewMatrixLocation;
        private static Mesh cubeMesh;
        private static string sourceFile;
        private static Color4 selectedColor = new Color4(0.5f, 0.5f, 0.5f, 1.0f);
      
        // Instance Fields
        private Color4 color;
        private Vector3 position;
        private Vector3 rotation;
        private Matrix4 modelviewMatrix;
        private int id;
        private bool isSelected;

        // Constructors
        static Cube()
        {
            sourceFile = @"C:\Temp\cube.obj";
            cubeMesh = Mesh.CreateMesh(sourceFile, name);
            fbManager = FrameBufferManager.Instance;
            
            cubeShader = Shader.CreateShader("cube.vert", "cube.frag", name);
            selectionShader = Shader.CreateShader("select.vert", "select.frag", "select");
            
            GL.EnableVertexAttribArray(0);
            GL.BindAttribLocation(cubeShader.ShaderID, 0, "vertex_position");
            GL.BindAttribLocation(selectionShader.ShaderID, 0, "vertex_position");
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, 0);

            GL.EnableVertexAttribArray(1);
            GL.BindAttribLocation(cubeShader.ShaderID, 1, "vertex_normal");
            GL.BindAttribLocation(selectionShader.ShaderID, 1, "vertex_normal");
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, Vector3.SizeInBytes);

            surfaceColorLocation = GL.GetUniformLocation(cubeShader.ShaderID, "surfaceColor");
            selectColorLocation = GL.GetUniformLocation(selectionShader.ShaderID, "surfaceColor");
            modelviewMatrixLocation = GL.GetUniformLocation(cubeShader.ShaderID, "modelview_matrix");
            selectModelviewMatrixLocation = GL.GetUniformLocation(selectionShader.ShaderID, "modelview_matrix");
        }
        public Cube(Vector3 position, Vector3 rotation, Color4 color)
        {
            this.position = position;
            this.rotation = rotation;

            this.registerCube();
            this.color = color;
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
            set { isSelected = value; }
        }

        // Methods
        private void registerCube()
        {
            id = PickRegister.Instance.GetId(this);
        }
        public void Draw()
        {
            modelviewMatrix = Matrix4.CreateRotationX(rotation.X) *
                              Matrix4.CreateRotationY(rotation.Y) *
                              Matrix4.CreateRotationZ(rotation.Z) *
                              Matrix4.CreateTranslation(position);

            if (fbManager.CurrentState == RenderState.Render)
            {
                cubeShader.Bind();
                GL.UniformMatrix4(modelviewMatrixLocation, false, ref modelviewMatrix);
                if (this.IsSelected == true)
                    GL.Uniform4(surfaceColorLocation, selectedColor);
                else
                    GL.Uniform4(surfaceColorLocation, color);
            }
            else
                if (fbManager.CurrentState == RenderState.Select)
                {
                    selectionShader.Bind();
                    GL.UniformMatrix4(selectModelviewMatrixLocation, false, ref modelviewMatrix);
                    GL.Uniform4(selectColorLocation, new Color4((byte)id, (byte)(id >> 8), (byte)(id >> 16), (byte)(id >> 24)));
                }
            
            cubeMesh.Draw();
        }
    }


    public interface ISelectable
    {
        int Id { get; }
        bool IsSelected { get; }
    }
}
