namespace Orbit;
internal class Mass{
    
    internal MassInfo mi;
    internal List<Vector2d> forces = new();
    internal double GM; 

    internal Vector2d SumForces() {
        Vector2d sum = new();
        foreach(Vector2d v in forces) {
            sum += v;
        }
        return sum;
    }
    internal void ClearForces() {
        forces.Clear();
    }

    public Mass(double mass, Vector2d velocity, Vector2d position, 
        bool hasTrail = true, bool stationary = false, int trailSteps = 400, int trailSkip = 0) {
        mi = new MassInfo();
        mi.hasTrail = hasTrail;
        mi.stationary = stationary;
        mi.mass = mass;
        mi.velocity = velocity;
        mi.position = position;
        mi.trailSkip = trailSkip;
        mi.trail = new Vector2d[trailSteps];

        this.GM = mass * Constant.G;

        mi.index = Shared.AddMass(mi);
    }
}

internal class MassInfo {
    internal int index;
    internal string name;
    internal int trailSkip;
    internal Vector2d[] trail;
    internal int trailOffset;
    internal bool hasTrail;

    internal bool stationary;

    internal Vector2d velocity;
    internal double mass;
    internal Vector2d position;

    public MassInfo() {
        trail = new Vector2d[0];
        name = "";
    }

    internal void CopyPhysicsInfo(MassInfo m)
    {
        velocity = m.velocity;
        position = m.position;
        m.trail.CopyTo(trail, 0);
    }
    internal void CopyNewInfo(MassInfo m) {
        velocity = m.velocity;
        position = m.position;
        mass = m.mass;
        stationary = m.stationary;

        m.trail.CopyTo(trail, 0);
    }

    internal MassInfo FullCopy()
    {
        MassInfo m = new();

        m.index = index;
        m.name = name;
        m.trailSkip = trailSkip;
        m.trail = trail;
        m.trailOffset = trailOffset;
        m.hasTrail = hasTrail;
        m.stationary = stationary;
        m.velocity = velocity;
        m.mass = mass;
        m.position = position;
        
        return m;
    }
}