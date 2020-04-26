using System;
using System.Collections.Generic;
using System.Text;

namespace IsingModel
{
    class Lattice_Graph
    {
        private List<List<int>> lattice;           //each element contains a list of neighboring element indices
        private List<int> ring_population;         //populaiton of each successive ring or corona around the central node
        private List<int> ring_map;                //map that takes in a site index and returns its ring number

        //private List<Distance_Handler> handler_list;   //used in the Ising model to determine distance

        public int P, Q, R;                                 //parameters of lattice, P is the number of sides to a polygon, Q is the number of polygons
                                                            //that meet at a point, R is the number of rings
        public int Size { get; }                            //total number of lattice sites

        public Lattice_Graph(int p, int q, int r_max, bool is_periodic = false)
        {
            lattice = new List<List<int>>(4000);
            ring_population = new List<int>();
            ring_map = new List<int>();

            P = p;
            Q = q;
            R = r_max;

            populate_lattice();

            //periodic boundary condition can only apply to Euclidean lattices for now
            if (is_periodic && (P - 2) * (Q - 2) == 4)
            {
                apply_periodic_boundary();
            }

            Size = 0;
            for (int i = 0; i < lattice.Count; i++)
                if (lattice[i].Count > 0)
                    Size++;
        }

        private List<int> reverse(List<int> list)
        {
            List<int> replace = new List<int>();
            for (int i = list.Count - 1; i >= 0; i--)
                replace.Add(list[i]);

            return replace;
        }

        //Add neighbor to lattice site but repeated pairs aren't allowed
        private void add_neighbor_with_enforced_uniqueness(int site, int neighbor)
        {
            if (!has_neighbor(site, neighbor))
            {
                lattice[site].Add(neighbor);
                return;
            }
        }


        private void remove_neighbor(int site, int neighbor)
        {
            for (int i = 0; i < lattice[site].Count; i++)
            {
                if (lattice[site][i] == neighbor)
                {
                    lattice[site].RemoveAt(i);
                    return;
                }
            }
        }

        //Remove all neighbors of a site
        private void isolate(int site)
        {
            for (int i = 0; i < lattice[site].Count; i++)
            {
                remove_neighbor(lattice[site][i], site);
            }

            lattice[site].Clear();
        }

        //Return true if lattice element at site contains neighbor
        private bool has_neighbor(int site, int neighbor)
        {
            for (int i = 0; i < lattice[site].Count; i++)
                if (lattice[site][i] == neighbor)
                    return true;

            return false;
        }

        //Take two sides of a polygon and add the neighbors that they share to each other
        private void connect(List<int> sideA, List<int> sideB)
        {
            if (sideA.Count != sideB.Count)
                return;

            sideA = reverse(sideA);

            int side_length = sideA.Count;

            for (int i = 0; i < side_length; i++)
            {
                int a = sideA[i];
                int b = sideB[i];

                List<int> neighbor_a = lattice[a];
                List<int> neighbor_b = lattice[b];

                for (int j = 0; j < neighbor_b.Count; j++)
                {
                    add_neighbor_with_enforced_uniqueness(a, neighbor_b[j]);
                    add_neighbor_with_enforced_uniqueness(neighbor_b[j], a);
                }

                for (int j = 0; j < neighbor_a.Count; j++)
                {
                    add_neighbor_with_enforced_uniqueness(b, neighbor_a[j]);
                    add_neighbor_with_enforced_uniqueness(neighbor_a[j], b);
                }
            }

            sideA = reverse(sideA);
        }

        //In a completely generated lattice each site must have Q neighbors 
        public bool is_complete()
        {
            for (int i = 0; i < lattice.Count; i++)
            {
                if (lattice[i].Count != Q && lattice[i].Count != 0)
                    return false;
            }

            return true;
        }

        private void apply_periodic_boundary()
        {
            //periodic boundary conditions aren't ready for hyperbolic lattices
            if ((Q - 2) * (P - 2) != 4)
                return;

            int S;

            if (P == 4 && Q == 4)
                S = 4;
            else
                S = 6;

            int N = ring_population[ring_population.Count - 1];

            int L = (N / S) + 1;

            List<List<int>> sides = new List<List<int>>();

            List<int> perim = new List<int>();

            for (int i = lattice.Count - 1; i >= (lattice.Count - N); i--)
                perim.Add(i);

            for (int i = 0; i < N; i += (L - 1))
            {
                List<int> side = new List<int>();
                for (int j = 0; j < L; j++)
                    side.Add(perim[(i + j) % N]);

                sides.Add(side);
            }

            for (int i = 0; i < S; i++)
                connect(sides[i], sides[(i + S / 2) % S]);

            for (int i = 0; (i < perim.Count) && !is_complete(); i++)
                isolate(perim[i]);
        }

        //public List<Double> get_distance_list(int index) { return handler_list[index].get_distances(); }

        /*public void read_in_distance_handlers(String s) throws FileNotFoundException
        {
            handler_list = new List<Distance_Handler>(lattice.Count);
           
        Scanner in = new Scanner(new File(s));
        
        int dist_size, list_size, site;
        double dist;

        int total = in.nextInt();
        
        for(int i = 0; i<lattice.Count; i++)
        {
            dist_size = in.nextInt();
        Distance_Handler handle = new Distance_Handler();

            for(int k = 0; k<dist_size; k++)
            {
                dist = in.nextInt();
        list_size = in.nextInt();

        List<int> sites = new List<int>(list_size);

                for(int j = 0; j<list_size; j++)
                    site = in.nextInt();

        handle.Add(dist, sites);
            }

    handler_list.Add(handle);
        }
    }*/
 
    
        public List<List<int>> get_vertices() { return lattice; }
        public List<int> get_ring_population() { return ring_population; }
        public List<int> get_ring_map() { return ring_map; }

