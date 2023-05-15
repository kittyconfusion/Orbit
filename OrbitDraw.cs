using Gtk;
using Gdk;

namespace Orbit;

public class OrbitDraw : Gtk.DrawingArea {
	private OrbitInfo infoWindow;
    internal double scale = 450000;
	internal Vector2d offset = new();
	private Vector2d lastMouse = new();
	private bool moved = false;
	OrbitSessionSettings se;
	//https://coolors.co/caba68-b26e63-8c406e-136f63-72a8d5
	private static Cairo.Color[] colors = {
		new Cairo.Color(0.79, 0.73, 0.41),
		new Cairo.Color(0.70, 0.43, 0.39),
		new Cairo.Color(0.55, 0.25, 0.43),
		new Cairo.Color(0.07, 0.44, 0.39),
		new Cairo.Color(0.45, 0.66, 0.84)
		};

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

	}
	private void ScrollZoom (object o, ScrollEventArgs args) {
		double oldscale = scale;
		if(args.Event.Direction == ScrollDirection.Up || args.Event.Direction == ScrollDirection.Down) {
			scale *= 1 + ((args.Event.Direction == ScrollDirection.Up ? -0.08 : 0.08) * OrbitSettings.ZoomSensitivity);
			scale = Math.Max(5, scale); //Limit to 1px per 5km
			scale = Math.Min(Double.MaxValue, scale);
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

		cr.LineWidth = arrowLineWidth;
		cr.Stroke();
	}
    private void Draw(object sender, DrawnArgs args)
    {
		Shared.ReadyDrawingCopy();

		double inverseScale = 1 / scale;

		Vector2d drawOffset = offset;
		if(Shared.trackedMass > -1 && Shared.drawingCopy.ContainsKey(Shared.trackedMass)) {
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
				for(int index = 0; index < Shared.drawingCopy.Count; index++) {
					MassInfo m = Shared.drawingCopy[index];
					if(!m.hasTrail || m.stationary || !m.currentlyUpdatingPhysics) { continue; }

					Vector2d[] trail = m.trail;
					int trailOffset = m.trailOffset;
					int trailLength = trail.Length;

					double transparency = 0.1;
					
					int perUpdate = trailLength / 10;
					int counter = 0;

					//Add the offset if the object is drawn relative to another
					Vector2d followingPosition = m.followingIndex > -1 ? 
						Shared.drawingCopy[m.followingIndex].position : new Vector2d(0,0);
					
					for(int i = trailOffset + 1; i < trailOffset + trailLength; i++) {
						Vector2d point = WorldToScreen(trail[i % trailLength] + followingPosition, inverseScale, drawOffset, windowCenter);
						cr.LineTo(point.X, point.Y);

						if(counter > perUpdate) {
							cr.SetSourceRGBA(0.6, 0.6, 0.6, transparency += 0.1);
							cr.Stroke();
							counter = -1;
							i--;
						}
						counter++;
					}
					Vector2d finalPoint = WorldToScreen(m.position, inverseScale, drawOffset, windowCenter);
					cr.LineTo(finalPoint.X, finalPoint.Y);
					cr.Stroke();
				}
			}
		int colorIndex = 0;

		//Draw mass circles
		if(se.drawMasses) {
			for(int index = 0; index < Shared.drawingCopy.Count; index++) {
				cr.SetSourceRGB(0,0,0);
				MassInfo m = Shared.drawingCopy[index];
				Vector2d position = !m.currentlyUpdatingPhysics && m.orbitingBodyIndex > -1
					? Shared.drawingCopy[m.orbitingBodyIndex].position + m.position : m.position;
				Vector2d point = WorldToScreen(position, inverseScale, drawOffset, windowCenter);

				cr.Arc(point.X, point.Y, MassToRadius(m.mass, inverseScale), 0, 2 * Math.PI);
				cr.StrokePreserve(); //Saves circle for filling in
				
				double radius = MassToGlyphSize(m.mass, inverseScale);
				if (radius > 0) {		
					cr.SetFontSize((int)radius);
					cr.ShowText(m.name);
				}

				cr.Fill(); //Currently fills with same color as outline
				
				//Draw velocity arrows
				if(se.drawVelocityVectors && radius > 0 && !m.stationary && m.currentlyUpdatingPhysics) {
					cr.SetSourceRGB(0.89, 0.34, 0.3);
					
					Vector2d direction;
					if(m.followingIndex > -1) { direction = (m.velocity - Shared.drawingCopy[m.followingIndex].velocity) * 1.25; }
					else { direction = (m.velocity) * 1.25; }
					
					if(se.normalizeVelocity) {direction = direction.Normalize() * 38; };
					DrawArrow(cr, point, direction, (int)(radius / 4 - 0.5));
				}
				

				//Draw scaled force arrows
				if(se.drawForceVectors && radius > 0 && !m.stationary && m.currentlyUpdatingPhysics) {
					if(se.linearForces && m.forces.Count > 0) {
						//Scale relative to the largest force on a per object basis
						double scale = m.forces.Max().Magnitude() / 45;

						foreach(Vector2d force in m.forces) {	
							double len = force.Magnitude() / scale;
							if(len > 0.002) {
								colorIndex = (colorIndex + 1) % 5;
								cr.SetSourceColor(colors[colorIndex]);
								DrawArrow(cr, point, force.Normalize() * len, (int)(radius / 8));
							}			
						}
					}
					else if(m.forces.Count > 0){
						//Scale relative to a log10 scale
						foreach(Vector2d force in m.forces) {	
							cr.SetSourceColor(colors[colorIndex]);
							colorIndex = (colorIndex + 1) % 5;			
							DrawArrow(cr, point, force.Normalize() * Math.Log(force.Magnitude()) * 2.25, (int)(radius / 8));
						}
					}

				}
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