using Gtk;

namespace Orbit;
class App : Gtk.Window
{
    OrbitDraw da;
	OrbitInfo li;
	OrbitSettings os;
	OrbitSessionSettings se;
	bool[] WASD = new bool[4];
	bool Shift;
    public App() : base("Orbit 🌌")
    {
		se = new();

        SetDefaultSize(1366, 768);
        SetPosition(WindowPosition.Center);
        DeleteEvent += delegate { Shared.Running = false; Application.Quit(); };

		if(File.Exists("orbiticon.png")) {
			SetIconFromFile("orbiticon.png");
		}
		
		Paned p1 = new(Orientation.Horizontal);
		Paned p2 = new(Orientation.Horizontal);
		p1.Pack2(p2,true,true);
		p2.WidthRequest = 820;
		
		Frame infoFrame = new Frame();
		li = new OrbitInfo(se);
		infoFrame.Add(li);

		Frame settingsFrame = new Frame();
		VBox leftBox = new();
		os = new(se);
		leftBox.Add(os);
		Label about = new("By Nathan Williams 2023");
		about.Justify = Justification.Left;
		leftBox.PackEnd(about, false, false, 2);

		settingsFrame.Add(leftBox);

		Frame drawFrame = new Frame();
		da = new OrbitDraw(li, se);
		drawFrame.Add (da);
		da.WidthRequest = 750;

		p1.Pack1(settingsFrame, true, true);
		p2.Pack1(drawFrame, true, true);
		p2.Pack2(infoFrame, true, true);
		
		Frame outerFrame = new();
		outerFrame.ShadowType = ShadowType.EtchedIn;
		outerFrame.MarginStart = 8;
		outerFrame.MarginEnd = 8;
		outerFrame.Add(p1);

		Add(outerFrame);

		p1.KeyReleaseEvent += (object o, KeyReleaseEventArgs args) => {OnKeyRelease(o, args);};
		p1.KeyPressEvent   += (object o, KeyPressEventArgs   args) => {OnKeyPress  (o, args);};
		
		ShowAll();
		li.InitHide();		
		os.InitHide();

		GLib.Timeout.Add(16, new GLib.TimeoutHandler(() => UpdateData()));
    }

	[GLib.ConnectBefore]
    private void OnKeyPress(object o, KeyPressEventArgs args)
    {
		//Arrow keys have some technical jank for some reason and so are disabled. Specifically up and down arrows
		if(args.Event.Key == Gdk.Key.w || args.Event.Key == Gdk.Key.W) { WASD[0] = true; }
		if(args.Event.Key == Gdk.Key.a || args.Event.Key == Gdk.Key.A) { WASD[1] = true; }
		if(args.Event.Key == Gdk.Key.s || args.Event.Key == Gdk.Key.S) { WASD[2] = true; }
		if(args.Event.Key == Gdk.Key.d || args.Event.Key == Gdk.Key.D) { WASD[3] = true; }
		
		if(args.Event.Key == Gdk.Key.Shift_L || args.Event.Key== Gdk.Key.Shift_R) { Shift = true; }

		if(args.Event.State == (Gdk.ModifierType.MetaMask | Gdk.ModifierType.Mod2Mask)  || args.Event.State == Gdk.ModifierType.ControlMask) {
			if(args.Event.Key == Gdk.Key.q || args.Event.Key == Gdk.Key.Q) {
				Shared.Running = false; 
				Application.Quit(); 
			}
			if(args.Event.Key == Gdk.Key.f || args.Event.Key == Gdk.Key.F) {
				CheckButton follow = (CheckButton)li.GetWidget("toFollow");
				follow.Active = !follow.Active;
			}
			if(args.Event.Key == Gdk.Key.p || args.Event.Key == Gdk.Key.P) {
				Shared.Paused = !Shared.Paused;
				os.paused.Active = Shared.Paused;
			}
			if(args.Event.Key == Gdk.Key.t || args.Event.Key == Gdk.Key.T) {
				se.drawTrails = !se.drawTrails;
				((CheckMenuItem)os.MenuButtons["Global Trail Draw"]!).Active = se.drawTrails;
			}
		}
    }
	private void OnKeyRelease(object o, KeyReleaseEventArgs args) {
		if(args.Event.Key == Gdk.Key.Up    || args.Event.Key == Gdk.Key.w || args.Event.Key == Gdk.Key.W) { WASD[0] = false; }
		if(args.Event.Key == Gdk.Key.Left  || args.Event.Key == Gdk.Key.a || args.Event.Key == Gdk.Key.A) { WASD[1] = false; }
		if(args.Event.Key == Gdk.Key.Down  || args.Event.Key == Gdk.Key.s || args.Event.Key == Gdk.Key.S) { WASD[2] = false; }
		if(args.Event.Key == Gdk.Key.Right || args.Event.Key == Gdk.Key.d || args.Event.Key == Gdk.Key.D) { WASD[3] = false; }
	
		if(args.Event.Key == Gdk.Key.Shift_L || args.Event.Key== Gdk.Key.Shift_R) { Shift = false; }
	}

    internal bool UpdateData() {
		//Keyboard input does not work within the DrawingArea for some reason, so hacked in here
		int xAdd = 0, yAdd = 0;
		if(WASD[0]) { yAdd -= 8; }
		if(WASD[1]) { xAdd -= 8; }
		if(WASD[2]) { yAdd += 8; }
		if(WASD[3]) { xAdd += 8; }
		da.offset += new Vector2d(xAdd, yAdd) * (Shift ? 4 : 1) * da.scale;

		da.QueueDraw();
		li.Refresh();
		if(Shared.ignoreNextUpdates > 0) {
			Shared.ignoreNextUpdates -= 1;
		}
		if(Shared.resolutionMode !=  os.expectedResolutionMode) {
			os.SetResolutionMode(Shared.resolutionMode);
		}
		return true;
	}
}
public class OrbitSessionSettings {
	public bool drawTrails = true;
	public bool drawMasses = true;
	public bool drawVelocityVectors = false;
	public bool drawForceVectors = false;
	public bool normalizeVelocity = false;
	public bool linearForces = false;
	public string positionDisplayUnits = "AU";
	public string massDisplayUnits = "Earth";
}