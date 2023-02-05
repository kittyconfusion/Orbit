using Gtk;
using Gdk;
using System.Collections.Specialized;

namespace Orbit;

public class OrbitInfo : Gtk.ListBox {
    ComboBoxText massChoose = new();
    ComboBoxText followChoose = new();
    internal CheckButton toFollow = new("Follow");
    Entry trailLength;
    CheckButton trailDraw;
    OrderedDictionary theRest = new();
    int selectedMassIndex = -1;
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

        theRest.Add("separator1", new Separator(Orientation.Horizontal));

        Label nameLabel = new("Name");
        theRest.Add("nameLabel", nameLabel);
        Entry name = new();

        name.Activated += (object? o, EventArgs a) => {
            GetRowAtIndex(4).GrabFocus();
            Shared.drawingCopy[selectedMassIndex].name = name.Text;
            RefreshMassChoose();
        };
        theRest.Add("name", name);
        theRest.Add("separator2", new Separator(Orientation.Horizontal));

        Label positionLabel = new("Position (km)");
        theRest.Add("positionLabel", positionLabel);
        Entry position = new();
        position.Activated += (object? o, EventArgs a) => {
            GetRowAtIndex(7).GrabFocus();
            Shared.changesToMake.Push(new string[] {"position", position.Text, selectedMassIndex.ToString()});
        };

        theRest.Add("position", position);

        Label velocityLabel = new("Velocity (km/s)");
        theRest.Add("velocityLabel", velocityLabel);
        Entry velocity = new();
        velocity.Activated += (object? o, EventArgs a) => {
            GetRowAtIndex(9).GrabFocus();
            Shared.changesToMake.Push(new string[] {"velocity", velocity.Text, selectedMassIndex.ToString()});
        };
        theRest.Add("velocity", velocity);

        Label massLabel = new("Mass (Gg)");
        theRest.Add("massLabel", massLabel);
        Entry mass = new();
        mass.Activated += (object? o, EventArgs a) => {
            GetRowAtIndex(11).GrabFocus();
            Shared.changesToMake.Push(new string[] {"mass", mass.Text, selectedMassIndex.ToString()});
        };
        theRest.Add("mass", mass);


        theRest.Add("separator3", new Separator(Orientation.Horizontal));

        HBox followBox = new();
        Label trailFollowLabel = new("Length");
        trailDraw = new("Draw");
        trailDraw.Toggled += (object? o, EventArgs a) => {
            selectedMass.hasTrail = trailDraw.Active;
        };
        
        HBox trailBox = new();
        trailBox.Add(new Label("Trail"));
        trailBox.Add(followChoose);
        
        theRest.Add("trailBox", trailBox);

        followBox.Add(trailFollowLabel);
        trailLength = new();
        trailLength.Activated += (object? o, EventArgs a) => {
            GetRowAtIndex(14).GrabFocus();
            if(int.TryParse(trailLength.Text, out int result)) {
                if(result > 0) {
                    Shared.changesToMake.Push(new string[] {"trail length", trailLength.Text, selectedMassIndex.ToString()});
                }
            }
            //Reset text to actual value
            else {
                trailLength.Text = selectedMass.trail.Length.ToString();
            }
        };
        trailLength.WidthChars = 2;
        followBox.Add(trailLength);
        followBox.Add(trailDraw);
        theRest.Add("followBox", followBox);

        //theRest.Add("followChoose", followChoose);

        followChoose.Changed += (object? o, EventArgs args) => {
            if(followChoose.Active != -1 && selectedMassIndex != -1) {
                int index = followChoose.Active - 1 < selectedMassIndex ? followChoose.Active - 1: followChoose.Active;
                Shared.changesToMake.Push(new string[] {"trail follow", index.ToString(), selectedMassIndex.ToString()});
            }
        };
        massChoose.Changed += (object? o, EventArgs args) => {
            OnChooseMass();
            followChoose.Active = selectedMass.followingIndex + 1;
        };

        foreach(Widget w in theRest.Values) {
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
        selectedMassIndex = Math.Max(-1, massChoose.Active - 1);
        
        if(selectedMassIndex > -1) {
            foreach(Widget w in theRest.Values) {
                w.Show();
            }
            selectedMass = Shared.drawingCopy[selectedMassIndex];
            ((Entry)theRest["name"]!).Text = selectedMass.name;
            trailDraw.Active = selectedMass.hasTrail;

            //Update the follow trail mass list
            followChoose.RemoveAll();
            followChoose.AppendText("No follow");

            for(int i = 0; i < Shared.massObjects; i++) {
                MassInfo m = Shared.drawingCopy[i];
                if(m.name.Length > 15 && m.index != selectedMassIndex) {
                    followChoose.AppendText(m.name.Substring(0,13) + "..");
                }
                else if (m.index != selectedMassIndex) {
                    followChoose.AppendText(m.name);
                }
                followChoose.Active = selectedMass.followingIndex + 1;
            }
            trailLength.Text = selectedMass.trail.Length.ToString();

        }
        else {
            foreach(Widget w in theRest.Values) {
                w.Hide();
            }
        }
        if(toFollow.Active) {
            Shared.trackedMass = massChoose.Active - 1;
        }
    }
    internal string UIString(string key) {
        switch(key) {
            case "position":
                return selectedMass.position.ToRoundedString();
            case "velocity":
                return selectedMass.velocity.ToRoundedString(digits: 3);
            case "mass":
                return selectedMass.mass.ToString();
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
        if(selectedMassIndex == -1) { return; }
        selectedMass = Shared.drawingCopy[selectedMassIndex];

        foreach(string s in UpdateKeys) {
            Entry row = (Entry)theRest[s]!;
            if(!row.IsFocus) {
                row.Text = UIString(s);
            }
        }
    }

    internal void InitHide() {
        foreach(Widget w in theRest.Values) {
            w.Hide();
        }
    }
}