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

		Console.WriteLine(Shared.deltaTime);
	}
	private void ScrollZoom (object o, ScrollEventArgs args) {
		double oldscale = scale;
		if(args.Event.Direction == ScrollDirection.Up || args.Event.Direction == ScrollDirection.Down) {
			scale *= 1 + ((args.Event.Direction == ScrollDirection.Up ? -0.08 : 0.08) * OrbitSettings.ZoomSensitivity);
			scale = Math.Max(5, scale); //Limit to 1px per 5km
			offset += (new Vector2d(args.Event.X, args.Event.Y) - new Vector2d(AllocatedWidth, AllocatedHeight) / 2) * (oldscale - scale);
		}
	}
	private void DrawArrow(Cairo.Context cr, Vector2d startScreenPosition, Vector2d screenArrowSize, int arrowLineWidth) {
		//Adapted from https://stackoverflow.com/a/57924851
		double arrowAngle = Math.Atan2(screenArrowSize.Y, screenArrowSize.X);
		const double arrowheadAngle = Math.PI/6;
		const double arrowheadLength = 8;

		cr.MoveTo(startScreenPosition.X, startScreenPosition.Y);

		cr.RelLineTo(screenArrowSize.X, screenArrowSize.Y);
		cr.RelMoveTo(-arrowheadLength * Math.Cos(arrowAngle - arrowheadAngle), -arrowheadLength * Math.Sin(arrowAngle - arrowheadAngle));
		cr.RelLineTo(arrowheadLength * Math.Cos(arrowAngle - arrowheadAngle), arrowheadLength * Math.Sin(arrowAngle - arrowheadAngle));
		cr.RelLineTo(-arrowheadLength * Math.Cos(arrowAngle + arrowheadAngle), -arrowheadLength * Math.Sin(arrowAngle + arrowheadAngle));

		cr.SetSourceRGB(0,0,0);
		cr.LineWidth = arrowLineWidth;
		cr.Stroke();
	}
    private void Draw(object sender, DrawnArgs args)
    {
		Shared.ReadyDrawingCopy();

		double inverseScale = 1 / scale;

		if(Shared.trackedMass > -1) {
			

		}
		Vector2d drawOffset = offset;
		if(Shared.trackedMass > -1) {
			MassInfo tracked = Shared.drawingCopy[Shared.trackedMass];
			drawOffset = tracked.position;
			if(!tracked.currentlyUpdatingPhysics) {
				drawOffset += Shared.drawingCopy[tracked.orbitingBodyIndex].position;
			}
			offset = drawOffset;
		}
		
		Vector2d windowCenter = new Vector2d(AllocatedWidth, AllocatedHeight) / 2 + 0.5;
		
		//A new CairoHelper should be created every draw call according to documentation
		using (var cr = args.Cr)
        {
			cr.LineWidth = 2;
			cr.SetSourceRGB(0.6, 0.6, 0.6);
			
			//Draw trails
			if(se.drawTrails) {
				for(int index = 0; index < Shared.massObjects; index++) {
					MassInfo m = Shared.drawingCopy[index];
					if(!m.hasTrail || m.stationary || !m.currentlyUpdatingPhysics) { continue; }

					Vector2d[] trail = m.trail;
					int trailOffset = m.trailOffset;
					int trailLength = trail.Length;

					double transparency = 0;
					
					int perUpdate = trailLength / 10;
					int counter = 0;

					//Add the offset if the object is drawn relative to another
					Vector2d followingPosition = m.followingIndex > -1 ? 
						Shared.drawingCopy[m.followingIndex].position : new Vector2d(0,0);
					
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
					Vector2d finalPoint = WorldToScreen(m.position, inverseScale, drawOffset, windowCenter);
					cr.LineTo(finalPoint.X, finalPoint.Y);
					cr.Stroke();
				}
			}
		
		//Draw mass circles
		cr.SetSourceRGB(0,0,0);
		for(int index = 0; index < Shared.massObjects; index++) {
			MassInfo m = Shared.drawingCopy[index];
			Vector2d position = !m.currentlyUpdatingPhysics 
				? Shared.drawingCopy[m.orbitingBodyIndex].position + m.position : m.position;
			Vector2d point = WorldToScreen(position, inverseScale, drawOffset, windowCenter);

			cr.Arc(point.X, point.Y, MassToRadius(m.mass, inverseScale), 0, 2 * Math.PI);
			cr.StrokePreserve(); //Saves circle for filling in
			
			double radius = MassToGlyphSize(m.mass, inverseScale);
			if (radius > 0) {		
				cr.SetFontSize((int)Math.Min(30, radius));
				cr.ShowText(m.name);
			}

			cr.Fill(); //Currently fills with same color as outline
			double massRadius = MassToGlyphSize(m.mass, inverseScale);
			if(massRadius > 0 && !m.stationary && m.currentlyUpdatingPhysics) {
				DrawArrow(cr, point, (m.velocity) * 1.1, (int)(massRadius / 5 - 0.5));
			}
					
		}
		cr.GetTarget().Dispose();
        }
    }
	private double MassToRadius(double mass, double inverseScale)
		=> Math.Max(0, (Math.Log(mass) + 1.25 * Math.Log(inverseScale) - 25));
	private double MassToGlyphSize(double mass, double inverseScale) {
		double size = Math.Min(28, Math.Log(mass) + 1.65 * Math.Log(inverseScale) - 4.5);
		return size >= 10 ? size : 0;
	}
	private Vector2d WorldToScreen(Vector2d point, double inverseScale, Vector2d drawOffset, Vector2d windowCenter) 
		=> ((point - drawOffset) * inverseScale) + windowCenter;
}