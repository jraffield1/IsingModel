using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace IsingModel
{
    class Vector2D
    {
        private Complex position;
        private double EPSILON = 0.0001;

        public Vector2D(double r, double i)
        {
            position = new Complex(r, i);
        }
        public Vector2D(Complex c)
        {
            position = c;
        }

        public double x() { return position.Real; }
        public double y() { return position.Imaginary; }

        public Complex GetPosition() { return position; }

        public double Distance(Vector2D other)
        {
            return (position - other.GetPosition()).Magnitude;
        }

        public double Magnitude()
        {
            return position.Magnitude;
        }

        public Vector2D Unit()
        {
            return new Vector2D(position / position.Magnitude);
        }

        public Vector2D Rotate(double theta)
        {
            double s = Math.Sin(theta);
            double c = Math.Cos(theta);

            return new Vector2D(position.Real * c - position.Imaginary * s, position.Real * s + position.Imaginary * c);
        }

        public Vector2D Add(Vector2D other)
        {
            return new Vector2D(position + other.GetPosition());
        }

        public Vector2D Multiply(double t)
        {
            return new Vector2D(position * t);
        }

        public Vector2D Negate()
        {
            return new Vector2D(-1 * position);
        }

        public double Phase()
        {
            return position.Phase;
        }

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

        public override int GetHashCode()
        {
            return position.GetHashCode();
        }

        public override string ToString()
        {
            return x() + " " + y();
        }
    }
}
