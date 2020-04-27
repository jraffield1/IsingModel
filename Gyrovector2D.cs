using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace IsingModel
{
    // Represents a gyrovector in the 2D Poincare disk
    class Gyrovector2D : Vector2D
    {
        public Gyrovector2D(double r, double i)
        {
            position = new Complex(r, i);
        }
        public Gyrovector2D(Complex c)
        {
            position = c;
        }

        public Gyrovector2D(Vector2D v)
        {
            position = v.position;
        }

        public override double Distance(Vector2D other)
        {
            Complex posA = position;
            Complex posB = other.position;

            double a = (1 - (Complex.Conjugate(posA) * posB)).Magnitude;
            double b = (posB - posA).Magnitude;

            return Math.Log((a + b) / (a - b));
        }

        public override double Magnitude()
        {
            double r = EuclideanMagnitude();

            return Math.Log((1 + r) / (1 - r));
        }

        public double EuclideanMagnitude(){ return position.Magnitude; }

        public override Vector2D Unit() { return new Gyrovector2D(position / Magnitude()); }

        public override Vector2D Add(Vector2D g)
        {
            Complex posA = position;
            Complex posB = g.position;

            Complex num = posA + posB;
            Complex den = 1 + Complex.Conjugate(posA)*posB;

            return new Gyrovector2D(num / den);
        }

        public override Vector2D Multiply(double t)
        {
            double mag = EuclideanMagnitude();

            double a = Math.Pow(1 + mag, t);
            double b = Math.Pow(1 - mag, t);

            return new Gyrovector2D(Unit() * ((a - b) / (a + b)));
        }

        public override Vector2D Rotate(double theta)
        {
            double s = Math.Sin(theta);
            double c = Math.Cos(theta);

            return new Gyrovector2D(position.Real * c - position.Imaginary * s, position.Real * s + position.Imaginary * c);
        }

        public override Vector2D Negate(){ return new Gyrovector2D(-1*position); }
    }
}
