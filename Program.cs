// See https://aka.ms/new-console-template for more information
using Orbit;
using Gtk;



//gtk invoke
/*
StationaryMass sun = new(10000000, new Vector2d(0,0));
Planet planet = new(100000,new Vector2d(0, 2 * Math.PI), new Vector2d(1, 0), sun);
Console.WriteLine("start");
using (StreamWriter writer = new StreamWriter("xy.txt"))  
{  
    for(int i = 0; i < 500000; i++ ) {
        planet.Update(0.005);
        writer.WriteLine(planet.mass.position.X + " " + planet.mass.position.Y);
        //Console.WriteLine(planet.mass.velocity);
    }
}
Console.WriteLine("end");
*/



static void StartApplication() {
    Application.Init();
    new App();
    Application.Run();

}

static void StartPhysics() {
    Mass earth = new(5.9736 * Math.Pow(10,18), new Vector2d(),new Vector2d(), stationary: true);
    Mass moon  = new(7.346  * Math.Pow(10,16), new Vector2d(1.022, 0), new Vector2d(0, 385000), trailSteps: 1200, trailSkip: 0);
    
    PhysicsRunner fr = new();
    fr.AddMass(earth);
    fr.AddMass(moon);

    double deltaTime = 3600; //How many seconds per update
    double multiplier = 86400 * 2; //How many seconds per real second

    while(Shared.Running) {
        //Console.WriteLine(moon.mi.velocity);
        fr.Update(deltaTime);
        Thread.Sleep((int)(1000 * (deltaTime / multiplier)));
        
        //Task.Delay(100);
    }
}

Thread thr1 = new Thread(StartPhysics);
Thread thr2 = new Thread(StartApplication);
thr1.Start();
thr2.Start();
