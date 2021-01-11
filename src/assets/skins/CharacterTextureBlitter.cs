/*
namespace LifeSim
{
    public class CharacterTextureBlitter
    {
        private RenderTexture _bodyRT;
        private RenderTexture _headRT;
        private RenderTexture _hairRT;

        private Material _bodyBlitMat;
        private Material _headBlitMat;
        private Material _hairBlitMat;

        public CharacterTextureBlitter()
        {
            if (this._bodyRT == null) this._bodyRT = this._NewRenderTexture(128, 128);
            if (this._hairRT == null) this._hairRT = this._NewRenderTexture(64, 64);
            if (this._headRT == null) this._headRT = this._NewRenderTexture(32, 32);

            this._bodyBlitMat = new Material(Shader.Find("LifeSim/BodyTextureBlitter"));
            this._headBlitMat = new Material(Shader.Find("LifeSim/HeadTextureBlitter"));
            this._hairBlitMat = new Material(Shader.Find("LifeSim/HairTextureBlitter"));
        }

        private RenderTexture _NewRenderTexture(int w, int h)
        {
            var rt = new RenderTexture(w, h, 0);
            rt.filterMode = FilterMode.Point;
            return rt;
        }

        public Material hairMaterial;
        public Material bodyMaterial;
        public Material headMaterial;

        public Texture2D skinBody = null;
        public Texture2D skinHead = null;
        public Texture2D hair = null;
        public Texture2D mouth = null;
        public Texture2D eyes = null;
        public Texture2D clothes = null;

        public Color[] skinPalette = null;
        public Color[] hairPalette = null;
        public Color[] eyesPalette = null;

        public void BlitBody()
        {
            Material m = this._bodyBlitMat;
            m.SetTexture("_SkinTex", this.skinBody);
            m.SetTexture("_ClothesTex", this.clothes);
            m.SetColorArray("_OldPallete", ColorLUT.skinOld);
            m.SetColorArray("_NewPallete", this.skinPalette);
            Graphics.Blit(null, this._bodyRT, m);
            this.bodyMaterial.SetTexture("_BaseMap", this._bodyRT);
        }

        public void BlitHair()
        {
            Material m = this._hairBlitMat;
            m.SetTexture("_HairTex", this.hair);
            m.SetColorArray("_OldPallete", ColorLUT.hairOld);
            m.SetColorArray("_NewPallete", this.hairPalette);
            Graphics.Blit(null, this._hairRT, m);
            this.hairMaterial.SetTexture("_BaseMap", this._hairRT);
        }

        private Color[] _ConcatPalettes(Color[] a, Color[] b, Color[] c)
        {
            var n = new Color[a.Length + b.Length + c.Length];
            a.CopyTo(n, 0);
            b.CopyTo(n, a.Length);
            c.CopyTo(n, a.Length + b.Length);
            return n;
        }

        public void BlitHead()
        {
            Material m = this._headBlitMat;
            m.SetTexture("_SkinTex", this.skinHead);
            m.SetTexture("_EyesTex", this.eyes);
            m.SetTexture("_MouthTex", this.mouth);
            m.SetColorArray("_OldPallete", this._ConcatPalettes(ColorLUT.hairOld, ColorLUT.eyesOld, ColorLUT.skinOld));
            m.SetColorArray("_NewPallete", this._ConcatPalettes(this.hairPalette, this.eyesPalette, this.skinPalette));
            Graphics.Blit(null, this._headRT, m);
            this.headMaterial.SetTexture("_BaseMap", this._headRT);
        }

    }
}
*/