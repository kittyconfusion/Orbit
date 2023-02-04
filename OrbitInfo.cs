using Gtk;
using Gdk;
using System.Collections.Specialized;

namespace Orbit;

public class OrbitInfo : Gtk.ListBox {
    ComboBoxText massChoose = new();
    internal CheckButton toFollow = new("Follow");
    OrderedDictionary theRest = new();
    int selectedMassIndex = -1;
    MassInfo selectedMass = new();
    int expectedNumberOfMasses = 0;
    static readonly string[] UpdateKeys = {"position", "velocity", "mass"};
    public OrbitInfo() {
        WidthRequest = 140;

        //Needed for keyboard input
		CanFocus = true;

        massChoose.Changed += OnChooseMass;

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

        Label positionLabel = new("Position (km)");
        theRest.Add("positionLabel", positionLabel);
        Entry position = new();
        position.Activated += (object? o, EventArgs a) => {
            GetRowAtIndex(2).GrabFocus();
            Shared.changesToMake.Push(new string[] {"position", position.Text, selectedMassIndex.ToString()});
        };
        theRest.Add("position", position);

        Label velocityLabel = new("Velocity (km/s)");
        theRest.Add("velocityLabel", velocityLabel);
        Entry velocity = new();
        velocity.Activated += (object? o, EventArgs a) => {
            GetRowAtIndex(4).GrabFocus();
            Shared.changesToMake.Push(new string[] {"velocity", velocity.Text, selectedMassIndex.ToString()});
        };
        theRest.Add("velocity", velocity);

        Label massLabel = new("Mass (Gg)");
        theRest.Add("massLabel", massLabel);
        Entry mass = new();
        mass.Activated += (object? o, EventArgs a) => {
            GetRowAtIndex(6).GrabFocus();
            Shared.changesToMake.Push(new string[] {"mass", mass.Text, selectedMassIndex.ToString()});
        };
        theRest.Add("mass", mass);

        Label nameLabel = new("Name");
        theRest.Add("nameLabel", nameLabel);
        Entry name = new();
        name.Activated += (object? o, EventArgs a) => {
            GetRowAtIndex(8).GrabFocus();
            Shared.drawingCopy[selectedMassIndex].name = name.Text;
            RefreshMassChoose();
        };
        theRest.Add("name", name);

        foreach(Widget w in theRest.Values) {
            Add(w);
            w.Hide();
        }
        
        for(int i = 0; i <= theRest.Keys.Count; i++) {
            this.GetRowAtIndex(i).Selectable = false;
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
            if(m.name.Length > 16) {
                massChoose.AppendText(m.name.Substring(0,14) + "..");
            }
            else {
                massChoose.AppendText(m.name);
            }
        }

        massChoose.Active = currentlySelected;
        OnChooseMass(null, EventArgs.Empty);
    }

    private void OnChooseMass(object? o, EventArgs args) {
        //Would otherwise be set to -2 during initialization
        selectedMassIndex = Math.Max(-1, massChoose.Active - 1);
        
        if(selectedMassIndex > -1) {
            foreach(Widget w in theRest.Values) {
                w.Show();
            }
            selectedMass = Shared.drawingCopy[selectedMassIndex];
            ((Entry)theRest["name"]!).Text = selectedMass.name;
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