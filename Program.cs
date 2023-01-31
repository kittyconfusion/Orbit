// See https://aka.ms/new-console-template for more information
using Orbit;
using Gtk;

static void StartApplication() {
    Application.Init();
    new App();
    Application.Run();

}

static void StartPhysics() {
    Mass sun   = new(1.989  * Math.Pow(10,24), new Vector2d(), new Vector2d(), stationary: true);
    Mass earth = new(5.9736 * Math.Pow(10,18), new Vector2d(29.76, 0),new Vector2d(0, 149600000), trailSteps: 200);
    Mass moon  = new(7.346  * Math.Pow(10,16), new Vector2d(1.022, 0) + earth.mi.velocity, new Vector2d(0, 385000) + earth.mi.position, trailSteps: 200, trailSkip: 0);
    
    PhysicsRunner fr = new();
    fr.AddMass(sun);
    fr.AddMass(earth);
    fr.AddMass(moon);

    double deltaTime = 3600 * 1; //How many seconds per update
    double multiplier = 86400 * 6; //How many seconds per real second

    while(Shared.Running) {
        //Console.WriteLine(moon.mi.velocity);
        fr.Update(deltaTime);
        Thread.Sleep((int)(1000 * (deltaTime / multiplier)));
        //Console.WriteLine(earth.mi.velocity + " " + moon.mi.velocity);
        //Task.Delay(100);
    }
}

Thread thr1 = new Thread(StartPhysics);
//Thread thr2 = new Thread(StartApplication);
thr1.Start();
//thr2.Start();
StartApplication();
