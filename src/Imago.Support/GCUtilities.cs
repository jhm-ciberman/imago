using System;
using System.Runtime;

namespace Imago.Support;

/// <summary>
/// Provides utility methods for garbage collection management.
/// </summary>
public static class GCUtilities
{
    /// <summary>
    /// Executes a full garbage collection and compacts the large object heap.
    /// This is useful for reducing memory fragmentation and reclaiming memory.
    /// </summary>
    public static void AgressiveGC()
    {
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
    }
}
