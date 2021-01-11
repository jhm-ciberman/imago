using System;
using System.Collections.Generic;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LifeSim.Assets
{
    public class TilemapLUT
    {
        private List<Rgba32> _lut;

        public TilemapLUT(Image<Rgba32> texture, RectInt rect)
        {
            var set = new HashSet<Rgba32>();

            float sum = 0f;
            for (int y = 0; y < rect.width; y++) {
                for (int x = 0; x < rect.width; x++) {
                    var col = texture[x, texture.Height - 1 - y];
                    sum += this._ToGrayscale(col);
                    set.Add(col);
                }
            }
            float avg = sum / (rect.width * rect.height);

            var list = set
                .Where(col => this._ToGrayscale(col) < avg)
                .OrderBy((col) => this._ToGrayscale(col))
                .ToList();

            if (list.Count >= 1) {
                var extraColor = list[0]; //;Color.LerpUnclamped(list[1], list[0], 1.5f);
                extraColor.A = 128;
                this._lut = list.Prepend(extraColor).ToList();
            } else {
                this._lut = list.ToList();
            }
        }

        private float _ToGrayscale(Rgba32 col) => 0.299f * col.R +  0.587f * col.G + 0.114f * col.B;

        public Rgba32 GetShadeColor(float shadePercent)
        {
            return this._lut[(int) MathF.Round((this._lut.Count - 1) * (1f - shadePercent))];
        }

    }
}