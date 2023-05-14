using Gtk;

namespace Orbit;

public class OrbitSettings : Gtk.ListBox {
    ComboBoxText timeUnits;
    Scale timeScale;
    internal CheckButton paused;
    internal Frame saveLoadFrame;
    bool saving;
    internal int expectedResolutionMode;
    internal static double ZoomSensitivity = 1;
    OrbitSessionSettings se;
    internal Dictionary<string, Widget> MenuButtons = new();
    public OrbitSettings(OrbitSessionSettings se) {
        this.se = se;
        WidthRequest = 140;
        SelectionMode = SelectionMode.None;

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
            saveLoadFrame!.Label = "Filepath to save to";
            ((Button)MenuButtons["saveLoadInputButton"]!).Label = "Save";
            paused!.Active = true;
            saveLoadFrame!.Show();
            saving = true;
        };

        MenuItem load = new("Load from file");
        load.Activated += (object? o, EventArgs a) => {
            saveLoadFrame!.Label = "Filepath to load from";
            ((Button)MenuButtons["saveLoadInputButton"]!).Label = "Load";
            paused!.Active = true;
            saveLoadFrame!.Show();
            saving = false;
        };

        MenuItem loadPreset = new("Load Preset");
            Menu presets = new();
            loadPreset.Submenu = presets;

            MenuItem solarSystem = new("Solar System");
                presets.Append(solarSystem);
                solarSystem.Activated += delegate { LoadPreset("solar system"); };

            MenuItem innerSolarSystem = new("Inner Solar System");
                presets.Append(innerSolarSystem);
                innerSolarSystem.Activated += delegate { LoadPreset("inner solar system"); };

            MenuItem proximaCentauri = new("Proxima Centauri");
                presets.Append(proximaCentauri);
                proximaCentauri.Activated += delegate {LoadPreset("proxima centauri"); };

            MenuItem hulseTaylor = new ("Hulse-Taylor binary");
                presets.Append(hulseTaylor);
                hulseTaylor.Activated += delegate { LoadPreset("hulse-taylor binary"); };

            MenuItem gliese876 = new ("Gliese 876");
                presets.Append(gliese876);
                gliese876.Activated += delegate { LoadPreset("gliese 876"); };

            MenuItem pluto = new ("Pluto");
                presets.Append(pluto);
                pluto.Activated += delegate { LoadPreset("pluto"); };

        fileMenu.Append(save);
        fileMenu.Append(new SeparatorMenuItem());
        fileMenu.Append(load);
        fileMenu.Append(loadPreset);
        //Initalize the mass menu
        MenuItem newMass = new("New Mass");
            newMass.Activated += (object? o, EventArgs e) => {
                paused!.Active = true;
                Shared.Paused = true;
                Shared.changesToMake.Push(new string[] {"new mass","","-1"});
            };

        MenuItem removeCurrentMass = new("Remove Current");
            removeCurrentMass.Activated += (object? o, EventArgs e) => {
                if(Shared.selectedMassIndex > -1) {
                    lock(Shared.DataLock) {
                        Shared.changesToMake.Push(new string[] {"remove mass", "", Shared.selectedMassIndex.ToString()});
                        Shared.RemoveDrawingMass(Shared.selectedMassIndex);
                        Shared.needToRefresh = true;
                    }
                }
            };
        MenuItem removeAllMasses = new("Remove All");

            removeAllMasses.Activated += (object? o, EventArgs e) => {
                //Are you sure you want to remove all masses window
                MessageDialog m = new Gtk.MessageDialog(new Window(WindowType.Popup), DialogFlags.Modal, MessageType.Warning, ButtonsType.OkCancel, "Remove all objects?");
                    Window.GetOrigin(out int x, out int y);
                    m.Move(x, y);
                    ResponseType result = (ResponseType)m.Run();
                    m.Dispose();
                    if (result == Gtk.ResponseType.Ok)
                    {
                        lock(Shared.DataLock) {
                            Shared.drawingCopy.Clear();
                            Shared.needToRefresh = true;
                        }
                        Shared.changesToMake.Push(new string[] {"remove all masses", "", "-1"});
                    }
            };
            
        massMenu.Append(newMass);
        massMenu.Append(new SeparatorMenuItem());
        massMenu.Append(removeCurrentMass);
        massMenu.Append(removeAllMasses);

        //Initalize the view menu

        CheckMenuItem globalDrawTrails = new("Draw Trails");
            globalDrawTrails.Active = se.drawTrails;
            globalDrawTrails.Toggled += (object? o, EventArgs a) => {se.drawTrails = globalDrawTrails.Active;};
            MenuButtons.Add("Global Trail Draw", globalDrawTrails);

