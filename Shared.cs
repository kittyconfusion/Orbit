using System.Collections.Concurrent;

namespace Orbit;

internal static class Shared {
    internal static double deltaTime = 86400 * 7 / 60; //How many hours per update
    internal static double multiplier = 86400 * 7; //How many days per real second
    internal static bool Running = true;
    internal static bool Paused = false;
    internal static int massObjects = 0;
    internal readonly static object DataLock = new();
    internal static int trackedMass = -1;
    internal static int selectedMassIndex = -1;
    internal static bool needToRefresh = true;
    internal static ConcurrentDictionary<int, MassInfo>massInfos = new ConcurrentDictionary<int, MassInfo>();
    internal static ConcurrentDictionary<int, MassInfo>drawingCopy = new ConcurrentDictionary<int, MassInfo>();
    internal static ConcurrentStack<string[]> changesToMake = new();
    internal static int ignoreNextUpdates = 0;
    internal static int resolutionMode = 0;
    internal static void AddMass(MassInfo m) {
        m.index = massObjects;
        massInfos.AddOrUpdate(massObjects, m, (key, oldValue) => m);
        drawingCopy.AddOrUpdate(massObjects, m.FullCopy(), (key, oldValue) => m.FullCopy());
        massObjects++;
    }
    internal static void RemoveDrawingMass(int index) {
        drawingCopy.Remove(Shared.selectedMassIndex, out _);

        trackedMass = Math.Max(trackedMass - 1, -1);

        for(int i = index + 1; i < drawingCopy.Count + 1; i++) {
            MassInfo drawing = drawingCopy[i];
            drawing.index -= 1;
            drawingCopy.Remove(i, out MassInfo? mm);
            drawingCopy.AddOrUpdate(i-1, mm!, (key, oldValue) => mm!);
        }
        for(int i = 0; i < drawingCopy.Count; i++) {
            MassInfo drawing = drawingCopy[i];

            if(drawing.orbitingBodyIndex == index) {
                //Orbit the object with the greatest force
                int newOrbitingIndex = -1;
                double highestForce = 0;
                for(int j = 0; j < drawingCopy.Count; j++) {
                    if(j == drawing.index) { continue; }
                    MassInfo test = drawingCopy[j];
                    
                    double X = test.position.X - drawing.position.X;
                    double Y = test.position.Y - drawing.position.Y;
                    double dist2 = Math.Pow(Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2)), 3);
                    
                    double force = (Constant.G * test.mass * drawing.mass) / dist2;
                    if(force > highestForce) { highestForce = force; newOrbitingIndex = j; }
                }
                drawing.orbitingBodyIndex = newOrbitingIndex;
            }
            else if(drawing.orbitingBodyIndex > index) {
                drawing.orbitingBodyIndex -= 1;
            }

            if(drawing.followingIndex == index ) {
                drawing.followingIndex = -1;
            }
            if(drawing.followingIndex > index) {
                drawing.followingIndex -= 1;
            }
        }
    }
    internal static void RemoveMass(int index) {
        lock(DataLock) {
            massInfos.Remove(index, out _);
            
            massObjects -= 1;
            
            for(int i = index + 1; i < massObjects + 1; i++) {
                MassInfo working = massInfos[i];

                working.index -= 1;
                massInfos.Remove(i, out MassInfo? m);
                massInfos.AddOrUpdate(i-1, m!, (key, oldValue) => m!);
            }
            for(int i = 0; i < massObjects; i++) {
                MassInfo working = massInfos[i];
                
                if(working.orbitingBodyIndex == index) {
                    //Orbit the object with the greatest force
                    int newOrbitingIndex = -1;
                    double highestForce = 0;
                    for(int j = 0; j < drawingCopy.Count; j++) {
                        if(j == working.index) { continue; }
                        MassInfo test = drawingCopy[j];
                        
                        double X = test.position.X - working.position.X;
                        double Y = test.position.Y - working.position.Y;
                        double dist2 = Math.Pow(Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2)), 3);
                        
                        double force = (Constant.G * test.mass * working.mass) / dist2;
                        if(force > highestForce) { highestForce = force; newOrbitingIndex = j; }
                    }
                    working.orbitingBodyIndex = newOrbitingIndex;
                }
                else if(working.orbitingBodyIndex > index) {
                    working.orbitingBodyIndex -= 1;
                }

                if(working.followingIndex == index ) {
                    working.followingIndex = -1;
                }
                if(working.followingIndex > index) {
                    working.followingIndex -= 1;
                }
            }
        }
    }
    internal static void ReadyDrawingCopy() {
        lock(DataLock) {
            if(drawingCopy.Count == massObjects) {
                for(int i = 0; i < massObjects; i++) {
                    drawingCopy[i].CopyPhysicsInfo(massInfos[i]);
                }
            }
        }
    }

    internal static void ResetDrawingMasses()
    {
        lock(DataLock) {
            drawingCopy.Clear();
        }
    }
}