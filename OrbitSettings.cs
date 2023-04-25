using Gtk;

namespace Orbit;

public class OrbitSettings : Gtk.ListBox {
    ComboBoxText timeUnits;
    Scale timeScale;
    internal CheckButton paused;
    public static double ZoomSensitivity = 1;
    OrbitSessionSettings se;
    internal Dictionary<string, MenuItem> MenuButtons = new();
    public OrbitSettings(OrbitSessionSettings se) {
        this.se = se;
        WidthRequest = 100;
        SelectionMode = SelectionMode.None;

        //Initialize the menu bar
        MenuBar optionMenu = new MenuBar();

        MenuItem file = new MenuItem("File");
        MenuItem mass = new MenuItem("Mass");
        MenuItem view = new MenuItem("View");

        Menu fileMenu = new();
        Menu massMenu = new();
        Menu viewMenu = new();

        optionMenu.Append(file);
        optionMenu.Append(mass);
        optionMenu.Append(view);
        
        file.Submenu = fileMenu;
        mass.Submenu = massMenu;
        view.Submenu = viewMenu;

        //Initialize the file menu
        MenuItem save = new("Save to file");
        save.Activated += (object? o, EventArgs a) => {
            se.middleWidgetState = "Start Save";
        };

        MenuItem load = new("Load from file");
        load.Activated += (object? o, EventArgs a) => {
            se.middleWidgetState = "Start Load";
        };

        MenuItem loadPreset = new("Load Preset");
            Menu presets = new();
            loadPreset.Submenu = presets;

            MenuItem solarSystem = new("Solar System");
                presets.Append(solarSystem);
                solarSystem.Activated += (object? o, EventArgs e) => {
                    LoadPreset("solar system");
                };

            MenuItem hulseTaylor = new ("Hulse-Taylor binary");
                presets.Append(hulseTaylor);
                hulseTaylor.Activated += (object? o, EventArgs e) => {
                    LoadPreset("hulse-taylor binary");
                };

        MenuItem about = new("About");
        about.Activated += (object? o, EventArgs a) => {
            ShowAboutDialog();
        };

        fileMenu.Append(save);
        fileMenu.Append(new SeparatorMenuItem());
        fileMenu.Append(load);
        fileMenu.Append(loadPreset);
        fileMenu.Append(new SeparatorMenuItem());
        fileMenu.Append(about);

        //Initalize the mass menu
        MenuItem add = new("Add Mass");
        MenuItem remove = new("Remove Mass");

        Menu addMenu = new();
        Menu removeMenu = new();

        add.Submenu = addMenu;
        remove.Submenu = removeMenu;

        massMenu.Append(add);
        massMenu.Append(remove);

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
                //Are you sure you want to remove all masses window
                MessageDialog m = new Gtk.MessageDialog(new Window(WindowType.Popup), DialogFlags.Modal, MessageType.Warning, ButtonsType.OkCancel, "Remove all objects?");
                    Window.GetOrigin(out int x, out int y);
                    m.Move(x, y);
                    ResponseType result = (ResponseType)m.Run();
                    m.Destroy();
                    if (result == Gtk.ResponseType.Ok)
                    {
                        Shared.changesToMake.Push(new string[] {"remove all masses", "", "-1"});
                    }
            };
        removeMenu.Append(removeCurrentMass);
        removeMenu.Append(removeAllMasses);

        //Initalize the view menu
        CheckMenuItem highResolutionMode = new("High Resolution Mode");
            highResolutionMode.Toggled += (object? o, EventArgs a) => { ChangeTime(); };
            MenuButtons.Add("High Resolution Mode", highResolutionMode);

        CheckMenuItem globalDrawTrails = new("Draw Trails");
            globalDrawTrails.Active = se.drawTrails;
            globalDrawTrails.Toggled += (object? o, EventArgs a) => {se.drawTrails = globalDrawTrails.Active;};
            MenuButtons.Add("Global Trail Draw", globalDrawTrails);

        MenuItem positionUnits = new("Set Position Unit");
        Menu positionUnitsMenu = new();
        positionUnits.Submenu = positionUnitsMenu;

            MenuItem kmPositionOption = new("km");
            kmPositionOption.Activated += (object? o, EventArgs e) => {
                se.positionDisplayUnits = "km";
            };
            MenuItem auPositionOption = new("AU");
            auPositionOption.Activated += (object? o, EventArgs e) => {
                se.positionDisplayUnits = "AU";
            };

            positionUnitsMenu.Append(auPositionOption);
            positionUnitsMenu.Append(kmPositionOption);

        MenuItem massUnits = new("Set Mass Unit");
        Menu massUnitsMenu = new();
        massUnits.Submenu = massUnitsMenu;

            MenuItem ggMassOption = new("Gg");
            ggMassOption.Activated += (object? o, EventArgs e) => {
                se.massDisplayUnits = "Gg";
            };
            MenuItem kgMassOption = new("kg");
            kgMassOption.Activated += (object? o, EventArgs e) => {
                se.massDisplayUnits = "kg";
            };
            MenuItem eaMassOption = new("Earth");
            eaMassOption.Activated += (object? o, EventArgs e) => {
                se.massDisplayUnits = "Earth";
            };
            MenuItem soMassOption = new("Solar");
            soMassOption.Activated += (object? o, EventArgs e) => {
                se.massDisplayUnits = "Solar";
            };

