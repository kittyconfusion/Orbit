# Orbit

This is a simple solar system sandbox and simulation created for the second semester of LASA's Computer Science Independent Study course.

The program was originally compiled in C# using .NET 7 and Visual Studio Code. The GtkSharp wrapper for the Gtk toolkit is required to compile.

The code conforms to Gtk version 3.24.

### Compiling in VS Code ###
The [NuGet Package Manager Extension](https://marketplace.visualstudio.com/items?itemName=jmrog.vscode-nuget-package-manager) is recommended.

To install GtkSharp using the extension press `Ctrl/Cmd + Shift + P` and type in and select `NuGet Package Manager: Add Package`. The package name is `GtkSharp`. This should install the Gtk wrapper for C#.

Windows will download Gtk automatically on first compile. 

On MacOS, it may be necessary to download Gtk manually. If brew is installed, in the terminal run `brew install gtk+3`.
