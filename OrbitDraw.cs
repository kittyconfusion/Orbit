using Gtk;
using Gdk;

namespace Orbit;

public class OrbitDraw : Gtk.DrawingArea {
	private OrbitInfo infoWindow;
    private double scale = 450000;
	private Vector2d offset = new();
	private Vector2d lastMouse = new();
	private bool moved = false;

    public OrbitDraw(OrbitInfo infoWindow)
    {
		//ExposeEvent has been replaced with Drawn in GDK 3
		Drawn += new DrawnHandler(Draw);
		Events |= EventMask.ScrollMask | EventMask.Button1MotionMask | EventMask.ButtonPressMask | EventMask.ButtonReleaseMask
		| EventMask.KeyPressMask | EventMask.KeyReleaseMask;

		ScrollEvent += new ScrollEventHandler(ScrollZoom);
		ButtonPressEvent += new ButtonPressEventHandler(ClickWindow);
		ButtonReleaseEvent += new ButtonReleaseEventHandler(ReleaseWindow);
		MotionNotifyEvent += new MotionNotifyEventHandler(MoveWindow);

		//Needed for keyboard input
		CanFocus = true;
		this.infoWindow = infoWindow;
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
		Shared.trackedMass = -1;
		infoWindow.toFollow.Active = false;
		offset += (lastMouse-new Vector2d(args.Event.X,args.Event.Y)) * scale;
		lastMouse = new Vector2d(args.Event.X, args.Event.Y);
	}
	private void Click(double x, double y) {

		Console.WriteLine(scale + " " + offset);
	}
	private void ScrollZoom (object o, ScrollEventArgs args) {
		double oldscale = scale;
		if(args.Event.Direction == ScrollDirection.Up || args.Event.Direction == ScrollDirection.Down) {
			scale *= 1 + ((args.Event.Direction == ScrollDirection.Up ? -0.06 : 0.06) * OrbitSettings.ZoomSensitivity);
			scale = Math.Max(0.001, scale); //Limit to 1px per meter
			offset += (new Vector2d(args.Event.X, args.Event.Y) - new Vector2d(AllocatedWidth, AllocatedHeight) / 2) * (oldscale - scale);
		}
	}

    private void Draw(object sender, DrawnArgs args)
    {
		Shared.ReadyDrawingCopy();

		double inverseScale = 1 / scale;

		if (Shared.trackedMass > -1) {
			offset = Shared.drawingCopy[Shared.trackedMass].position;
		}
		Vector2d drawOffset = offset;
		Vector2d windowCenter = new Vector2d(AllocatedWidth, AllocatedHeight) / 2 + 0.5;
		
		//A new CairoHelper should be created every draw call according to documentation
		using (var cr = args.Cr)
        {
			cr.LineWidth = 2;
			cr.SetSourceRGB(0.6,0.6,0.6);
			for(int index = 0; index < Shared.massObjects; index++) {
				MassInfo m = Shared.drawingCopy[index];
				if(!m.hasTrail) { continue; }

				Vector2d[] trail = m.trail;
				int trailOffset = m.trailOffset;
				int trailLength = trail.Length;

				double transPerLine = 1f / trail.Length;
				
				if(m.followingIndex != -1) {
					Vector2d followingPosition = Shared.drawingCopy[m.followingIndex].position;
					for(int i = trailOffset + 1; i < trailOffset + trailLength; i++) {
						Vector2d point = WorldToScreen(trail[i % trailLength] + followingPosition, inverseScale, drawOffset, windowCenter);
						cr.LineTo(point.X, point.Y);
					}
				}
				else {
					for(int i = trailOffset + 1; i < trailOffset + trailLength; i++) {
						Vector2d point = WorldToScreen(trail[i % trailLength], inverseScale, drawOffset, windowCenter);
						cr.LineTo(point.X, point.Y);
					}
				}
				Vector2d finalPoint = WorldToScreen(m.position, inverseScale, drawOffset, windowCenter);
				cr.LineTo(finalPoint.X, finalPoint.Y);
				cr.Stroke();
			}
			cr.SetSourceRGB(0,0,0);
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