namespace Orbit;
internal class Mass {
    internal Vector2d position;
    internal Vector2d velocity;

    internal static double CalculateDistance(Mass m1, Mass m2) {
        return Math.Sqrt((double)(m1.position.X * m2.position.X + m1.position.Y * m2.position.Y));
    }

}

