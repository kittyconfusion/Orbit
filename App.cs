using Gtk;
namespace Orbit;
class App : Window
{
    
    //DrawingArea darea;
    Gdk.Color col;

    public App() : base("Orbit 🐱‍🏍")
    {
        SetDefaultSize(390, 240);
        SetPosition(WindowPosition.Center);
        DeleteEvent += delegate { Application.Quit(); };
        

       		Frame frame = new Frame ();
			frame.ShadowType = ShadowType.In;

			DrawingArea da = new DrawingArea ();
			// set a minimum size
			da.SetSizeRequest (100, 100);
			frame.Add (da);

			// Signals used to handle backing pixmap
			da.Drawn += new DrawnHandler (ScribbleDrawn);
			da.ConfigureEvent += new ConfigureEventHandler (ScribbleConfigure);

			// Event signals
			da.MotionNotifyEvent += new MotionNotifyEventHandler (ScribbleMotionNotify);
			da.ButtonPressEvent += new ButtonPressEventHandler (ScribbleButtonPress);

    }
}