namespace Orbit;
internal class PhysicsRunner {
    List<Mass> masses;
    internal PhysicsRunner() {
        masses = new();
    }
    internal void AddMass(Mass m) {
        masses.Add(m);
        Shared.AddMass(m.mi);
    }
    internal void ProcessDataChanges() {
        //Change[0] is type, change[1] is value, change[2] is index to change
        string[] change;
        while (Shared.changesToMake.TryPop(out change!)) {
            MassInfo mass = new();
            if(change[2] != "-1") {
                mass = masses[Int32.Parse(change[2])].mi;
            }
            
            switch(change[0]) {
                case "position":
                    mass.position = ParseVector2dFromString(change[1], mass.position);
                    break;
                case "velocity":
                    mass.velocity = ParseVector2dFromString(change[1], mass.velocity);
                    break;
                case "mass":
                    if(double.TryParse(change[1], System.Globalization.NumberStyles.Float, null, out double newMass)) {
                        mass.mass = newMass;
                        //mass.mass = Double.Parse(change[1] , System.Globalization.NumberStyles.Float);
                    }
                    break;
                case "new mass":
                    AddMass(new Mass(1e+18,new Vector2d(), new Vector2d()));
                    break;
                case "trail follow":
                    if(mass.followingIndex != Convert.ToInt32(change[1])) {
                        mass.followingIndex = Convert.ToInt32(change[1]);
                        //Clear the trail
                        if(mass.followingIndex != -1) {
                            for(int i = 0; i < mass.trail.Length; i++) { 
                                mass.trail[i] = mass.position - masses[mass.followingIndex].mi.position;
                            }
                        }
                        else {
                            for(int i = 0; i < mass.trail.Length; i++) { 
                                mass.trail[i] = mass.position;
                            }
                        }
                    }
                    break;
                case "trail length":
                    mass.trail = new Vector2d[Math.Min(10000,int.Parse(change[1]))];
                    mass.trailOffset = 0;
                    if(mass.followingIndex != -1) {
                        for(int i = 0; i < mass.trail.Length; i++) { 
                            mass.trail[i] = mass.position - masses[mass.followingIndex].mi.position;
                        }
                    }
                    else {
                        for(int i = 0; i < mass.trail.Length; i++) { 
                            mass.trail[i] = mass.position;
                        }
                    }
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
            //Shared.ReadyWorkingCopy();
            foreach(Mass m in masses) {
                MassInfo mi = m.mi;
                if(mi.stationary) { continue; }
                
                foreach(Mass n in masses) {
                    if(mi.index == n.mi.index) { continue; } //Skip calculations if the same object
                    

                    double x = mi.position.X - n.mi.position.X;
                    double y = mi.position.Y - n.mi.position.Y;

                    double dist2 = Math.Pow(CalculateDistance(x, y), 3);
                    double force = (Constant.G * mi.mass * n.mi.mass) / dist2;

                    m.forces.Add(new Vector2d(x, y) * -force);
                }
                
            }
            
            foreach(Mass m in masses) {
                MassInfo mi = m.mi;
                if(!mi.hasTrail) { continue; }

                mi.velocity += (m.SumForces() / mi.mass) * deltaTime;
                
                m.ClearForces();

                mi.position += (mi.velocity * deltaTime);
                
                mi.trailCounter++;

                if(mi.trailCounter > mi.trailSkip) {
                    if(mi.followingIndex != -1) {
                        mi.trail[mi.trailOffset] = mi.position - masses[mi.followingIndex].mi.position;
                    }
                    else {
                        mi.trail[mi.trailOffset] = mi.position;
                    }
                    mi.trailOffset = (mi.trailOffset + 1) % mi.trail.Length;
                    mi.trailCounter = 0;
                }
            }
        
    }

    private double CalculateDistance(double x, double y)
    {
        return Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
    }
}

