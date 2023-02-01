using Gtk;
using Gdk;
using System.Collections.Specialized;

namespace Orbit;

public class OrbitInfo : Gtk.ListBox {
    ComboBoxText massChoose = new();
    OrderedDictionary theRest = new();
    int selectedMass = -1;
    public OrbitInfo() {
        WidthRequest = 250;

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

        Label positionLabel = new("Position");
        positionLabel.CanFocus = false;
        theRest.Add("positionLabel", positionLabel);

        Entry position = new();
        theRest.Add("position", position);

        foreach(Widget w in theRest.Values) {
            Add(w);
            w.Hide();
        }
    }

    private void OnChooseMass(object? o, EventArgs args) {
        selectedMass = massChoose.Active - 1;
        
        if(selectedMass == -1) {
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

    internal void Refresh() {
        if(selectedMass == -1) { return; }
        MassInfo m = Shared.drawingCopy[selectedMass];
        ((Entry)theRest["position"]).Text = m.position.ToRoundedString();
        //((ComboBoxText)theRest["position"]).AppendText("");
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