namespace LifeSim.Rendering
{
    public class AnimationPlayer
    {
        private Node3D _root;

        private BindedAnimation? _animation = null;

        public AnimationPlayer(Node3D root)
        {
            this._root = root;
        }

        public void Play(Animation animation)
        {
            this._animation = new BindedAnimation(this._root, animation);
        }

        public void Update(float deltaTime)
        {
            if (this._animation == null) return;

            this._animation.Update(deltaTime);
        }
    }
}