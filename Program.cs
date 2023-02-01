using Orbit;
using Gtk;

static void StartApplication() {
    Application.Init();
    new App();
    Application.Run();
}

static void StartPhysics() {
    Mass sun   = new(1.989  * Math.Pow(10,24), new Vector2d(), new Vector2d(), name: "Sun", stationary: true);
    Mass earth = new(5.9736 * Math.Pow(10,18), new Vector2d(29.76, 0), new Vector2d(0, 149600000), name: "Earth", trailSteps: 400);
    Mass moon  = new(7.346  * Math.Pow(10,16), new Vector2d(1.022, 0) + earth.mi.velocity, new Vector2d(0, 385000) + earth.mi.position, name: "Moon", trailSteps: 200);
    
    PhysicsRunner fr = new();
    fr.AddMass(sun);
    fr.AddMass(earth);
    fr.AddMass(moon);

    while(Shared.Running) {
        fr.Update(Shared.deltaTime);
        Thread.Sleep((int)(1000 * (Shared.deltaTime / Shared.multiplier)));
    }
}

Thread thr1 = new Thread(StartPhysics);
thr1.Start();
StartApplication();
