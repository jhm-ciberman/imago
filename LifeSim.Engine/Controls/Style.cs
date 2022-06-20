using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace LifeSim.Engine.Controls;

public class StyleManager
{
    private static readonly Dictionary<Type, Style> _defaultStyles = new Dictionary<Type, Style>();

    private static readonly Dictionary<string, Style> _styles = new Dictionary<string, Style>();

    /// <summary>
    /// An empty style.
    /// </summary>
    public static Style Empty { get; } = new Style(typeof(object));

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
    /// <param name="style">The style to register.</param>
    /// <exception cref="ArgumentException">Thrown if the style with the same name already exists.</exception>
    public static void RegisterStyle(Style style)
    {
        Style.ValidateStyle(style);

        if (!style.IsDefault && style.Name == null)
        {
            throw new ArgumentException("A style must have a name or be marked as default.");
        }

        if (style.IsDefault)
        {
            _defaultStyles[style.TargetType] = style;
        }

        if (style.Name != null)
        {
            if (_styles.ContainsKey(style.Name))
            {
                throw new ArgumentException($"A style with the name '{style.Name}' already exists.");
            }

            _styles[style.Name] = style;
        }
    }

    /// <summary>
    /// Gets the style with the specified name.
    /// </summary>
    /// <param name="name">The name of the style.</param>
    /// <returns>The style with the specified name.</returns>
    /// <exception cref="ArgumentException">Thrown if the style with the specified name does not exist.</exception>
    public static Style GetStyle(string name)
    {
        if (!_styles.TryGetValue(name, out var style))
        {
            throw new ArgumentException($"A style with the name {name} does not exist.");
        }

        return style;
    }
}


public class Style
{
    private readonly Dictionary<string, object> _properties = new Dictionary<string, object>();

    /// <summary>
    /// Gets or sets the name of the style.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the parent style. All the properties of the parent style are also available in this style.
    /// </summary>
    public Style? BaseStyle { get; set; }

    /// <summary>
    /// Gets the target type of the style. This is used to determine the default style for the target type.
    /// </summary>
    public Type TargetType { get; }

    /// <summary>
    /// Gets or sets whether the style is the default style for the target type.
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Gets or sets the name of the parent style.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the parent style with the specified name does not exist.</exception>
    public string? BasedOn
    {
        get => this.BaseStyle?.Name;
        set => this.BaseStyle = value == null ? null : StyleManager.GetStyle(value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Style"/> class.
    /// </summary>
    /// <param name="targetType">The target type of the style.</param>
    /// <param name="name">The name of the style.</param>
    /// <param name="baseStyle">The parent style.</param>
    public Style(Type targetType, string? name = null, Style? baseStyle = null)
    {
        this.TargetType = targetType;
        this.Name = name;
        this.BaseStyle = baseStyle;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Style"/> class.
    /// </summary>
    /// <param name="targetType">The target type of the style.</param>
    /// <param name="baseStyle">The parent style.</param>
    protected Style(Type targetType, Style? baseStyle = null)
        : this(targetType, null, baseStyle)
    {
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

            if (this.BaseStyle != null)
            {
                return this.BaseStyle[key];
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
        this.BaseStyle?.Apply(target);

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
    /// Validates a style and ensure that all the properties are valid, assignable to the target type, 
    /// and the properties values are of the correct type.
    /// </summary>
    /// <param name="style">The style to validate.</param>
    /// <exception cref="InvalidOperationException">Thrown if the style is invalid.</exception>
    public static void ValidateStyle(Style style)
    {
        if (style.BaseStyle != null)
        {
            ValidateStyle(style.BaseStyle);
        }

        foreach (var property in style._properties)
        {
            var targetProperty = style.TargetType?.GetProperty(property.Key);
            if (targetProperty == null)
            {
                throw new InvalidOperationException($"Property '{property.Key}' not found in the target type '{style.TargetType?.Name}'.");
            }

            var propertyType = targetProperty.PropertyType;
            if (!propertyType.IsAssignableFrom(property.Value.GetType()))
            {
                throw new InvalidOperationException($"Property '{property.Key}' has an invalid value of type '{property.Value.GetType().Name}'. It must be assignable to '{propertyType.Name}'.");
            }
        }
    }

    // Implicit string to Style conversion
    public static implicit operator Style(string name)
    {
        return StyleManager.GetStyle(name);
    }
}



public class Style<T> : Style
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Style{T}"/> class.
    /// </summary>
    /// <param name="name">The name of the style.</param>
    /// <param name="baseStyle">The parent style.</param>
    public Style(string? name = null, Style? baseStyle = null)
        : base(typeof(T), name, baseStyle)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Style{T}"/> class.
    /// </summary>
    /// <param name="baseStyle">The parent style.</param>
    protected Style(Style? baseStyle = null)
        : base(typeof(T), baseStyle)
    {
    }
}