using System;
using System.Collections.Generic;
using System.Text;

namespace IsingModel
{
    class Lattice_Graph
    {
        private List<List<int>> Lattice;           //each element contains a list of neighboring element indices
        private List<int> RingPopulation;         //population of each successive ring or corona around the central node
        private List<int> RingMap;                //map that takes in a site index and returns its ring number

        public int P, Q, R;                                 //parameters of Lattice, P is the number of sides to a polygon, Q is the number of polygons
                                                            //that meet at a point, R is the number of rings
        public int Size { get; }                            //total number of Lattice sites

        public Lattice_Graph(int p, int q, int r_max, bool is_periodic = false)
        {
            Lattice = new List<List<int>>(4000);
            RingPopulation = new List<int>();
            RingMap = new List<int>();

            P = p;
            Q = q;
            R = r_max;

            PopulateLattice();

            //periodic boundary condition can only apply to Euclidean Lattices for now
            if (is_periodic && (P - 2) * (Q - 2) == 4)
            {
                ApplyPeriodicBoundary();
            }

            Size = 0;
            for (int i = 0; i < Lattice.Count; i++)
                if (Lattice[i].Count > 0)
                    Size++;
        }

        public int GetCurvatureSign()
        {
            int defect = (P - 2) * (Q - 2);

            if (defect < 4)
                return 1;
            else if (defect > 4)
                return -1;
            else
                return 0;
        }

        //Add neighbor to Lattice site but repeated pairs aren't allowed
        private void AddNeighborUnique(int site, int neighbor)
        {
            if (!HasNeighbor(site, neighbor))
            {
                Lattice[site].Add(neighbor);
                return;
            }
        }

        private void RemoveNeighbor(int site, int neighbor)
        {
            for (int i = 0; i < Lattice[site].Count; i++)
            {
                if (Lattice[site][i] == neighbor)
                {
                    Lattice[site].RemoveAt(i);
                    return;
                }
            }
        }

        //Remove all neighbors of a site
        private void Isolate(int site)
        {
            for (int i = 0; i < Lattice[site].Count; i++)
            {
                RemoveNeighbor(Lattice[site][i], site);
            }

            Lattice[site].Clear();
        }

        //Return true if Lattice element at site contains neighbor
        private bool HasNeighbor(int site, int neighbor)
        {
            return Lattice[site].Contains(neighbor);
        }

        //Take two sides of a polygon and add the neighbors that they share to each other
        private void Connect(List<int> sideA, List<int> sideB)
        {
            if (sideA.Count != sideB.Count)
                return;

            sideA.Reverse();

            int side_length = sideA.Count;

            for (int i = 0; i < side_length; i++)
            {
                int a = sideA[i];
                int b = sideB[i];

                List<int> neighbor_a = Lattice[a];
                List<int> neighbor_b = Lattice[b];

                for (int j = 0; j < neighbor_b.Count; j++)
                {
                    AddNeighborUnique(a, neighbor_b[j]);
                    AddNeighborUnique(neighbor_b[j], a);
                }

                for (int j = 0; j < neighbor_a.Count; j++)
                {
                    AddNeighborUnique(b, neighbor_a[j]);
                    AddNeighborUnique(neighbor_a[j], b);
                }
            }

            sideA.Reverse();
        }

        //In a completely generated Lattice each site must have Q neighbors 
        public bool IsComplete()
        {
            for (int i = 0; i < Lattice.Count; i++)
            {
                if (Lattice[i].Count != Q && Lattice[i].Count != 0)
                    return false;
            }

            return true;
        }

        private void ApplyPeriodicBoundary()
        {
            //periodic boundary conditions aren't ready for hyperbolic Lattices
            if ((Q - 2) * (P - 2) != 4)
                return;

            int S;

            if (P == 4 && Q == 4)
                S = 4;
            else
                S = 6;

            int N = RingPopulation[RingPopulation.Count - 1];

            int L = (N / S) + 1;

            List<List<int>> sides = new List<List<int>>();

            List<int> perim = new List<int>();

            for (int i = Lattice.Count - 1; i >= (Lattice.Count - N); i--)
                perim.Add(i);

            for (int i = 0; i < N; i += (L - 1))
            {
                List<int> side = new List<int>();
                for (int j = 0; j < L; j++)
                    side.Add(perim[(i + j) % N]);

                sides.Add(side);
            }

            for (int i = 0; i < S; i++)
                Connect(sides[i], sides[(i + S / 2) % S]);

            for (int i = 0; (i < perim.Count) && !IsComplete(); i++)
                Isolate(perim[i]);
        }

        public List<List<int>> GetVertices() { return Lattice; }
        public List<int> GetRingPopulation() { return RingPopulation; }
        public List<int> GetRingMap() { return RingMap; }

