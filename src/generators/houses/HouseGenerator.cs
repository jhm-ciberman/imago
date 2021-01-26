using System.Collections.Generic;
using LifeSim.Simulation;

namespace LifeSim.Generation
{
    public class HouseGenerator 
    {
        public static IRoofModel ROOF_GABLE_HORIZONTAL = new GableRoofModel(true);
        public static IRoofModel ROOF_GABLE_VERTICAL = new GableRoofModel(false);
        public static IRoofModel ROOF_HIP = new HipRoofModel();
        public static IRoofModel ROOF_FLAT = new FlatRoofModel();

        private System.Random _random;

        private IContainer _container;

        public HouseGenerator(IContainer container, System.Random random)
        {
            this._container = container;
            this._random = random;
        }

        public House Generate(HouseLot lot)
        {
            HousePreset housePreset = this._MakeHousePreset();
            House house = new House(lot);

            Cover coverExt = this._Choose(housePreset.wallsExterior);
            Cover coverInt = this._Choose(housePreset.wallsInterior);

            Cover floor   = this._Choose(housePreset.floors);
            Cover ceiling = this._Choose(housePreset.floors);
            
            Aperture.Model window = this._Choose(housePreset.windows);
            Aperture.Model door = this._Choose(housePreset.doors);

            var storeyCount = this._random.Next(1, 1);
            house.SetStoreyCount(storeyCount);
            for (var i = 0; i < storeyCount; i++) {
                this._AddStorey(house, floor, i == 0 ? null : ceiling, i);
            }

            this._AddRoof(house, housePreset);

            this._CreateExteriorWalls(house, new CoverPair(coverExt, coverInt));
            this._AddApertures(house, window, door);
            return house;
        }

        private T _Choose<T>(T[] array)
        {
            return array[this._random.Next(0, array.Length)];
        }

        private void _AddApertures(House house, Aperture.Model window, Aperture.Model door)
        {
            if (house.size.y >= 5) {
                var wall = house.GetCell(new Vector2Int(0, 4), 0).wallWest;
                var aperture = new Aperture(door, wall);
                wall.SetAperture(aperture);
                house.AddSubentity(aperture);
            }

            for (var i = 0; i < house.storeysCount; i++) {
                foreach (var wall in house.GetWallsAtLevel(i)) {
                    if (wall.aperture != null) continue;
                    if (this._random.NextDouble() < 0.8f) continue;

                    var aperture = new Aperture(window, wall.sideInt);
                    wall.SetAperture(aperture);
                    house.AddSubentity(aperture);
                }
            }
        }

        private void _CreateExteriorWalls(House house, CoverPair covers)
        {
            foreach (var cell in house.cells) {
                if (! cell.hasFloor) continue;
                
                if (! cell.north.hasFloor) cell.wallNorth.SetCovers(covers);
                if (! cell.east.hasFloor ) cell.wallEast.SetCovers(covers);
                if (! cell.south.hasFloor) cell.wallSouth.SetCovers(covers);
                if (! cell.west.hasFloor ) cell.wallWest.SetCovers(covers);
            }

            if (house.size.x >= 5) {
                for (var y = 0; y < house.size.y; y++) {
                    var cell = house.GetCell(new Vector2Int(3, y), 0);
                    cell.wallWest.SetCovers(covers);
                }
            }
        }

