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
                case "positionkm":
                    mass.position = ParseVector2dFromString(change[1], mass.position);
                    break;
                case "positionAU":
                    mass.position = ParseVector2dFromString(change[1], mass.position.ToAstronomicalUnits()).ToKmUnits();
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
                case "remove mass":
                    int index = Convert.ToInt32(change[2]);
                    masses.RemoveAt(index);
                    Shared.RemoveMass(index);
                    break;
                case "remove all masses":
                    ClearAllMasses();
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
                case "trail skip":
                    mass.trailSkip = int.Parse(change[1]);
                    break;
                case "trail length":
                    if(mass.trail.Length == int.Parse(change[1])) {break;}
                    mass.trail = new Vector2d[int.Parse(change[1])];
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
                case "load preset":
                    ClearAllMasses();
                    LoadPreset(change[1]);
                    break;
            }
        }
    }
    internal void ClearAllMasses() {
        for(int i = Shared.massObjects - 1; i >= 0; i--) {
            masses.RemoveAt(i);
            Shared.RemoveMass(i);
        }
    }
    internal Vector2d ParseVector2dFromString(string s, Vector2d ifNull) {
        char[] toTrim = {' ', '(', ')'};
        s = s.Trim(toTrim);
        s.Split(',');
        try {
            return new Vector2d(Double.Parse(s.Split(',')[0] , System.Globalization.NumberStyles.Float),
                                -Double.Parse(s.Split(',')[1] , System.Globalization.NumberStyles.Float));
        }
        catch {
            return ifNull;
        }
        
    }
    internal void Update(double deltaTime)
    {
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

                if(mi.trailCounter >= mi.trailSkip) {
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
    internal void LoadPreset(string name) {
        switch (name){
            case "solar system":
                AddMass(new Mass (Constant.MassOfSun, new Vector2d(), new Vector2d(), name: "Sun", stationary: false));
                
                AddMass(new Mass (0.330 * Math.Pow(10,18), new Vector2d(47.4, 0),  new Vector2d(0, 57900000), name: "Mercury", trailSteps: 50, followingIndex: 0));
                AddMass(new Mass (4.87  * Math.Pow(10,18), new Vector2d(35.0, 0),  new Vector2d(0, 108200000), name: "Venus", trailSteps: 100, followingIndex: 0));
                AddMass(new Mass (5.9736* Math.Pow(10,18), new Vector2d(29.76, 0), new Vector2d(0, 149600000), name: "Earth", trailSkip: 16, followingIndex: 0));
                AddMass(new Mass (7.346 * Math.Pow(10,16), new Vector2d(1.022, 0) +new Vector2d(29.76, 0), new Vector2d(0, 385000) + new Vector2d(0, 149600000), name: "Moon", trailSteps: 50, trailSkip: 2, followingIndex: 3));
                AddMass(new Mass (0.642 * Math.Pow(10,18), new Vector2d(24.1, 0),  new Vector2d(0, 228000000), name: "Mars", trailSteps: 200, trailSkip: 32, followingIndex: 0));

                AddMass(new Mass (1898  * Math.Pow(10,18), new Vector2d(13.1, 0), new Vector2d(0, 778500000), name: "Jupiter", trailSteps: 200, trailSkip: 64, followingIndex: 0));
                AddMass(new Mass ( 568  * Math.Pow(10,18), new Vector2d( 9.7, 0), new Vector2d(0,1432000000), name: "Saturn",  trailSteps: 250, trailSkip: 128, followingIndex: 0));
                AddMass(new Mass ( 86.8 * Math.Pow(10,18), new Vector2d( 6.8, 0), new Vector2d(0,2867000000), name: "Uranus",  trailSteps: 275, trailSkip: 128, followingIndex: 0));
                AddMass(new Mass ( 102  * Math.Pow(10,18), new Vector2d( 5.4, 0), new Vector2d(0,4515000000), name: "Neptune", trailSteps: 300, trailSkip: 128, followingIndex: 0));
                break;

            case "hulse-taylor binary":
                AddMass(new Mass(Constant.MassOfSun * 1.387, new Vector2d(0, 110), new Vector2d(7466000/2, 0), name: "Neutron Star", trailSteps: 800, trailSkip: 2));
                AddMass(new Mass(Constant.MassOfSun * 1.441, new Vector2d(0, -110), new Vector2d(-746600/2, 0), name: "Pulsar", trailSteps: 800, trailSkip: 2));
                break;
        }
    }
}

