using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;

namespace LifeSim.Imago.SceneGraph.Prefabs;

/// <summary>
/// Identifies a procedural model request by name and parameters. Used as a cache key in the procedural model registry.
/// </summary>
public readonly struct ProceduralModelRequest : IEquatable<ProceduralModelRequest>
{
    private static readonly IReadOnlyDictionary<string, string> _emptyParameters = new Dictionary<string, string>();

    private readonly IReadOnlyDictionary<string, string> _parameters;

    private readonly int _hashCode;

    /// <summary>
    /// Gets the name of the procedural model.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProceduralModelRequest"/> struct.
    /// </summary>
    /// <param name="name">The procedural model name.</param>
    /// <param name="parameters">The query parameters.</param>
    public ProceduralModelRequest(string name, IReadOnlyDictionary<string, string> parameters)
    {
        this.Name = name;
        this._parameters = parameters;
        this._hashCode = ComputeHash(name, parameters);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProceduralModelRequest"/> struct with no parameters.
    /// </summary>
    /// <param name="name">The procedural model name.</param>
    public ProceduralModelRequest(string name)
    {
        this.Name = name;
        this._parameters = _emptyParameters;
        this._hashCode = ComputeHash(name, _emptyParameters);
    }

    /// <summary>
    /// Gets a float value from the parameters.
    /// </summary>
    /// <param name="key">The parameter key.</param>
    /// <returns>The parsed value, or <c>null</c> if the key is missing or parsing fails.</returns>
    public float? GetFloat(string key)
    {
        if (this._parameters.TryGetValue(key, out var value) && float.TryParse(value, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        return null;
    }

    /// <summary>
    /// Gets an integer value from the parameters.
    /// </summary>
    /// <param name="key">The parameter key.</param>
    /// <returns>The parsed value, or <c>null</c> if the key is missing or parsing fails.</returns>
    public int? GetInt(string key)
    {
        if (this._parameters.TryGetValue(key, out var value) && int.TryParse(value, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        return null;
    }

    /// <summary>
    /// Gets a Vector3 value from the parameters. Expects comma-separated X,Y,Z values.
    /// </summary>
    /// <param name="key">The parameter key.</param>
    /// <returns>The parsed value, or <c>null</c> if the key is missing or parsing fails.</returns>
    public Vector3? GetVector3(string key)
    {
        if (this._parameters.TryGetValue(key, out var value))
        {
            var parts = value.Split(',');
            var ic = CultureInfo.InvariantCulture;
            if (parts.Length == 3 && float.TryParse(parts[0], ic, out var x) && float.TryParse(parts[1], ic, out var y) && float.TryParse(parts[2], ic, out var z))
            {
                return new Vector3(x, y, z);
            }
        }

        return null;
    }

    /// <inheritdoc />
    public bool Equals(ProceduralModelRequest other)
    {
        return this.Name == other.Name
            && this._parameters.Count == other._parameters.Count
            && this._parameters.All(kv => other._parameters.TryGetValue(kv.Key, out var v) && kv.Value == v);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is ProceduralModelRequest other && this.Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return this._hashCode;
    }

    /// <summary>
    /// Determines whether two requests are equal.
    /// </summary>
    public static bool operator ==(ProceduralModelRequest left, ProceduralModelRequest right) => left.Equals(right);

    /// <summary>
    /// Determines whether two requests are not equal.
    /// </summary>
    public static bool operator !=(ProceduralModelRequest left, ProceduralModelRequest right) => !left.Equals(right);

    private static int ComputeHash(string name, IReadOnlyDictionary<string, string> parameters)
    {
        var hash = new HashCode();
        hash.Add(name);

        foreach (var kv in parameters.OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            hash.Add(kv.Key);
            hash.Add(kv.Value);
        }

        return hash.ToHashCode();
    }
}
