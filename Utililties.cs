using System;
using System.Collections.Generic;
using System.Text;

namespace IsingModel
{
    class Utililties
    {
        //Generate a hyperbolic spatial lattice
        public static Lattice_Geometry CreateLatticeGeometry(int p, int q, int r, double side_length = 1.0)
        {
            if ((p - 2) * (q - 2) > 4)
            {
                //Sidelengths in hyperbolic geometry are unique to a given spatial tiling
                side_length = HyperbolicToPpoincare(HyperbolicSideLength(p, q));
            }

            return new Lattice_Geometry(p, q, r, side_length);
        }

        //Take a LatticeGeometry and return the polygons that spatially make it up
        //This is useful for display purposes, each lattice element then corresponds to a polygon element
        public static void calculate_polygon_tiling(List<List<Vector2D>> polygon_list, Lattice_Geometry lattice_geometry)
        {
            Lattice_Graph lattice_graph = lattice_geometry.Lattice;

            List<List<int>> graph = lattice_graph.GetVertices();
            List<Vector2D> geometry = lattice_geometry.GeometryPoints;

            int p = lattice_graph.P;
            int q = lattice_graph.Q;

            double base_length = lattice_geometry.LengthScale * EuclideanTransversalLength(p,q);

            if(lattice_graph.GetCurvatureSign() == -1)
                base_length = HyperbolicToPpoincare(HyperbolicTransversalLength(p, q));

            Vector2D base_vector = new Gyrovector2D(base_length, 0.0);

            double dt = 2 * Math.PI / p;

            base_vector = base_vector.Rotate(Math.PI / p);

            if (lattice_graph.Size == 1)
            {
                List<Vector2D> poly = new List<Vector2D>();
                for (int i = 0; i < p; i++)
                    poly.Add(base_vector.Rotate(i * dt));
                polygon_list.Add(poly);

                return;
            }

            for (int i = 0; i < lattice_graph.Size; i++)
            {
                List<Vector2D> poly = new List<Vector2D>(p);

                Vector2D current = geometry[i];
                Vector2D neighbor = geometry[ graph[i][0] ];

                double guide_direction = ((-current) + neighbor).Phase();

                Vector2D offset = base_vector.Rotate(guide_direction);

                for (int k = 0; k < p; k++)
                {
                    poly.Add(current + offset.Rotate(k * dt));
                }

                polygon_list.Add(poly);
            }
        }


        //Take a spatial polygon list and generate a list of spatial edges, useful for display purposes, can take either Vector or Gyrovector
        public static void polygon_list_to_edge_list(List<List<Vector2D>> polygon_list, List<Edge<Vector2D>> edge_list)
        {
            int poly_size = polygon_list[0].Count;

            for (int i = 0; i < polygon_list.Count; i++)
            {
                for (int j = 0; j < poly_size; j++)
                {
                    Edge<Vector2D> edge = new Edge<Vector2D>(polygon_list[i][j], polygon_list[i][(j + 1) % poly_size]);

                    if (!edge_list.Contains(edge))
                        edge_list.Add(edge);
                }
            }
        }

        //Take a polygon list and generate a list of edges, useful for display purposes  
        public static void graph_to_edge_list(Lattice_Graph lattice, List<Edge<int>> edge_list)
        {
            List<List<int>> graph = lattice.GetVertices();

            for (int i = 0; i < graph.Count; i++)
            {
                List<int> vertex = graph[i];

                for (int j = 0; j < vertex.Count; j++)
                {
                    Edge<int> edge = new Edge<int>(i, vertex[j]);

                    if (!edge_list.Contains(edge))
                        edge_list.Add(edge);
                }
            }
        }

        public static double HyperbolicToPpoincare(double p)
        {
            return Math.Tanh(0.5 * p);
        }

        public static double PoincareToHyperbolic(double r)
        {
            return Math.Log((1 + r) / (1 - r));
        }

        public static double HyperbolicSideLength(int p, int q)
        {
            double a = 2 * Math.PI / p;
            double b = Math.PI / q;

            return Math.Acosh((Math.Cos(a) + Math.Cos(b) * Math.Cos(b)) / (Math.Sin(b) * Math.Sin(b)));
        }

        public static double PoincareSideLength(int p, int q)
        {
            return HyperbolicToPpoincare(HyperbolicSideLength(p, q));
        }

        public static double HyperbolicTransversalLength(int p, int q)
        {
            double a = 2 * Math.PI / p;
            double b = Math.PI / q;

            return Math.Acosh((Math.Cos(b) + Math.Cos(a) * Math.Cos(b)) / (Math.Sin(a) * Math.Sin(b)));
        }

        public static double EuclideanTransversalLength(int p, int q)
        {
            return Math.Sin(Math.PI / p) / Math.Sin(2 * Math.PI / q);
        }
    }
}
