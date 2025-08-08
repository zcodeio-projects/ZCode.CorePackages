namespace ZCode.Core.Domain.ValueObjects;

/// <summary>
/// Base class for value objects in Domain-Driven Design.
/// Value objects are immutable and defined by their attributes rather than identity.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Gets the equality components that define the value object's identity
    /// </summary>
    /// <returns>Collection of components used for equality comparison</returns>
    protected abstract IEnumerable<object> GetEqualityComponents();

    /// <summary>
    /// Determines whether two value objects are equal
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
            return false;

        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <summary>
    /// Determines whether two value objects are equal
    /// </summary>
    public bool Equals(ValueObject? other)
    {
        if (other == null || other.GetType() != GetType())
            return false;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <summary>
    /// Gets the hash code for the value object
    /// </summary>
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);
    }

    /// <summary>
    /// Equality operator
    /// </summary>
    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
            return true;

        if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            return false;

        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator
    /// </summary>
    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Creates a copy of the value object
    /// </summary>
    public virtual ValueObject Copy()
    {
        return (ValueObject)MemberwiseClone();
    }

    /// <summary>
    /// Legacy method for backward compatibility
    /// </summary>
    [Obsolete("Use Copy() method instead")]
    public ValueObject GetCopy()
    {
        return Copy();
    }

    /// <summary>
    /// Protected helper method for equality comparison
    /// </summary>
    protected static bool EqualOperator(ValueObject? left, ValueObject? right)
    {
        return left == right;
    }

    /// <summary>
    /// Protected helper method for inequality comparison
    /// </summary>
    protected static bool NotEqualOperator(ValueObject? left, ValueObject? right)
    {
        return left != right;
    }
}
