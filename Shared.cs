using System.Collections.Concurrent;

namespace Orbit;

internal static class Shared {
    internal static double deltaTime = 3600 * 8; //How many hours per update
    internal static double multiplier = 86400 * 6 * 3; //How many days per real second
    internal static bool Running = true;
    internal static bool Paused = false;
    internal static int massObjects = 0;
    internal static object DataLock = new();
    internal static int trackedMass = -1;
    internal static int selectedMassIndex = -1;
    internal static ConcurrentDictionary<int, MassInfo>massInfos = new ConcurrentDictionary<int, MassInfo>();
    internal static ConcurrentDictionary<int, MassInfo>drawingCopy = new ConcurrentDictionary<int, MassInfo>();
    internal static ConcurrentStack<string[]> changesToMake = new();
    internal static void AddMass(MassInfo m) {
        m.index = massObjects;
        massInfos.AddOrUpdate(massObjects, m, (key, oldValue) => m);
        drawingCopy.AddOrUpdate(massObjects, m.FullCopy(), (key, oldValue) => m.FullCopy());
        massObjects++;
    }
    internal static void RemoveMass(int index) {
        massInfos.Remove(index, out _);
        drawingCopy.Remove(index, out _);

        for(int i = index + 1; i < massObjects; i++) {
            massInfos.Remove(i, out MassInfo? m);
            m!.index = i-1;
            m!.followingIndex = Math.Max(-1, m!.followingIndex - 1);
            massInfos.AddOrUpdate(i-1, m, (key, oldValue) => m);
            
            drawingCopy.Remove(i, out MassInfo? mm);
            mm!.index = i-1;
            m!.followingIndex = Math.Max(-1, m!.followingIndex - 1);
            drawingCopy.AddOrUpdate(i-1, mm, (key, oldValue) => mm);

        }
        massObjects -= 1;
        trackedMass -= 1;
    }
    internal static void ReadyDrawingCopy() {
        lock(DataLock) {
            for(int i = 0; i < massObjects; i++) {
                drawingCopy[i].CopyPhysicsInfo(massInfos[i]);
            }
        }
    }
}