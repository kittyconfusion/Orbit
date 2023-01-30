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
    internal void Update(double deltaTime)
    {
        

        foreach(Mass m in masses) {
            if(m.mi.stationary) { continue; }
            
            foreach(Mass n in masses) {
                if(m.mi.index == n.mi.index) { continue; } //Skip calculations if the same object


                double x = m.mi.position.X - n.mi.position.X;
                double y = m.mi.position.Y - n.mi.position.Y;

                double dist2 = Math.Pow(CalculateDistance(x, y), 3);
                double force = (Constant.G * m.mi.mass * n.mi.mass) / dist2;

                //https://math.stackexchange.com/a/3004599
                //double angle = -2 * Math.Atan(y / (x + Math.Sqrt(Math.Pow(x,2) + Math.Pow(y,2))));
                //double angle = Math.Atan2(y,x);
                //Console.WriteLine(Math.Atan2(11,0));

                m.forces.Add(new Vector2d(x, y) * -force);
                //Console.WriteLine(new Vector2d(force * Math.Cos(angle), force * Math.Sin(angle)) * deltaTime);
            }
            
        }
        
        foreach(Mass m in masses) {
            MassInfo mi = m.mi;
            if(!mi.hasTrail) { continue; }

            mi.velocity += (m.SumForces() / m.mi.mass) * deltaTime;
            
            m.ClearForces();
            mi.position += (m.mi.velocity * deltaTime);
            

            mi.trail[m.mi.trailOffset] = m.mi.position;
            mi.trailOffset = (m.mi.trailOffset + 1) % m.mi.trail.Length;
            /*
            mi.trailCounter++;
            if(mi.trailCounter >= mi.trailOffset) {
                mi.trail[m.mi.trailOffset] = m.mi.position;
                mi.trailOffset = (m.mi.trailOffset + 1) % m.mi.trail.Length;
                mi.trailCounter = 0;
            }
            */
            
        }
    }

    private double CalculateDistance(double x, double y)
    {
        return Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
    }
}

