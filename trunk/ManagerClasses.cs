using System;
using System.IO;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;

namespace LearnShader
{
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


    public sealed class ApplicationState
    {
        private static ApplicationState instance;
        RenderState currentState;
        FrameBufferManager frameBufferManager;

        private ApplicationState()
        {
            frameBufferManager = FrameBufferManager.Instance;
            currentState = RenderState.Render; //Default state
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


}
