using System.Collections.Generic;
using System.Numerics;
using LifeSim.Engine;
using LifeSim.Engine.Rendering;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LifeSim.Assets
{
    public class TilemapManager
    {
        private readonly Vector2Int _gridSize;

        private int _mipMapLevels;

        private int _tileSize;

        private Image<Rgba32> _image;

        private Queue<Vector2Int> _freeTiles;

        private Dictionary<TileRequest, PackedTile> _tiles = new Dictionary<TileRequest, PackedTile>();
        private List<TileDrawOperation> _pendingOperations = new List<TileDrawOperation>();

        private GPUTexture _texture;
        private SurfaceMaterial _material;

        private int textureMaxSize;

        private TileDescriptorFactory _tileDescriptorFactory;

        public TilemapManager(AssetManager assetManager, AssetsContainer assetsContainer, int textureMaxSize, int mipMapLevels, int tileSize)
        {
            this.textureMaxSize = textureMaxSize;
            this._gridSize = new Vector2Int(textureMaxSize, textureMaxSize) / tileSize;
            this._tileSize = tileSize;
            this._mipMapLevels = mipMapLevels > 0 ? mipMapLevels : 0;

            int count = this._gridSize.x * this._gridSize.y;
            this._freeTiles = new Queue<Vector2Int>(count);
            for (int index = 0; index < count; index++)
            {
                Vector2Int coord = new Vector2Int(index % this._gridSize.x, index / this._gridSize.y);
                this._freeTiles.Enqueue(coord);
            }

            this._image = new Image<Rgba32>(textureMaxSize, textureMaxSize);
            this._texture = assetManager.MakeTexture(this._image);
            this._material = assetManager.MakeSurfaceMaterial(this._texture);

            this._tileDescriptorFactory = new TileDescriptorFactory(assetsContainer);
        }

        public GPUTexture texture => this._texture;
        public SurfaceMaterial material => this._material;

        public void UpdateTilemap()
        {
            foreach (var op in this._pendingOperations)
                op.DrawTile();

            this._texture.Update(this._image);

            this._pendingOperations.Clear();
        }

        public PackedTile RequestPackedTile(TileRequest request)
        {
            if (! this._tiles.TryGetValue(request, out PackedTile? tile))
            {
                tile = this._MakeNewTile(request);
                this._tiles[request] = tile;
            }

            return tile;
        }

        private PackedTile _MakeNewTile(TileRequest request)
        {
            var descriptor = this._tileDescriptorFactory.BuildDescriptor(request);
            
            Vector2Int coord = this._freeTiles.Dequeue();
            
            Vector2 uv1 = new Vector2(coord.x    , coord.y    ) / this._gridSize;
            Vector2 uv2 = new Vector2(coord.x + 1, coord.y + 1) / this._gridSize;
            var packedTile = new PackedTile(uv1, uv2, this._texture);

            var op = new TileDrawOperation(descriptor, this._tileSize, this._image, coord);
            this._pendingOperations.Add(op);

            packedTile.rotable = descriptor.rotable;
            return packedTile;
        }
    }
}