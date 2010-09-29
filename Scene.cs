using System;
using System.IO;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;

namespace LearnShader
{
    public class Scene
    {
        // Fields
        private FrameBufferManager fbManager;
        private List<ISelectable> actorList;

        // Constructors
        public Scene()
        {
            fbManager = FrameBufferManager.Instance;
            actorList = new List<ISelectable>();
        }

        // Properties
        public List<ISelectable> ActorList { get { return actorList; } }

        // Methods
        public void AddActor(ISelectable actor)
        {
            actorList.Add(actor);
        }
        public void DrawScene(RenderState state)
        {
            fbManager.BindFBO(state);
            if (state == RenderState.Render)
                GL.ClearColor(0.2f, 0.2f, 0.2f, 1);
            else
                if (state == RenderState.Select)
                    GL.ClearColor(0.0f, 0.0f, 0.0f, 0);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            foreach (ISelectable sample in actorList)
            {
                sample.Draw();
            }
        }
    }
}
