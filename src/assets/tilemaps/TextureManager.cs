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

        private Dictionary<TileRequest, PackedTile> _tiles = new Dictionary<TileRequest, PackedTile>();
        
        private List<TileDrawOperation> _pendingOperations = new List<TileDrawOperation>();

        private SurfaceMaterial _materialTerrain;
        private SurfaceMaterial _materialHouses;
        private SurfaceMaterial _materialWater;

        private Atlas _atlas;

        private Dictionary<string, Tilemap> _tilemaps = new Dictionary<string, Tilemap>();

        private Dictionary<string, PackedTexture> _packedTextures = new Dictionary<string, PackedTexture>();

        public TextureManager(ResourceFactory assetManager, uint atlasSize, uint tileSize)
        {
            this._gridSize = new Vector2Int(atlasSize, atlasSize) / tileSize;
            this._tileSize = tileSize;

            this._atlas = new Atlas(assetManager, atlasSize, tileSize);

            this._materialTerrain = assetManager.MakeSurfaceMaterial(this._atlas.texture);
            this._materialTerrain.castShadows = false;
            this._materialHouses = assetManager.MakeSurfaceMaterial(this._atlas.texture);
            this._materialHouses.castShadows = true;
            this._materialWater = assetManager.MakeSurfaceMaterial(this._atlas.texture);
            this._materialWater.castShadows = false;
        }


        public void RegisterTilemap(Tilemap tilemap)
        {
            this._tilemaps[tilemap.id] = tilemap;
        }

        public void RegisterTextures(IEnumerable<UnpackedTexture> textures)
        {
            foreach (var result in this._atlas.Pack(textures)) {
                var packed = new PackedTexture(result.uv1, result.uv2, this._atlas.texture);
                this._packedTextures[result.element.id] = packed;
            }
        }

        public PackedTexture RequestTexture(Cover cover)
        {
            this._atlas.Apply();
            return this._packedTextures["tex:" + cover.id];
        }

        public PackedTexture RequestWaterTexture()
        {
            this._atlas.Apply();
            return this._packedTextures["tex:water"];
        }

        public PackedTexture RequestPackedTexture(Simulation.Object obj)
        {
            this._atlas.Apply();
            return this._packedTextures["tex:" + obj.id];
        }

        public SurfaceMaterial materialTerrain => this._materialTerrain;
        public SurfaceMaterial materialHouses  => this._materialHouses;
        public SurfaceMaterial materialWater   => this._materialWater;

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
            
            var op = new TileDrawOperation(descriptor, this._tileSize);
            var result = this._atlas.PackOne(op);

            var packedTile = new PackedTile(result.uv1, result.uv2, this._atlas.texture);

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