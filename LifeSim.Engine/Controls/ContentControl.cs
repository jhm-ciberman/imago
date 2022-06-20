namespace LifeSim.Engine.Controls;

public class ContentControl : ContentControlBase<Control>
{
    public Control? Content
    {
        get => this.ContentInternal;
        set => this.ContentInternal = value;
    }
}