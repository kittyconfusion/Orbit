using Orbit;
using Gtk;
using System.Diagnostics;

static void StartApplication() {
    Application.Init();
    new App();
    Application.Run();
}

static void StartPhysics() {
    Mass sun   = new(1.989  * Math.Pow(10,24), new Vector2d(), new Vector2d(), name: "Sun", stationary: false);
    Mass earth = new(5.9736 * Math.Pow(10,18), new Vector2d(29.76, 0), new Vector2d(0, 149600000), name: "Earth", trailSkip: 0);
    Mass moon  = new(7.346  * Math.Pow(10,16), new Vector2d(1.022, 0) + earth.mi.velocity, new Vector2d(0, 385000) + earth.mi.position, name: "Moon", trailSteps: 50);
    
    Mass mercury = new(0.330 * Math.Pow(10,18), new Vector2d(47.4, 0), new Vector2d(0, 57900000), name: "Mercury", trailSteps: 50);
    Mass venus   = new(4.87  * Math.Pow(10,18), new Vector2d(35.0, 0), new Vector2d(0, 108200000), name: "Venus", trailSteps: 100);
    Mass mars    = new(0.642 * Math.Pow(10,18), new Vector2d(24.1, 0), new Vector2d(0, 228000000), name: "Mars", trailSteps: 200, trailSkip: 25);

    Mass jupiter = new(1898  * Math.Pow(10,18), new Vector2d(13.1, 0), new Vector2d(0, 778500000), name: "Jupiter", trailSteps: 225, trailSkip: 70);
    Mass saturn  = new( 568  * Math.Pow(10,18), new Vector2d( 9.7, 0), new Vector2d(0,1432000000), name: "Saturn",  trailSteps: 250, trailSkip: 140);
    Mass uranus  = new( 86.8 * Math.Pow(10,18), new Vector2d( 6.8, 0), new Vector2d(0,2867000000), name: "Uranus",  trailSteps: 275, trailSkip: 200);
    Mass neptune = new( 102  * Math.Pow(10,18), new Vector2d( 5.4, 0), new Vector2d(0,4515000000), name: "Neptune", trailSteps: 300, trailSkip: 300);

    PhysicsRunner fr = new();
    
    fr.AddMass(sun);
    fr.AddMass(mercury); 
    fr.AddMass(venus);
    fr.AddMass(earth);
    fr.AddMass(moon);
    fr.AddMass(mars);
    fr.AddMass(jupiter);
    fr.AddMass(saturn);
    fr.AddMass(uranus);
    fr.AddMass(neptune);

    Stopwatch time = Stopwatch.StartNew();
    double wantedDelta, timeTook = 0;

    while(Shared.Running) {
        wantedDelta = (1000 * (Shared.deltaTime / Shared.multiplier));
        
        lock(Shared.DataLock) {
            fr.ProcessDataChanges();
            while(timeTook > wantedDelta) {
                fr.Update(Shared.deltaTime);
                timeTook -= wantedDelta;
            }   
        }

        Thread.Sleep(10);
        if(!Shared.Paused) {
            timeTook += time.ElapsedMilliseconds;
        }
        
        time.Restart();
    }
}

Thread thr1 = new Thread(StartPhysics);
thr1.Start();
StartApplication();
