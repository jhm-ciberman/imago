using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;

namespace LifeSim;

/// <summary>
/// The ResourceUri class represents a URI that can be used to load resources. The URI is
/// composed of a protocol and a path. The protocol is used to determine the type of the
/// resource and the path is used to determine the location of the resource.
/// 
/// For example, the following URI specifies a texture:
/// "texture:path/to/texture.png"
/// 
/// Additionally, URIs can contain arguments. These arguments are specified after the
/// file path using a query string. For example:
/// "texture:path/to/texture.png?srgb=true"
/// "model:path/to/model.gltf?scene=Scene&scale=0.5&origin=0.5,0.5"
/// "procedural:multiplane?width=<width>&height=<height>&planes=<planes>"
/// 
/// URIs without scheme are will be parsed with a default empty protocol.
/// 
/// Arguments can be specified in any order and are case insensitive. 
/// The supported types of arguments are: int, float, bool, string, Vector2, Vector3.
/// </summary>
public class ResourceUri
{
    /// <summary>
    /// Gets the original URI string.
    /// </summary>
    public string OriginalUri { get; }

    /// <summary>
    /// Gets the scheme (protocol) of the URI.
    /// </summary>
    public string Scheme { get; }

    /// <summary>
    /// Gets the path of the URI. (Without the protocol nor the query string.)
    /// </summary>
    public string Path { get; }
    private readonly Dictionary<string, string> _arguments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a read-only dictionary of arguments and their values.
    /// </summary>
    public IReadOnlyDictionary<string, string> Arguments => this._arguments;

    /// <summary>
    /// Constructs a ResourceUri from a string.
    /// </summary>
    /// <param name="uri">The URI to parse.</param>
    public ResourceUri(string uri)
    {
        this.OriginalUri = uri;

        // First we fast check for simple URIs without protocol nor arguments. (not ":" and not "?")
        if (!uri.Contains(':') && !uri.Contains('?'))
        {
            this.Scheme = "";
            this.Path = uri;
            return;
        }

        // We need to handle the case where the URI does not contain a protocol.
        // In this case, we will assume the protocol is empty.

        if (!uri.Contains(':'))
        {
            uri = ':' + uri;
        }

        var uriParts = uri.Split(':');
        this.Scheme = uriParts[0];

        // We need to handle the case where the file path does not contain a query string.

        if (!uriParts[1].Contains('?'))
        {
            this.Path = uriParts[1];
            return;
        }

        var filePathParts = uriParts[1].Split('?', StringSplitOptions.None);
        this.Path = filePathParts[0];

        if (filePathParts.Length == 1)
        {
            return;
        }

        var queryString = filePathParts[1];
        var queryStringParts = queryString.Split('&');
        foreach (var queryStringPart in queryStringParts)
        {
            var queryStringPartParts = queryStringPart.Split('=');
            // If there is no value, we will asume "1" as the value. Since this value can be converted to any supported type.
            var value = queryStringPartParts.Length == 1 ? "1" : queryStringPartParts[1];
            this._arguments.Add(queryStringPartParts[0], value);
        }
    }

    /// <summary>
    /// Gets the value of the argument with the specified name as a string.
    /// </summary>
    /// <param name="key">The name of the argument.</param>
    /// <param name="defaultValue">The default value to return if the argument is not found.</param>
    /// <returns>The value of the argument with the specified name as a string.</returns>
    public string GetString(string key, string defaultValue = "")
    {
        if (this._arguments.ContainsKey(key))
        {
            return this._arguments[key];
        }
        return defaultValue;
    }

    /// <summary>
    /// Gets the value of the argument with the specified name as an int.
    /// </summary>
    /// <param name="key">The name of the argument.</param>
    /// <param name="defaultValue">The default value to return if the argument is not found.</param>
    /// <returns>The value of the argument with the specified name as an int.</returns>
    public int GetInt(string key, int defaultValue = 0)
    {
        if (this._arguments.ContainsKey(key))
        {
            return int.Parse(this._arguments[key], CultureInfo.InvariantCulture);
        }
        return defaultValue;
    }

