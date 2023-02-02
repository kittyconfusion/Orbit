using Gtk;
using Gdk;
using System.Collections.Specialized;

namespace Orbit;

public class OrbitInfo : Gtk.ListBox {
    ComboBoxText massChoose = new();
    OrderedDictionary theRest = new();
    int selectedMassIndex = -1;
    MassInfo selectedMass = new();
    static readonly string[] UpdateKeys = {"position", "velocity", "mass"};
    public OrbitInfo() {
        WidthRequest = 150;

        //Needed for keyboard input
		CanFocus = true;

        KeyPressEvent += new KeyPressEventHandler(KeyPress);
        Events |= EventMask.KeyPressMask;

        massChoose.AppendText("");

        for(int i = 0; i < Shared.massObjects; i++) {
            MassInfo m = Shared.drawingCopy[i];
            if(m.name == "") { continue; }
            massChoose.AppendText(m.name);
        }

        Add(massChoose);
        massChoose.Changed += OnChooseMass;
        
        Label positionLabel = new("Position (km)");
        theRest.Add("positionLabel", positionLabel);
        Entry position = new();
        position.Changed += (object? o, EventArgs a) => {if(position.IsFocus) {
            Shared.changesToMake.Push(new string[] {"position", position.Text, selectedMassIndex.ToString()});
        }};
        theRest.Add("position", position);

        Label velocityLabel = new("Velocity (km/s)");
        theRest.Add("velocityLabel", velocityLabel);
        Entry velocity = new();
        velocity.Changed += (object? o, EventArgs a) => {if(velocity.IsFocus) {
            Shared.changesToMake.Push(new string[] {"velocity", velocity.Text, selectedMassIndex.ToString()});
        }};
        theRest.Add("velocity", velocity);

        Label massLabel = new("Mass (Gg)");
        theRest.Add("massLabel", massLabel);
        Entry mass = new();
        mass.Changed += (object? o, EventArgs a) => {if(mass.IsFocus) {
            Shared.changesToMake.Push(new string[] {"mass", mass.Text, selectedMassIndex.ToString()});
        }};
        theRest.Add("mass", mass);


        foreach(Widget w in theRest.Values) {
            Add(w);
            w.Hide();
        }
        
        for(int i = 0; i <= theRest.Keys.Count; i++) {
            this.GetRowAtIndex(i).Selectable = false;
        }

    }

    private void OnChooseMass(object? o, EventArgs args) {
        selectedMassIndex = massChoose.Active - 1;
        
        if(selectedMassIndex == -1) {
            foreach(Widget w in theRest.Values) {
                w.Hide();
            }
        }
        else {
            foreach(Widget w in theRest.Values) {
                w.Show();
            }
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
        if(selectedMassIndex == -1) { return; }
        selectedMass = Shared.drawingCopy[selectedMassIndex];

        foreach(string s in UpdateKeys) {
            Entry row = (Entry)theRest[s]!;
            if(!row.IsFocus) {
                row.Text = UIString(s);
            }
        }
    }

    private void KeyPress(object o, KeyPressEventArgs args) {
        if(args.Event.Key == Gdk.Key.space) {
            Shared.trackedMass = massChoose.Active - 1;
            Console.WriteLine(Shared.trackedMass);
        }
    }

    internal void InitHide() {
        foreach(Widget w in theRest.Values) {
            w.Hide();
        }
    }
}