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
    
    fr.LoadPreset("solar system");
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
        }
        
        time.Restart();
    }
}

Thread thr1 = new Thread(StartPhysics);
thr1.Start();
StartApplication();
