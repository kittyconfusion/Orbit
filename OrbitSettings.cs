using Gtk;

namespace Orbit;

public class OrbitSettings : Gtk.ListBox {
    ComboBoxText timeUnits;
    Scale timeScale;
    internal CheckButton paused;
    internal Frame saveLoadFrame;
    bool saving;
    internal static double ZoomSensitivity = 1;
    OrbitSessionSettings se;
    internal Dictionary<string, Widget> MenuButtons = new();
    public OrbitSettings(OrbitSessionSettings se) {
        this.se = se;
        WidthRequest = 100;
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
            highResolutionMode.Active = true;
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
    internal void ShowAboutDialog() {
            AboutDialog about = new AboutDialog();
            about.Decorated = true;
            about.ProgramName = "Orbit";
            about.Copyright = "Nathan Williams 2023";
            about.Comments = @"The gravity thing that works-ish";
            if(File.Exists("orbiticon.png")) {
                about.Logo = new Gdk.Pixbuf("orbiticon.png");
            }
            about.Run();
            about.Destroy();
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