using System;
using System.Collections.Generic;
using System.Text;

namespace IsingModel
{
    class Lattice_Geometry
    {
        public Lattice_Graph Lattice;
        public List<Vector2D> GeometryPoints;
        public double LengthScale;

        private List<bool> valid;

        private Vector2D GuideVector;

        public Lattice_Geometry()
        {
            valid = new List<bool>(4000);
            GeometryPoints = new List<Vector2D>(4000);
        }

        public Lattice_Geometry(Lattice_Graph lat, double spacing)
        {
            Lattice = lat;

            if(lat.GetCurvatureSign() == 0)
                GuideVector = new Euclvector2D(spacing, 0.0);
            else if(lat.GetCurvatureSign() == -1)
                GuideVector = new Gyrovector2D(spacing, 0.0);

            valid = new List<bool>(4000);
            GeometryPoints = new List<Vector2D>(4000);
            LengthScale = spacing;

            Compose();
        }

        public Lattice_Geometry(int p, int q, int r, double spacing)
        {
            Lattice = new Lattice_Graph(p, q, r, false);

            if (Lattice.GetCurvatureSign() == 0)
                GuideVector = new Euclvector2D(spacing, 0.0);
            else if (Lattice.GetCurvatureSign() == -1)
                GuideVector = new Gyrovector2D(spacing, 0.0);

            valid = new List<bool>(4000);
            GeometryPoints = new List<Vector2D>(4000);
            LengthScale = spacing;

            Compose();
        }

        public int Size() { return Lattice.Size; }

        public void PrintToScreen()
        {
            for (int i = 0; i < GeometryPoints.Count; i++)
                Console.WriteLine(GeometryPoints[i].ToString());
        }

        public void Compose()
        {
            // set up variables
            for (int i = 0; i < Lattice.Size; i++)
            {
                valid.Add(false);   // whether or not a position has been chosen yet

                if (Lattice.GetCurvatureSign() == 0)
                    GeometryPoints.Add(new Euclvector2D(0.0, 0.0));
                else if (Lattice.GetCurvatureSign() == -1)
                    GeometryPoints.Add(new Gyrovector2D(0.0, 0.0));
            }

            if (Lattice.R == 0)
            {
                valid[0] = true;
                return;
            }

            int p = Lattice.P;
            int q = Lattice.Q;

            // The first two sites have a defined positions
            GeometryPoints[1] = GuideVector;
            valid[0] = true;
            valid[1] = true;

            // Iterate through each site, using its position to determine the position
            // of its neighbors
            for (int i = 1; i < Lattice.Size; i++)
            {
                Connect(i);
            }
        }

        // Every site's neighbors can be divided into at most three groups,
        // the ring below (R0), the current ring (R1), and the ring above (R2)
        public void SplitIntoRingComponents(int index, out List<int> R0, out List<int> R1, out List<int> R2)
        {
            R0 = new List<int>();
            R1 = new List<int>();
            R2 = new List<int>();

            List<int> map = Lattice.GetRingMap();

            int refr = map[index];
            int sub;

            List<List<int>> list = Lattice.GetVertices();

            for (int i = 0; i < list[index].Count; i++)
            {
                sub = map[ list[index][i] ];

                if (sub > refr)
                    R2.Add(list[index][i]);
                else if (sub == refr)
                    R1.Add(list[index][i]);
                else
                    R0.Add(list[index][i]);
            }
        }

        // Rearrange the neighbors so that a single angle separates them, going in a CCW direction

        public void determine_rotation_map(int index, List<int> rot_map)
        {
            rot_map.Clear();

            SplitIntoRingComponents(index, out List<int> R0, out List<int> R1, out List<int> R2);

            // Start with the most CCW neighbor on the incident ring
            rot_map.Add(R1[1]);

            // Because it is CCW and the neighbors decrease CW, the neighbors
            // from the ring below are added in reverse order
            for (int i = R0.Count - 1; i >= 0; i--)
                rot_map.Add(R0[i]);

            // Once again reaching the incident ring, add the remaining neighbor
            rot_map.Add(R1[0]);

            // Add the neighbors from the ring above, they decrease CCW so no reversal is needed
            for (int i = 0; i < R2.Count; i++)
                rot_map.Add(R2[i]);
        }

        // Use the position of a site and one of its defined neighbors
        // to calculate the positions of the rest of them
        public void Connect(int index)
        {
            double dt = 2 * Math.PI / Lattice.Q;     //angular separation between each neighbor

            List<int> rotation_map = new List<int>();

            determine_rotation_map(index, rotation_map);

            List<List<int>> vertex_list = Lattice.GetVertices();

            List<int> v = vertex_list[index];      // neighbor list of the site at index

            int stem = 0;           // index of a neighbor with a defined position
            int total = 0;          // number of defined neighbors

            // Search through all of index's neighbors to find at least one
            // that has been defined
            for (int i = 0; i < rotation_map.Count; i++)
            {
                if (valid[rotation_map[i]])
                {
                    stem = i;
                    total++;
                }
            }

            // if all or none of the neighbors are defined, there is nothing to do
            if (total == rotation_map.Count || total == 0)
                return;

            // direction of the vector going from index to the stem
            double dir = ( (-GeometryPoints[index]) + GeometryPoints[rotation_map[stem]] ).Phase();

            // Orient the guide vector so that it faces the correct way
            var between = GuideVector.Rotate(dir);

            for (int i = 0; i < rotation_map.Count; i++)
            {
                int k = rotation_map[i];

                // if k isn't already defined
                if (!valid[k])
                {
                    // whether between is rotated CW or CWW depends on the ordering of the rotation_map
                    GeometryPoints[k] = GeometryPoints[index] + between.Rotate((i - stem) * dt);
                    valid[k] = true;
                }
            }
        }
    }
}
