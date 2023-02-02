using Gtk;
using Gdk;

namespace Orbit;
class App : Gtk.Window
{
    DrawingArea da;
	OrbitInfo li;

    public App() : base("Orbit 🐱‍🏍")
    {
        SetDefaultSize(900, 600);
        SetPosition(WindowPosition.Center);
        DeleteEvent += delegate { Shared.Running = false; Application.Quit(); };

		Paned p1 = new(Orientation.Horizontal);
		Paned p2 = new(Orientation.Horizontal);
		p1.Pack2(p2,true,true);
		Add(p1);
		p2.WidthRequest = 650;

		Frame drawFrame = new Frame();
		drawFrame.ShadowType = ShadowType.In;

		da = new OrbitDraw();
		drawFrame.Add (da);
		da.WidthRequest = 400;
		
		Frame infoFrame = new Frame();
		li = new OrbitInfo();
		infoFrame.Add(li);

		Frame settingsFrame = new Frame();
		OrbitSettings os = new();
		settingsFrame.Add(os);

		
		p1.Pack1(settingsFrame, true, true);
		p2.Pack1(drawFrame, true, true);
		p2.Pack2(infoFrame, true, true);

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
