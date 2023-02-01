namespace Orbit;

public static class Constant{
    public const double originalG = 0.000000000066743; //kg m sec

    public static readonly double G = originalG / 1000; // * Math.Pow(10, -6) * Math.Pow(10, -6);

    //public static readonly double MassOfSun = 2 * Math.Pow(10, 26); //2 * 10^30 kg

    //public static readonly double MassOfSun = 5973600000000000000; //REPLACE. ACTUALLY MASS OF EARTH.
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
    public static Vector2d operator +(Vector2d a, int b)
        => new Vector2d(a.X + b, a.Y + b);
    public static Vector2d operator +(Vector2d a, double b)
        => new Vector2d(a.X + b, a.Y + b);
    public static Vector2d operator -(Vector2d a, Vector2d b)
        => new Vector2d(a.X - b.X, a.Y - b.Y);
    public static Vector2d operator *(Vector2d a, Vector2d b)
        => new Vector2d(a.X * b.X, a.Y * b.Y);
    public static Vector2d operator *(Vector2d a, int c)
        => new Vector2d(a.X  * c, a.Y * c);
    public static Vector2d operator *(Vector2d a, double c)
        => new Vector2d(a.X * c, a.Y * c);
    public static Vector2d operator /(Vector2d a, double b)
        => new Vector2d(a.X / b, a.Y / b);

    public override string ToString()
    {
        return "(" + X + ", " + Y + ")";
    }
    public string ToRoundedString()
    {
        return "(" + Math.Round(X) + ", " + Math.Round(Y) + ")";
    }
}
