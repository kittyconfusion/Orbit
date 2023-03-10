using Gtk;
using Gdk;

namespace Orbit;

public class OrbitDraw : Gtk.DrawingArea {
	private OrbitInfo infoWindow;
    private double scale = 450000;
	private Vector2d offset = new();
	private Vector2d lastMouse = new();
	private bool moved = false;
	OrbitSessionSettings se;

    public OrbitDraw(OrbitInfo infoWindow, OrbitSessionSettings se)
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
		this.se = se;
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
		((CheckButton)infoWindow.GetWidget("toFollow")).Active = false;
		offset += (lastMouse-new Vector2d(args.Event.X,args.Event.Y)) * scale;
		lastMouse = new Vector2d(args.Event.X, args.Event.Y);
	}
	private void Click(double x, double y) {

		Console.WriteLine(scale + " " + offset);
	}
	private void ScrollZoom (object o, ScrollEventArgs args) {
		double oldscale = scale;
		if(args.Event.Direction == ScrollDirection.Up || args.Event.Direction == ScrollDirection.Down) {
			scale *= 1 + ((args.Event.Direction == ScrollDirection.Up ? -0.08 : 0.08) * OrbitSettings.ZoomSensitivity);
			scale = Math.Max(5, scale); //Limit to 1px per 5km
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
			cr.SetSourceRGB(0.6, 0.6, 0.6);
			
			for(int index = 0; index < Shared.massObjects; index++) {
				MassInfo m = Shared.drawingCopy[index];
				if(!m.hasTrail) { continue; }

				Vector2d[] trail = m.trail;
				int trailOffset = m.trailOffset;
				int trailLength = trail.Length;

				double transparency = 0;
				
				int perUpdate = trailLength / 10;
				int counter = 0;

				if(m.followingIndex != -1 && se.drawTrails) {
					Vector2d followingPosition = Shared.drawingCopy[m.followingIndex].position;
					for(int i = trailOffset + 1; i < trailOffset + trailLength; i++) {
						Vector2d point = WorldToScreen(trail[i % trailLength] + followingPosition, inverseScale, drawOffset, windowCenter);
						cr.LineTo(point.X, point.Y);

						if(counter > perUpdate) {
							cr.SetSourceRGBA(0.6, 0.6, 0.6, transparency);
							cr.Stroke();
							counter = -1;
							transparency += 0.1;
							i--;
						}
						counter++;
					}
				}
				else if(se.drawTrails) {
					for(int i = trailOffset + 1; i < trailOffset + trailLength; i++) {
						Vector2d point = WorldToScreen(trail[i % trailLength], inverseScale, drawOffset, windowCenter);
						cr.LineTo(point.X, point.Y);
						if(counter > perUpdate) {
							cr.SetSourceRGBA(0.6, 0.6, 0.6, transparency);
							cr.Stroke();
							counter = -1;
							transparency += 0.1;
							i--;
						}
						counter++;
						
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
				
				double radius = MassToGlyphSize(m.mass, inverseScale);
				if (radius > 0) {		
					cr.SetFontSize((int)Math.Min(30, radius));
					cr.ShowText(m.name);
				}

				cr.Fill(); //Currently fills with same color as outline				
			}
			
			cr.GetTarget().Dispose();
        }
    }
	private int MassToRadius(double mass, double inverseScale)
		=> Math.Max(0, (int)(Math.Log(mass) + 1.25 * Math.Log(inverseScale) - 25));
	private double MassToGlyphSize(double mass, double inverseScale) {
		double size = Math.Min(28, Math.Log(mass) + 1.65 * Math.Log(inverseScale) - 4.5);
		return size >= 10 ? size : 0;
	}
	private Vector2d WorldToScreen(Vector2d point, double inverseScale, Vector2d drawOffset, Vector2d windowCenter) 
		=> ((point - drawOffset) * inverseScale) + windowCenter;
}