using System;
using System.Collections.Generic;
using System.Text;

namespace IsingModel
{
    class LatticeGeometry
    {
        private Lattice_Graph lattice;

        private List<bool> valid;
        private List<Vector2D> geometryPoints;

        private Vector2D guide_vector;

        public LatticeGeometry()
        {
            valid = new List<bool>(4000);
            geometryPoints = new List<Vector2D>(4000);
        }

        public LatticeGeometry(Lattice_Graph lat, double spacing)
        {
            lattice = lat;
            guide_vector = new Vector2D(spacing, 0.0);
            valid = new List<bool>(4000);
            geometryPoints = new List<Vector2D>(4000);

            Compose();
        }

        public LatticeGeometry(int p, int q, int r, double spacing)
        {
            lattice = new Lattice_Graph(p, q, r, false);
            guide_vector = new Vector2D(spacing, 0.0);
            valid = new List<bool>(4000);
            geometryPoints = new List<Vector2D>(4000);

            Compose();
        }

        List<int> GetRingMap() { return lattice.GetRingMap(); }
        List<bool> GetValid() { return valid; }
        List<Vector2D> GetPoints() { return geometryPoints; }
        Lattice_Graph GetGraph() { return lattice; }
        int Size() { return lattice.Size; }

        public void PrintToScreen()
        {
            for (int i = 0; i < geometryPoints.Count; i++)
                Console.WriteLine(geometryPoints[i].ToString());
        }

        public void Compose()
        {
            // set up variables
            for (int i = 0; i < lattice.Size; i++)
            {
                valid.Add(false);                                // whether or not a position has been chosen yet
                geometryPoints.Add(new Vector2D(0.0, 0.0));      // position vector for each site
            }

            if (lattice.R == 0)
            {
                valid[0] = true;
                return;
            }

            int p = lattice.P;
            int q = lattice.Q;

            // The first two sites have a defined positions
            geometryPoints[1] = guide_vector;
            valid[0] = true;
            valid[1] = true;

            // Iterate through each site, using its position to determine the position
            // of its neighbors
            for (int i = 1; i < lattice.Size; i++)
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

            List<int> map = lattice.GetRingMap();

            int refr = map[index];
            int sub;

            List<List<int>> list = lattice.GetVertices();

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
            double dt = 2 * Math.PI / lattice.Q;     //angular separation between each neighbor

            List<int> rotation_map = new List<int>();

            determine_rotation_map(index, rotation_map);

            List<List<int>> vertex_list = lattice.GetVertices();

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
            double dir = ( (-geometryPoints[index]) + geometryPoints[rotation_map[stem]] ).Phase();

            // Orient the guide vector so that it faces the correct way
            Vector2D between = guide_vector.Rotate(dir);

            for (int i = 0; i < rotation_map.Count; i++)
            {
                int k = rotation_map[i];

                // if k isn't already defined
                if (!valid[k])
                {
                    // whether between is rotated CW or CWW depends on the ordering of the rotation_map
                    geometryPoints[k] = geometryPoints[index] + between.Rotate((i - stem) * dt);
                    valid[k] = true;
                }
            }
        }
    }
}
