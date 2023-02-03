namespace Orbit;
internal class Mass{
    
    internal MassInfo mi;
    internal List<Vector2d> forces = new();

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

    public Mass(double mass, Vector2d velocity, Vector2d position, string name = "",
        bool hasTrail = true, bool stationary = false, int trailSteps = 150, int trailSkip = 4) {
        if(name == "") { name = GenerateName(); }
        mi = new MassInfo();
        mi.hasTrail = hasTrail;
        mi.stationary = stationary;
        mi.mass = mass;
        mi.velocity = velocity;
        mi.position = position;
        mi.trailSkip = trailSkip;
        mi.name = name;
        mi.trail = new Vector2d[trailSteps];
        for(int i = 0; i < mi.trail.Length; i++) { mi.trail[i] = new Vector2d(position.X, position.Y);}
    }

    private string GenerateName()
    {
        string[] adjectives = File.ReadAllLines("adjectives.txt");
        string[] animals = File.ReadAllLines("animals.txt");
        
        Random r = new();
        string adjective = adjectives[r.Next(0, adjectives.Length - 1)];
        string animal = animals[r.Next(0, animals.Length - 1)];
        
        return adjective + " " + animal;
    }
}

internal class MassInfo {
    internal int index;
    internal string name;
    internal int trailSkip;
    internal int trailCounter;
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
        trailOffset = m.trailOffset;
        mass = m.mass;
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

        m.name = name;
        m.index = index;
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