namespace Orbit;

internal class PhysicsRunner {
    List<Mass> masses;
    const double FourPISquared = 4 * Math.PI * Math.PI;

    internal PhysicsRunner() {
        masses = new();
    }
    internal void AddMass(Mass m) {
        masses.Add(m);
        Shared.AddMass(m.mi);
    }
    internal void Update()
    {
        int deltaTime = 60;// * 60; //160 * 60 * 24;

        foreach(Mass m in masses) {
            if(m.mi.stationary) { continue; }
            
            foreach(Mass n in masses) {
                if(m.mi.index == n.mi.index) { continue; } //Skip calculations if the same object

                double dist2 = Math.Pow(CalculateDistance(m, n), 2);

                double force = (Constant.G * m.mi.mass * n.mi.mass) / dist2;
                
                double x = m.mi.position.X - n.mi.position.X;
                double y = m.mi.position.Y - n.mi.position.Y;

                //https://math.stackexchange.com/a/3004599
                //double angle = -2 * Math.Atan(y / (x + Math.Sqrt(Math.Pow(x,2) + Math.Pow(y,2))));
                double angle = Math.Atan2(y,x);
                //Console.WriteLine(Math.Atan2(11,0));
                m.forces.Add(new Vector2d(-Math.Sin(angle), Math.Cos(angle)) * force * deltaTime);
                //Console.WriteLine(new Vector2d(force * Math.Cos(angle), force * Math.Sin(angle)) * deltaTime);
            }
            
        }
        
        foreach(Mass m in masses) {
            if(!m.mi.hasTrail) { continue; }

            m.mi.velocity += m.SumForces() / m.mi.mass;
            
            m.ClearForces();
            m.mi.position += (m.mi.velocity * deltaTime);
            
            m.mi.trail[m.mi.trailOffset] = m.mi.position;
            m.mi.trailOffset = (m.mi.trailOffset + 1) % m.mi.trail.Length;
        }
    }

    private double CalculateDistance(Mass m1, Mass m2)
    {
        return Math.Sqrt(Math.Pow(m1.mi.position.X, 2) + Math.Pow(m2.mi.position.Y, 2));
    }
}

