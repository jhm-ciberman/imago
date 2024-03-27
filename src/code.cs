using System;

namespace LifeSim.Core.Content;

public class CharacterAnimation : GamePrototype
{
    public Guid AnimationId { get; set; } = Guid.Empty;

    public float Duration { get; set; } = float.NaN;

    public bool Loop { get; set; } = false;

    public float Speed { get; set; } = 1f;

    public void Validate()
    {
        if (float.IsNaN(this.Duration) && !this.Loop)
        {
            throw new Exception("Animation duration must be defined for non-looping animations");
        }
    }
}
using System;
using System.ComponentModel.DataAnnotations;
using LifeSim.Support.Drawing;

namespace LifeSim.Core.Content;

public class FloorCover : GamePrototype
{
    /// <summary>
    /// Gets or sets the name of the floor.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the color of the floor. This color is used in the top-down view map.
    /// </summary>
    public Color Color { get; set; } = Color.White;

    public Guid TextureId { get; set; } = Guid.Empty;

    public int Priority { get; set; } = 0;
}
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using LifeSim.Core.Characters;
using LifeSim.Core.Furnitures;
using LifeSim.Support;

namespace LifeSim.Core.Content;


public interface IIdentifiable
{
    string Id { get; }
}


public class Catalog<T> where T : IIdentifiable
{
    private readonly Dictionary<string, T> _elements = new Dictionary<string, T>();
    private readonly List<T> _elementsList = new List<T>();

    public int Count => this._elements.Count;



    public void Add(T element)
    {
        if (this._elements.ContainsKey(element.Id))
            throw new InvalidOperationException($"Element with identifier '{element.Id}' already exists.");

        this._elements.Add(element.Id, element);
        this._elementsList.Add(element);
    }

    public void AddRange(IEnumerable<T> elements)
    {
        foreach (var element in elements)
        {
            this.Add(element);
        }
    }

    public T Get(string identifier)
    {
        if (!this._elements.TryGetValue(identifier, out var element))
        {
            List<string> similarIds = this.FindSimilarIds(identifier);
            if (similarIds.Count > 0)
            {
                throw new InvalidOperationException($"Element with identifier '{identifier}' does not exist. Did you mean: {string.Join(", ", similarIds)}?");
            }
            else
            {
                throw new InvalidOperationException($"Element with identifier '{identifier}' does not exist.");
            }
        }

        return element;
    }

    public bool TryGet(string identifier, [NotNullWhen(true)] out T? element)
    {
        return this._elements.TryGetValue(identifier, out element);
    }

    public bool Contains(string identifier)
    {
        return this._elements.ContainsKey(identifier);
    }

    public IEnumerable<T> GetAll()
    {
        return this._elements.Values;
    }

    public IEnumerable<TElement> GetAll<TElement>() where TElement : T
    {
        return this._elements.Values.OfType<TElement>();
    }

    public T First()
    {
        return this._elements.Values.First();
    }

    public T Random(Random random)
    {
        var index = random.Next(this._elementsList.Count);
        return this._elementsList[index];
    }

    private List<string> FindSimilarIds(string identifier)
    {
        var similarIds = new List<string>();
        int minDistance = 30;
        int tolerance = 3;
        foreach (var element in this._elements.Values)
        {
            var distance = element.Id.LevenshteinDistance(identifier);
            if (distance < minDistance - tolerance)
            {
                similarIds.Clear();
                similarIds.Add(element.Id);
            }
            else if (distance < minDistance)
            {
                similarIds.Add(element.Id);
            }
        }

        return similarIds;
    }
}

public static class GameContent
{
    public static Catalog<Ground> Grounds { get; } = new();

    public static Catalog<FloorCover> Floors { get; } = new();

    public static Catalog<WallCover> Walls { get; } = new();

    public static Catalog<RoofCover> Roofs { get; } = new();

    public static Catalog<Outfit> Outfits { get; } = new();

    public static Catalog<Eyes> Eyes { get; } = new();

    public static Catalog<Mouth> Mouths { get; } = new();

    public static Catalog<Hair> Hairs { get; } = new();

    public static Catalog<Item> Items { get; } = new();

