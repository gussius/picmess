using System;
using System.IO;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;

namespace LearnShader
{
    public sealed class ApplicationState
    {
        private static ApplicationState instance;
        RenderState currentState;
        FrameBufferManager frameBufferManager;

        // Private Instance Constructor
        private ApplicationState()
        {
            Console.WriteLine("ApplicationState() constructor executed");
        }

        static ApplicationState()
        {
            instance = new ApplicationState();
            instance.frameBufferManager = FrameBufferManager.Instance;
            instance.currentState = RenderState.Render;
        }

        public static ApplicationState Instance
        {
            get { return instance; }
        }

        RenderState CurrentState
        {
            get { return currentState; }
        }

        public void SetRenderState(RenderState state)
        {
            currentState = state;
            // Manipulate the FrameBufferManager to change system RenderBuffers.
        }
    }


    public class FrameBufferManager
    {
        private static FrameBufferManager instance;
        uint frameBuffer;
        uint[] renderBuffer;
        int viewportWidth, viewportHeight;

        // Private Instance Constructor
        private FrameBufferManager()
        {
            Console.WriteLine("FrameBufferManager() constructor executed");
        }

        static FrameBufferManager()
        {
            instance = new FrameBufferManager();

            // This is where we initialise the GL FrameBuffers and RenderBuffers.
            instance.renderBuffer = new uint[(int)RenderBuffer.NumRenderBuffers];
            GL.GenRenderbuffers((int)RenderBuffer.NumRenderBuffers, instance.renderBuffer);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, instance.renderBuffer[(int)RenderBuffer.Color]);
            
            int[] viewport = new int[4];
            GL.GetInteger(GetPName.Viewport, viewport);
            instance.viewportWidth = viewport[2];
            instance.viewportHeight = viewport[3];
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer,
                                   RenderbufferStorage.Rgba8,
                                   instance.viewportWidth,
                                   instance.viewportHeight);

            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, instance.renderBuffer[(int)RenderBuffer.Depth]);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer,
                                   RenderbufferStorage.DepthComponent24,
                                   instance.viewportWidth,
                                   instance.viewportHeight);

            GL.GenFramebuffers(1, out instance.frameBuffer);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, instance.frameBuffer);
            GL.FramebufferRenderbuffer(FramebufferTarget.DrawFramebuffer,
                                       FramebufferAttachment.ColorAttachment0,
                                       RenderbufferTarget.Renderbuffer,
                                       instance.renderBuffer[(int)RenderBuffer.Color]);
            GL.FramebufferRenderbuffer(FramebufferTarget.DrawFramebuffer,
                                       FramebufferAttachment.DepthAttachment,
                                       RenderbufferTarget.Renderbuffer,
                                       instance.renderBuffer[(int)RenderBuffer.Depth]);

            GL.Enable(EnableCap.DepthTest);

            // Set back to default FrameBuffer
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
        }
        
        public static FrameBufferManager Instance
        {
            get { return instance; }
        }

        uint ColorBuffer
        {
            get { return renderBuffer[(uint)RenderBuffer.Color]; }
        }

        uint DepthBuffer
        {
            get { return renderBuffer[(uint)RenderBuffer.Depth]; }
        }
    }


    public sealed class PickRegister
    {
        private static readonly PickRegister instance = new PickRegister();
        private Dictionary<int, ISelectable> register = new Dictionary<int, ISelectable>();
        private static int nextId = 8000000;

        public int GetId(ISelectable selectableRef)
        {
            nextId = nextId + 100000;
            register.Add(nextId, selectableRef);

            return nextId;
        }

        public void Remove(int key)
        {
            register.Remove(key);
        }

        public ISelectable LookupSelectable(int id)
        {
            if (register.ContainsKey(id))
                return register[id];
            return null;
        }

        private PickRegister()
        {
        }

        public static PickRegister Instance
        {
            get { return instance; }
        }
    }


    public enum RenderState
    {
        Select,
        Render
    }


    public enum RenderBuffer
    {
        Color,
        Depth,
        NumRenderBuffers
    }

}