        private void _AddRoof(House house, HousePreset housePreset)
        {
            Cover roofExterior = this._Choose(housePreset.roofsExterior);
            Cover roofInterior = this._Choose(housePreset.roofsInterior);

            Cover wallExterior = this._Choose(housePreset.wallsExterior);
            Cover wallInterior = this._Choose(housePreset.wallsInterior);

            

            IRoofModel[] roofsModelsList = new IRoofModel[] {
                HouseGenerator.ROOF_GABLE_HORIZONTAL,
                HouseGenerator.ROOF_GABLE_VERTICAL,
                HouseGenerator.ROOF_HIP,
                HouseGenerator.ROOF_FLAT,
            };
            var mainRoof = this._Choose(roofsModelsList);
            var wall = new CoverPair(wallExterior, wallInterior);

            var style = new RoofStyle(roofExterior, roofInterior, wall);

            foreach (var cell in this.GetHighestCells(house)) {
                var model = this._random.NextDouble() > 0.8f ? this._Choose(roofsModelsList) : mainRoof;
                var roof = cell.tile.roof;
                roof.SetBaseLevel(cell.level + 1);
                roof.SetStyle(style);
                roof.SetModel(model);
                if (cell.level >= 0) {
                    VoxelLight.RemoveLight(cell);
                }
            }

            // TODO: Update roof heights?
            house.RecomputeRoofHeights();
        }

        public IEnumerable<Cell> GetHighestCells(House house)
        {
            foreach (var tile in house.tiles) {
                for (int level = house.storeysCount - 1; level >= 0; level--) {
                    var cell = tile.GetCellAtLevel(level);
                    if (cell.hasFloor) {
                        yield return cell;
                        break;
                    }
                }
            }
        }

        private void _AddStorey(House house, Cover floorCover, Cover? ceilingCover, int level)
        {
            foreach (var tile in house.tiles) {
                //if (! house.CellIsBuildable(x, y)) continue; // TODO: Support non square house lots?

                //if (x == 1 && y == 1) continue;
                var cell = tile.GetCellAtLevel(level);
                cell.SetFloorCeiling(floorCover, ceilingCover);
                if (level > 0) {
                    VoxelLight.RemoveLight(cell.cellBelow);
                }
            }
        }

        private HousePreset _MakeHousePreset()
        {
            return new HousePreset {
                roofsExterior = new Cover[] {
                    this._container.Get<Cover>("roof.thatchdark"),
                    this._container.Get<Cover>("roof.thatchlight"),
                    this._container.Get<Cover>("roof.tilered"),
                    this._container.Get<Cover>("roof.tilebrown"),
                },
                roofsInterior = new Cover[] {
                    this._container.Get<Cover>("floor.lumberjackdestiny")
                },
                wallsExterior = new Cover[] {
                    //this._container.Get<Cover>("uvs"),
                    //this._container.Get<Cover>("wall.bricks"),
                    //this._container.Get<Cover>("wall.lumberjackdestiny"),
                    this._container.Get<Cover>("wall.mediewall"),
                    //this._container.Get<Cover>("wall.mediewallbricks"),
                    //this._container.Get<Cover>("wall.mediewallquad"),
                    //this._container.Get<Cover>("wall.woodenplanks"),
                },
                wallsInterior = new Cover[] {
                    //this._container.Get<Cover>("uvs"),
                    //this._container.Get<Cover>("wall.bricks"),
                    //this._container.Get<Cover>("wall.lumberjackdestiny"),
                    this._container.Get<Cover>("wall.mediewall"),
                    //this._container.Get<Cover>("wall.mediewallbricks"),
                    //this._container.Get<Cover>("wall.mediewallquad"),
                    //this._container.Get<Cover>("wall.woodenplanks"),
                },
                floors = new Cover[] {
                    this._container.Get<Cover>("floor.oakdream"),
                    this._container.Get<Cover>("floor.lumberjackdestiny"),
                },
                windows = new Aperture.Model[] {
                    this._container.Get<Aperture.Model>("aperture.mediewindow"),
                },
                doors = new Aperture.Model[] {
                    this._container.Get<Aperture.Model>("aperture.mediedoor"),
                },
            };
        }

        struct HousePreset
        {
            public Cover[] roofsExterior;

            public Cover[] roofsInterior;

            public Cover[] wallsExterior;

            public Cover[] wallsInterior;

            public Cover[] floors;

            public Aperture.Model[] windows;

            public Aperture.Model[] doors;
        }

    } 
}