    public static Catalog<FurniturePrototype> Furniture { get; } = new();

    public static Catalog<EyeColor> EyeColors { get; } = new();

    public static Catalog<SkinColor> SkinColors { get; } = new();

    public static Catalog<HairColor> HairColors { get; } = new();

    public static Catalog<CharacterAnimation> CharacterAnimations { get; } = new();

    public static void Register(IIdentifiable content)
    {
        switch (content)
        {
            case Ground ground:
                Grounds.Add(ground);
                break;
            case FloorCover floor:
                Floors.Add(floor);
                break;
            case WallCover wall:
                Walls.Add(wall);
                break;
            case RoofCover roof:
                Roofs.Add(roof);
                break;
            case Outfit outfit:
                Outfits.Add(outfit);
                break;
            case Eyes eyes:
                Eyes.Add(eyes);
                break;
            case Mouth mouth:
                Mouths.Add(mouth);
                break;
            case Hair hair:
                Hairs.Add(hair);
                break;
            case Item item:
                Items.Add(item);
                break;
            case FurniturePrototype furniture:
                Furniture.Add(furniture);
                break;
            case EyeColor eyesColor:
                EyeColors.Add(eyesColor);
                break;
            case SkinColor skinColor:
                SkinColors.Add(skinColor);
                break;
            case HairColor hairColor:
                HairColors.Add(hairColor);
                break;
            default:
                throw new InvalidOperationException($"Unknown content type '{content.GetType().Name}'.");
        }
    }

