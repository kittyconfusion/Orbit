namespace Orbit;

public static class Constant{
    public const double originalG = 0.000000000066743; //kg m sec

    public static readonly double G = originalG / 1000; // * Math.Pow(10, -6) * Math.Pow(10, -6);

    public static readonly double MassOfSun = 1.989  * Math.Pow(10,24);
    public static readonly double MassOfEarth = 5.9736 * Math.Pow(10,18);

    public const int MINUTES = 60;
    public const int HOURS   = 60 * 60;
    public const int DAYS    = 60 * 60 * 24;
    public const int WEEKS   = 60 * 60 * 24 * 7;
    public const int YEARS   = (int)(60 * 60 * 24 * 365.25);

    internal static double kgToGg(double kg)
    {
        return kg / 1000000;
    }

    internal static double AUtokm(double au)
    {
        return au * 149597870.7;
    }
}

public struct Vector2d
{
    public double X {get; set;}
    public double Y {get; set;}

    public Vector2d(double X, double Y)
    {
        this.X = X;
        this.Y = Y;
    }

    public double Magnitude()
        => Math.Sqrt(X*X + Y*Y);
    public Vector2d Normalize()
        => this / Magnitude();
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
    public string ToScientificString(int digits = 0)
    {
        return "(" + Math.Round(X, digits).ToString("E3") + ", " + Math.Round(-Y, digits).ToString("E3") + ")";
    }
    public string ToRoundedString(int digits = 0) {
        return "(" + Math.Round(X, digits).ToString() + ", " + Math.Round(-Y, digits).ToString() + ")";
    }
    public Vector2d ToAstronomicalUnits() 
        => new Vector2d(X / 149600000, Y / 149600000);
    public Vector2d ToKmUnits()
        => new Vector2d(X * 149600000, Y * 149600000);
}
