using System.Collections.Generic;
using LifeSim.Engine;
using LifeSim.Engine.Rendering;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using V = ClipperLib.IntPoint;

namespace LifeSim.Assets
{
    public class AssetsProvider
    {
        private AssetManager _assetsManager;

        public AssetsProvider(AssetManager assetsManager)
        {
            this._assetsManager = assetsManager;
        }
        
        public AssetsContainer MakeContainer()
        {
            AssetsContainer container = new AssetsContainer();

            //container.Register("tex:avatar.skin.body",      this._Tex("avatar/skin_body"));
            //container.Register("tex:avatar.skin.head",      this._Tex("avatar/skin_face"));
            //container.Register("tex:avatar.clothes.farmer", this._Tex("avatar/clothe_farmer"));
            //container.Register("tex:avatar.hairs.hair1",    this._Tex("avatar/hair"));
            //container.Register("tex:avatar.eyes.eyes",      this._Tex("avatar/eyes"));
            //container.Register("tex:avatar.mouth",          this._Tex("avatar/mouth"));

            List<(string, string)> packed = new List<(string, string)>() {
                ("tex:aperture.mediedoor"      , "apertures/mediedoor.png"       ),
                ("tex:aperture.mediewindow"    , "apertures/mediewindow.png"     ),
                ("tex:floor.lumberjackdestiny" , "floors/lumberjack_destiny.png" ),
                ("tex:floor.oakdream"          , "floors/oak_dream_floor.png"    ),
                ("tex:floor.woodenplanks"      , "floors/wooden_planks.png"      ),
                ("tex:furniture.beds.bed"      , "furniture/bed.png"             ),
                ("tex:furniture.tables.table"  , "furniture/table.png"           ),
                ("tex:furniture.waterthrough"  , "furniture/water_through.png"   ),
                ("tex:plant.bush"              , "plants/bush.png"               ),
                ("tex:plant.pine"              , "plants/pine_tree.png"          ),
                ("tex:plant.spruce"            , "plants/spruce_tree.png"        ),
                ("tex:roof.thatchdark"         , "roofs/roof_thatch_dark.png"    ),
                ("tex:roof.thatchlight"        , "roofs/roof_thatch_light.png"   ),
                ("tex:roof.tilebrown"          , "roofs/roof_tile_brown.png"     ),
                ("tex:roof.tilered"            , "roofs/roof_tile_red.png"       ),
                ("tex:wall.bricks"             , "walls/bricks.png"              ),
                ("tex:wall.lumberjackdestiny"  , "walls/lumberjack_destiny.png"  ),
                ("tex:wall.mediewall"          , "walls/mediewall.png"           ),
                ("tex:wall.mediewallbricks"    , "walls/mediewall_bricks.png"    ),
                ("tex:wall.mediewallquad"      , "walls/mediewall_quad.png"      ),
                ("tex:wall.woodenplanks"       , "walls/wooden_planks_wall.png"  ),
                ("tex:uvs"                     , "_test/uvs.jpg"                 ),
                ("tex:water"                   , "water.png"                     ),
            };

            var packer = new TexturePacker(this._assetsManager, 6, 1024);
            foreach ((string id, string path) in packed) {
                packer.Add(new UnpackedTexture(id, this._Tex(path))); 
            }
            foreach ((string id, PackedTexture packedTexture) in packer.Pack()) {
                container.Register(id, packedTexture);
            }

            container.mainAtlasTexture = packer.texture;

            //ColorLUT loader = new ColorLUT(this._Tex("avatar/lut"));
            //this._MapLutAndRegister(container, "avatar.lut.hair.", loader.hair);
            //this._MapLutAndRegister(container, "avatar.lut.eyes.", loader.eyes);
            //this._MapLutAndRegister(container, "avatar.lut.skin.", loader.skin);

            container.Register("tilemap:tilecover.mud",   new Tilemap(this._Tex("tilemap/mud.png"),           Tilemap.Layout.Simple , 16, false));
            container.Register("tilemap:tilecover.sand",  new Tilemap(this._Tex("tilemap/sand_tileset.png"),  Tilemap.Layout.Corners, 16, true ));
            container.Register("tilemap:tilecover.stone", new Tilemap(this._Tex("tilemap/simple_stone.png"),  Tilemap.Layout.Corners, 16, false));
            container.Register("tilemap:tilecover.grass", new Tilemap(this._Tex("tilemap/grass_tileset.png"), Tilemap.Layout.Corners, 16, true ));

            /*
            container.Register("e3d:plant.bush", new Plant3DData {
                texture = container.GetElement<PackedTexture>("tex:plant.bush"),
                billboardSize = new Vector2(1, 1),
                origin = new Vector2(0.5f, 0.5f),
                billboardPlanes = 4,
            }.MakeNewEntity3D());

            container.Register("e3d:plant.spruce", new Plant3DData {
                texture = container.GetElement<PackedTexture>("tex:plant.spruce"),
                billboardSize = new Vector2(2, 4),
                origin = new Vector2(1f, 1f),
                billboardPlanes = 4,
            }.MakeNewEntity3D());

            container.Register("e3d:plant.pine", new Plant3DData {
                texture = container.GetElement<PackedTexture>("tex:plant.pine"),
                billboardSize = new Vector2(2, 4),
                origin = new Vector2(1f, 1f),
                billboardPlanes = 4,
            }.MakeNewEntity3D());
            */

            var mediewindow = new View3D.ApertureAssetBuilder(
                container.Get<PackedTexture>("tex:aperture.mediewindow"), false, new V(12 + 4, 46 + 20),
                new List<V> { new V(12 + 0 , 46 + 0), new V(12 + 40, 46 + 0), new V(12 + 40, 46 + 40), new V(12 + 0 , 46 + 40)},
                new List<V> { new V(12 + 4 , 46 + 4), new V(12 + 36, 46 + 4), new V(12 + 36, 46 + 36), new V(12 + 4 , 46 + 36)}
            );

            var mediedoor = new View3D.ApertureAssetBuilder(
                container.Get<PackedTexture>("tex:aperture.mediedoor"), true, new V(10, 32),
                new List<V> { new V(4, 26), new V(60, 26), new V(60, 128), new V(4, 128)},
                new List<V> { new V(10, 32), new V(54, 32), new V(54, 128), new V(10, 128)}
            );

            container.Register("asset:aperture.mediewindow", mediewindow.BuildAsset());
            container.Register("asset:aperture.mediedoor", mediedoor.BuildAsset());

            container.Register("font:default", new FontAsset(this._assetsManager.MakeFontSystem(new string[] {
                @"res/fonts/DroidSans.ttf",
                @"res/fonts/DroidSansJapanese.ttf",
                @"res/fonts/Symbola-Emoji.ttf",
            })));

            return container;
        }

        private Image<Rgba32> _Tex(string path)
        {
            var tex = Image.Load<Rgba32>("res/" + path);
            if (tex == null) {
                throw new System.Exception("Texture \"" + path + "\" cannot be loaded");
            }
            return tex;
        }

        //private void _MapLutAndRegister<T>(Container container, string prefix, IEnumerable<T> pallettes)
        //{
        //    int i = 1;
        //    foreach (var p in pallettes) container.Register(prefix + (i++), p);
        //}

    }
}