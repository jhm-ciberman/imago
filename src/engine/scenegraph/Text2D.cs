namespace LifeSim.Engine.SceneGraph
{
    public class Text2D : Node2D
    {
        private string _text;

        public Text2D()
        {
            this._text = "";
        }

        public Text2D(string text)
        {
            this._text = text;
        }

        public string text { get => this._text; set => this._text = value; }
    }
}