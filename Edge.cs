using System;
using System.Collections.Generic;
using System.Text;

namespace IsingModel
{
    class Edge<T>
    {
        public T First;
        public T Second;

        public Edge(T a, T b)
        {
            First = a;
            Second = b;
        }

        public override bool Equals(Object obj)
        {
            if (obj == null || !(obj is Edge<T>))
                return false;

            Edge<T> other = (Edge<T>)obj;

            return (First.Equals(other.First) && Second.Equals(other.Second)) || (First.Equals(other.Second) && Second.Equals(other.First));
        }

        public override int GetHashCode() 
        {
            int a = First.GetHashCode();
            int b = Second.GetHashCode();
            return a * b;
        }
    }
}
