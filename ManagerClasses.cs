using System;
using System.IO;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;

namespace LearnShader
{
    public class FrameBufferManager
    {
        // Fields
        private static FrameBufferManager instance;
        private uint selectionBuffer;
        private uint[] renderBuffer;
        private int viewportWidth, viewportHeight;
        private RenderState currentState;

        // Constructors
        static FrameBufferManager()
        {
            // Initialise singleton instance
            instance = new FrameBufferManager();

            // This is where we initialise the GL FrameBuffers and RenderBuffers.
            instance.renderBuffer = new uint[(int)RenderBuffer.NumRenderBuffers];
            GL.GenRenderbuffers((int)RenderBuffer.NumRenderBuffers, instance.renderBuffer);

            int[] viewport = new int[4];
            GL.GetInteger(GetPName.Viewport, viewport);
            instance.viewportWidth = viewport[2];
            instance.viewportHeight = viewport[3];

            SetRenderbufferStorage(instance.viewportWidth, instance.viewportHeight);

            GL.GenFramebuffers(1, out instance.selectionBuffer);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, instance.selectionBuffer);
            GL.FramebufferRenderbuffer(FramebufferTarget.DrawFramebuffer,
                                       FramebufferAttachment.ColorAttachment0,
                                       RenderbufferTarget.Renderbuffer,
                                       instance.renderBuffer[(int)RenderBuffer.Color]);
            GL.FramebufferRenderbuffer(FramebufferTarget.DrawFramebuffer,
                                       FramebufferAttachment.DepthAttachment,
                                       RenderbufferTarget.Renderbuffer,
                                       instance.renderBuffer[(int)RenderBuffer.Depth]);

            // Set back to default FrameBuffer
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
        }        

        // Properties
        public static FrameBufferManager Instance
        {
            get { return instance; }
        }
        public RenderState CurrentState
        {
            get { return currentState; }
        }

        // Methods
        private static void SetRenderbufferStorage(int width, int height)
        {
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, instance.renderBuffer[(int)RenderBuffer.Color]);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer,
                                   RenderbufferStorage.Rgba8,
                                   width,
                                   height);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, instance.renderBuffer[(int)RenderBuffer.Depth]);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer,
                                   RenderbufferStorage.DepthComponent24,
                                   width,
                                   height);
        }
        public void UpdateSelectionViewport(int width, int height)
        {
            viewportWidth = width;
            viewportHeight = height;
            SetRenderbufferStorage(width, height);
        }
        public void BindFBO(RenderState buffer)
        {
            if (buffer == RenderState.Select)
            {
                currentState = RenderState.Select;
                GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, instance.selectionBuffer);
            }
            if (buffer == RenderState.Render)
            {
                currentState = RenderState.Render;
                GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
            }
        }
        public void ReadFBO(RenderState buffer)
        {
            if (buffer == RenderState.Select)
                GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, instance.selectionBuffer);
            if (buffer == RenderState.Render)
                GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
        }
    }
    

    public sealed class PickRegister
    {
        // Fields
        private static readonly PickRegister instance = new PickRegister();
        private Dictionary<int, ISelectable> register = new Dictionary<int, ISelectable>();
        private static int nextId = 350; //start at 350 so id colors are visible.

        // Constructors
        private PickRegister()
        {
        }

        // Properties
        public static PickRegister Instance
        {
            get { return instance; }
        }

        // Methods
        public int GetId(ISelectable selectableRef)
        {
            nextId = nextId + 10; // Add 10 each time so that id colors look different.
            register.Add(nextId, selectableRef);
            return nextId;
        }
        public ISelectable LookupSelectable(int id)
        {
            if (register.ContainsKey(id))
                return register[id];
            return null;
        }
        public void Remove(int key)
        {
            register.Remove(key);
        }        
    }


    public enum RenderState
    {
        Render,
        Select
    }
    public enum RenderBuffer
    {
        Color,
        Depth,
        NumRenderBuffers
    }

}
