using Gtk;

namespace Orbit;
class App : Gtk.Window
{
    OrbitDraw da;
	FileChooserWidget fc;
	VBox drawBox;
	Frame drawFrame;
	OrbitInfo li;
	OrbitSettings os;
	HBox fileButtonBox;
	OrbitSessionSettings se;
	bool Control = false;
    public App() : base("Orbit 🌌")
    {
		se = new();
		
        SetDefaultSize(1366, 768);
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
		VBox settingsBox = new();
		os = new(se);
		settingsFrame.Add(settingsBox);
		settingsBox.Add(os);
		
		fileButtonBox = new();
		fileButtonBox.Valign = Align.End;
		fileButtonBox.HeightRequest = 40;
		settingsBox.Add(fileButtonBox);

		Button cancelButton = new("Cancel");
		Button commitButton = new("Commit");
		commitButton.Pressed += (object? o, EventArgs a) => {
			Console.WriteLine(fc!.Filename);
		};

		fileButtonBox.Add(cancelButton);
		fileButtonBox.Add(commitButton);

		drawFrame = new Frame();
		da = new OrbitDraw(li, se);

		drawBox = new();
		drawBox.Add(da);
		drawFrame.Add(drawBox);

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
		
		ShowAll();
		li.InitHide();
		fileButtonBox.Hide();		

		GLib.Timeout.Add(33, new GLib.TimeoutHandler(() => UpdateData()));
		
    }

    private void OnKeyPress(object o, KeyPressEventArgs args)
    {
        if(args.Event.Key == Gdk.Key.Control_L || args.Event.Key == Gdk.Key.Control_R) {
			Control = true;
		}
    }
	internal void SwitchToSaveDialog() {
		//Make the Save/Cancel buttons visible
		//Switch out the OrbitDraw widget for a save widget
		fileButtonBox.Show();
		fc = new(FileChooserAction.Save);

		FileFilter jsonFilter = new();
		jsonFilter.Name = "JSON files";
		jsonFilter.AddMimeType("application/json");

		FileFilter allFilter = new();
		allFilter.Name = "All files";
		allFilter.AddMimeType("*");
		
		fc.AddFilter(jsonFilter);
		fc.AddFilter(allFilter);
		
		drawFrame.Remove(drawBox);
		drawBox = new VBox();
		drawBox.Add(fc);
		drawFrame.Add(drawBox);
		
		ShowAll();
		Console.WriteLine(fc.Filename);
	}
    internal bool UpdateData() {
		if(se.needToSave) {
			SwitchToSaveDialog();
			se.needToSave = false;
		}
		da.QueueDraw();
		li.Refresh();
		if(Shared.ignoreNextUpdates > 0) {
			Shared.ignoreNextUpdates -= 1;
		}
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
	public bool needToSave = false;
	public bool drawTrails = true;
	public bool drawVelocityVectors = false;
	public bool drawForceVectors = false;
	public bool normalizeVelocity = false;
	public string positionDisplayUnits = "AU";
	public string massDisplayUnits = "Earth";
}