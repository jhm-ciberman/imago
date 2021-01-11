using System.Collections.Generic;
using System;
using System.Linq;

namespace LifeSim
{
    public class TileDescriptor : IEquatable<TileDescriptor>
    {
        public readonly struct Layer
        {
            public readonly RectInt srcRect;
            public readonly Vector2Int dstOffset;
            public readonly Tilemap tilemap;

            public Layer(Tilemap tilemap, RectInt srcRect, Vector2Int dstOffset)
            {
                var pos    = (srcRect.coords * tilemap.tileSize);
                var size   = (srcRect.size   * tilemap.tileSize);
                var offset = (dstOffset      * tilemap.tileSize);

                this.tilemap = tilemap;
                this.dstOffset = offset;
                this.srcRect = new RectInt(pos, size);
            }

            public override string ToString() 
            {
                return "Layer(" + this.tilemap + "," + this.srcRect + "," + this.dstOffset + ")";
            }
        }

        public bool rotable;

        public TileDescriptor(Tilemap baseTilemap)
        {
            this._layers.Add(baseTilemap.layerBase);
            this.rotable = baseTilemap.centerRotable;
        }

        public override string ToString() 
        {
            string str = "TileDescriptor {";
            foreach (var layer in this.layers)
                str += layer.ToString() + ",";
            str += ")";
            return str;
        }

        List<Layer> _layers = new List<Layer>();

        public IEnumerable<Layer> layers => this._layers;
        public int layersCount => this._layers.Count;

        public void AddLayer(Layer layer)
        {
            this._layers.Add(layer);
            this.rotable = false;
        }

        public override int GetHashCode()
        {
            int hc = 0;
            foreach (var p in this._layers)
            {
                hc ^= p.GetHashCode();
            }
            return hc;
        }

        public bool Equals(TileDescriptor? other)
        {
            return (other != null && this._layers.SequenceEqual(other._layers));
        }
    }
}