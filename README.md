# Using [Orbit](https://github.com/kittyconfusion/Orbit)

Orbit is a simple solar system sandbox and simulation created for the second semester of LASA's Computer Science Independent Study course.

Numerous masses can be simulated, with timescales ranging from minutes to decades per seconds.
The window is taken up by three resizable panels from left the right: the Settings panel, the Drawing panel, and the Info panel. 

## The Settings Panel ##
The Settings panel controls the overall simulation and drawing.

### File Menu ###
- **Save** - save the current simulation state to a JSON file
- **Load** - load a simulation state from a given JSON file
- **Load Preset** - load a simulation state from a list of hardcoded options

### Mass Menu ###
- **New Mass** - Create a blank new mass
- **Remove Current** - Removes the currently selected mass in the Info panel
- **Remove All** - On confirmation, removes all masses from the simulation

### View Menu ###
- **Draw Trails** - Toggle the drawing of all trails, does not effect them being updated internally
- **Toggle All Trails** - Set the state of all masses to have/no longer have trails, useful for isolating a single object's trail
- **Draw Masses** - Toggle the drawing of all mass circles and name labels
- **Global Follow Current Mass** - Set the Trail Follow (mass to draw trails relative to) for all masses to the currently selected mass
- **Set Position Unit** - Choose between AU and km (km is used internally)
- **Set Mass Unit** - Choose between Earth masses, solar masses, Gg, and kg (Gg are used internally (10^6 kg))

### Timescale ###
The dropdown and associated slider sets the amount of simulation time to pass every real second. This is separate from the resolution step, the amount of time passed every discrete simulation step.

The **Pause** checkbox pauses the simulation. User changes are still processed while the simulation is paused.

### Vectors ###
- **Velocity Vectors** show the current direction of motion for all masses either in magnitude (1 km/s == 1 pixel), or in pure direction with a constant length if **Normalize** is selected.
- **Force Vectors** show the majority of forces acting on each mass, as long as they are above a small minimum threshold. 
  - If **Log** is selected, the lengths of the arrows are drawn on a natural log scale. This visually means for example that for our solar system the force of the Sun will be noticeably larger for any given planet than any interplanetary forces, but not overwhelmingly large. 
  - If **Linear** is selected, for any given mass, the largest force is given a specific length and all other forces will drawn relative to the largest linearly. The threshold for drawing these other forces is 0.2% the magnitude of the greatest force.
  
### Resolution Mode ###
Resolution mode sets a hard ceiling for the amount of time that passes between each simulation update (delta time). Keep in mind that when running high resolution modes on large timescales, the program may be unable to keep up. When the simulation state is saved, this option is also saved.

**No Restriction** sets delta time to 1/60 of the expected time to pass every second

## Drawing Panel ##
The Drawing panel can be controlled by the mouse to move and zoom and by the keyboard using WASD and Shift to move. Keyboard shortcuts to Quit (Ctrl/Cmd + Q), Pause (Ctrl/Cmd + P), Toggle Trails (Ctrl/Cmd + T), and Follow (Ctrl/Cmd + F) exist.

## Info Panel ##
The Info panel allows for viewing and editing various attributes for each individual mass. Press Enter to save any changes. 

The **Trail Follow** option allows for calculating and drawing a mass' trails relative to that of another mass instead of in global coordinates.

Increasing the **Trail Length** by 1 increases the number of trail steps stored by 100, while increasing the **Trail Quality** by 1 increases the trail step resolution by a factor of 10.

Toggling **Has Trail** will turn on/off whether the mass has its trail both calculated and drawn.

**Stationary** masses undergo no physics calculations and only move by direct user modification of their position.

## Other Features/Explanation of JSON mass properties ##
If you go poking around the JSON files that Orbit outputs, you may see some unusual options that were unable to be fit into the GUI. These are all fully functional and made use of especially within the normal Solar System preset. One such output for the Moon is shown below.
```
      "Index": 4,
      "Name": "Moon",
      "MinorMass": true,
      "MassInGg": 73460000000000000,
      "PositionInKm": {
        "X": 148461868.42828664,
        "Y": -14422447.431730954
      },
      "VelocityInKmS": {
        "X": -3.845118318245941,
        "Y": -29.72178132726493
      },
      "Stationary": false,
      "HasTrail": true,
      "TrailLength": 4,
      "TrailQuality": 6,
      "FollowingIndex": 3,
      "OrbitingBodyIndex": 3,
      "Satellite": true,
      "PrecisionPriorityLimitInSeconds": 86400
```
Some settings are explained below:

**Index** - Each mass is given a sequentially numbered index that is used to identify every mass in the application and for other masses to be able to refer back to it. Indexes start at 0.

**MinorMass** - Minor Masses exist for performance and the occasional simulation-breaking reasons. They exert no forces on other objects, as they are deemed insubstantial. Moons are generally classified as Minor Masses, as is Charon in the Pluto preset because its strong influence leads to the ejection of other moons. Due to the limitations of the simulation, Charon was made a minor mass and its mass was added onto Pluto.

**FollowingIndex** - The Index of the mass which the Trail Follow setting is currently selected for. A value of -1 means none.

**Satellite** - Enables the PrecisionPriorityLimit to be used. Legacy option.

**PrecisionPriorityLimit** - If Satellite is true and FollowingIndex > -1, if the resolution is low enough such that simulation delta time falls above the PrecisionPriorityLimit, the mass simulation is deemed no longer accurate an so the mass is frozen and its position and velocity relative to the following mass are stored. When unfrozen (if the delta time decreases), the mass will resume its orbit. This should rarely occur on a preset's default resolution mode.

# Compiling Orbit

The program was originally compiled in C# using .NET 7 and Visual Studio Code. The GtkSharp wrapper for the Gtk toolkit is required to compile.

The code conforms to Gtk version 3.24.

### Compiling in VS Code ###
The [NuGet Package Manager Extension](https://marketplace.visualstudio.com/items?itemName=jmrog.vscode-nuget-package-manager) is recommended.

To install GtkSharp using the extension press `Ctrl/Cmd + Shift + P` and type in and select `NuGet Package Manager: Add Package`. The package name is `GtkSharp`. This should install the Gtk wrapper for C#.

Windows will download Gtk automatically on first compile. 

On MacOS, it may be necessary to download Gtk manually. If brew is installed, in the terminal run `brew install gtk+3`.
