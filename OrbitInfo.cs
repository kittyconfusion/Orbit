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
    public OrbitInfo() {
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

        PrimaryWidgets.Add(new Label("Position (km)"));
        Entry position = new();
        UpdatableWidgets.Add("position", position);
        PrimaryWidgets.Add(position);
        position.Activated += (object? o, EventArgs a) => {
            GetRowAtIndex(7).GrabFocus();
            Shared.changesToMake.Push(new string[] {"position", position.Text, Shared.selectedMassIndex.ToString()});
        };

        PrimaryWidgets.Add(new Label("Velocity (km/s)"));

        Entry velocity = new();
        UpdatableWidgets.Add("velocity", velocity);
        PrimaryWidgets.Add(velocity);
        velocity.Activated += (object? o, EventArgs a) => {
            GetRowAtIndex(9).GrabFocus();
            Shared.changesToMake.Push(new string[] {"velocity", velocity.Text, Shared.selectedMassIndex.ToString()});
        };

        PrimaryWidgets.Add(new Label("Mass (Gg)"));
        Entry mass = new();
        UpdatableWidgets.Add("mass", mass);
        PrimaryWidgets.Add(mass);
        mass.Activated += (object? o, EventArgs a) => {
            GetRowAtIndex(11).GrabFocus();
            Shared.changesToMake.Push(new string[] {"mass", mass.Text, Shared.selectedMassIndex.ToString()});
        };

        PrimaryWidgets.Add(new Separator(Orientation.Horizontal));

        HBox trailBox = new();
        trailBox.Add(new Label("Trail"));
        trailBox.Add(followChoose);
        
        PrimaryWidgets.Add(trailBox);
        UpdatableWidgets.Add("followChoose", followChoose);

        HScale trailLength = new(1, 1000, 50);
        trailLength.ValueChanged += (object? o, EventArgs args) => {
            Shared.changesToMake.Push(new string[] {"trail length", trailLength.Value.ToString(), Shared.selectedMassIndex.ToString()});
            UpdateTrailTimeEstimate(selectedMass.trailSkip, trailLength.Value);
        };

        HBox trailLengthBox = new();
        trailLengthBox.Add(new Label("Length"));
        trailLengthBox.Add(new Label("Quality"));

        PrimaryWidgets.Add(trailLengthBox);
        UpdatableWidgets.Add("trailLength", trailLength);

        HScale trailQuality = new(1, 10, 1);
        trailQuality.ValueChanged += (object? o, EventArgs args) => {
            Shared.changesToMake.Push(new string[] {"trail skip", Math.Pow(2, 10 - trailQuality.Value).ToString(), Shared.selectedMassIndex.ToString()});
            UpdateTrailTimeEstimate(Math.Pow(2, 10 - trailQuality.Value), selectedMass.trail.Length);
        };

        HBox trailQualityBox = new();
        trailQualityBox.Add(trailLength);
        trailQualityBox.Add(trailQuality);

        PrimaryWidgets.Add(trailQualityBox);
        UpdatableWidgets.Add("trailQuality", trailQuality);

        HBox trailInfoDrawBox = new();

        CheckButton trailDraw = new("Draw");
        trailDraw.Toggled += (object? o, EventArgs a) => {
            selectedMass.hasTrail = trailDraw.Active;
        };
        Label trailTime = new();
        trailInfoDrawBox.Add(trailDraw);
        trailInfoDrawBox.Add(trailTime);
        
        PrimaryWidgets.Add(trailInfoDrawBox);
        UpdatableWidgets.Add("trailDraw", trailDraw);
        UpdatableWidgets.Add("trailTime", trailTime);

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
            if(m.name.Length > 15) {
                massChoose.AppendText(m.name.Substring(0,13) + "..");
            }
            else {
                massChoose.AppendText(m.name);
            }
        }

        massChoose.Active = currentlySelected;
        OnChooseMass();
    }

    private void OnChooseMass() {
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
            
            ((HScale)UpdatableWidgets["trailLength"]!).Value = selectedMass.trail.Length;
            ((HScale)UpdatableWidgets["trailQuality"]!).Value = 10 - Math.Log2(selectedMass.trailSkip);
            UpdateTrailTimeEstimate(selectedMass.trailSkip, selectedMass.trail.Length);

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
    internal void UpdateTrailTimeEstimate(double trailSkip, double trailLength) {
        ((Label)UpdatableWidgets["trailTime"]!).Text = (Math.Round(trailSkip * (trailLength - 1) / 30, 1)).ToString() + " weeks";
    }
    internal string UIString(string key) {
        switch(key) {
            case "position":
                return selectedMass.position.ToScientificString();
            case "velocity":
                return selectedMass.velocity.ToRoundedString(digits: 3);
            case "mass":
                return selectedMass.mass.ToString("E2");
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