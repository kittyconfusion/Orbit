namespace Orbit;
internal class PhysicsRunner {
    List<Mass> masses;
    List<Mass> minorMasses;
    List<Mass> allMasses;
    internal PhysicsRunner() {
        masses = new();
        minorMasses = new();
        allMasses = new();
    }
    internal void AddMass(Mass m) {
        masses.Add(m);
        allMasses.Add(m);
        Shared.AddMass(m.mi);
    }
    internal void AddMinorMass(Mass m)
    {
        minorMasses.Add(m);
        allMasses.Add(m);
        Shared.AddMass(m.mi);
    }
    internal void InitializeMasses() {
        foreach(Mass m in minorMasses) {
            //Find the object which it orbits
            if(m.mi.satellite) {
                m.mi.orbitingBodyIndex = FindBestOrbitingBody(m);
                ResetTrail(m.mi);
            }
        }
    }
    internal int FindBestOrbitingBody(Mass m) {
        int bestMassIndex = -1;
        double bestMassDistance = -1;
        foreach(Mass n in masses) {
            if(m.mi.index == n.mi.index) { continue; }
            double hillSphereRadius = n.mi.semiMajorAxisLength * Math.Pow(n.mi.mass / (3 * Constant.MassOfSun), 1f / 3);

            double distance = CalculateDistance(m.mi.position.X - n.mi.position.X, m.mi.position.Y - n.mi.position.Y);

            if(distance <= hillSphereRadius) {
                if(bestMassIndex == -1) {
                    bestMassIndex = n.mi.index;
                    bestMassDistance = distance;
                }
                else if(distance < bestMassDistance) {
                    bestMassIndex = n.mi.index;
                    bestMassDistance = distance;
                }
            }
        }
        return bestMassIndex;
    }
    internal void FixMassAngle(Mass m, int aphelionOffsetAngle, Vector2d orbitObjectPos) {
        aphelionOffsetAngle = 360 - aphelionOffsetAngle;
        double rad = (Math.PI * aphelionOffsetAngle) / 180;;
        m.mi.position = new Vector2d(Math.Cos(rad), Math.Sin(rad)) * (m.mi.position - orbitObjectPos).Magnitude();
        m.mi.velocity = new Vector2d(Math.Sin(rad), -Math.Cos(rad)) * m.mi.velocity.Magnitude();
        ResetTrail(m.mi);
    }
    internal void ProcessDataChanges() {
        //Change[0] is type, change[1] is value, change[2] is index to change
        string[] change;
        while (Shared.changesToMake.TryPop(out change!)) {
            MassInfo mass = new();
            
            if(change[2] != "-1") {
                mass = allMasses[Int32.Parse(change[2])].mi;
            }
            
            switch(change[0]) {
                case "positionkm":
                    if(Shared.ignoreNextUpdates > 0) { continue; }
                    mass.position = ParseVector2dFromString(change[1], mass.position);
                    break;
                case "positionAU":
                    if(Shared.ignoreNextUpdates > 0) { continue; }
                    mass.position = ParseVector2dFromString(change[1], mass.position.ToAstronomicalUnits()).ToKmUnits();
                    break;
                case "velocity":
                    if(Shared.ignoreNextUpdates > 0) { continue; }
                    mass.velocity = ParseVector2dFromString(change[1], mass.velocity);
                    break;
                case "massGg":
                    if(Shared.ignoreNextUpdates > 0) { continue; }
                    if(double.TryParse(change[1], System.Globalization.NumberStyles.Float, null, out double newGgMass)) {
                        mass.mass = newGgMass;
                    }
                    break;
                case "masskg":
                    if(Shared.ignoreNextUpdates > 0) { continue; }
                    if(double.TryParse(change[1], System.Globalization.NumberStyles.Float, null, out double newkgMass)) {
                        mass.mass = newkgMass / Math.Pow(10,6);
                    }
                    break;
                case "massEarth":
                    if(Shared.ignoreNextUpdates > 0) { continue; }
                    if(double.TryParse(change[1], System.Globalization.NumberStyles.Float, null, out double newEarthMass)) {
                        mass.mass = newEarthMass * Constant.MassOfEarth;
                    }
                    break;
                case "massSolar":
                    if(Shared.ignoreNextUpdates > 0) { continue; }
                    if(double.TryParse(change[1], System.Globalization.NumberStyles.Float, null, out double newSolarMass)) {
                        mass.mass = newSolarMass * Constant.MassOfSun;
                    }
                    break;
                case "new mass":
                    Random r = new();
                    AddMass(new Mass(Constant.MassOfEarth, new Vector2d(), (new Vector2d(r.NextDouble(), r.NextDouble()) * 2 - 1) * Constant.AUtokm(1)));
                    Shared.needToRefresh = true;
                    break;
                case "remove mass":
                    int index = Convert.ToInt32(change[2]);
                    Mass toRemove = allMasses[index];
                    masses.Remove(toRemove);
                    minorMasses.Remove(toRemove);
                    allMasses.RemoveAt(index);
                    Shared.RemoveMass(index);
                    InitializeMasses();
                    break;
                case "remove all masses":
                    ClearAllMasses();
                    break;
                case "trail follow":
                    if(Shared.ignoreNextUpdates > 0) { continue; }
                    if(mass.followingIndex != Convert.ToInt32(change[1])) {
                        mass.followingIndex = Convert.ToInt32(change[1]);
                        ResetTrail(mass);
                    }
                    break;
                case "trail quality":
                    if(Shared.ignoreNextUpdates > 0) { continue; }
                    //mass.trail = new Vector2d[Math.Max(1,(int)((mass.trail.Length / mass.trailQuality) * double.Parse(change[1])))];
                    mass.trailQuality = Math.Pow(10, 8 - double.Parse(change[1])) * 60;
                    break;
                case "trail length":
                    if(Shared.ignoreNextUpdates > 0) { continue; }
                    if(mass.trail.Length == int.Parse(change[1])) {break;}
                    mass.trail = new Vector2d[int.Parse(change[1]) * 100];
                    ResetTrail(mass);
                    break;
                case "has trail":
                    if(Shared.ignoreNextUpdates > 0) { continue; }
                    mass.hasTrail = bool.Parse(change[1]);
                    if(mass.hasTrail) { ResetTrail(mass); }
                    break;
                case "load preset":
                    ClearAllMasses();
                    LoadPreset(change[1]);
                    break;
                case "stationary":
                    if(Shared.ignoreNextUpdates > 0) { continue; }
                    mass.stationary = bool.Parse(change[1]);
                    break;
                case "save":
                    //Set if each mass is minor or major
                    foreach(Mass m in allMasses) {
                        m.mi.minorMass = minorMasses.Contains(m);
                    }
                    MassJsonHelper.SaveMassesToFile(allMasses, change[1]);
                    break;
                case "load":
                    List<Mass>? fileMasses = MassJsonHelper.LoadMassesFromFile(change[1]);
                    if(fileMasses != null) {
                        ClearAllMasses();
                        foreach(Mass m in fileMasses) {
                            if(m.mi.minorMass) {
                                AddMinorMass(m);
                            }
                            else {
                                AddMass(m);
                            }
                        }
                        
                        Shared.needToRefresh = true;
                    }
                    break;
                
            }
        }
    }
    internal void ResetTrail(MassInfo mass) {
        mass.trailOffset = 0;

        if(mass.followingIndex != -1) {
            for(int i = 0; i < mass.trail.Length; i++) { 
                mass.trail[i] = mass.position - allMasses[mass.followingIndex].mi.position;
            }
        }
        else {
            for(int i = 0; i < mass.trail.Length; i++) { 
                mass.trail[i] = mass.position;
            }
        }
    }
    internal void ClearAllMasses() {
        for(int i = Shared.massObjects - 1; i >= 0; i--) {
            allMasses.RemoveAt(i);
            Shared.RemoveMass(i);
        }
        masses.Clear();
        minorMasses.Clear();
        Shared.ResetDrawingMasses();
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
    internal void Update(double deltaTime) {
        SatellitePauseUnpause(deltaTime);
        UpdateForces(deltaTime);

        //Update positions
        UpdatePositions(deltaTime);
    }
    internal void SatellitePauseUnpause(double deltaTime) {
        foreach(Mass m in allMasses) {
            MassInfo mi = m.mi;
            if(!mi.satellite || mi.precisionPriorityLimit == -1) { continue; }
            
            //Pause the object if need be
            if(mi.currentlyUpdatingPhysics && deltaTime > mi.precisionPriorityLimit) {
                mi.currentlyUpdatingPhysics = false;
                //Save the current position/velocity to be relative to the orbited object if needed
                if(mi.orbitingBodyIndex > -1 && mi.orbitingBodyIndex > -1) {
                    mi.position -= allMasses[mi.orbitingBodyIndex].mi.position;
                    mi.velocity -= allMasses[mi.orbitingBodyIndex].mi.velocity;
                }
            }
            //Unpause the object if need be
            else if(!mi.currentlyUpdatingPhysics && deltaTime <= mi.precisionPriorityLimit) {
                mi.currentlyUpdatingPhysics = true;
                //Place back the object in global coordinates with appropriate
                //position and velocity relative to orbited object
                if(mi.orbitingBodyIndex > -1 && mi.orbitingBodyIndex > -1) {
                    mi.position += allMasses[mi.orbitingBodyIndex].mi.position;
                    mi.velocity += allMasses[mi.orbitingBodyIndex].mi.velocity;
                }
                //Reset the object's trail upon restart
                if(mi.followingIndex == -1 || mi.orbitingBodyIndex == -1) {
                    Array.Fill(mi.trail, mi.position);
                }
                else {
                    Array.Fill(mi.trail, mi.position - allMasses[mi.orbitingBodyIndex].mi.position);
                }
            }
        }
    }
    internal void UpdateForces(double deltaTime) {
        foreach(Mass m in allMasses) {
            m.ClearForces();
            MassInfo mi = m.mi;
            if(mi.stationary || !mi.currentlyUpdatingPhysics) { continue; }
            
            foreach(Mass n in masses) {
                if(mi.index == n.mi.index) { continue; } //Skip calculations if the same object
                
                double x = mi.position.X - n.mi.position.X;
                double y = mi.position.Y - n.mi.position.Y;

                double dist2 = Math.Pow(CalculateDistance(x, y), 3);
                double force = (Constant.G * mi.mass * n.mi.mass) / dist2;

                mi.forces.Add(new Vector2d(x, y) * -force);
            }
        }
    }
    internal void UpdatePositions(double deltaTime) {
        foreach(Mass m in allMasses) {
            MassInfo mi = m.mi;
            if(mi.stationary || !mi.currentlyUpdatingPhysics) { continue; }

            mi.velocity += (m.SumForces() / mi.mass) * deltaTime;

            mi.position += (mi.velocity * deltaTime);
            
            if(!mi.hasTrail) { continue; }

            mi.trailCounter += deltaTime;

            while(mi.trailCounter >= mi.trailQuality) {
                if(mi.followingIndex != -1) {
                    mi.trail[mi.trailOffset] = mi.position - allMasses[mi.followingIndex].mi.position;
                }
                else {
                    mi.trail[mi.trailOffset] = mi.position;
                }
                mi.trailOffset = (mi.trailOffset + 1) % mi.trail.Length;
                mi.trailCounter -= mi.trailQuality;
            }
        }
    }
    private double CalculateDistance(double x, double y)
    {
        return Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
    }
    internal void LoadPreset(string name) {
        switch (name){
            case "gliese 876":
                AddMass(new Mass(0.346 * Constant.MassOfSun,     new Vector2d(0, 0), new Vector2d(0, 0), stationary: true, name: "Gliese 876"));
                AddMass(new Mass(7.55  * Constant.MassOfEarth,   new Vector2d(115.8, 0), new Vector2d(0, Constant.AUtokm(0.023083)), name: "d", trailLength: 3, trailQuality: 7));
                AddMass(new Mass(0.8357*Constant.MassOfJupiter,  new Vector2d(37.8 , 0), new Vector2d(0, Constant.AUtokm(0.17102) ), name: "c", trailLength: 5, trailQuality: 6));
                AddMass(new Mass(2.660 * Constant.MassOfJupiter, new Vector2d(37.68, 0), new Vector2d(0, Constant.AUtokm(0.22573) ), name: "b", trailLength: 1, trailQuality: 5));
                AddMass(new Mass(15.8  * Constant.MassOfEarth,   new Vector2d(29.85, 0), new Vector2d(0, Constant.AUtokm(0.3606)  ), name: "e", trailLength: 2, trailQuality: 5));
                
                FixMassAngle(allMasses[2], 90, new Vector2d(0,0));
                FixMassAngle(allMasses[3], 180, new Vector2d(0,0));
                FixMassAngle(allMasses[4], 270, new Vector2d(0,0));
                Shared.resolutionMode = 3;
                break;
                
            case "proxima centauri":
                AddMass(new Mass(0.1221*Constant.MassOfSun, new Vector2d(0, 0), new Vector2d(0, 0), name: "Proxima Centauri", stationary: true));
                AddMass(new Mass(0.26*Constant.MassOfEarth, new Vector2d(58.87, 0), new Vector2d(0, Constant.AUtokm(0.030004)), name: "d", trailLength: 8, trailQuality: 7));
                AddMass(new Mass(1.07*Constant.MassOfEarth, new Vector2d(42.35, 0), new Vector2d(0, Constant.AUtokm(0.05386)),  name: "b", trailLength: 2, trailQuality: 6));

                Shared.resolutionMode = 3;
                break;

            case "earth":
                AddMass(new Mass (Constant.MassOfEarth   , new Vector2d(29.76, 0), new Vector2d(0, 149600000), name: "Earth", trailLength: 24, trailQuality: 12, followingIndex: 0, precisionPriorityLimit: Constant.YEARS / 60 * 2));
                AddMinorMass(new Mass (7.346 * Math.Pow(10,16), new Vector2d(1.022, 0) + new Vector2d(29.76, 0), new Vector2d(0, 385000) + new Vector2d(0, 149600000), name: "Moon", trailLength: 12, trailQuality: 10, followingIndex: 0, precisionPriorityLimit: Constant.DAYS));
                AddMinorMass(new Mass(Constant.kgToGg(420000), new Vector2d(29.76 + 0, 0 + 7.66), new Vector2d(0 + 4212.8, 149600000 + 0), name: "International Space Station", trailLength: 30, trailQuality: 50, precisionPriorityLimit: Constant.MINUTES * 2, satellite: true, followingIndex: 0));
                break;

            case "inner solar system":
                AddMass(new Mass (Constant.MassOfSun, new Vector2d(), new Vector2d(), name: "Sun", stationary: true, satellite: false));
                
                Mass InnerMercury = new(0.33010 * Math.Pow(10,18), new Vector2d(38.86, 0),  new Vector2d(0, 69818000), name: "Mercury", trailLength: 2, trailQuality: 5, followingIndex: 0, precisionPriorityLimit: Constant.WEEKS * 24 / 60);
                FixMassAngle(InnerMercury, 255, new Vector2d(0,0));
                AddMass(InnerMercury);
                AddMass(new Mass (4.87  * Math.Pow(10,18), new Vector2d(35.0, 0),  new Vector2d(0, 108200000), name: "Venus", trailLength: 7, trailQuality: 5, followingIndex: 0, precisionPriorityLimit: Constant.YEARS / 60));
                AddMass(new Mass (Constant.MassOfEarth   , new Vector2d(29.76, 0), new Vector2d(0, 149600000), name: "Earth", trailLength: 6, trailQuality: 5, followingIndex: 0, precisionPriorityLimit: Constant.YEARS / 60 * 2));
                AddMinorMass(new Mass (7.346 * Math.Pow(10,16), new Vector2d(1.022, 0) + new Vector2d(29.76, 0), new Vector2d(0, 385000) + new Vector2d(0, 149600000), name: "Moon", trailLength: 4, trailQuality: 6, followingIndex: 3, precisionPriorityLimit: Constant.DAYS));
                AddMass(new Mass (0.642 * Math.Pow(10,18), new Vector2d(24.1, 0),  new Vector2d(0, 228000000), name: "Mars", trailLength: 10, trailQuality: 5, followingIndex: 0, precisionPriorityLimit: Constant.YEARS / 60 * 4));
                AddMinorMass(new Mass(1.0659 * Math.Pow(10,10), new Vector2d(2.138, 0) + new Vector2d(24.1, 0), new Vector2d(0, 9376) + new Vector2d(0, 228000000), name: "Phobos", trailLength: 5, trailQuality: 8, followingIndex: 5, precisionPriorityLimit: Constant.MINUTES * 20));
                AddMinorMass(new Mass(1.4762 * Math.Pow(10,9),  new Vector2d(1.3513, 0)+ new Vector2d(24.1, 0), new Vector2d(0, 23463.2)+ new Vector2d(0, 228000000), name: "Deimos", trailLength: 2, trailQuality: 7, followingIndex: 5, precisionPriorityLimit: Constant.HOURS));
                
                Mass Bennu = new Mass(Constant.kgToGg(7.329 * Math.Pow(10,10)), new Vector2d(22.828, 0), new Vector2d(0, Constant.AUtokm(1.3559)), name: "Bennu", trailLength: 2, trailQuality: 5);
                Bennu.mi.hasTrail = false;
                FixMassAngle(Bennu, 265, new Vector2d(0,0));
                AddMinorMass(Bennu);
                Shared.resolutionMode = 3;
                break;

            case "solar system":
                AddMass(new Mass (Constant.MassOfSun, new Vector2d(), new Vector2d(), name: "Sun", stationary: true, satellite: false));
                
                Mass MercuryMass = new(0.33010 * Math.Pow(10,18), new Vector2d(38.86, 0),  new Vector2d(0, 69818000), name: "Mercury", trailLength: 2, trailQuality: 5, followingIndex: 0, precisionPriorityLimit: Constant.WEEKS * 24 / 60);
                FixMassAngle(MercuryMass, 255, new Vector2d(0,0));
                AddMass(MercuryMass);
                AddMass(new Mass (4.87  * Math.Pow(10,18), new Vector2d(35.0, 0),  new Vector2d(0, 108200000), name: "Venus", trailLength: 7, trailQuality: 5, followingIndex: 0, precisionPriorityLimit: Constant.YEARS / 60));
                AddMass(new Mass (Constant.MassOfEarth   , new Vector2d(29.76, 0), new Vector2d(0, 149600000), name: "Earth", trailLength: 6, trailQuality: 5, followingIndex: 0, precisionPriorityLimit: Constant.YEARS / 60 * 2));
                AddMinorMass(new Mass (7.346 * Math.Pow(10,16), new Vector2d(1.022, 0) +new Vector2d(29.76, 0), new Vector2d(0, 385000) + new Vector2d(0, 149600000), name: "Moon", trailLength: 4, trailQuality: 6, followingIndex: 3, precisionPriorityLimit: Constant.DAYS));
                AddMass(new Mass (0.642 * Math.Pow(10,18), new Vector2d(24.1, 0),  new Vector2d(0, 228000000), name: "Mars", trailLength: 10, trailQuality: 5, followingIndex: 0, precisionPriorityLimit: Constant.YEARS / 60 * 4));

                AddMass(new Mass (1898  * Math.Pow(10,18), new Vector2d(13.1, 0), new Vector2d(0, 778500000), name: "Jupiter", trailLength: 6, trailQuality: 4, followingIndex: 0, precisionPriorityLimit: Constant.DECADES * 3));
                AddMass(new Mass ( 568  * Math.Pow(10,18), new Vector2d( 9.7, 0), new Vector2d(0,1432000000), name: "Saturn",  trailLength: 6, trailQuality: 4, followingIndex: 0));
                AddMass(new Mass ( 86.8 * Math.Pow(10,18), new Vector2d( 6.8, 0), new Vector2d(0,2867000000), name: "Uranus",  trailLength: 6, trailQuality: 3, followingIndex: 0));
                AddMass(new Mass ( 102  * Math.Pow(10,18), new Vector2d( 5.4, 0), new Vector2d(0,4515000000), name: "Neptune", trailLength: 6, trailQuality: 3, followingIndex: 0));
                
                //
                //Mass Bennu = new Mass(Constant.kgToGg(7.329 * Math.Pow(10,10)), new Vector2d(22.828, 0), new Vector2d(0, Constant.AUtokm(1.3559)), name: "Bennu", trailSeconds: 6);
                //FixMassAngle(Bennu, 265, new Vector2d(0,0));
                //AddMinorMass(Bennu);
                Shared.resolutionMode = 2;
                
                break;

            case "hulse-taylor binary":
                AddMass(new Mass(Constant.MassOfSun * 1.387, new Vector2d(0, 110), new Vector2d(3153600/2, 0), name: "Neutron Star", trailLength: 6, trailQuality: 8));
                AddMass(new Mass(Constant.MassOfSun * 1.441, new Vector2d(0, -110), new Vector2d(-3153600/2, 0), name: "Pulsar", trailLength: 6, trailQuality: 8));
                
                Shared.resolutionMode = 3;
                break;
        }
        InitializeMasses();
        Shared.needToRefresh = true;
    }
}