    /// <summary>
    /// Gets the value of the argument with the specified name as a float.
    /// </summary>
    /// <param name="key">The name of the argument.</param>
    /// <param name="defaultValue">The default value to return if the argument is not found.</param>
    /// <returns>The value of the argument with the specified name as a float.</returns>
    public float GetFloat(string key, float defaultValue = 0)
    {
        if (this._arguments.ContainsKey(key))
        {
            return float.Parse(this._arguments[key], CultureInfo.InvariantCulture);
        }
        return defaultValue;
    }

    /// <summary>
    /// Gets the value of the argument with the specified name as a bool.
    /// </summary>
    /// <param name="key">The name of the argument.</param>
    /// <param name="defaultValue">The default value to return if the argument is not found.</param>
    /// <returns>The value of the argument with the specified name as a bool.</returns>
    public bool GetBool(string key, bool defaultValue = false)
    {
        if (this._arguments.ContainsKey(key))
        {
            return bool.Parse(this._arguments[key]);
        }
        return defaultValue;
    }

    /// <summary>
    /// Gets the value of the argument with the specified name as a Vector2.
    /// </summary>
    /// <param name="key">The name of the argument.</param>
    /// <param name="defaultValue">The default value to return if the argument is not found.</param>
    /// <returns>The value of the argument with the specified name as a Vector2.</returns>
    public Vector2 GetVector2(string key, Vector2 defaultValue = default(Vector2))
    {
        if (this._arguments.ContainsKey(key))
        {
            NumberStyles style = NumberStyles.Float;
            if (this._arguments[key].Contains(','))
            {
                var parts = this._arguments[key].Split(',');
                if (parts.Length != 2)
                {
                    throw new FormatException($"Invalid Vector2 format for argument {key}. Expected format is \"x,y\".");
                }
                float x = float.Parse(parts[0], style, CultureInfo.InvariantCulture);
                float y = float.Parse(parts[1], style, CultureInfo.InvariantCulture);
                return new Vector2(x, y);
            }
            else
            {
                float x = float.Parse(this._arguments[key], style, CultureInfo.InvariantCulture);
                return new Vector2(x);
            }
        }
        return defaultValue;
    }

    /// <summary>
    /// Gets the value of the argument with the specified name as a Vector3.
    /// </summary>
    /// <param name="key">The name of the argument.</param>
    /// <param name="defaultValue">The default value to return if the argument is not found.</param>
    /// <returns>The value of the argument with the specified name as a Vector3.</returns>
    public Vector3 GetVector3(string key, Vector3 defaultValue = default(Vector3))
    {
        if (this._arguments.ContainsKey(key))
        {
            NumberStyles style = NumberStyles.Float;
            if (this._arguments[key].Contains(','))
            {
                var parts = this._arguments[key].Split(',');
                if (parts.Length != 3)
                {
                    throw new ArgumentException($"Invalid Vector3 format for argument {key}: {this._arguments[key]}. Expected format is \"x,y,z\".");
                }
                float x = float.Parse(parts[0], style, CultureInfo.InvariantCulture);
                float y = float.Parse(parts[1], style, CultureInfo.InvariantCulture);
                float z = float.Parse(parts[2], style, CultureInfo.InvariantCulture);
                return new Vector3(x, y, z);
            }
            else
            {
                float x = float.Parse(this._arguments[key], style, CultureInfo.InvariantCulture);
                return new Vector3(x);
            }
        }
        return defaultValue;
    }

    /// <summary>
    /// Gets the value of the argument with the specified name as an enum.
    /// </summary>
    /// <typeparam name="T">The type of the enum.</typeparam>
    /// <param name="key">The name of the argument.</param>
    /// <param name="defaultValue">The default value to return if the argument is not found.</param>
    /// <returns>The value of the argument with the specified name as an enum.</returns>
    public T GetEnum<T>(string key, T defaultValue = default(T)) where T : struct
    {
        if (this._arguments.ContainsKey(key))
        {
            return (T)Enum.Parse(typeof(T), this._arguments[key], ignoreCase: true);
        }
        return defaultValue;
    }

    public override string ToString()
    {
        return this.OriginalUri;
    }

    public static implicit operator string(ResourceUri uri)
    {
        return uri.ToString();
    }

    public static implicit operator ResourceUri(string uri)
    {
        return new ResourceUri(uri);
    }
}