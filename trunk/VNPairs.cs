using System;

namespace LearnShader
{
    struct VNPair
    {
        private int position;
        private int normal;

        public VNPair(int pos_ref, int norm_ref)
        {
            this.position = pos_ref;
            this.normal = norm_ref;
        }

        public static bool operator ==(VNPair pair1, VNPair pair2)
        {
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
