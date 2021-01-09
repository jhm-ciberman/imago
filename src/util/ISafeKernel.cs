namespace LifeSim
{
    /// <summary>
    /// Safe Kernel represents a 2D convolution kernel for an infinite matrix centered arround a point (x,y) which is the actual "value".
    /// For example, if you have a have a Kernel which center is (200,200) then ISafeKernel.value will return the 
    /// value in the position (200,200) of the underlaying matrix. Whereas SafeGet(1, -5, null) will return the value
    /// in the position (201, 195), or null if the position is invalid/outside the underlying matrix.
    /// It is called "Safe" Kernel because the only value that is asured to exists is the center value, and all the other 
    /// neighbour values should be accessed through the SafeGet method.
    /// </summary>
    /// <typeparam name="T">The kernel type</typeparam>
    public interface ISafeKernel<T>
    {
        T value {get;}
        T SafeGet(int x, int y, T defaultValue);
    }

}