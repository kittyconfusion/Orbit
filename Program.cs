// See https://aka.ms/new-console-template for more information
using Orbit;
using Gtk;

/*
Application.Init();
new DemoDrawingArea();
Application.Run();
*/

StationaryMass sun = new(10000000, new Vector2d(0,0));
Planet planet = new(100000,new Vector2d(-0.5, 0.8), new Vector2d(20, 6), sun);

using (StreamWriter writer = new StreamWriter("xy.txt"))  
{  
    for(int i = 0; i < 250; i++ ) {
        planet.Update(1);
        writer.WriteLine(planet.mass.position.X + " " + planet.mass.position.Y);
        //Console.WriteLine(planet.mass.velocity);
    }
}
/*
for(int i = 0; i < 1000; i++ ) {
    planet.Update(1000);
    Console.WriteLine(planet.mass.position.X);
}
*/