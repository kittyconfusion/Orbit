using Gtk;
using System.Collections.Specialized;

namespace Orbit;

public class OrbitInfo : Gtk.ListBox {
    ComboBoxText massChoose = new();
    ComboBoxText followChoose = new();
    List<Widget> PrimaryWidgets = new();
    OrderedDictionary UpdatableWidgets = new();
    MassInfo selectedMass = new();
    int expectedNumberOfMasses = 0;
    static readonly string[] UpdateKeys = {"position", "velocity", "mass"};
    OrbitSessionSettings se;
    public OrbitInfo(OrbitSessionSettings se) {
        this.se = se;
        WidthRequest = 140;
        SelectionMode = SelectionMode.None;

        //Needed for keyboard input
		CanFocus = true;

        Label massBoxLabel = new("Choose a mass");
        Add(massBoxLabel);

        HBox massBox = new();
        massBox.Add(massChoose);
        
        CheckButton toFollow = new("Follow");
        toFollow.Toggled += (object? o, EventArgs a) => {
            if(toFollow.Active) {
                Shared.trackedMass = massChoose.Active - 1;
            }
            else {
                Shared.trackedMass = -1;
            }
        };
        massBox.Add(toFollow);
        Add(massBox);

        UpdatableWidgets.Add("toFollow", toFollow);

        PrimaryWidgets.Add(new Separator(Orientation.Horizontal));

        PrimaryWidgets.Add(new Label("Name"));
        Entry name = new();
        UpdatableWidgets.Add("name", name);
        PrimaryWidgets.Add(name);
        name.Activated += (object? o, EventArgs a) => {
            GetRowAtIndex(4).GrabFocus();
            Shared.drawingCopy[Shared.selectedMassIndex].name = name.Text;
            RefreshMassChoose();
        };

        PrimaryWidgets.Add(new Separator(Orientation.Horizontal));

        Label positionLabel = new("Position (AU)");
        UpdatableWidgets.Add("positionLabel", positionLabel);
        PrimaryWidgets.Add(positionLabel);

        Entry position = new();
        UpdatableWidgets.Add("position", position);
        PrimaryWidgets.Add(position);
        position.Activated += (object? o, EventArgs a) => {
            GetRowAtIndex(7).GrabFocus();
            Shared.changesToMake.Push(new string[] {"position" + se.positionDisplayUnits, position.Text, Shared.selectedMassIndex.ToString()});
        };

        Label velocityLabel = new("Velocity (km/s)");
        UpdatableWidgets.Add("velocityLabel", velocityLabel);
        PrimaryWidgets.Add(velocityLabel);

        Entry velocity = new();
        UpdatableWidgets.Add("velocity", velocity);
        PrimaryWidgets.Add(velocity);
        velocity.Activated += (object? o, EventArgs a) => {
            GetRowAtIndex(9).GrabFocus();
            Shared.changesToMake.Push(new string[] {"velocity", velocity.Text, Shared.selectedMassIndex.ToString()});
        };

        Label massLabel = new("Mass (Earth)");
        UpdatableWidgets.Add("massLabel", massLabel);
        PrimaryWidgets.Add(massLabel);
        
        Entry mass = new();
        UpdatableWidgets.Add("mass", mass);
        PrimaryWidgets.Add(mass);
        mass.Activated += (object? o, EventArgs a) => {
            GetRowAtIndex(11).GrabFocus();
            Shared.changesToMake.Push(new string[] {"mass" + se.massDisplayUnits, mass.Text, Shared.selectedMassIndex.ToString()});
        };

        PrimaryWidgets.Add(new Separator(Orientation.Horizontal));

        HBox trailBox = new();
        trailBox.Add(new Label("Trail"));
        trailBox.Add(followChoose);
        
        PrimaryWidgets.Add(trailBox);
        UpdatableWidgets.Add("followChoose", followChoose);

        Scale trailLength = new(Orientation.Horizontal, 0.1, 2, 0.1);
        trailLength.ValueChanged += (object? o, EventArgs args) => {
            Shared.changesToMake.Push(new string[] {"trail length", (60 * trailLength.Value).ToString(), Shared.selectedMassIndex.ToString()});
        };

        HBox trailLengthBox = new();
        trailLengthBox.Add(new Label("Length (min)"));
        trailLengthBox.Add(new Label("Ticks (per sec)"));

        PrimaryWidgets.Add(trailLengthBox);
        UpdatableWidgets.Add("trailLength", trailLength);

        Scale trailQuality = new(Orientation.Horizontal, 1, 16, 1);
        trailQuality.ValueChanged += (object? o, EventArgs args) => {
            Shared.changesToMake.Push(new string[] {"trail skip", trailQuality.Value.ToString(), Shared.selectedMassIndex.ToString()});
        };

        HBox trailQualityBox = new();
        trailQualityBox.Add(trailLength);
        trailQualityBox.Add(trailQuality);

        PrimaryWidgets.Add(trailQualityBox);
        UpdatableWidgets.Add("trailQuality", trailQuality);

        PrimaryWidgets.Add(new Separator(Orientation.Horizontal));

        HBox objectSettingBox = new();

        CheckButton trailDraw = new("Draw Trail");
        trailDraw.Toggled += (object? o, EventArgs a) => {
            selectedMass.hasTrail = trailDraw.Active;
        };
        CheckButton stationary = new("Stationary");
        stationary.Toggled += (object? o, EventArgs a) => {
            selectedMass.stationary = stationary.Active;
            Shared.changesToMake.Push(new string[] {"stationary", stationary.Active.ToString(), Shared.selectedMassIndex.ToString()});
        };

        objectSettingBox.Add(trailDraw);
        objectSettingBox.Add(stationary);
        
        PrimaryWidgets.Add(objectSettingBox);
        UpdatableWidgets.Add("trailDraw", trailDraw);
        UpdatableWidgets.Add("stationary", stationary);

        followChoose.Changed += (object? o, EventArgs args) => {
            if(followChoose.Active != -1 && Shared.selectedMassIndex != -1) {
                int index = followChoose.Active - 1 < Shared.selectedMassIndex ? followChoose.Active - 1: followChoose.Active;
                Shared.changesToMake.Push(new string[] {"trail follow", index.ToString(), Shared.selectedMassIndex.ToString()});
            }
        };
        massChoose.Changed += (object? o, EventArgs args) => {
            OnChooseMass();
        };

        foreach(Widget w in PrimaryWidgets) {
            Add(w);
            w.Hide();
        }
    }

