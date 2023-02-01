using Gtk;
using Gdk;
using System.Collections.Specialized;

namespace Orbit;

public class OrbitSettings : Gtk.ListBox {
    ComboBoxText timeUnits;
    HScale timeScale;

    public OrbitSettings() {
        WidthRequest = 140;

        timeUnits = new();
        timeUnits.AppendText("Seconds");
        timeUnits.AppendText("Minutes");
        timeUnits.AppendText("Hours");
        timeUnits.AppendText("Days");
        timeUnits.Changed += UnitChange;
        Add(timeUnits);
        
        timeScale = new(0,60,1);
        timeScale.ValueChanged += UnitChange; 
        Add(timeScale);

        timeUnits.Active = 3;
        timeScale.Value = Shared.multiplier / 86400;
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

        Shared.multiplier = multiplier;
        Shared.deltaTime = multiplier / 30;
    }
    private void UnitChange(object? o, EventArgs args) {
        ChangeTime();
    }
}