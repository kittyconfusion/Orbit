using Gtk;
using Gdk;

namespace Orbit;

public class OrbitInfo : Gtk.ListBox {
    ComboBoxText massChoose = new();
    public OrbitInfo() {
        WidthRequest = 260;

        massChoose.AppendText("");

        for(int i = 0; i < Shared.massObjects; i++) {
            MassInfo m = Shared.drawingCopy[i];
            if(m.name == "") { continue; }
            massChoose.AppendText(m.name);
        }

        Add(massChoose);

        //Needed for keyboard input
		CanFocus = true;

        KeyPressEvent += new KeyPressEventHandler(KeyPress);
        Events |= EventMask.KeyPressMask;
    }

    internal void Refresh()
    {
        throw new NotImplementedException();
    }

    private void KeyPress(object o, KeyPressEventArgs args) {
        if(args.Event.Key == Gdk.Key.space) {
            Shared.trackedMass = massChoose.Active - 1;
            Console.WriteLine(Shared.trackedMass);
        }
    }
}