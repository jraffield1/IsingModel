using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace IsingModel
{
    class Vector2D
    {
        private Complex position;

        public static Vector2D ZERO = new Vector2D(0,0);

        public Vector2D(double r, double i)
        {
            position = new Complex(r, i);
        }
        public Vector2D(Complex c)
        {
            position = c;
        }

        public double X() { return position.Real; }
        public double Y() { return position.Imaginary; }

        public Complex GetPosition() { return position; }

        public double Distance(Vector2D other){ return (position - other.GetPosition()).Magnitude; }

        public double Magnitude(){ return position.Magnitude; }

        public Vector2D Unit() { return new Vector2D(position / position.Magnitude); }

        public Vector2D Rotate(double theta)
        {
            double s = Math.Sin(theta);
            double c = Math.Cos(theta);

            return new Vector2D(position.Real * c - position.Imaginary * s, position.Real * s + position.Imaginary * c);
        }

        public Vector2D Add(Vector2D other){ return new Vector2D(position + other.GetPosition()); }
        public static Vector2D operator +(Vector2D A, Vector2D B){ return A.Add(B); }

        public Vector2D Subtract(Vector2D other) { return new Vector2D(position - other.GetPosition()); }
        public static Vector2D operator -(Vector2D A, Vector2D B) { return A.Subtract(B); }

        public Vector2D Multiply(double t){ return new Vector2D(position * t); }
        public static Vector2D operator *(Vector2D A, double B) { return A.Multiply(B); }
        public static Vector2D operator *(double A, Vector2D B) { return B.Multiply(A); }

        public Vector2D Negate(){ return new Vector2D(-1 * position); }
        public static Vector2D operator -(Vector2D A) { return A.Negate(); }

        public double Phase(){ return position.Phase; }

        public override bool Equals(Object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Vector2D other = (Vector2D)obj;
                return position.Equals(other.GetPosition());
            }
        }

        public override int GetHashCode(){ return position.GetHashCode(); }

        public override string ToString()
        {
            return position.Real + " " + position.Imaginary;
        }
    }
}
