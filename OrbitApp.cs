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

		da = new OrbitDraw(200,200);
		drawFrame.Add (da);
		
		Frame infoFrame = new Frame();
		li = new OrbitInfo();
		infoFrame.Add(li);

		Frame settingsFrame = new Frame();
		OrbitSettings os = new();
		settingsFrame.Add(os);

		hbox.PackStart(settingsFrame, false, true, 0);
		hbox.PackStart (drawFrame, true, true, 0);
		hbox.PackEnd(infoFrame, false, true, 0);

		ShowAll ();
		li.InitHide();		

		GLib.Timeout.Add(16, new GLib.TimeoutHandler(() => UpdateData()));
    }

	internal bool UpdateData() {
		da.QueueDraw();
		li.Refresh();
		return true;
	}
}