        //Generate the Lattice
        private void PopulateLattice()
        {
            PopulateZerothRing();

            RingPopulation.Add(1);

            if (R == 0)
            {
                return;
            }

            PopulateFirstRing();

            int current_ring_start = 1;
            int current_ring_end;

            //Go ring by ring and generate new Lattice sites, connecting the next to the current and current to the previous
            for (int r = 1; r < R; r++)
            {
                current_ring_end = Lattice.Count;

                //Go through each site in the current ring and generate a polygon splitting off from it,
                //these new sites will become the next ring
                for (int site = current_ring_start; site < current_ring_end; site++)
                {
                    ExtendCorner(Lattice[site][0], site, Lattice[site][1]);
                }

                RingPopulation.Add(current_ring_end - current_ring_start);

                current_ring_start = current_ring_end;
            }

            RingPopulation.Add(Lattice.Count - current_ring_start);

            int sum_so_far = 1;

            //Going around the ring creates an error when it gets back to the original site of the ring
            for (int i = 1; i < RingPopulation.Count; i++)
            {
                CorrectJoint(sum_so_far, sum_so_far + RingPopulation[i] - 1);
                sum_so_far += RingPopulation[i];
            }

            //Fill in the ring map
            for (int i = 0; i < RingPopulation.Count; i++)
            {
                for (int j = 0; j < RingPopulation[i]; j++)
                {
                    RingMap.Add(i);
                }
            }
        }

        //In a few known places the neighbor lists are in the incorrect order, which creates problems
        //when it comes to rendering them graphically
        private void CorrectJoint(int start, int end)
        {
            int index_a = 0, index_b = 0;
            int temp = 0;

            for (int i = 0; i < Lattice[start].Count; i++)
            {
                if (Lattice[start][i] == (start + 1))
                    index_a = i;
                if (Lattice[start][i] == end)
                    index_b = i;
            }

            temp = Lattice[start][index_a];
            Lattice[start][index_a] = Lattice[start][index_b];
            Lattice[start][index_b] = temp;

            for (int i = 0; i < Lattice[end].Count; i++)
            {
                if (Lattice[end][i] == (end - 1))
                    index_a = i;
                if (Lattice[end][i] == start)
                    index_b = i;
            }

            temp = Lattice[end][index_a];
            Lattice[end][index_a] = Lattice[end][index_b];
            Lattice[end][index_b] = temp;

            for (int i = 0; i < Lattice[start - 1].Count; i++)
            {
                if (Lattice[start - 1][i] == start)
                {
                    index_a = i;
                    break;
                }
            }

            temp = Lattice[start - 1][index_a];

            for (int i = index_a; i > 0; i--)
                Lattice[start - 1][i] = Lattice[start - 1][i - 1];

            Lattice[start - 1][0] = temp;
        }

        private void PopulateZerothRing()
        {
            Lattice.Add(new List<int>());
        }

        private void PopulateFirstRing()
        {
            int num_new_vertices = Q * (P - 2); // Number of vertices to be added

            // Create enough new vertices for the first ring
            for (int i = 0; i < num_new_vertices; i++)
            {
                Lattice.Add(new List<int>());
            }

            // Connect each new site to the one in front of it, looping around back to itself
            for (int i = 0; i < num_new_vertices; i++)
                Connect(i + 1, (i + 1) % num_new_vertices + 1);

            // Connect all the branch vertices to the origin
            for (int i = 1; i <= num_new_vertices; i += (P - 2))
                Connect(0, i);
        }

        private void ExtendCorner(int left, int current, int right)
        {
            int spanning = P - 3;                        // How many vertices lie between branches
            int branches = Q - Lattice[current].Count;   // How many branch vertices there are (how many more neighbors needed)
            int num_new_vertices = branches + (branches + 1) * spanning; // Number of new vertices

            // If current's highest neighbors are on the same ring as current,
            // This spot has been overtaken by a previous polygon, so do nothing
            if ((left - 2) == right)
            {
                return;
            }

            // Continue around the interior of the current polygon
            // until a receptive site is found
            if (branches == 0)
            {
                while (Lattice[right].Count == Q)
                {
                    num_new_vertices--;
                    right = Lattice[right][0];
                }
            }

            // Indices describing where the next ring portion goes in the Lattice list
            int next_ring_start = Lattice.Count;
            int next_ring_end = Lattice.Count + num_new_vertices - 1;

            // Create enough vertices to contain the next ring portion
            for (int i = 0; i < num_new_vertices; i++)
                Lattice.Add(new List<int>());

            // Connect each new site the one in front of it
            for (int i = 0; i < (num_new_vertices - 1); i++)
                Connect(next_ring_start + i, next_ring_start + i + 1);

            // Connect each branch site to the current site
            for (int i = spanning; i < num_new_vertices; i += (spanning + 1))
                Connect(current, next_ring_start + i);

            // Connect the dangling ends of the ring portion to the current site's bounding neighbors
            Connect(left, next_ring_start);
            Connect(right, next_ring_end);
        }

        private void AddNeighbor(List<int> vertex, int neighbor)
        {
            int index;

            // Find an index such that Si > neighbor > Si+1, that way the neighbor list
            // will remain sorted from largest to smallest
            for (index = 0; (index < vertex.Count) && vertex[index] > neighbor; index++) ;

            vertex.Insert(index, neighbor);
        }

        private void Connect(int i, int j)
        {
            AddNeighbor(Lattice[i], j);
            AddNeighbor(Lattice[j], i);
        }
    }
}
