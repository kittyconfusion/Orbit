namespace Orbit;
using System.Diagnostics;

internal class PhysicsRunner {
    List<Mass> masses;
    internal PhysicsRunner() {
        masses = new();
    }
    internal void AddMass(Mass m) {
        masses.Add(m);
        m.mi.index = Shared.AddMass(m.mi);
    }
    internal void ProcessDataChanges() {
        //Change[0] is type, change[1] is value, change[2] is index to change
        string[] change;
        while (Shared.changesToMake.TryPop(out change!)) {
            MassInfo mass = masses[Int32.Parse(change[2])].mi;
            switch(change[0]) {
                case "position":
                    mass.position = ParseVector2dFromString(change[1], mass.position);
                    break;
                case "velocity":
                    mass.velocity = ParseVector2dFromString(change[1], mass.velocity);
                    break;
                case "mass":
                    try {
                        mass.mass = Double.Parse(change[1] , System.Globalization.NumberStyles.Float);
                    } catch {}
                    break;
            }
        }
    }
    internal Vector2d ParseVector2dFromString(string s, Vector2d ifNull) {
        char[] toTrim = {' ', '(', ')'};
        s = s.Trim(toTrim);
        s.Split(',');
        try {
            return new Vector2d(Double.Parse(s.Split(',')[0] , System.Globalization.NumberStyles.Float),
                                Double.Parse(s.Split(',')[1] , System.Globalization.NumberStyles.Float));
        }
        catch {
            return ifNull;
        }
        
    }
    internal void Update(double deltaTime)
    {
        lock(Shared.DataLock) {
            ProcessDataChanges();
            //Shared.ReadyWorkingCopy();
            foreach(Mass m in masses) {
                if(m.mi.stationary) { continue; }
                
                foreach(Mass n in masses) {
                    if(m.mi.index == n.mi.index) { continue; } //Skip calculations if the same object
                    

                    double x = m.mi.position.X - n.mi.position.X;
                    double y = m.mi.position.Y - n.mi.position.Y;

                    double dist2 = Math.Pow(CalculateDistance(x, y), 3);
                    double force = (Constant.G * m.mi.mass * n.mi.mass) / dist2;

                    m.forces.Add(new Vector2d(x, y) * -force);
                }
                
            }
            
            foreach(Mass m in masses) {
                MassInfo mi = m.mi;
                if(!mi.hasTrail) { continue; }

                mi.velocity += (m.SumForces() / m.mi.mass) * deltaTime;
                
                m.ClearForces();

                mi.position += (m.mi.velocity * deltaTime);
                
                mi.trailCounter++;

                if(mi.trailCounter >= mi.trailSkip) {
                    mi.trail[m.mi.trailOffset] = m.mi.position;
                    mi.trailOffset = (m.mi.trailOffset + 1) % m.mi.trail.Length;
                    mi.trailCounter = 0;
                }
            }
        }
    }

    private double CalculateDistance(double x, double y)
    {
        return Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
    }
}

