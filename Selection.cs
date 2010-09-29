using System;
using System.IO;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;

namespace LearnShader
{
    public class Selection
    {
        // Structs
        #region SelectionInfo Structure
        public struct SelectionInfo // TODO: Rename Struct
        {
            // Private Fields
            private ISelectable selected;
            private Vector4 surfaceCoordinate;
            private Vector3 selectionOffset;

            // Constructors
            public SelectionInfo(ISelectable selected, Vector4 surfaceCoordinate, Vector3 selectionOffset)
            {
                this.selected = selected;
                this.surfaceCoordinate = surfaceCoordinate;
                this.selectionOffset = selectionOffset;
            }

            // Properties
            public ISelectable Selected { get { return selected; } set { selected = value; } }
            public Vector4 SurfaceCoordinate { get { return surfaceCoordinate; } set { surfaceCoordinate = value; } }
            public Vector3 SelectionOffset { get { return selectionOffset; } set { selectionOffset = value; } }
        }
        #endregion

        // Fields
        private SelectionInfo info;
        FrameBufferManager fbManager;
        FullScreenQuad console;
        private bool selectDrag = false;
        private Vector3 initialPosition;
        
        // Constructors
        public Selection()
        {
            fbManager = new FrameBufferManager();
            console = FullScreenQuad.Console;
            info.Selected = null;
        }

        // Properties
        public SelectionInfo Info { get { return info; } }

        // Methods
        public void Pick(Scene scene, int x, int y)
        {
            scene.DrawScene(RenderState.Select);
            fbManager.ReadFBO(RenderState.Select);
            int[] viewport = new int[4];
            GL.GetInteger(GetPName.Viewport, viewport);
            int height = viewport[3];
            Byte4 pixel = new Byte4();
            GL.ReadPixels(x, height - y, 1, 1, PixelFormat.Rgba, PixelType.UnsignedByte, ref pixel);

            // Unselect previously selected object
            if (info.Selected != null)
                info.Selected.IsSelected = false;

            // Get newly selected object reference
            ISelectable newPick = PickRegister.Instance.LookupSelectable((int)pixel.ToUInt32());

            if (newPick != null)
            {
                info.Selected = newPick;
                info.SurfaceCoordinate = unProject(x, y);
                info.Selected.IsSelected = true;
                console.AddText("Cube Id #" + info.Selected.Id + " selected");
                info.SelectionOffset = info.Selected.Position - info.SurfaceCoordinate.Xyz;
                EnableDrag();
            }
        }
        public void DragTo(int screenX, int screenY)
        {
            // If we are in draging mode and if there is something selected then
            // unproject the mouse position, define a new position for the object
            // and update it's position.
            if (selectDrag == true)
            {
                if (info.Selected != null)
                {
                    Vector3 newPosition = unProject(screenX, screenY, info.SurfaceCoordinate.W);
                    newPosition = newPosition + info.SelectionOffset;
                    newPosition.Z = info.Selected.Position.Z;
                    info.Selected.Position = newPosition;
                }
            }

        }
        public void DisableDrag()
        {
            selectDrag = false;
        }
        public void EnableDrag()
        {
            selectDrag = true;
        }
        private Vector4 unProject(int screenX, int screenY)
        {
            int[] viewport = new int[4];
            GL.GetInteger(GetPName.Viewport, viewport);
            int width = viewport[2];
            int height = viewport[3];
            fbManager.ReadFBO(RenderState.Select);
            screenY = height - screenY;
            float zDepth = new float();
            GL.ReadPixels((int)screenX, (int)screenY, 1, 1, PixelFormat.DepthComponent, PixelType.Float, ref zDepth);
            float xDeviceCoord = screenX / (float)width * 2.0f - 1.0f;
            float yDeviceCoord = screenY / (float)height * 2.0f - 1.0f;
            float zDeviceCoord = zDepth * 2.0f - 1.0f;
            Matrix4 projectionMatrix = Shader.ProjectionMatrix;
            Vector4 deviceCoorinates = new Vector4(xDeviceCoord, yDeviceCoord, zDeviceCoord, 1.0f);
            Vector4 worldCoordinates = Vector4.Transform(deviceCoorinates, Matrix4.Invert(projectionMatrix));
            worldCoordinates = worldCoordinates / worldCoordinates.W;
            Vector4 outCoordinate = new Vector4(worldCoordinates.X, worldCoordinates.Y, worldCoordinates.Z, zDeviceCoord);

            return outCoordinate;
        }
        private Vector3 unProject(int screenX, int screenY, float zDeviceCoord)
        {
            int[] viewport = new int[4];
            GL.GetInteger(GetPName.Viewport, viewport);
            int width = viewport[2];
            int height = viewport[3];
            screenY = height - screenY;
            float xDeviceCoord = screenX / (float)width * 2.0f - 1.0f;
            float yDeviceCoord = screenY / (float)height * 2.0f - 1.0f;
            Vector4 deviceCoorinates = new Vector4(xDeviceCoord, yDeviceCoord, zDeviceCoord, 1.0f);
            Vector4 worldCoordinates = Vector4.Transform(deviceCoorinates, Matrix4.Invert(Shader.ProjectionMatrix));
            worldCoordinates = worldCoordinates / worldCoordinates.W;
            Vector3 outCoordinate = new Vector3(worldCoordinates.X, worldCoordinates.Y, worldCoordinates.Z);

            return outCoordinate;
        }
    }
}
