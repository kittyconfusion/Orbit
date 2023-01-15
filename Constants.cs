namespace Orbit;

public struct Vector2d
{
    public decimal X;
    public decimal Y;

    public Vector2d(decimal X, decimal Y)
    {
        this.X = X;
        this.Y = Y;
    }

    public static Vector2d operator +(Vector2d a, Vector2d b)
        => new Vector2d(a.X + b.X, a.Y + b.Y);
    public static Vector2d operator -(Vector2d a, Vector2d b)
    => new Vector2d(a.X - b.X, a.Y - b.Y);
    public static Vector2d operator *(Vector2d a, Vector2d b)
    => new Vector2d(a.X * b.X, a.Y * b.Y);
}
