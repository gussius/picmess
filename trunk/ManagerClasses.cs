using System;
using System.IO;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;

namespace LearnShader
{
    public class ApplicationState
    {
        private static ApplicationState instance;
        RenderState currentState;
        FrameBufferManager frameBufferManager;

        public event EventHandler<ApplicationStateEventArgs> ApplicationStateEvent;

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
            EventHandler<ApplicationStateEventArgs> localHandler = ApplicationStateEvent;

            if (localHandler != null)
            {
                if (state == RenderState.Select)
                {
                    localHandler(this, new ApplicationStateEventArgs(RenderState.Select));
                }
                if (state == RenderState.Render)
                {
                    localHandler(this, new ApplicationStateEventArgs(RenderState.Render));
                }
            }
        }
    }

    public class ApplicationStateEventArgs : EventArgs
    {
        RenderState state;

        public ApplicationStateEventArgs(RenderState state)
        {
            this.state = state;
        }

        public RenderState State
        {
            get { return state; }
        }
    }

    public class FrameBufferManager
    {
        private static FrameBufferManager instance;
        private uint selectionBuffer;
        private uint[] renderBuffer;
        private int viewportWidth, viewportHeight;

        // Private Instance Constructor
        private FrameBufferManager()
        {
            Console.WriteLine("FrameBufferManager() constructor executed");
        }
        static FrameBufferManager()
        {
            instance = new FrameBufferManager();

            //ApplicationState.Instance.ApplicationStateEvent += new EventHandler<ApplicationStateEventArgs>(instance.Instance_ApplicationStateEvent);

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

            GL.Enable(EnableCap.DepthTest);

            // Set back to default FrameBuffer
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
        }
        /*
        private void Instance_ApplicationStateEvent(object sender, ApplicationStateEventArgs e)
        {
            if (e.State == RenderState.Select)
                GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, instance.selectionBuffer);
            
            if (e.State == RenderState.Render)
                GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
        }
         * 
         */

        public static FrameBufferManager Instance
        {
            get { return instance; }
        }
        public uint ColorBuffer
        {
            get { return renderBuffer[(uint)RenderBuffer.Color]; }
        }
        public uint DepthBuffer
        {
            get { return renderBuffer[(uint)RenderBuffer.Depth]; }
        }
        public uint SelectionBuffer
        {
            get { return selectionBuffer; }
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
