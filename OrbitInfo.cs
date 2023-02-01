using Gtk;
using Gdk;

namespace Orbit;

public class OrbitInfo : Gtk.ListBox {
    ComboBoxText massChoose = new();
    public OrbitInfo() {
        WidthRequest = 260;

        
        massChoose.Append("-1", "");


        for(int i = 0; i < Shared.massObjects; i++) {
            MassInfo m = Shared.drawingCopy[i];
            if(m.name == "") { continue; }
            massChoose.Append(m.index.ToString(), m.name);
        }

        Add(massChoose);


        //Needed for keyboard input
		CanFocus = true;

        KeyPressEvent += new KeyPressEventHandler(KeyPress);
        Events |= EventMask.KeyPressMask;
    }
    private void KeyPress(object o, KeyPressEventArgs args) {
        if(args.Event.Key == Gdk.Key.space) {
            Console.WriteLine(massChoose.Active - 1);
        }
    }
}