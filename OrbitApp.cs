using Gtk;
using Gdk;

namespace Orbit;
class App : Gtk.Window
{
    DrawingArea da;
	OrbitInfo li;
    Gdk.Color col;

    public App() : base("Orbit 🐱‍🏍")
    {
        SetDefaultSize(860, 600);
        SetPosition(WindowPosition.Center);
        DeleteEvent += delegate { Shared.Running = false; Application.Quit(); };

		HBox hbox = new HBox (false, 8);
		hbox.BorderWidth = 8;
		Add (hbox);
		
		Frame drawFrame = new Frame();
		drawFrame.ShadowType = ShadowType.In;

		hbox.PackStart (drawFrame, true, true, 0);

		da = new OrbitDraw(200,200);
		drawFrame.Add (da);
		
		Frame infoFrame = new Frame();
		
		hbox.PackEnd(infoFrame, false, false, 0);

		li = new OrbitInfo();
		
		infoFrame.Add(li);

		ShowAll ();

		GLib.Timeout.Add(40, new GLib.TimeoutHandler(() => {this.da.QueueDraw(); return true;}));
    }

	internal bool UpdateData() {
		da.QueueDraw();
		li.Refresh();
		return true;
	}
}
