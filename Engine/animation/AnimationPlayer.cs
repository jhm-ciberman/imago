using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.Anim
{
    public class AnimationPlayer
    {
        private readonly Node3D _root;

        private BindedAnimation? _animation = null;

        public AnimationPlayer(Node3D root)
        {
            this._root = root;
        }

        public void Play(BindedAnimation animation)
        {
            this._animation = animation;
        }

        public void Play(Animation animation)
        {
            var binder = new SimpleAnimationBinder();
            this._animation = binder.Bind(this._root, animation);;
        }

        public void Update(float deltaTime)
        {
            if (this._animation == null) return;

            this._animation.Update(deltaTime);
        }
    }
}