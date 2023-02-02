using Gtk;
using Gdk;
using System.Collections.Specialized;

namespace Orbit;

public class OrbitSettings : Gtk.ListBox {
    ComboBoxText timeUnits;
    HScale timeScale;
    public static double ZoomSensitivity = 1;
    public OrbitSettings() {
        WidthRequest = 100;

        timeUnits = new();
        timeUnits.AppendText("Seconds");
        timeUnits.AppendText("Minutes");
        timeUnits.AppendText("Hours");
        timeUnits.AppendText("Days");
        timeUnits.AppendText("Weeks");
        timeUnits.Changed += UnitChange;
        Add(timeUnits);
        
        timeScale = new(0,60,1);
        timeScale.ValueChanged += UnitChange; 
        Add(timeScale);

        timeUnits.Active = 3;
        timeScale.Value = Shared.multiplier / 86400;
        
        Label sensitivityLabel = new("Zoom Sensitivity");
        HScale sensitivity = new(0.5, 4, 0.1);
        sensitivity.Value = 1;
        sensitivity.ValueChanged += (object? o, EventArgs a) => {ZoomSensitivity = sensitivity.Value;};
        Add(sensitivityLabel);
        Add(sensitivity);

        for(int i = 0; i <= 3; i++) {
            this.GetRowAtIndex(i).Selectable = false;
        }
    }

    private void ChangeTime() {
        double multiplier = 1;
        if (timeUnits.Active == 0) {
            timeScale.SetRange(1, 60);
            multiplier = timeScale.Value;
        }
        else if (timeUnits.Active == 1) {
            timeScale.SetRange(1, 60);
            multiplier = timeScale.Value * 60;
        }
        else if (timeUnits.Active == 2) {
            timeScale.SetRange(1, 24);
            multiplier = timeScale.Value * 3600;
        }
        else if (timeUnits.Active == 3) {
            timeScale.SetRange(1, 10);
            multiplier = timeScale.Value * 86400;
        }
        else if(timeUnits.Active == 4) {
            timeScale.SetRange(1, 8);
            multiplier = timeScale.Value * 604800;
        }
        Shared.multiplier = multiplier;
        Shared.deltaTime = Math.Min(3600 * 8, multiplier / 30);
    }
    private void UnitChange(object? o, EventArgs args) {
        ChangeTime();
    }
}