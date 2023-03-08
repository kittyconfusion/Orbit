using Gtk;

namespace Orbit;
class App : Gtk.Window
{
    OrbitDraw da;
	OrbitInfo li;
	OrbitSettings os;
	OrbitSessionSettings se;
	bool Control = false;
    public App() : base("Orbit 🐱‍🏍")
    {
		se = new();

        SetDefaultSize(950, 600);
        SetPosition(WindowPosition.Center);
        DeleteEvent += delegate { Shared.Running = false; Application.Quit(); };
		
		Paned p1 = new(Orientation.Horizontal);
		Paned p2 = new(Orientation.Horizontal);
		p1.Pack2(p2,true,true);
		p2.WidthRequest = 650;
		
		Frame infoFrame = new Frame();
		li = new OrbitInfo(se);
		infoFrame.Add(li);

		Frame settingsFrame = new Frame();
		os = new(se);
		settingsFrame.Add(os);

		Frame drawFrame = new Frame();
		da = new OrbitDraw(li, se);
		drawFrame.Add (da);
		da.WidthRequest = 400;

		p1.Pack1(settingsFrame, true, true);
		p2.Pack1(drawFrame, true, true);
		p2.Pack2(infoFrame, true, true);
		
		Frame outerFrame = new();
		outerFrame.ShadowType = ShadowType.EtchedIn;
		outerFrame.MarginStart = 8;
		outerFrame.MarginEnd = 8;
		//outerFrame.MarginBottom = 4;
		outerFrame.Add(p1);

		Add(outerFrame);

		p1.KeyReleaseEvent += (object o, KeyReleaseEventArgs args) => {OnKeyRelease(o, args);};
		p1.KeyPressEvent   += (object o, KeyPressEventArgs   args) => {OnKeyPress  (o, args);};
		
		ShowAll ();
		li.InitHide();		

		GLib.Timeout.Add(33, new GLib.TimeoutHandler(() => UpdateData()));
    }

    private void OnKeyPress(object o, KeyPressEventArgs args)
    {
        if(args.Event.Key == Gdk.Key.Control_L || args.Event.Key == Gdk.Key.Control_R) {
			Control = true;
		}
    }

    internal bool UpdateData() {
		da.QueueDraw();
		li.Refresh();
		return true;
	}
	private void OnKeyRelease(object o, KeyReleaseEventArgs args) {
		if(args.Event.Key == Gdk.Key.f && Control) {
			CheckButton follow = (CheckButton)li.GetWidget("toFollow");
			follow.Active = !follow.Active;
		}
		if(args.Event.Key == Gdk.Key.p && Control) {
			Shared.Paused = !Shared.Paused;
			os.paused.Active = Shared.Paused;
		}
		if(args.Event.Key == Gdk.Key.Control_L || args.Event.Key == Gdk.Key.Control_R) {
			Control = false;
		}
		if(args.Event.Key == Gdk.Key.t && Control) {
			se.drawTrails = !se.drawTrails;
			((CheckMenuItem)os.MenuButtons["Global Trail Draw"]!).Active = se.drawTrails;
		}
	}
}

public class OrbitSessionSettings {
	public bool drawTrails = true;
	public string positionDisplayUnits = "AU";
	public string massDisplayUnits = "Earth";
}