    public static void Validate()
    {
        foreach (var animation in CharacterAnimations.GetAll())
        {
            animation.Validate();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace LifeSim.Core.Content;

public abstract class GamePrototype : IIdentifiable, IEquatable<GamePrototype>
{
    private string _id = string.Empty;

    /// <summary>
    /// Gets the identifier of this object. This is the unique identifier of this object within the pack.
    /// For example: "furniture.modern_table".
    /// </summary>
    [DefaultValue("")]
    public string Id
    {
        get => this._id;
        set => this._id = string.Intern(value);
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    public override string ToString()
    {
        return $"{this.GetType().Name} '{this.Id}'";
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return this.Equals(obj as GamePrototype);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="other">The object to compare with the current object.</param>
    /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
    public bool Equals(GamePrototype? other)
    {
        return other != null &&
               this.Id == other.Id;
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.Id);
    }

    /// <summary>
    /// Determines whether two specified objects have the same value.
    /// </summary>
    /// <param name="left">The first object to compare, or null.</param>
    /// <param name="right">The second object to compare, or null.</param>
    /// <returns>true if the value of left is the same as the value of right; otherwise, false.</returns>
    public static bool operator ==(GamePrototype? left, GamePrototype? right)
    {
        return EqualityComparer<GamePrototype>.Default.Equals(left, right);
    }

    /// <summary>
    /// Determines whether two specified objects have different values.
    /// </summary>
    /// <param name="left">The first object to compare, or null.</param>
    /// <param name="right">The second object to compare, or null.</param>
    /// <returns>true if the value of left is different from the value of right; otherwise, false.</returns>
    public static bool operator !=(GamePrototype? left, GamePrototype? right)
    {
        return !(left == right);
    }
}
using System;
using LifeSim.Support.Drawing;

namespace LifeSim.Core.Content;

public class Ground : GamePrototype
{
    public float FertilityMin { get; set; } = 0f;
    public float FertilityMax { get; set; } = 1f;

    public float NavgridCost { get; set; } = 1f;

    /// <summary>
    /// Gets or sets the name of the floor.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the color of the floor. This color is used in the top-down view map.
    /// </summary>
    public Color Color { get; set; } = Color.White;

    /// <summary>
    /// Gets or sets the priority of this floor. A higher priority floor will be drawn on top of a lower priority floor.
    /// </summary>
    public int Priority { get; set; } = 0;

    /// <summary>
    /// Gets or sets the texture ID of the floor. This texture is used in the 3D view.
    /// </summary>
    public Guid TextureId { get; set; } = Guid.Empty;
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LifeSim.Core.Characters;
using LifeSim.Core.Furnitures;
using LifeSim.Core.Furnitures.Behaviors;
using LifeSim.Core.Terrain;

namespace LifeSim.Core.Content;


public interface IItemTrait
{
    // Nothing, just a marker interface.
}

public class Item : GamePrototype
{
    /// <summary>
    /// Gets or sets the name of the object.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the object.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ID of the small icon.
    /// </summary>
    public Guid SmallIconId { get; set; } = Guid.Empty;

    /// <summary>
    /// Gets or sets the ID of the large icon.
    /// </summary>
    public Guid LargeIconId { get; set; } = Guid.Empty;

    /// <summary>
    /// Gets or sets the texture Id.
    /// </summary>
    public Guid TextureId { get; set; } = Guid.Empty;

    /// <summary>
    /// Gets or sets the model Id.
    /// </summary>
    public Guid ModelId { get; set; } = Guid.Empty;

    /// <summary>
    /// Gets or sets the traits of the object.
    /// </summary>
    public IList<IItemTrait> Traits { get; set; } = Array.Empty<IItemTrait>();

    /// <summary>
    /// Gets the trait of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the trait.</typeparam>
    /// <returns>The trait of the specified type.</returns>
    public T GetTrait<T>() where T : IItemTrait
    {
        foreach (var trait in this.Traits)
        {
            if (trait is T t) return t;
        }

        throw new InvalidOperationException($"Item does not have a trait of type {typeof(T).Name}.");
    }

    /// <summary>
    /// Determines whether the object has a trait of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the trait.</typeparam>
    /// <returns>True if the object has a trait of the specified type; otherwise, false.</returns>
    public bool HasTrait<T>() where T : IItemTrait
    {
        foreach (var trait in this.Traits)
        {
            if (trait is T) return true;
        }

        return false;
    }

    public Furniture PlaceAt(Cell cell)
    {
        return this.GetFurniturePrototype().PlaceAt(cell, CardinalDirection.South);
    }

    private PlacedItemPrototype? _prototype = null;

    private PlacedItemPrototype GetFurniturePrototype()
    {
        this._prototype ??= new PlacedItemPrototype(this)
        {
            Id = this.Id,
            Name = this.Name,
            Description = this.Description,
            ModelId = this.ModelId,
            TextureId = this.TextureId,
        };

        return this._prototype;
    }
}

public class DropItemToFloorInteraction : IInteraction
{
    public string Name => this.Character.ItemInHand != null
        ? $"Drop {this.Character.ItemInHand.Name}"
        : "Drop item";

    public Character Character { get; }

    public Cell Target { get; set; }

    public DropItemToFloorInteraction(Character character, Cell target)
    {
        this.Character = character;
        this.Target = target;
    }

    public bool CanInteract(Character character)
    {
        return character.ItemInHand != null;
    }

    public void Interact(Character character)
    {
        if (character.TryDrop(out var item))
        {
            item.PlaceAt(this.Target);
        }
    }
}

public class PlantableTrait : IItemTrait
{
    public string CropId { get; set; } = string.Empty;

    public float GrowthTime { get; set; } = 3f;
}

namespace LifeSim.Core.Content;

public enum MaterialKind
{
    Opaque,
    AlphaTest,
    Transparent,
}
using System;
using System.ComponentModel.DataAnnotations;

namespace LifeSim.Core.Content;

public class RoofCover : GamePrototype
{
    /// <summary>
    /// Gets or sets the name of the roof.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    public Guid TextureId { get; set; } = Guid.Empty;
}
using System;
using System.ComponentModel.DataAnnotations;

namespace LifeSim.Core.Content;

public class WallCover : GamePrototype
{
    /// <summary>
    /// Gets or sets the name of the wall.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    public Guid TextureId { get; set; } = Guid.Empty;
}
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using LifeSim.Core;
using LifeSim.Core.Content;

namespace LifeSim.Json.Content;

[JsonDerivedType(typeof(JsonFloorCover), typeDiscriminator: "FloorCover")]
[JsonDerivedType(typeof(JsonFurnitureDefinition), typeDiscriminator: "Furniture")]
[JsonDerivedType(typeof(JsonHair), typeDiscriminator: "Hair")]
[JsonDerivedType(typeof(JsonItem), typeDiscriminator: "Item")]
[JsonDerivedType(typeof(JsonOutfit), typeDiscriminator: "Outfit")]
[JsonDerivedType(typeof(JsonRoofCover), typeDiscriminator: "RoofCover")]
[JsonDerivedType(typeof(JsonWallCover), typeDiscriminator: "WallCover")]
public abstract class JsonContent
{
    /// <summary>
    /// Gets or sets the identifier of this object. This is the unique identifier of this object within the pack.
    /// For example: "furniture.modern_table".
    /// </summary>
    [Required]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Converts this <see cref="JsonContent"/> content to a game definition.
    /// </summary>
    /// <param name="loader">The resource loader.</param>
    /// <returns>The game definition.</returns>
    public abstract GamePrototype ToModel(IResourceRegistry loader);
}
using System.ComponentModel.DataAnnotations;
using LifeSim.Core;
using LifeSim.Core.Content;
using LifeSim.Support.Drawing;

namespace LifeSim.Json.Content;

public class JsonFloorCover : JsonContent
{
    /// <summary>
    /// Gets or sets the name of the floor.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the color of the floor. This color is used in the top-down view map.
    /// </summary>
    public Color Color { get; set; } = Color.White;

    [Required]
    public ResourceUri TexturePath { get; set; } = ResourceUri.Empty;

    public int Priority { get; set; } = 0;

    public override GamePrototype ToModel(IResourceRegistry loader)
    {
        return new FloorCover
        {
            Id = this.Id,
            Name = this.Name,
            Color = this.Color,
            Priority = this.Priority,
            TextureId = loader.RegisterImage(this.TexturePath),
        };
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LifeSim.Core;
using LifeSim.Core.Content;
using LifeSim.Core.Furnitures;
using LifeSim.Support;

namespace LifeSim.Json.Content;

public class JsonFurnitureDefinition : JsonContent
{
    /// <summary>
    /// Gets or sets the name of the furniture.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the furniture.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    public ResourceUri TexturePath { get; set; } = string.Empty;

    public ResourceUri ModelPath { get; set; } = string.Empty;

    public CardinalDirection ModelFacing { get; set; } = CardinalDirection.South;

    public WallCarving? WallCarving { get; set; } = null;

    public string Behavior { get; set; } = string.Empty;

    public Dictionary<string, object>? Config { get; set; } = null;

    public override GamePrototype ToModel(IResourceRegistry loader)
    {
        var prototype = this.CreatePrototype(this.Behavior);

        prototype.Id = this.Id;
        prototype.Name = this.Name;
        prototype.Description = this.Description;
        prototype.TextureId = loader.RegisterImage(this.TexturePath);
        prototype.ModelId = loader.RegisterModel(this.ModelPath);
        prototype.ModelFacing = this.ModelFacing;
        prototype.WallCarving = this.WallCarving;

        prototype.FromConfig(this.Config);

        return prototype;
    }

    private Dictionary<string, Type>? _definitions = null;

    private FurniturePrototype CreatePrototype(string name)
    {
        this._definitions ??= ScanPrototypes();

        if (this._definitions.TryGetValue(name, out var type))
        {
            return (FurniturePrototype)Activator.CreateInstance(type)!;
        }

        throw new InvalidOperationException($"Unknown furniture behavior '{name}'.");
    }

    private static Dictionary<string, Type> ScanPrototypes()
    {
        var prototypes = new Dictionary<string, Type>();

        foreach (var type in typeof(FurniturePrototype).Assembly.GetTypes())
        {
            if (type.IsSubclassOf(typeof(FurniturePrototype)))
            {
                var name = type.Name.Replace("Prototype", string.Empty);

                name = name.ToSnakeCase();

                prototypes[name] = type;
            }
        }

        prototypes[""] = typeof(FurniturePrototype);

        return prototypes;
    }
}
using LifeSim.Core;
using LifeSim.Core.Characters;
using LifeSim.Core.Content;

namespace LifeSim.Json.Content;

public class JsonHair : JsonContent
{
    /// <summary>
    /// Gets or sets the name of the hair.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    public ResourceUri TexturePath { get; set; } = ResourceUri.Empty;

    public ResourceUri ModelPath { get; set; } = ResourceUri.Empty;

    public override GamePrototype ToModel(IResourceRegistry loader)
    {
        return new Hair
        {
            Id = this.Id,
            Name = this.Name,
            TextureId = loader.RegisterImage(this.TexturePath),
            ModelId = loader.RegisterModel(this.ModelPath),
        };
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using LifeSim.Core;
using LifeSim.Core.Content;

namespace LifeSim.Json.Content;

public class JsonItem : JsonContent
{
    /// <summary>
    /// Gets or sets the name of the object.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the object.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    public ResourceUri SmallIconPath { get; set; } = ResourceUri.Empty;

    public ResourceUri LargeIconPath { get; set; } = ResourceUri.Empty;

    public ResourceUri TexturePath { get; set; } = ResourceUri.Empty;
    public ResourceUri ModelPath { get; set; } = ResourceUri.Empty;

    public IList<IJsonItemTrait> Traits { get; set; } = Array.Empty<IJsonItemTrait>();

    public override GamePrototype ToModel(IResourceRegistry loader)
    {
        return new Item
        {
            Id = this.Id,
            Name = this.Name,
            Description = this.Description,
            SmallIconId = loader.RegisterImage(this.SmallIconPath),
            LargeIconId = loader.RegisterImage(this.LargeIconPath),
            TextureId = loader.RegisterImage(this.TexturePath),
            ModelId = loader.RegisterModel(this.ModelPath),
            Traits = this.Traits.Select(t => t.ToModel(loader)).ToList(),
        };
    }
}

[JsonDerivedType(typeof(JsonPlantableTrait), typeDiscriminator: "Plantable")]
public interface IJsonItemTrait
{
    public IItemTrait ToModel(IResourceRegistry loader);
}

public class JsonPlantableTrait : IJsonItemTrait
{
    public string CropId { get; set; } = string.Empty;

    public float GrowthTime { get; set; } = 3f;

    public IItemTrait ToModel(IResourceRegistry loader)
    {
        return new PlantableTrait
        {
            CropId = this.CropId,
            GrowthTime = this.GrowthTime,
        };
    }
}

using LifeSim.Core;
using LifeSim.Core.Characters;
using LifeSim.Core.Content;

namespace LifeSim.Json.Content;

public class JsonOutfit : JsonContent
{
    /// <summary>
    /// Gets or sets the name of the outfit.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the outfit.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    public ResourceUri TexturePath { get; set; } = ResourceUri.Empty;

    public ResourceUri ModelPath { get; set; } = ResourceUri.Empty;

    public override GamePrototype ToModel(IResourceRegistry loader)
    {
        return new Outfit
        {
            Id = this.Id,
            Name = this.Name,
            Description = this.Description,
            TextureId = loader.RegisterImage(this.TexturePath),
            ModelId = loader.RegisterModel(this.ModelPath),
        };
    }
}
using System.ComponentModel.DataAnnotations;
using LifeSim.Core;
using LifeSim.Core.Content;

namespace LifeSim.Json.Content;

public class JsonRoofCover : JsonContent
{
    /// <summary>
    /// Gets or sets the name of the roof.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    public ResourceUri TexturePath { get; set; } = ResourceUri.Empty;


    public override GamePrototype ToModel(IResourceRegistry loader)
    {
        return new RoofCover
        {
            Id = this.Id,
            Name = this.Name,
            TextureId = loader.RegisterImage(this.TexturePath),
        };
    }
}
using System.ComponentModel.DataAnnotations;
using LifeSim.Core;
using LifeSim.Core.Content;

namespace LifeSim.Json.Content;

public class JsonWallCover : JsonContent
{
    /// <summary>
    /// Gets or sets the name of the wall.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    public ResourceUri TexturePath { get; set; } = ResourceUri.Empty;

    public override GamePrototype ToModel(IResourceRegistry loader)
    {
        return new WallCover
        {
            Id = this.Id,
            Name = this.Name,
            TextureId = loader.RegisterImage(this.TexturePath),
        };
    }
}
