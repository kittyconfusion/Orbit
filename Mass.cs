namespace Orbit;
internal class Mass{
    internal MassInfo mi;

    internal Vector2d SumForces() {
        Vector2d sum = new();
        foreach(Vector2d v in mi.forces) {
            sum += v;
        }
        return sum;
    }
    internal void ClearForces() {
        mi.forces.Clear();
    }

    public Mass(double mass, Vector2d velocity, Vector2d position, string name = "",
        bool hasTrail = true, bool stationary = false, int trailLength = 5, double trailQuality = 5, 
        int followingIndex = -1, int orbitingBodyIndex = -1, int precisionPriorityLimit = -1, bool satellite = true) {
        mi = new MassInfo();
        mi.hasTrail = hasTrail;
        mi.stationary = stationary;
        mi.mass = mass;
        mi.velocity = velocity;
        mi.position = position;
        //Excessive trailQuality values cause the program to freeze. Better to not break.
        mi.trailQuality = Math.Pow(10, 8 - Math.Clamp(trailQuality, 1, 8)) * 60;
        mi.followingIndex = followingIndex;
        mi.name = name == "" ? GenerateName() : name;
        mi.trail = new Vector2d[trailLength * 100];
        mi.precisionPriorityLimit = precisionPriorityLimit;
        mi.orbitingBodyIndex = orbitingBodyIndex;
        mi.satellite = satellite;
        mi.currentlyUpdatingPhysics = true;
        for(int i = 0; i < mi.trail.Length; i++) { mi.trail[i] = new Vector2d(position.X, position.Y);}
    } 

    private string GenerateName()
    {   
        if(!(File.Exists("adjectives.txt") && File.Exists("animals.txt"))) {
            return "New Mass";
        }
        string[] adjectives = File.ReadAllLines("adjectives.txt");
        string[] animals = File.ReadAllLines("animals.txt");
        
        Random r = new();
        string adjective = adjectives[r.Next(adjectives.Length)];
        string animal = animals[r.Next(animals.Length)];

        if(r.Next(100) == 0) { mi.mass = 5.9736 * Math.Pow(10,18) * 6.66 * Math.Pow(10,6); return "Illuminati Secret Base"; }
        return adjective + " " + animal;
    }
}

internal class MassInfo {
    internal List<Vector2d> forces = new();
    internal int index;
    internal string name;
    internal bool minorMass;
    internal double trailQuality;
    internal double trailCounter;
    internal Vector2d[] trail;
    internal int trailOffset;
    internal bool hasTrail;
    internal bool stationary;
    internal int followingIndex;

    internal Vector2d velocity;
    internal double mass;
    internal Vector2d position;
    internal int precisionPriorityLimit;
    internal bool satellite;
    internal int orbitingBodyIndex;
    internal bool currentlyUpdatingPhysics;

    public MassInfo() {
        trail = new Vector2d[0];
        followingIndex = -1;
        name = "";
    }

    internal void CopyPhysicsInfo(MassInfo m)
    {
        if(trail.Length != m.trail.Length) {
            trail = new Vector2d[m.trail.Length];
        }
        velocity = m.velocity;
        position = m.position;
        mass = m.mass;
        m.trail.CopyTo(trail, 0);
        trailOffset = m.trailOffset;
        followingIndex = m.followingIndex;
        trailQuality = m.trailQuality;
        orbitingBodyIndex = m.orbitingBodyIndex;
        currentlyUpdatingPhysics = m.currentlyUpdatingPhysics;
        forces = new List<Vector2d>(m.forces);
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
        m.trailQuality = trailQuality;
        m.trail = trail;
        m.trailOffset = trailOffset;
        m.hasTrail = hasTrail;
        m.stationary = stationary;
        m.velocity = velocity;
        m.mass = mass;
        m.position = position;
        m.followingIndex = followingIndex;
        m.precisionPriorityLimit = precisionPriorityLimit;
        m.satellite = satellite;
        m.orbitingBodyIndex = orbitingBodyIndex;
        
        return m;
    }
}