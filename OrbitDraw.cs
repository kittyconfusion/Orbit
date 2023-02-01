using Gtk;
using Gdk;

namespace Orbit;

public class OrbitDraw : Gtk.DrawingArea {
    private double scale = 450000;
	private Vector2d offset = new();
	private Vector2d lastMouse = new();
	private bool moved = false;

    public OrbitDraw(int width, int height)
    {
		//ExposeEvent has been replaced with Drawn in GDK 3
		Drawn += new DrawnHandler(Draw);
		Events |= EventMask.ScrollMask | EventMask.Button1MotionMask | EventMask.ButtonPressMask | EventMask.ButtonReleaseMask
		| EventMask.KeyPressMask;

		ScrollEvent += new ScrollEventHandler(ScrollZoom);
		ButtonPressEvent += new ButtonPressEventHandler(ClickWindow);
		ButtonReleaseEvent += new ButtonReleaseEventHandler(ReleaseWindow);
		MotionNotifyEvent += new MotionNotifyEventHandler(MoveWindow);
		KeyPressEvent += new KeyPressEventHandler(KeyPress);

		WidthRequest = width;
		HeightRequest = height;

		//Needed for keyboard input
		CanFocus = true;
    }

	private void KeyPress(object o, KeyPressEventArgs args) {
		//TO DO (use Gdk.Key)
	}
	private void ClickWindow(object o, ButtonPressEventArgs args) {
		lastMouse = new Vector2d(args.Event.X, args.Event.Y);
		moved = false;
	}
	private void ReleaseWindow(object o, ButtonReleaseEventArgs args) {
		if(!moved) { Click(args.Event.X, args.Event.Y); }
	}
	private void MoveWindow(object o, MotionNotifyEventArgs args) {
		moved = true;
		offset += (lastMouse-new Vector2d(args.Event.X,args.Event.Y)) * scale;
		lastMouse = new Vector2d(args.Event.X, args.Event.Y);
	}
	private void Click(double x, double y) {

		Console.WriteLine(scale + " " + offset);
	}
	private void ScrollZoom (object o, ScrollEventArgs args) {
		double oldscale = scale;
		if(args.Event.Direction == ScrollDirection.Up || args.Event.Direction == ScrollDirection.Down) {
			scale *= args.Event.Direction == ScrollDirection.Up ? 0.98 : 1.02;
			scale = Math.Max(0.001, scale); //Limit to 1px per meter
			offset += (new Vector2d(args.Event.X, args.Event.Y) - new Vector2d(WidthRequest, HeightRequest) / 2) * (oldscale - scale);
		}
	}

    private void Draw(object sender, DrawnArgs args)
    {
		Shared.ReadyDrawingCopy();

		double inverseScale = 1 / scale;
		Vector2d drawOffset = offset;
		//drawOffset = Shared.drawingCopy[1].position;
		Vector2d windowCenter = new Vector2d(WidthRequest, HeightRequest) / 2 + 0.5;
		
		//A new CairoHelper should be created every draw call according to documentation
		using (var cr = args.Cr)
        {
			cr.LineWidth = 2;

			for(int index = 0; index < Shared.massObjects; index++) {
				if(!Shared.drawingCopy[index].hasTrail) { continue; }

				Vector2d[] trail = Shared.drawingCopy[index].trail;
				int trailOffset = Shared.drawingCopy[index].trailOffset + 1;
				int trailLength = trail.Length;

				double transPerLine = 1f / trail.Length;
				
				for(int i = trailOffset; i < trailOffset + trailLength - 1; i++) {
					
					Vector2d point = WorldToScreen(trail[i % trailLength], inverseScale, drawOffset, windowCenter);

					cr.LineTo(point.X, point.Y);
				}
				cr.Stroke();
			}

			//Draw mass circles
			for(int index = 0; index < Shared.massObjects; index++) {
				MassInfo m = Shared.drawingCopy[index];
				Vector2d point = WorldToScreen(m.position, inverseScale, drawOffset, windowCenter);

				cr.Arc(point.X, point.Y, MassToRadius(m.mass, inverseScale), 0, 2 * Math.PI);
				cr.StrokePreserve(); //Saves circle for filling in
				cr.Fill(); //Currently fills with same color as outline
			}
			
			cr.GetTarget().Dispose();
        }
    }
	private int MassToRadius(double mass, double inverseScale)
		=> Math.Max(0, (int)(Math.Log(mass) + 1.25 * Math.Log(inverseScale) - 25));
	private Vector2d WorldToScreen(Vector2d point, double inverseScale, Vector2d drawOffset, Vector2d windowCenter) 
		=> ((point - drawOffset) * inverseScale) + windowCenter;
}