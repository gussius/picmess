using System;
using OpenTK;

namespace LearnShader
{
    struct Vertex
    {
        private Vector3 position;
        private Vector3 normal;

        public Vertex()
        {
            this.position = new Vector3(0, 0, 0);
            this.normal = new Vector3(0, 0, 0);
        }

        public Vertex(Vector3 position, Vector3 normal)
        {
            this.position = position;
            this.normal = normal;
        }

        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        public Vector3 Normal
        {
            get { return normal; }
            set { normal = value; }
        }
    }


    class VNPair
    {
        private int position;
        private int normal;

        public VNPair(int pos_ref, int norm_ref)
        {
            this.position = pos_ref;
            this.normal = norm_ref;
        }

        public VNPair()
        {
            this.position = 0;
            this.normal = 0;
        }

        public static bool operator ==(VNPair pair1, VNPair pair2)
        {
            if (((object)pair1 == null) && ((object)pair2 == null))
                return true;
            if (((object)pair1 != null) && ((object)pair2 == null))
                return false;
            if (((object)pair1 == null) && ((object)pair2 != null))
                return false;

            if ((pair1.P == pair2.P) && (pair1.N == pair2.N))
            {
                return true;
            }
            return false;
        }

        public static bool operator !=(VNPair pair1, VNPair pair2)
        {
            if ((pair1.P != pair2.P) || (pair1.N != pair2.N))
            {
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return "{" + this.P + ", " + this.N + "}";
        }

        public int P
        {
            get { return position; }
            set { position = value; }
        }

        public int N
        {
            get { return normal; }
            set { normal = value; }
        }
    }
}
