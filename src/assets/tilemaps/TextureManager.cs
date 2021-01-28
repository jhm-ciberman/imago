using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LifeSim.Engine;
using LifeSim.Engine.Rendering;
using LifeSim.Simulation;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LifeSim.Assets
{
    public class TextureManager
    {
        private readonly Vector2Int _gridSize;

        private uint _tileSize;

        private Image<Rgba32> _image;

        private Queue<Vector2Int> _freeTiles;

        private Dictionary<TileRequest, PackedTile> _tiles = new Dictionary<TileRequest, PackedTile>();
        
        private List<TileDrawOperation> _pendingOperations = new List<TileDrawOperation>();

        private GPUTexture _texture;
        private SurfaceMaterial _materialTerrain;
        private SurfaceMaterial _materialHouses;
        private SurfaceMaterial _materialWater;

        private Dictionary<string, Tilemap> _tilemaps = new Dictionary<string, Tilemap>();

        private Dictionary<string, PackedTexture> _packedTextures = new Dictionary<string, PackedTexture>();

        public TextureManager(ResourceFactory assetManager, GPUTexture mainAtlasTexture, uint textureMaxSize, uint tileSize)
        {
            tileSize = this._NextPowOfTwo(tileSize);
            textureMaxSize = (uint) 1 << BitOperations.Log2(textureMaxSize);
            var mipMapLevels = (uint) BitOperations.Log2(tileSize);

            this._gridSize = new Vector2Int(textureMaxSize, textureMaxSize) / tileSize;
            this._tileSize = tileSize;

            int count = this._gridSize.x * this._gridSize.y;
            this._freeTiles = new Queue<Vector2Int>(count);
            for (int index = 0; index < count; index++) {
                Vector2Int coord = new Vector2Int(index % this._gridSize.x, index / this._gridSize.y);
                this._freeTiles.Enqueue(coord);
            }

            this._image = new Image<Rgba32>((int) textureMaxSize, (int) textureMaxSize);
            this._texture = assetManager.MakeTexture(this._image, mipMapLevels);

            this._materialTerrain = assetManager.MakeSurfaceMaterial(this._texture);
            this._materialHouses = assetManager.MakeSurfaceMaterial(mainAtlasTexture);
            this._materialWater = assetManager.MakeSurfaceMaterial(mainAtlasTexture);
            this._materialWater.castShadows = false;
            this._materialTerrain.castShadows = false;
        }

        private uint _NextPowOfTwo(uint x)
        {
            --x;
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;
            return x + 1;
        }

        public void AddTilemap(string id, Tilemap tilemap)
        {
            this._tilemaps[id] = tilemap;
        }

        public void AddPackedTexture(string id, PackedTexture packedTexture)
        {
            this._packedTextures[id] = packedTexture;
        }

        public PackedTexture GetPackedTexture(Cover cover)
        {
            return this._packedTextures["tex:" + cover.id];
        }

        public PackedTexture GetWaterTexture()
        {
            return this._packedTextures["tex:water"];
        }

        public PackedTexture GetPackedTexture(Simulation.Object obj)
        {
            return this._packedTextures["tex:" + obj.id];
        }

        public SurfaceMaterial materialTerrain => this._materialTerrain;
        public SurfaceMaterial materialHouses  => this._materialHouses;
        public SurfaceMaterial materialWater   => this._materialWater;

        public void UpdateTilemap()
        {
            foreach (var op in this._pendingOperations) {
                op.DrawTile();
            }

            this._texture.Update(this._image);

            this._pendingOperations.Clear();
        }

        public PackedTile RequestPackedTile(TileRequest request)
        {
            if (! this._tiles.TryGetValue(request, out PackedTile? tile)) {
                tile = this._MakeNewTile(request);
                this._tiles[request] = tile;
            }

            return tile;
        }

        private PackedTile _MakeNewTile(TileRequest request)
        {
            var descriptor = this._BuildDescriptor(request);
            
            Vector2Int coord = this._freeTiles.Dequeue();
            
            Vector2 uv1 = new Vector2(coord.x    , coord.y    ) / this._gridSize;
            Vector2 uv2 = new Vector2(coord.x + 1, coord.y + 1) / this._gridSize;
            var packedTile = new PackedTile(uv1, uv2, this._texture);

            var op = new TileDrawOperation(descriptor, this._tileSize, this._image, coord);
            this._pendingOperations.Add(op);

            packedTile.rotable = descriptor.rotable;
            return packedTile;
        }

        private TileDescriptor _BuildDescriptor(TileRequest request)
        {
            var centerTileMap = this._tilemaps["tilemap:" + request.center.id];
            var descriptor = new TileDescriptor(centerTileMap);

            foreach (var entry in request.layers.OrderBy(entry => entry.Key.dominance))
            {
                var bytemask = entry.Value;
                var layerTileMap = this._tilemaps["tilemap:" + entry.Key.id];
                descriptor.AddLayers(layerTileMap, bytemask);
            }

            return descriptor;
        }
    }
}