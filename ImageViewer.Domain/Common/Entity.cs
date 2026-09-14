namespace ImageViewer.Domain.Common;

public abstract class Entity<TId> : IEquatable<Entity<TId>> where TId : notnull
{
    public TId Id { get; }

    #region constructors

    protected Entity(TId id)
    {
        Id = id;
    }

    #endregion

    public bool Equals(Entity<TId>? other)
    {
        return other is not null && other.GetType() == GetType() && Id.Equals(other.Id);
    }

    public override bool Equals(object? obj)
    {
        return obj is Entity<TId> other && Equals(other);
    }
    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        return left?.Equals(right) ?? right is null;
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !(left == right);
    }
}