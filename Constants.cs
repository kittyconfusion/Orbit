namespace Orbit;

public static class Constant{
    public const double G = 0.000000000066743;

    internal static double CalculateDistance(StationaryMass m1, StationaryMass m2) {
        return Math.Abs(Math.Sqrt((double)(Math.Pow(m1.position.X - m2.position.X, 2) + Math.Pow(m1.position.Y - m2.position.Y, 2))));
    }
}

public struct Vector2d
{
    public double X;
    public double Y;

    public Vector2d(double X, double Y)
    {
        this.X = X;
        this.Y = Y;
    }

    public double Magnitude()
        => Math.Sqrt(X*X + Y*Y);

    public static Vector2d operator +(Vector2d a, Vector2d b)
        => new Vector2d(a.X + b.X, a.Y + b.Y);
    public static Vector2d operator -(Vector2d a, Vector2d b)
        => new Vector2d(a.X - b.X, a.Y - b.Y);
    public static Vector2d operator *(Vector2d a, Vector2d b)
        => new Vector2d(a.X * b.X, a.Y * b.Y);
    public static Vector2d operator *(Vector2d a, int c)
        => new Vector2d(a.X  * c, a.Y * c);
    public static Vector2d operator *(Vector2d a, double c)
        => new Vector2d(a.X * c, a.Y * c);
        
    public override string ToString()
    {
        return "(" + X + ", " + Y + ")";
    }
}
