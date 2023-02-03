using Gtk;
using Gdk;

namespace Orbit;
class App : Gtk.Window
{
    OrbitDraw da;
	OrbitInfo li;
	OrbitSettings os;
    public App() : base("Orbit 🐱‍🏍")
    {
        SetDefaultSize(950, 600);
        SetPosition(WindowPosition.Center);
        DeleteEvent += delegate { Shared.Running = false; Application.Quit(); };

		Paned p1 = new(Orientation.Horizontal);
		Paned p2 = new(Orientation.Horizontal);
		p1.Pack2(p2,true,true);
		Add(p1);
		p2.WidthRequest = 650;
		
		Frame infoFrame = new Frame();
		li = new OrbitInfo();
		infoFrame.Add(li);

		Frame settingsFrame = new Frame();
		os = new();
		settingsFrame.Add(os);

		Frame drawFrame = new Frame();
		drawFrame.ShadowType = ShadowType.In;

		da = new OrbitDraw(li);
		drawFrame.Add (da);
		da.WidthRequest = 400;

		p1.Pack1(settingsFrame, true, true);
		p2.Pack1(drawFrame, true, true);
		p2.Pack2(infoFrame, true, true);

		//p1.KeyPressEvent += (object o, KeyPressEventArgs args) => {da.OnKe};
		p1.KeyReleaseEvent += (object o, KeyReleaseEventArgs args) => {OnKeyRelease(o,args);};
		ShowAll ();
		li.InitHide();		

		GLib.Timeout.Add(33, new GLib.TimeoutHandler(() => UpdateData()));
    }

	internal bool UpdateData() {
		da.QueueDraw();
		li.Refresh();
		return true;
	}
	private void OnKeyRelease(object o, KeyReleaseEventArgs args) {
		if(args.Event.Key == Gdk.Key.f) {
			li.toFollow.Active = !li.toFollow.Active;
		}
		if(args.Event.Key == Gdk.Key.p) {
			Shared.Paused = !Shared.Paused;
			os.paused.Active = Shared.Paused;
		}
	}
}
