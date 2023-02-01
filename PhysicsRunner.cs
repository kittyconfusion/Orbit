namespace Orbit;
using System.Diagnostics;

internal class PhysicsRunner {
    Stopwatch s = Stopwatch.StartNew(); 
    List<Mass> masses;
    internal PhysicsRunner() {
        masses = new();
    }
    internal void AddMass(Mass m) {
        masses.Add(m);
    }
    internal void Update(double deltaTime)
    {
        lock(Shared.DataLock) {
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
                //Vector2d oldpos = mi.position;
                //Console.Write(mi.position + " ");
                mi.position += (m.mi.velocity * deltaTime);
                //Console.Write(mi.position + " | ");

                //Console.WriteLine("Update: " + s.ElapsedMilliseconds);
                //s.Restart();
                

                
                mi.trail[mi.trailOffset] = mi.position;
                mi.trailOffset = (mi.trailOffset + 1) % mi.trail.Length;
                /*
                for(int i = mi.trailOffset; i < mi.trailOffset + mi.trail.Length; i++) {
                    Console.Write(mi.trail[i % mi.trail.Length] + " ");
                }
                Console.WriteLine(mi.position - oldpos);
                */
                /*
                mi.trailCounter++;

                if(mi.trailCounter >= mi.trailSkip) {
                    mi.trail[m.mi.trailOffset] = m.mi.position;
                    mi.trailOffset = (m.mi.trailOffset + 1) % m.mi.trail.Length;
                    mi.trailCounter = 0;
                }
                */
                
            }
        }
    }

    private double CalculateDistance(double x, double y)
    {
        return Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
    }
}

