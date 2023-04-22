using Gtk;

namespace Orbit;
class App : Gtk.Window
{
    OrbitDraw da;
	FileChooserWidget? fc;
	Widget currentDrawFrameWidget;
	Frame drawFrame;
	Button commitButton;
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
		cancelButton.Pressed += (object? o, EventArgs a) => {
			ExitBackToDraw();
		};
		commitButton = new("Commit");
		commitButton.Pressed += (object? o, EventArgs a) => {
			CommitButtonPress();
		};

		fileButtonBox.Add(cancelButton);
		fileButtonBox.Add(commitButton);

		drawFrame = new Frame();
		da = new OrbitDraw(li, se);

		currentDrawFrameWidget = da;
		drawFrame.Add(da);

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
	internal void CommitButtonPress() {
		if(fc != null) {
			string name = fc.Filename;
			if(Directory.Exists(System.IO.Path.GetPathRoot(name)) && System.IO.Path.GetFileNameWithoutExtension(name) != "") {
				Shared.changesToMake.Push(new string[] {"save", fc.Filename, "-1"});
				ExitBackToDraw();
			}
		}
		else {
			ExitBackToDraw();
		}
	}
	internal void ExitBackToDraw() {
		fileButtonBox.Hide();
		fc = null;
		drawFrame.Remove(currentDrawFrameWidget);
		drawFrame.Add(da);
		currentDrawFrameWidget = da;
	}
	internal void SwitchToSaveDialog() {
		Shared.Paused = true;
		//Switch out the OrbitDraw widget for a save widget
		fileButtonBox.Show();
		commitButton.Label = "Save";
		
		//Make the Save/Cancel buttons visibless
		fc = new(FileChooserAction.Save);

		FileFilter jsonFilter = new();
		jsonFilter.Name = "JSON files";
		jsonFilter.AddMimeType("application/json");

		FileFilter allFilter = new();
		allFilter.Name = "All files";
		allFilter.AddMimeType("*");
		
		fc.AddFilter(jsonFilter);
		fc.AddFilter(allFilter);
		
		drawFrame.Remove(currentDrawFrameWidget);
		currentDrawFrameWidget = fc;
		drawFrame.Add(fc);
		
		fc.Show();
	}
    internal bool UpdateData() {
		if(se.middleWidgetState == "Start Save") {
			SwitchToSaveDialog();
			se.middleWidgetState = "In Save";
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
	public string middleWidgetState = "Draw";
	public bool drawTrails = true;
	public bool drawVelocityVectors = false;
	public bool drawForceVectors = false;
	public bool normalizeVelocity = false;
	public string positionDisplayUnits = "AU";
	public string massDisplayUnits = "Earth";
}