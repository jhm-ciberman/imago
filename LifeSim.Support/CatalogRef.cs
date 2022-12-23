namespace LifeSim.Support;

/// <summary>
/// Represents a reference to a catalog element. The reference is resolved at runtime in a lazy fashion.
/// </summary>
/// <typeparam name="TObj">The type of the element referenced by the reference.</typeparam>
public abstract class CatalogRef<TObj> where TObj : class, IIdentifiable
{
    /// <summary>
    /// Gets the identifier for this reference.
    /// </summary>
    public string Identifier { get; }

    private TObj? _element = null;

    public CatalogRef(string identifier)
    {
        this.Identifier = identifier;
    }

    /// <summary>
    /// Gets the object referenced by this reference.
    /// </summary>
    public TObj Value
    {
        get
        {
            this._element ??= this.Resolve(this.Identifier);

            return this._element;
        }
    }

    protected abstract TObj Resolve(string identifier);


    public override string ToString()
    {
        return this.Identifier;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not CatalogRef<TObj> other)
        {
            return false;
        }

        return this.Identifier == other.Identifier;
    }

    public override int GetHashCode()
    {
        return this.Identifier.GetHashCode();
    }

    public static bool operator ==(CatalogRef<TObj> left, CatalogRef<TObj> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(CatalogRef<TObj> left, CatalogRef<TObj> right)
    {
        return !(left == right);
    }

    public static implicit operator TObj(CatalogRef<TObj> reference)
    {
        return reference.Value;
    }
}
