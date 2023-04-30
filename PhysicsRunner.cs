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
        
    }
    internal void ProcessDataChanges() {
        //Change[0] is type, change[1] is value, change[2] is index to change
        string[] change;
        while (Shared.changesToMake.TryPop(out change!)) {
            MassInfo mass = new();
            if(Shared.ignoreNextUpdates > 0) { continue; }
            if(change[2] != "-1") {
                mass = allMasses[Int32.Parse(change[2])].mi;
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
                case "massGg":
                    if(double.TryParse(change[1], System.Globalization.NumberStyles.Float, null, out double newGgMass)) {
                        mass.mass = newGgMass;
                    }
                    break;
                case "masskg":
                    if(double.TryParse(change[1], System.Globalization.NumberStyles.Float, null, out double newkgMass)) {
                        mass.mass = newkgMass / Math.Pow(10,6);
                    }
                    break;
                case "massEarth":
                    if(double.TryParse(change[1], System.Globalization.NumberStyles.Float, null, out double newEarthMass)) {
                        mass.mass = newEarthMass * Constant.MassOfEarth;
                    }
                    break;
                case "massSolar":
                    if(double.TryParse(change[1], System.Globalization.NumberStyles.Float, null, out double newSolarMass)) {
                        mass.mass = newSolarMass * Constant.MassOfSun;
                    }
                    break;
                case "new mass":
                    Random r = new();
                    AddMass(new Mass(Constant.MassOfEarth, new Vector2d(), new Vector2d(r.NextDouble(), r.NextDouble()) * Constant.AUtokm(1)));
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
                    if(mass.followingIndex != Convert.ToInt32(change[1])) {
                        mass.followingIndex = Convert.ToInt32(change[1]);
                        ResetTrail(mass);
                    }
                    break;
                case "trail skip":
                    mass.trail = new Vector2d[Math.Max(1,(int)((mass.trail.Length / mass.trailQuality) * double.Parse(change[1])))];
                    mass.trailQuality = double.Parse(change[1]);
                    ResetTrail(mass);
                    break;
                case "trail length":
                    if(mass.trail.Length * mass.trailQuality == int.Parse(change[1])) {break;}
                    mass.trail = new Vector2d[Math.Max(1,(int)(int.Parse(change[1]) * mass.trailQuality))];
                    ResetTrail(mass);
                    break;
                case "load preset":
                    ClearAllMasses();
                    LoadPreset(change[1]);
                    break;
                case "stationary":
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

            mi.trailCounter++;

            if(mi.trailCounter >= mi.trailQuality) {
                if(mi.followingIndex != -1) {
                    mi.trail[mi.trailOffset] = mi.position - allMasses[mi.followingIndex].mi.position;
                }
                else {
                    mi.trail[mi.trailOffset] = mi.position;
                }
                mi.trailOffset = (mi.trailOffset + 1) % mi.trail.Length;
                mi.trailCounter -= 60f / mi.trailQuality;
            }
        }
    }
    private double CalculateDistance(double x, double y)
    {
        return Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
    }
    internal void LoadPreset(string name) {
        switch (name){
            case "proxima centauri":
                AddMass(new Mass(0.1221*Constant.MassOfSun, new Vector2d(0, 0), new Vector2d(0, 0), name: "Proxima Centauri", stationary: true));
                AddMass(new Mass(0.26*Constant.MassOfEarth, new Vector2d(58.87, 0), new Vector2d(0, Constant.AUtokm(0.030004)), name: "Proxima Centauri d", trailQuality: 14));
                AddMass(new Mass(1.07*Constant.MassOfEarth, new Vector2d(42.35, 0), new Vector2d(0, Constant.AUtokm(0.05386)), name: "Proxima Centauri b"));

                Shared.resolutionMode = 3;
                break;
            case "inner solar system":
                AddMass(new Mass (Constant.MassOfSun, new Vector2d(), new Vector2d(), name: "Sun", stationary: true, satellite: false));
                
                Mass InnerMercury = new(0.33010 * Math.Pow(10,18), new Vector2d(38.86, 0),  new Vector2d(0, 69818000), name: "Mercury", trailSeconds: 12, trailQuality: 16, followingIndex: 0, precisionPriorityLimit: Constant.WEEKS * 24 / 60);
                FixMassAngle(InnerMercury, 255, new Vector2d(0,0));
                AddMass(InnerMercury);
                AddMass(new Mass (4.87  * Math.Pow(10,18), new Vector2d(35.0, 0),  new Vector2d(0, 108200000), name: "Venus", trailSeconds: 18, trailQuality: 14, followingIndex: 0, precisionPriorityLimit: Constant.YEARS / 60));
                AddMass(new Mass (Constant.MassOfEarth   , new Vector2d(29.76, 0), new Vector2d(0, 149600000), name: "Earth", trailSeconds: 24, trailQuality: 12, followingIndex: 0, precisionPriorityLimit: Constant.YEARS / 60 * 2));
                AddMinorMass(new Mass (7.346 * Math.Pow(10,16), new Vector2d(1.022, 0) + new Vector2d(29.76, 0), new Vector2d(0, 385000) + new Vector2d(0, 149600000), name: "Moon", trailSeconds: 12, trailQuality: 10, followingIndex: 3, precisionPriorityLimit: Constant.DAYS));
                AddMass(new Mass (0.642 * Math.Pow(10,18), new Vector2d(24.1, 0),  new Vector2d(0, 228000000), name: "Mars", trailSeconds: 36, trailQuality: 10, followingIndex: 0, precisionPriorityLimit: Constant.YEARS / 60 * 4));
                AddMinorMass(new Mass(1.0659 * Math.Pow(10,10), new Vector2d(2.138, 0) + new Vector2d(24.1, 0), new Vector2d(0, 9376) + new Vector2d(0, 228000000), name: "Phobos", trailSeconds: 6, trailQuality: 12, followingIndex: 5, precisionPriorityLimit: Constant.MINUTES * 20));
                AddMinorMass(new Mass(1.4762 * Math.Pow(10,9),  new Vector2d(1.3513, 0)+ new Vector2d(24.1, 0), new Vector2d(0, 23463.2)+ new Vector2d(0, 228000000), name: "Deimos", trailSeconds: 12, trailQuality: 12, followingIndex: 5, precisionPriorityLimit: Constant.HOURS));

                Shared.resolutionMode = 3;
                break;

            case "solar system":
                AddMass(new Mass (Constant.MassOfSun, new Vector2d(), new Vector2d(), name: "Sun", stationary: true, satellite: false));
                
                Mass MercuryMass = new(0.33010 * Math.Pow(10,18), new Vector2d(38.86, 0),  new Vector2d(0, 69818000), name: "Mercury", trailSeconds: 12, trailQuality: 16, followingIndex: 0, precisionPriorityLimit: Constant.WEEKS * 24 / 60);
                FixMassAngle(MercuryMass, 255, new Vector2d(0,0));
                AddMass(MercuryMass);
                AddMass(new Mass (4.87  * Math.Pow(10,18), new Vector2d(35.0, 0),  new Vector2d(0, 108200000), name: "Venus", trailSeconds: 18, trailQuality: 14, followingIndex: 0, precisionPriorityLimit: Constant.YEARS / 60));
                AddMass(new Mass (Constant.MassOfEarth   , new Vector2d(29.76, 0), new Vector2d(0, 149600000), name: "Earth", trailSeconds: 24, trailQuality: 12, followingIndex: 0, precisionPriorityLimit: Constant.YEARS / 60 * 2));
                //AddMinorMass(new Mass (7.346 * Math.Pow(10,16), new Vector2d(1.022, 0) +new Vector2d(29.76, 0), new Vector2d(0, 385000) + new Vector2d(0, 149600000), name: "Moon", trailSeconds: 12, trailQuality: 10, followingIndex: 3, precisionPriorityLimit: Constant.DAYS));
                AddMass(new Mass (0.642 * Math.Pow(10,18), new Vector2d(24.1, 0),  new Vector2d(0, 228000000), name: "Mars", trailSeconds: 36, trailQuality: 10, followingIndex: 0, precisionPriorityLimit: Constant.YEARS / 60 * 4));

                AddMass(new Mass (1898  * Math.Pow(10,18), new Vector2d(13.1, 0), new Vector2d(0, 778500000), name: "Jupiter", trailSeconds: 90, trailQuality: 6, followingIndex: 0));
                AddMass(new Mass ( 568  * Math.Pow(10,18), new Vector2d( 9.7, 0), new Vector2d(0,1432000000), name: "Saturn",  trailSeconds: 102, trailQuality: 5, followingIndex: 0));
                AddMass(new Mass ( 86.8 * Math.Pow(10,18), new Vector2d( 6.8, 0), new Vector2d(0,2867000000), name: "Uranus",  trailSeconds: 108, trailQuality: 4, followingIndex: 0));
                AddMass(new Mass ( 102  * Math.Pow(10,18), new Vector2d( 5.4, 0), new Vector2d(0,4515000000), name: "Neptune", trailSeconds: 108, trailQuality: 4, followingIndex: 0));
                
                //AddMinorMass(new Mass(Constant.kgToGg(420000), new Vector2d(0, 7.66), new Vector2d(0 + 4212.8, 149600000 + 0), name: "International Space Station", hasTrail: false, precisionPriorityLimit: Constant.HOURS, satellite: true));
                //Mass Bennu = new Mass(Constant.kgToGg(7.329 * Math.Pow(10,10)), new Vector2d(22.828, 0), new Vector2d(0, Constant.AUtokm(1.3559)), name: "Bennu", trailSeconds: 6);
                //FixMassAngle(Bennu, 265, new Vector2d(0,0));
                //AddMinorMass(Bennu);
                Shared.resolutionMode = 2;
                
                break;

            case "hulse-taylor binary":
                AddMass(new Mass(Constant.MassOfSun * 1.387, new Vector2d(0, 110), new Vector2d(7466000/2, 0), name: "Neutron Star", trailSeconds: 12, trailQuality: 16));
                AddMass(new Mass(Constant.MassOfSun * 1.441, new Vector2d(0, -110), new Vector2d(-746600/2, 0), name: "Pulsar", trailSeconds: 12, trailQuality: 16));
                
                Shared.resolutionMode = 3;
                break;
        }
        InitializeMasses();
    }
}

