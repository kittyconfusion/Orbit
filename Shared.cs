using System.Collections.Concurrent;

namespace Orbit;

internal static class Shared {
    internal static bool Running = true;
    internal static int massObjects = 0;
    internal static object DataLock = new();
    internal static int trackedMass = -1;
    internal static ConcurrentDictionary<int, MassInfo>massInfos = new ConcurrentDictionary<int, MassInfo>();
    internal static ConcurrentDictionary<int, MassInfo>drawingCopy = new ConcurrentDictionary<int, MassInfo>();
    
    internal static int AddMass(MassInfo m) {
        massInfos.AddOrUpdate(massObjects, m, (key, oldValue) => m);
        drawingCopy.AddOrUpdate(massObjects, m.FullCopy(), (key, oldValue) => m.FullCopy());
        massObjects++;
        return massObjects - 1;
    }
    internal static void ReadyDrawingCopy() {
        lock(DataLock) {
            for(int i = 0; i < massObjects; i++) {
                drawingCopy[i].CopyPhysicsInfo(massInfos[i]);
            }
        }
    }
    internal static void ReadyWorkingCopy() {
        for(int i = 0; i < massObjects; i++) {
            massInfos[i].CopyNewInfo(drawingCopy[i]);
        }
    }
}