/*
using System.Collections.Generic;

namespace LifeSim.Assets
{
    public class ColorLUT
    {
        public static Color[] hairOld;
        public static Color[] eyesOld;
        public static Color[] skinOld;

        public readonly List<Color[]> hair;
        public readonly List<Color[]> eyes;
        public readonly List<Color[]> skin;

        public ColorLUT(Texture2D lut)
        {
            this.hair = this._LoadPaletteList(lut, 1, 10, 6);
            this.eyes = this._LoadPaletteList(lut, 1,  4, 6);
            this.skin = this._LoadPaletteList(lut, 1,  0, 4);

            ColorLUT.hairOld = this._LoadPalette(lut, 0, 10, 6);
            ColorLUT.eyesOld = this._LoadPalette(lut, 0,  4, 6);
            ColorLUT.skinOld = this._LoadPalette(lut, 0,  0, 4);
        }

        private List<Color[]> _LoadPaletteList(Texture2D lut, int xStart, int yStart, int palleteSize)
        {
            var list = new List<Color[]>();

            for (int x = xStart; x < lut.width; x++)
            {
                var c = this._LoadPalette(lut, x, yStart, palleteSize);
                if (c == null) return list;
                list.Add(c);
            }

            return list;
        }

        private Color[] _LoadPalette(Texture2D lut, int x, int yStart, int palleteSize)
        {
            Color[] p = new Color[palleteSize];
            for (int y = 0; y < palleteSize; y++)
            {
                var col = lut.GetPixel(x, yStart + y);
                if (col.a == 0f) return null;
                p[y] = col;
            }
            return p;
        }

    }
}
*/