using Gtk;

namespace Orbit;

public class OrbitSettings : Gtk.ListBox {
    ComboBoxText timeUnits;
    HScale timeScale;
    internal CheckButton paused;
    public static double ZoomSensitivity = 1;
    public OrbitSettings() {
        WidthRequest = 100;
        SelectionMode = SelectionMode.None;

        MenuBar optionMenu = new MenuBar();
        MenuItem add = new MenuItem("Add");
        MenuItem remove = new MenuItem("Remove");
        Menu addMenu = new();
        Menu removeMenu = new();
        add.Submenu = addMenu;
        remove.Submenu = removeMenu;

        MenuItem newMass = new("New Mass");
        newMass.Activated += (object? o, EventArgs e) => {
            paused!.Active = true;
            Shared.Paused = true;
            Shared.changesToMake.Push(new string[] {"new mass","","-1"});
        };
        addMenu.Append(newMass);

        MenuItem removeCurrentMass = new("Current Mass");
        removeCurrentMass.Activated += (object? o, EventArgs e) => {
            if(Shared.selectedMassIndex > -1) {
                Shared.changesToMake.Push(new string[] {"remove mass", "", Shared.selectedMassIndex.ToString()});
            }
        };
        MenuItem removeAllMasses = new("All Masses");

        removeAllMasses.Activated += (object? o, EventArgs e) => {
            MessageDialog m = new Gtk.MessageDialog(new Window(WindowType.Popup), DialogFlags.Modal, MessageType.Warning, ButtonsType.OkCancel, "Remove all objects?");
                Window.GetOrigin(out int x, out int y);
                m.Move(x, y);
                ResponseType result = (ResponseType)m.Run();
                m.Destroy();
                if (result == Gtk.ResponseType.Ok)
                {
                    for(int i = 0; i < Shared.massObjects; i++) {
                        Shared.changesToMake.Push(new string[] {"remove mass", "", i.ToString()});
                    }
                }
        };
        removeMenu.Append(removeCurrentMass);
        removeMenu.Append(removeAllMasses);

        optionMenu.Append(add);
        optionMenu.Append(remove);

        VBox vbox = new VBox(false, 2);
        vbox.PackStart(optionMenu, false, false, 0);

        Add(vbox);

        HBox time = new();
        timeUnits = new();
        timeUnits.AppendText("Seconds");
        timeUnits.AppendText("Minutes");
        timeUnits.AppendText("Hours");
        timeUnits.AppendText("Days");
        timeUnits.AppendText("Weeks");
        timeUnits.Changed += UnitChange;

        paused = new("Pause");
        paused.Toggled += (object? o, EventArgs a) => {Shared.Paused = paused.Active;};
        time.Add(timeUnits);
        time.Add(paused);
        Add(time);
        
        timeScale = new(0,52,1);
        timeScale.ValueChanged += (object? o, EventArgs args) => {ChangeTime();}; 
        Add(timeScale);

        timeUnits.Active = 4;
        ChangeTime();
        
        Label sensitivityLabel = new("Zoom Sensitivity");
        HScale sensitivity = new(0.5, 4, 0.1);
        sensitivity.Value = 1;
        sensitivity.ValueChanged += (object? o, EventArgs a) => {ZoomSensitivity = sensitivity.Value;};
        Add(sensitivityLabel);
        Add(sensitivity);
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
            timeScale.SetRange(1, 14);
            multiplier = timeScale.Value * 86400;
        }
        else if(timeUnits.Active == 4) {
            timeScale.SetRange(1, 52);
            multiplier = timeScale.Value * 604800;
        }
        Shared.multiplier = multiplier;
        Shared.deltaTime = Math.Min(3600 * 8, multiplier / 30);
    }
    private void UnitChange(object? o, EventArgs args) {
        ChangeTime();
        timeScale.SetIncrements(1, 1);
        timeScale.Value = 1;
    }
}