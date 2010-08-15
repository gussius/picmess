using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagedClass
{
    class Mesh
    {
        private DateTime timeStamp = new DateTime();
        private static Dictionary<string, Mesh> meshRegister = new Dictionary<string, Mesh>();

        public static Mesh CreateMesh(string name)
        {
            if (meshRegister.ContainsKey(name))
            {
                return meshRegister[name];
            }
            return new Mesh(name);
        }

        private Mesh(string name)
        {
            timeStamp = DateTime.Now;
            meshRegister.Add(name, this);

            if (meshRegister.ContainsKey(name))
            {
                foreach (KeyValuePair<string, Mesh> member in meshRegister)
                    Console.Write(member.Key);
                Console.WriteLine();
            }
        }

        public DateTime TimeStamp
        {
            get { return timeStamp; }
        }

        public void Draw()
        {
            Console.WriteLine("Drawing mesh");
            Console.WriteLine(this.timeStamp.ToString());
        }
    }
    
    class Cube
    {
        private Mesh cubeMesh;

        public Cube()
        {
            cubeMesh = Mesh.CreateMesh("cube");
        }

        public void Draw()
        {
            cubeMesh.Draw();
        }
    }

    class Sphere
    {
        private Mesh sphereMesh;

        public Sphere()
        {
            sphereMesh = Mesh.CreateMesh("sphere");
        }

        public void Draw()
        {
            sphereMesh.Draw();
        }
    }

    

    class EntryPoint
    {
        public static void Main()
        {
            int pauseTime = 1050;

            Cube cube1 = new Cube();
            System.Threading.Thread.Sleep(pauseTime);

            Cube cube2 = new Cube();
            System.Threading.Thread.Sleep(pauseTime);

            Sphere sphere1 = new Sphere();
            System.Threading.Thread.Sleep(pauseTime);

            Cube cube3 = new Cube();
            System.Threading.Thread.Sleep(pauseTime);

            Sphere sphere2 = new Sphere();
            System.Threading.Thread.Sleep(pauseTime);

            Cube cube4 = new Cube();


            cube1.Draw();
            cube2.Draw();
            sphere1.Draw();
            cube3.Draw();
            sphere2.Draw();
            cube4.Draw();
                        
            Console.ReadKey();
        }
    }
}