            massUnitsMenu.Append(eaMassOption);
            massUnitsMenu.Append(soMassOption);
            massUnitsMenu.Append(ggMassOption);
            massUnitsMenu.Append(kgMassOption);

        
        CheckMenuItem drawTrails = new("Toggle All Trails");
            drawTrails.Active = true;
            drawTrails.Toggled += (object? o, EventArgs a) => {
                for(int i = 0; i < Shared.massObjects; i++) {
                    Shared.drawingCopy[i].hasTrail = drawTrails.Active;
                }
            };
        MenuItem resetTrailsToMass = new("Follow Current Mass");
            resetTrailsToMass.Activated += (object? o, EventArgs a) => {

                for(int i = 0; i < Shared.massObjects; i++) {
                    if(i != Shared.selectedMassIndex) {
                        Shared.changesToMake.Push(new string[] {"trail follow", Shared.selectedMassIndex.ToString(), i.ToString()});
                    }
                }
                
            };
        viewMenu.Append(highResolutionMode);
        viewMenu.Append(globalDrawTrails);
        viewMenu.Append(positionUnits);
        viewMenu.Append(massUnits);
        viewMenu.Append(drawTrails);
        viewMenu.Append(resetTrailsToMass);

        //End of menu bar initalization

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
            timeUnits.AppendText("Years");
            timeUnits.AppendText("Decades");
            timeUnits.Changed += UnitChange;

            paused = new("Pause");
            paused.Toggled += (object? o, EventArgs a) => {Shared.Paused = paused.Active;};
            time.Add(timeUnits);
            time.Add(paused);
            Add(time);
        
        timeScale = new(Orientation.Horizontal, 0,52,1);
            timeScale.ValueChanged += (object? o, EventArgs args) => {ChangeTime();}; 
            Add(timeScale);

        //Start the time at weeks
        timeUnits.Active = 4;
        ChangeTime();

        Add(new Separator(Orientation.Horizontal));
        
        Label sensitivityLabel = new("Zoom Sensitivity");
        Scale sensitivity = new(Orientation.Horizontal, 0.5, 5, 0.1);
        sensitivity.Value = 1;
        sensitivity.ValueChanged += (object? o, EventArgs a) => {ZoomSensitivity = sensitivity.Value;};
        Add(sensitivityLabel);
        Add(sensitivity);

        Add(new Separator(Orientation.Horizontal));

        Add(new Label("Show Vectors"));
        HBox vectorBox = new();

        CheckButton drawVelocityVectors = new("Velocity");
        CheckButton drawForceVectors = new("Forces");
        drawVelocityVectors.Toggled += (object? o, EventArgs a) => { se.drawVelocityVectors = drawVelocityVectors.Active; };
        drawForceVectors.Toggled += (object? o, EventArgs a) => { se.drawForceVectors = drawForceVectors.Active; };

        vectorBox.Add(drawVelocityVectors);
        vectorBox.Add(drawForceVectors);
        Add(vectorBox);

        CheckButton normalizeVelocity = new("Normalize Velocity");
        normalizeVelocity.Toggled += (object? o, EventArgs a) => { se.normalizeVelocity = normalizeVelocity.Active; };
        Add(normalizeVelocity);
    }
private void ShowAboutDialog() {
        AboutDialog about = new AboutDialog();
        about.Decorated = true;
        about.ProgramName = "Orbit";
        //about.Version = "0.1";
        about.Copyright = "(c) Jan Bodnar";
        about.Comments = @"Battery is a simple tool for 
battery checking";
        //about.Website = "http://www.zetcode.com";
        //about.Logo = new Gdk.Pixbuf("battery.png");
        about.Run();
        //about.Destroy();
}
    private void LoadPreset(string key) {
        switch(key) {
            case "solar system":
                Shared.changesToMake.Push(new string[] {"load preset", "solar system", "-1"});
                break;

            case "hulse-taylor binary":
                Shared.changesToMake.Push(new string[] {"load preset", "hulse-taylor binary", "-1"});
                timeUnits!.Active = 1;
                timeScale!.Value = 30;
                ChangeTime();
                break;
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
            timeScale.SetRange(1, 14);
            multiplier = timeScale.Value * 86400;
        }
        else if(timeUnits.Active == 4) {
            timeScale.SetRange(1, 52);
            multiplier = timeScale.Value * 604800;
        }
        else if(timeUnits.Active == 5) {
            timeScale.SetRange(1, 20);
            multiplier = timeScale.Value * 31449600;
        }
        else if(timeUnits.Active == 6) {
            timeScale.SetRange(1, 6);
            multiplier = timeScale.Value * 314496000;
        }
        Shared.multiplier = multiplier;
        if(((CheckMenuItem)MenuButtons["High Resolution Mode"]).Active) {
            Shared.deltaTime = Math.Min(3600 * 8, multiplier / 30);
        }
        else {
            Shared.deltaTime = multiplier / 60;
        }
    }
    private void UnitChange(object? o, EventArgs args) {
        ChangeTime();
        timeScale.SetIncrements(1, 1);
        timeScale.Value = 1;
    }
}