        //Generate the lattice
        private void populate_lattice()
        {
            populate_zeroth_ring();

            ring_population.Add(1);

            if (R == 0)
            {
                return;
            }

            populate_first_ring();

            int current_ring_start = 1;
            int current_ring_end;

            //Go ring by ring and generate new lattice sites, connecting the next to the current and current to the previous
            for (int r = 1; r < R; r++)
            {
                current_ring_end = lattice.Count;

                //Go through each site in the current ring and generate a polygon splitting off from it,
                //these new sites will become the next ring
                for (int site = current_ring_start; site < current_ring_end; site++)
                {
                    extend_corner(lattice[site][0], site, lattice[site][1]);
                }

                ring_population.Add(current_ring_end - current_ring_start);

                current_ring_start = current_ring_end;
            }

            ring_population.Add(lattice.Count - current_ring_start);

            int sum_so_far = 1;

            //Going around the ring creates an error when it gets back to the original site of the ring
            //this fixes that
            for (int i = 1; i < ring_population.Count; i++)
            {
                correct_joint(sum_so_far, sum_so_far + ring_population[i] - 1);
                sum_so_far += ring_population[i];
            }

            //Fill in the ring map
            for (int i = 0; i < ring_population.Count; i++)
            {
                for (int j = 0; j < ring_population[i]; j++)
                {
                    ring_map.Add(i);
                }
            }
        }

        //In a few known places the neighbor lists are in the incorrect order, which creates problems
        //when it comes to rendering them graphically
        private void correct_joint(int start, int end)
        {
            int index_a = 0, index_b = 0;
            int temp = 0;

            for (int i = 0; i < lattice[start].Count; i++)
            {
                if (lattice[start][i] == (start + 1))
                    index_a = i;
                if (lattice[start][i] == end)
                    index_b = i;
            }

            temp = lattice[start][index_a];
            lattice[start][index_a] = lattice[start][index_b];
            lattice[start][index_b] = temp;

            for (int i = 0; i < lattice[end].Count; i++)
            {
                if (lattice[end][i] == (end - 1))
                    index_a = i;
                if (lattice[end][i] == start)
                    index_b = i;
            }

            temp = lattice[end][index_a];
            lattice[end][index_a] = lattice[end][index_b];
            lattice[end][index_b] = temp;

            for (int i = 0; i < lattice[start - 1].Count; i++)
            {
                if (lattice[start - 1][i] == start)
                {
                    index_a = i;
                    break;
                }
            }

            temp = lattice[start - 1][index_a];

            for (int i = index_a; i > 0; i--)
                lattice[start - 1][i] = lattice[start - 1][i - 1];

            lattice[start - 1][0] = temp;
        }

        private void populate_zeroth_ring()
        {
            lattice.Add(new List<int>());
        }

        private void populate_first_ring()
        {
            int num_new_vertices = Q * (P - 2); // Number of vertices to be added

            // Create enough new vertices for the first ring
            for (int i = 0; i < num_new_vertices; i++)
            {
                lattice.Add(new List<int>());
            }

            // Connect each new site to the one in front of it, looping around back to itself
            for (int i = 0; i < num_new_vertices; i++)
                connect(i + 1, (i + 1) % num_new_vertices + 1);

            // Connect all the branch vertices to the origin
            for (int i = 1; i <= num_new_vertices; i += (P - 2))
                connect(0, i);
        }

        private void extend_corner(int left, int current, int right)
        {
            int spanning = P - 3;                        // How many vertices lie between branches
            int branches = Q - lattice[current].Count;   // How many branch vertices there are (how many more neighbors needed)
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
                while (lattice[right].Count == Q)
                {
                    num_new_vertices--;
                    right = lattice[right][0];
                }
            }

            // Indices describing where the next ring portion goes in the lattice list
            int next_ring_start = lattice.Count;
            int next_ring_end = lattice.Count + num_new_vertices - 1;

            // Create enough vertices to contain the next ring portion
            for (int i = 0; i < num_new_vertices; i++)
                lattice.Add(new List<int>());

            // Connect each new site the one in front of it
            for (int i = 0; i < (num_new_vertices - 1); i++)
                connect(next_ring_start + i, next_ring_start + i + 1);

            // Connect each branch site to the current site
            for (int i = spanning; i < num_new_vertices; i += (spanning + 1))
                connect(current, next_ring_start + i);

            // Connect the dangling ends of the ring portion to the current site's bounding neighbors
            connect(left, next_ring_start);
            connect(right, next_ring_end);
        }

        private void add_neighbor(List<int> vertex, int neighbor)
        {
            int index;

            // Find an index such that Si > neighbor > Si+1, that way the neighbor list
            // will remain sorted from largest to smallest
            for (index = 0; (index < vertex.Count) && vertex[index] > neighbor; index++) ;

            vertex.Insert(index, neighbor);
        }

        private void connect(int i, int j)
        {
            add_neighbor(lattice[i], j);
            add_neighbor(lattice[j], i);
        }
    }
}