        CheckMenuItem drawMassesAndLabels = new("Draw Masses");
            drawMassesAndLabels.Active = se.drawMasses;
            drawMassesAndLabels.Toggled += delegate { se.drawMasses = drawMassesAndLabels.Active; };

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
                    Shared.changesToMake.Push(new string[] {"has trail", drawTrails.Active.ToString(), i.ToString()});
                }
            };
        MenuItem resetTrailsToMass = new("Global Follow Current Mass");
            resetTrailsToMass.Activated += (object? o, EventArgs a) => {

                for(int i = 0; i < Shared.massObjects; i++) {
                    if(i != Shared.selectedMassIndex) {
                        Shared.changesToMake.Push(new string[] {"trail follow", Shared.selectedMassIndex.ToString(), i.ToString()});
                    }
                }
                
            };

        viewMenu.Append(new SeparatorMenuItem());
        viewMenu.Append(globalDrawTrails);
        viewMenu.Append(drawTrails);
        viewMenu.Append(drawMassesAndLabels);
        viewMenu.Append(new SeparatorMenuItem());
        viewMenu.Append(resetTrailsToMass);
        viewMenu.Append(new SeparatorMenuItem());
        viewMenu.Append(positionUnits);
        viewMenu.Append(massUnits);

        //End of menu bar initalization

        Add(optionMenu);

        //Start of save/load

        saveLoadFrame = new("Filepath");
        
        VBox saveLoadBox = new();
        HBox saveLoadButtonBox = new();

        Label warningLabel = new();
            warningLabel.Halign = Align.End;
            MenuButtons.Add("warningLabel", warningLabel);

        Entry filepath = new(Directory.GetCurrentDirectory() + @"\");
            filepath.Changed += (object? o, EventArgs a) => {
                FilepathTextChanged(filepath.Text);
            };
        Button cancelButton = new("Cancel");
            cancelButton.Pressed += (object? o, EventArgs a) => {
                saveLoadFrame.Hide();
            };
        Button inputButton = new("Save");
            inputButton.Pressed += (object? o, EventArgs a) => {
                FileSubmitButtonPressed(filepath.Text);
            };
            MenuButtons.Add("saveLoadInputButton", inputButton);

        saveLoadFrame.Add(saveLoadBox);
        saveLoadBox.Add(warningLabel);
        saveLoadBox.Add(filepath);
        saveLoadBox.Add(saveLoadButtonBox);
        saveLoadButtonBox.Add(cancelButton);
        saveLoadButtonBox.Add(inputButton);
        
        Add(saveLoadFrame);
        saveLoadFrame.BorderWidth = 4;
        saveLoadFrame.MarginBottom = 8;

        saveLoadBox.Add(new Separator(Orientation.Horizontal));

        //End of save/load

        //Start of session settings

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
        
        //Zoom sensitivity

        Label sensitivityLabel = new("Zoom Sensitivity");
        Scale sensitivity = new(Orientation.Horizontal, 0.5, 4, 0.1);
        sensitivity.Value = 1;
        sensitivity.ValueChanged += (object? o, EventArgs a) => {ZoomSensitivity = sensitivity.Value;};
        Add(sensitivityLabel);
        Add(sensitivity);

        Add(new Separator(Orientation.Horizontal));
        
        //Vectors

        Add(new Label("Show Vectors"));
        HBox velocityBox = new();
        HBox forceBox = new();

        CheckButton drawVelocityVectors = new("Velocity");
        CheckButton normalizeVelocity = new("Normalize");

        CheckButton drawForceVectors = new("Forces");
        RadioButton logForces = new("Log");
        RadioButton linearForces = new(logForces, "Linear");

        logForces.Pressed += delegate { se.linearForces = false; };
        linearForces.Pressed += delegate { se.linearForces = true; };

        MenuButtons.Add("Normalize", normalizeVelocity);
        MenuButtons.Add("Log", logForces);
        MenuButtons.Add("Linear", linearForces);

        drawVelocityVectors.Toggled += (object? o, EventArgs a) => { 
            se.drawVelocityVectors = drawVelocityVectors.Active; 
            if(drawVelocityVectors.Active) { normalizeVelocity.Show(); }
            else { normalizeVelocity.Hide(); }
        };
        drawForceVectors.Toggled += (object? o, EventArgs a) => { 
            se.drawForceVectors = drawForceVectors.Active;
            if(drawForceVectors.Active) { logForces.Show(); linearForces.Show(); }
            else { logForces.Hide(); linearForces.Hide(); }
        };
        normalizeVelocity.Toggled += (object? o, EventArgs a) => { se.normalizeVelocity = normalizeVelocity.Active; };

        velocityBox.Add(drawVelocityVectors);
        velocityBox.Add(normalizeVelocity);
        forceBox.Add(drawForceVectors);
        forceBox.Add(logForces);
        forceBox.Add(linearForces);
        Add(velocityBox);
        Add(forceBox);

        Add(new Separator(Orientation.Horizontal));

        //Resolution mode
        Label resolution = new("Resolution Mode");
        RadioButton noRestriction = new(                 "No Restriction");
        RadioButton lowRestriction = new(noRestriction,  "Low    (1 week)");
        RadioButton midRestriction = new(noRestriction,  "Medium (8 hours)");
        RadioButton highRestriction = new(noRestriction, "High   (10 minutes)");

        MenuButtons.Add("resolution0", noRestriction);
        MenuButtons.Add("resolution1", lowRestriction);
        MenuButtons.Add("resolution2", midRestriction);
        MenuButtons.Add("resolution3", highRestriction);

        noRestriction.Pressed += delegate { SetResolutionMode(0); };
        lowRestriction.Pressed += delegate { SetResolutionMode(1); };
        midRestriction.Pressed += delegate { SetResolutionMode(2); };
        highRestriction.Pressed += delegate { SetResolutionMode(3); };

        Add(resolution);
        Add(noRestriction);
        Add(lowRestriction);
        Add(midRestriction);
        Add(highRestriction);
        
        Add(new Separator(Orientation.Horizontal));
    }
    internal void InitHide() {
        MenuButtons["Normalize"].Hide();
        MenuButtons["Log"].Hide();
        MenuButtons["Linear"].Hide();
        saveLoadFrame.Hide();
    }
    internal void SetResolutionMode(int mode) {
        if(MenuButtons.ContainsKey("resolution" + mode)) {
            ((RadioButton)MenuButtons["resolution" + mode]).Active = true;
            Shared.resolutionMode = mode;
            expectedResolutionMode = mode;
            ChangeTime();
        }
    }
    internal void SetWarningLabel(string text, string color) {
        ((Label)MenuButtons["warningLabel"]).Markup = $"<span foreground='{color}' font-weight= 'bold'>{text} </span>";
    }
    internal void FilepathTextChanged(string filepath) {
        //If in save mode and file already exists, put a warning
        
        if(saving) {
            if((File.Exists(filepath) || File.Exists(filepath + ".json")) && System.IO.Path.GetFileNameWithoutExtension(filepath) != "") {
                SetWarningLabel("File already exists", "orange");
            }
            else {
                SetWarningLabel("", "orange");
            }
        }

    }
    internal void FileSubmitButtonPressed(string filepath) {
        if(saving) {
            if(!Directory.Exists(System.IO.Path.GetDirectoryName(filepath))) {
                SetWarningLabel("Directory does not exist", "red");
            }
            else if(System.IO.Path.GetFileNameWithoutExtension(filepath) == "") {
                SetWarningLabel("File name is empty", "red");
            }
            else {
                //Save
                Shared.changesToMake.Push(new string[] {"save", filepath, "-1"});
                saveLoadFrame.Hide();
                SetWarningLabel("", "orange");
            }
        }
        else {
            if(File.Exists(filepath)) {
                //Load
                Shared.changesToMake.Push(new string[] {"load", filepath, "-1"});
                saveLoadFrame.Hide();
                SetWarningLabel("", "orange");
            }
            else if(File.Exists(filepath + ".json")) {
                //Load modified filepath
                Shared.changesToMake.Push(new string[] {"load", filepath + ".json", "-1"});
                saveLoadFrame.Hide();
                SetWarningLabel("", "orange");
            }
            else if(!Directory.Exists(System.IO.Path.GetDirectoryName(filepath))) {
                SetWarningLabel("Directory does not exist", "red");
            }
            else {
                SetWarningLabel("File does not exist", "red");
            }
        }
    }
    private void LoadPreset(string key) {

        Shared.changesToMake.Push(new string[] {"load preset", key, "-1"});
        lock(Shared.DataLock) {
            Shared.drawingCopy.Clear();
            Shared.needToRefresh = true;
        }
        switch(key) {
            case "pluto":
                timeUnits!.Active = 4;
                timeScale.Value = 1;
                ChangeTime();
                break;
            case "gliese 876":
                timeUnits!.Active = 3;
                timeScale!.Value = 4;
                ChangeTime();
                break;

            case "proxima centauri":
                timeUnits!.Active = 3;
                timeScale!.Value = 1;
                ChangeTime();
                break;

            case "solar system":
                break;
            
            case "inner solar system":
                break;

            case "hulse-taylor binary":
                //Turn down the time scale
                timeUnits!.Active = 2;
                timeScale!.Value = 1;
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

        switch(Shared.resolutionMode) {
            case 0:
                Shared.deltaTime = multiplier / 60;
                break;
            case 1:
                Shared.deltaTime = Math.Min(Constant.WEEKS, multiplier / 60);
                break;
            case 2:
                Shared.deltaTime = Math.Min(Constant.HOURS * 8, multiplier / 60);
                break;
            case 3:
                Shared.deltaTime = Math.Min(Constant.MINUTES * 10, multiplier / 60);
                break;
        }
    }
    private void UnitChange(object? o, EventArgs args) {
        ChangeTime();
        timeScale.SetIncrements(1, 1);
        timeScale.Value = 1;
    }
}