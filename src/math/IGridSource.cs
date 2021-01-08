namespace LifeSim
{
    public interface IGridSource<T>
    {
        T this[int x, int y] {get;}
        bool HasValue(int x, int y);
    }
}