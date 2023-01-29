using Gtk;
using Gdk;

namespace Orbit;
class App : Gtk.Window
{
    
    DrawingArea da;
    Gdk.Color col;

	private Cairo.Surface surface = null;

    public App() : base("Orbit 🐱‍🏍")
    {
        SetDefaultSize(390, 240);
        SetPosition(WindowPosition.Center);
        DeleteEvent += delegate { Shared.Running = false; Application.Quit(); };

		HBox hbox = new HBox (false, 8);
		hbox.BorderWidth = 8;
		Add (hbox);
		
		Frame frame = new Frame ();
		frame.ShadowType = ShadowType.In;

		hbox.PackStart (frame, false, false, 0);

		da = new OrbitDraw(800,800);
		// set a minimum size
		//da.SetSizeRequest (100, 100);
		frame.Add (da);
		
		// Signals used to handle backing pixmap
		//da.Drawn += new DrawnHandler (ScribbleDrawn);
		//da.ConfigureEvent += new ConfigureEventHandler (ScribbleConfigure);

		// Event signals
		//da.MotionNotifyEvent += new MotionNotifyEventHandler (ScribbleMotionNotify);
		//da.ButtonPressEvent += new ButtonPressEventHandler (ScribbleButtonPress);


		// Ask to receive events the drawing area doesn't normally
		// subscribe to
		da.Events |= EventMask.LeaveNotifyMask | EventMask.ButtonPressMask |
			EventMask.PointerMotionMask | EventMask.PointerMotionHintMask;
		//da.QueueDraw();
		ShowAll ();
		

		GLib.Timeout.Add(50, new GLib.TimeoutHandler(UpdateData));
    }

	internal bool UpdateData() {
		Shared.ReadyDrawingCopy();
		this.da.QueueDraw();
		return true;
	}
}

public class OrbitDraw : Gtk.DrawingArea {       
    public OrbitDraw(int width, int height)
    {
		//ExposeEvent has been replaced with Drawn in GDK 3
		Drawn += Draw;

		WidthRequest = width;
		HeightRequest = height;
		
    }

    private void Draw(object sender, DrawnArgs args)
    {
		int offset = 400;
		//A new CairoHelper should be created every draw call according to documentation
        using (var cr = Gdk.CairoHelper.Create( this.GdkWindow ))
        {
			cr.LineWidth = 2;

			for(int index = 0; index < Shared.massObjects; index++) {
				if(!Shared.drawingCopy[index].hasTrail) { continue; }

				Vector2d[] trail = Shared.drawingCopy[index].trail;
				int trailOffset = Shared.drawingCopy[index].trailOffset;
				int trailLength = trail.Length;

				double transPerLine = 1f / trail.Length;
				double transparency = 1f;
				
				cr.MoveTo(trail[trailOffset].X / 10000 + offset, trail[trailOffset].Y / 10000 + offset);

				for(int i = trailOffset; i < trailOffset + trailLength; i++) {
					//cr.SetSourceRGB(transparency,0,0);
					//transparency -= transPerLine;
					

					Vector2d point = trail[i % trailLength];
					cr.LineTo(point.X / 10000 + 0.5 + offset, point.Y / 10000 + 0.5 + offset);
				}
				cr.Stroke();
			}
			
			//Reset transparency
			//cr.SetSourceRGBA(0,0,0,1);

			//Draw mass circles
			for(int index = 0; index < Shared.massObjects; index++) {
				MassInfo m = Shared.drawingCopy[index];

				cr.Arc(m.position.X / 10000 + offset, m.position.Y / 10000 + offset, 4, 0, 2 * Math.PI);
				cr.StrokePreserve(); //Saves circle for filling in
				cr.Fill(); //Currently fills with same color as outline

			}
			
			cr.GetTarget().Dispose();
        }
    }
}