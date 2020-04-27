using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace IsingModel
{
    abstract class Vector2D : IEquatable<Vector2D>
    {
        public Complex position;

        public double X() { return position.Real; }
        public double Y() { return position.Imaginary; }

        public double Phase() { return position.Phase; }

        public override int GetHashCode() { return position.GetHashCode(); }

        public override string ToString()
        {
            return position.Real + " " + position.Imaginary;
        }

        public bool Equals(Vector2D other)
        {
            if (other == null)
                return false;

            return position.Equals(other.position);
        }

        public abstract double Distance(Vector2D other);
        public abstract double Magnitude();
        public abstract Vector2D Unit();

        public abstract Vector2D Multiply(double t);
        public abstract Vector2D Add(Vector2D other);
        public abstract Vector2D Negate();

        public abstract Vector2D Rotate(double theta);

        public static Vector2D operator +(Vector2D A, Vector2D B) { return A.Add(B); }
        public static Vector2D operator -(Vector2D A, Vector2D B) { return A.Add(-B); }

        public static Vector2D operator *(Vector2D A, double t) { return A.Multiply(t); }
        public static Vector2D operator *(double t, Vector2D A) { return A.Multiply(t); }
        public static Vector2D operator /(Vector2D A, double t) { return A.Multiply(1.0/t); }

        public static Vector2D operator -(Vector2D A) { return A.Negate(); }
    }

    class Euclvector2D : Vector2D
    {
        public Euclvector2D(double r, double i)
        {
            position = new Complex(r, i);
        }

        public Euclvector2D(Complex c)
        {
            position = c;
        }

        public override double Distance(Vector2D other){ return (position - other.position).Magnitude; }

        public override double Magnitude(){ return position.Magnitude; }

        public override Vector2D Unit() { return new Euclvector2D(position / Magnitude()); }

        public override Vector2D Rotate(double theta)
        {
            double s = Math.Sin(theta);
            double c = Math.Cos(theta);

            return new Euclvector2D(position.Real * c - position.Imaginary * s, position.Real * s + position.Imaginary * c);
        }

        public override Vector2D Add(Vector2D other){ return new Euclvector2D(position + other.position); }

        public override Vector2D Multiply(double t){ return new Euclvector2D(position * t); }

        public override Vector2D Negate(){ return new Euclvector2D(-1 * position); }
    }
}