    internal void RefreshMassChoose()
    {
        int currentlySelected = massChoose.Active;
        //If the selected mass number is no longer valid
        if(currentlySelected > Shared.massObjects) {currentlySelected = Shared.massObjects;}
        
        massChoose.RemoveAll();

        massChoose.AppendText("");

        for(int i = 0; i < Shared.massObjects; i++) {
            MassInfo m = Shared.drawingCopy[i];
            if(m.name.Length > 22) {
                massChoose.AppendText(m.name.Substring(0,20) + "..");
            }
            else {
                massChoose.AppendText(m.name);
            }
        }

        massChoose.Active = currentlySelected;
        OnChooseMass();
    }

    private void OnChooseMass() {
        Shared.ignoreNextUpdates = 3;
        //Would otherwise be set to -2 during initialization
        Shared.selectedMassIndex = Math.Max(-1, massChoose.Active - 1);
        
        if(Shared.selectedMassIndex > -1) {
            foreach(Widget w in PrimaryWidgets) {
                w.Show();
            }
            selectedMass = Shared.drawingCopy[Shared.selectedMassIndex];
            ((Entry)UpdatableWidgets["name"]!).Text = selectedMass.name;
            ((ToggleButton)UpdatableWidgets["trailDraw"]!).Active = selectedMass.hasTrail;

            //Update the follow trail mass list
            followChoose.RemoveAll();
            followChoose.AppendText("No follow");

            for(int i = 0; i < Shared.massObjects; i++) {
                MassInfo m = Shared.drawingCopy[i];
                if(m.name.Length > 15 && m.index != Shared.selectedMassIndex) {
                    followChoose.AppendText(m.name.Substring(0,13) + "..");
                }
                else if (m.index != Shared.selectedMassIndex) {
                    followChoose.AppendText(m.name);
                }
            }
            followChoose.Active = selectedMass.followingIndex <= Shared.selectedMassIndex ? selectedMass.followingIndex + 1 : selectedMass.followingIndex;
            
            ((Scale)UpdatableWidgets["trailLength"]!).Value = (double)selectedMass.trail.Length / selectedMass.trailQuality / 60;
            ((Scale)UpdatableWidgets["trailQuality"]!).Value = selectedMass.trailQuality;
            ((CheckButton)UpdatableWidgets["stationary"]!).Active = selectedMass.stationary;

        }
        else {
            foreach(Widget w in PrimaryWidgets) {
                w.Hide();
            }
        }
        if(((CheckButton)UpdatableWidgets["toFollow"]!).Active) {
            Shared.trackedMass = massChoose.Active - 1;
        }
    }
    internal string UIString(string key) {
        switch(key) {
            case "position":
                Vector2d position = selectedMass.position + (selectedMass.satellite && !selectedMass.currentlyUpdatingPhysics 
                    ? Shared.drawingCopy[selectedMass.orbitingBodyIndex].position : new Vector2d(0,0));
                if(se.positionDisplayUnits == "AU") {
                    return position.ToAstronomicalUnits().ToRoundedString(3);
                }
                else {
                    return position.ToScientificString();
                }

            case "velocity":
                Vector2d velocity = selectedMass.velocity  + (selectedMass.satellite && !selectedMass.currentlyUpdatingPhysics 
                    ? Shared.drawingCopy[selectedMass.orbitingBodyIndex].velocity : new Vector2d(0,0));
                return velocity.ToRoundedString(digits: 3);
            case "mass":
                switch(se.massDisplayUnits) {
                    case "Earth":
                        return (selectedMass.mass / Constant.MassOfEarth).ToString("E3");
                    case "Solar":
                        return (selectedMass.mass / Constant.MassOfSun).ToString("E3");
                    case "Gg":
                        return selectedMass.mass.ToString("E2");
                    case "kg":
                        return (selectedMass.mass * Math.Pow(10,6)).ToString("E2");
                    default:
                        return selectedMass.mass.ToString("E2");
                }
            case "positionLabel":
                return "Position (" + se.positionDisplayUnits + ")";
            case "massLabel":
                return "Mass (" + se.massDisplayUnits + ")";
            default:
                return "";
        }
    }

    internal void Refresh() {
        //Checks if a mass has been added or removed
        if(expectedNumberOfMasses != Shared.massObjects) {
            RefreshMassChoose();
            expectedNumberOfMasses = Shared.massObjects;
        }
        if(Shared.selectedMassIndex == -1) { return; }
        selectedMass = Shared.drawingCopy[Shared.selectedMassIndex];
        
        foreach(string s in UpdateKeys) {
            Entry row = (Entry)UpdatableWidgets[s]!;
            if(!row.IsFocus) {
                row.Text = UIString(s);
            }
        }

        Label positionLabel = (Label)UpdatableWidgets["positionLabel"]!;
        if(positionLabel.Text != UIString("positionLabel")) {
            positionLabel.Text = UIString("positionLabel");
        }

        Label massLabel = (Label)UpdatableWidgets["massLabel"]!;
        if(massLabel.Text != UIString("massLabel")) {
            massLabel.Text = UIString("massLabel");
        }
    }

    internal void InitHide() {
        foreach(Widget w in PrimaryWidgets) {
            w.Hide();
        }
    }
    internal Widget GetWidget(string key) { 
        return (Widget)UpdatableWidgets[key]!;
    }
}