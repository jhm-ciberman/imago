namespace LifeSim.Imago.SceneGraph.Picking;

public interface IPickableTarget
{
    public void MouseEnter(HitInfo hitInfo);
    public void MouseMove(HitInfo hitInfo);
    public void MouseLeave();
}
