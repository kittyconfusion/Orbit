namespace Orbit;
internal class Mass : StationaryMass{
    internal const int TRAIL_STEPS = 1000;
    internal Vector2d[] trail;
    internal bool hasTrail;

    internal List<Vector2d> forces = new();
    internal Vector2d velocity;
    internal double GM;

    internal Vector2d AddForces() {
        Vector2d sum = new();
        foreach(Vector2d v in forces) {
            sum += v;
        }
        return sum;
    }

    public Mass(double mass, Vector2d velocity, Vector2d position, bool hasTrail = true)  : base(mass, position){
        this.hasTrail = hasTrail;
        this.velocity = velocity;

        this.GM = mass * Constant.G;
        if(hasTrail) {
            trail = new Vector2d[TRAIL_STEPS];
        }
        else {
            trail = new Vector2d[0];
        }
    }

}

