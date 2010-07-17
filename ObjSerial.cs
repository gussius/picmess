using System;
using OpenTK;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using LearnShader;

namespace LearnShader
{
    public class ObjSerial
    {/*
        public static void Save(Vertex[] vertArray, int[] intArray)
        {
            using (Stream stream = File.Open("Verts.vert", FileMode.Create))
            {
                BinaryFormatter bformatter = new BinaryFormatter();

                Console.WriteLine("Writing Employee Information");
                bformatter.Serialize(stream, vertArray);
                bformatter.Serialize(stream, intArray);
                stream.Close();
            }
        }

        public static void Load()
        {
            Vertex[] verts;
            int[] ints;

            using (Stream stream = File.Open("Verts.vert", FileMode.Open))
            {
                BinaryFormatter bformatter = new BinaryFormatter();

                Console.WriteLine("Reading Employee Information");
                verts = (Vertex[])bformatter.Deserialize(stream);
                ints = (int[])bformatter.Deserialize(stream);

                stream.Close();
            }
        }
      */
    }
}