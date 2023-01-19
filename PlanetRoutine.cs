namespace Orbit;

internal class Planet {
    internal Mass mass;
    internal StationaryMass sun;

    internal Planet(double mass, Vector2d velocity, Vector2d position, StationaryMass sun) {
        this.mass = new(mass, velocity, position);
        this.sun = sun;
    }
    internal void Update(double timeSkip) {
        double distanceToSun = (double)Constant.CalculateDistance(mass,sun);
        double distanceToSunCubed = distanceToSun * distanceToSun * distanceToSun;
        const double pi4squ = (double)(4 * Math.PI * Math.PI);

        Vector2d gravity = new();
        gravity.X = (pi4squ * mass.position.X / distanceToSunCubed);
        gravity.Y = (pi4squ * mass.position.Y / distanceToSunCubed);
        mass.velocity = mass.velocity - (gravity * timeSkip);
        mass.position += mass.velocity * timeSkip;

        //Console.WriteLine(mass.position + " " + sun.position);
        //Console.WriteLine(Constant.CalculateDistance(mass,sun) + " " + gravity.Magnitude());
    }

    public override string ToString()
    {
        return "Position " + mass.position + " Velocity " + mass.velocity;
    }
}