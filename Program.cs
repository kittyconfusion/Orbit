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
    Mass earth = new(5.9736 * Math.Pow(10,18), new Vector2d(29.76, 0), new Vector2d(0, 149600000), name: "Earth", trailSkip: 10);
    Mass moon  = new(7.346  * Math.Pow(10,16), new Vector2d(1.022, 0) + earth.mi.velocity, new Vector2d(0, 385000) + earth.mi.position, name: "Moon", trailSteps: 50);
    
    Mass mercury = new(0.330 * Math.Pow(10,18), new Vector2d(47.4, 0), new Vector2d(0, 57900000), name: "Mercury", trailSteps: 50);
    Mass venus   = new(4.87  * Math.Pow(10,18), new Vector2d(35.0, 0), new Vector2d(0, 108200000), name: "Venus", trailSteps: 100);
    Mass mars    = new(0.642 * Math.Pow(10,18), new Vector2d(24.1, 0), new Vector2d(0, 228000000), name: "Mars");
    //Mass test = new(1 * Math.Pow(10,18), new Vector2d(20,0), new Vector2d(0,50000000));

    PhysicsRunner fr = new();
    //fr.AddMass(test);
    fr.AddMass(sun);
    fr.AddMass(mercury);
    fr.AddMass(venus);
    fr.AddMass(earth);
    fr.AddMass(moon);
    fr.AddMass(mars);

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
