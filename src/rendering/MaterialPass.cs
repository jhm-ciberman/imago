namespace LifeSim.Rendering
{
    public class MaterialPass
    {
        private static uint _globalIdCount = 0;

        public readonly uint id;
        private Shader _shader;
        public Shader shader => this._shader;

        public MaterialPass(Shader shader)
        {
            this.id = ++MaterialPass._globalIdCount;
            this._shader = shader;
        }
    }
}