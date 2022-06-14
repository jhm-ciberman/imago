using System;
using System.Collections.Generic;

namespace LifeSim.Engine.Controls;

public class Style
{
    private static readonly Dictionary<Type, Style> _defaultStyles = new Dictionary<Type, Style>();

    private static readonly Dictionary<string, Style> _styles = new Dictionary<string, Style>();

    /// <summary>
    /// An empty style.
    /// </summary>
    public static Style Empty { get; } = new Style();

    /// <summary>
    /// Registers a style to be used by default for all the controls of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the control.</typeparam>
    /// <param name="style">The style to register.</param>
    public static void RegisterDefaultStyle<T>(Style style)
    {
        _defaultStyles[typeof(T)] = style;
    }

    /// <summary>
    /// Gets the default style for the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the control.</typeparam>
    /// <returns>The default style for the specified type.</returns>
    public static Style GetDefaultStyle<T>()
    {
        return GetDefaultStyle(typeof(T));
    }

    /// <summary>
    /// Gets the default style for the specified type.
    /// </summary>
    /// <param name="type">The type of the control.</param>
    /// <returns>The default style for the specified type.</returns>
    public static Style GetDefaultStyle(Type type)
    {
        // Use the whole hierarchy of T to register the default style, from more specific to more general.
        Type? currentType = type;
        while (currentType != null)
        {
            if (_defaultStyles.TryGetValue(currentType, out var style))
            {
                return style;
            }

            currentType = currentType.BaseType;
        }

        return Empty;
    }


    /// <summary>
    /// Registers a style with the specified name.
    /// </summary>
    /// <param name="name">The name of the style.</param>
    /// <param name="style">The style to register.</param>
    /// <exception cref="ArgumentException">Thrown if the style with the same name already exists.</exception>
    public static void RegisterStyle(string name, Style style)
    {
        if (_styles.ContainsKey(name))
        {
            throw new ArgumentException($"A style with the name {name} already exists.");
        }

        _styles[name] = style;
    }

    /// <summary>
    /// Gets the style with the specified name.
    /// </summary>
    /// <param name="name">The name of the style.</param>
    /// <returns>The style with the specified name.</returns>
    /// <exception cref="ArgumentException">Thrown if the style with the specified name does not exist.</exception>
    public static Style Find(string name)
    {
        if (!_styles.TryGetValue(name, out var style))
        {
            throw new ArgumentException($"A style with the name {name} does not exist.");
        }

        return style;
    }

    private readonly Dictionary<string, object> _properties = new Dictionary<string, object>();

    /// <summary>
    /// Gets or sets the parent style. All the properties of the parent style are also available in this style.
    /// </summary>
    public Style? BasedOn { get; set; }

    /// <summary>
    /// Gets or sets the target type of the style. This is used to determine the default style for the target type.
    /// </summary>
    public Type? TargetType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Style"/> class.
    /// </summary>
    public Style()
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Style"/> class.
    /// </summary>
    /// <param name="targetType">The target type of the style.</param>
    public Style(Type targetType)
    {
        this.TargetType = targetType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Style"/> class.
    /// </summary>
    /// <param name="targetType">The target type of the style.</param>
    /// <param name="basedOn">The parent style.</param>
    public Style(Type targetType, Style? basedOn)
    {
        this.TargetType = targetType;
        this.BasedOn = basedOn;
    }


    /// <summary>
    /// Gets or sets the value of the property with the specified name.
    /// </summary>
    /// <param name="key">The name of the property.</param>
    /// <returns>The value of the property with the specified name.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the property with the specified name does not exist.</exception>
    public object this[string key]
    {
        get
        {
            if (this._properties.TryGetValue(key, out object? value))
            {
                return value;
            }

            if (this.BasedOn != null)
            {
                return this.BasedOn[key];
            }

            throw new KeyNotFoundException($"Property '{key}' not found in the style.");
        }
        set => this._properties[key] = value;
    }

    /// <summary>
    /// Applies the style to the specified object.
    /// </summary>
    /// <param name="target">The object to apply the style to.</param>
    public void Apply(object target)
    {
        this.BasedOn?.Apply(target);

        foreach (var property in this._properties)
        {
            var targetProperty = target.GetType().GetProperty(property.Key);
            if (targetProperty != null)
            {
                targetProperty.SetValue(target, property.Value);
            }
        }
    }

    /// <summary>
    /// Copies all the style properties to the specified style.
    /// </summary>
    /// <param name="destination">The style to copy the properties to.</param>
    public void Copy(Style destination)
    {
        if (this.BasedOn != null)
        {
            this.BasedOn.Copy(destination);
        }

        foreach (var property in this._properties)
        {
            destination[property.Key] = property.Value;
        }
    }

    /// <summary>
    /// Creates a clone of the style.
    /// </summary>
    /// <returns>A clone of the style.</returns>
    public Style Clone()
    {
        var clone = new Style();
        this.Copy(clone);
        return clone;
    }

    /// <summary>
    /// Merges two styles.
    /// </summary>
    /// <param name="style1">The first style to merge.</param>
    /// <param name="style2">The second style to merge.</param>
    /// <returns>The merged style.</returns>
    public static Style Merge(Style style1, Style style2)
    {
        var merged = new Style();
        style1.Copy(merged);
        style2.Copy(merged);
        return merged;
    }
}