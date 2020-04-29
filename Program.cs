using System;

namespace IsingModel
{
    class Program
    {
        static void Main(string[] args)
        {
            var geomGraph = Utililties.CreateLatticeGeometry(3, 7, 5);

            foreach(var point in geomGraph.GeometryPoints)
            {
                Console.WriteLine(point.ToString());
            }
        }
    }
}
