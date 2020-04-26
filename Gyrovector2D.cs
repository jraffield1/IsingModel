using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace IsingModel
{
    // Represents a gyrovector in the 2D Poincare disk
    class Gyrovector2D : Vector2D
    {
        public Gyrovector2D(Complex c) : base(c) { }
        public Gyrovector2D(double x, double y) : base(x,y) { }
        public Gyrovector2D(Vector2D v) : base(v.GetPosition()) { }

        public new double Distance(Vector2D other)
        {
            Complex posA = GetPosition();
            Complex posB = other.GetPosition();

            double a = (1 - (Complex.Conjugate(posA) * posB)).Magnitude;
            double b = (posB - posA).Magnitude;

            return Math.Log((a + b) / (a - b));
        }

        public new double Magnitude()
        {
            double r = EuclideanMagnitude();

            return Math.Log((1 + r) / (1 - r));
        }

        public double EuclideanMagnitude(){ return base.Magnitude(); }

        public new Gyrovector2D Unit(){ return new Gyrovector2D(base.Unit()); }

        public Gyrovector2D Add(Gyrovector2D g)
        {
            Complex posA = GetPosition();
            Complex posB = g.GetPosition();

            Complex num = posA + posB;
            Complex den = 1 + Complex.Conjugate(posA)*posB;

            return new Gyrovector2D(num / den);
        }
        public static Gyrovector2D operator +(Gyrovector2D A, Gyrovector2D B) { return A.Add(B); }

        public new Gyrovector2D Multiply(double t)
        {
            double mag = EuclideanMagnitude();

            double a = Math.Pow(1 + mag, t);
            double b = Math.Pow(1 - mag, t);

            return new Gyrovector2D(Unit() * ((a - b) / (a + b)));
        }

        public static Gyrovector2D operator *(Gyrovector2D A, double B) { return A.Multiply(B); }
        public static Gyrovector2D operator *(double A, Gyrovector2D B) { return B.Multiply(A); }

        public new Gyrovector2D Rotate(double theta){ return new Gyrovector2D(base.Rotate(theta)); }

        public new Gyrovector2D Negate(){ return new Gyrovector2D(base.Negate()); }
        public static Gyrovector2D operator -(Gyrovector2D A) { return A.Negate();  }
    }
}
