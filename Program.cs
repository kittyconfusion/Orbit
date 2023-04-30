using Orbit;
using Gtk;
using System.Diagnostics;

static void StartApplication() {
    Application.Init();
    new App();
    Application.Run();
}

static void StartPhysics() {

    PhysicsRunner fr = new();
    
    fr.LoadPreset("proxima centauri");
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

        Thread.Sleep(5);
        if(!Shared.Paused) {
            timeTook += time.ElapsedMilliseconds;
            //Stop the program from completely hanging if too far behind
            //while also giving the user noticable slowdown to notify
            if(timeTook > 250) { Console.WriteLine(timeTook); timeTook = 100; }
        }
        
        time.Restart();
    }
}

Thread thr1 = new Thread(StartPhysics);
thr1.Start();
StartApplication();
