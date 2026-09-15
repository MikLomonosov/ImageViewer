using ImageViewer.Domain.Common;
using Xunit;

namespace ImageViewer.Tests.Domain;

public class EntityTests
{
    private sealed class EntityA : Entity<Guid>
    {
        public EntityA(Guid id) : base(id) { }
    }

    private sealed class EntityB : Entity<Guid>
    {
        public EntityB(Guid id) : base(id) { }
    }

    [Fact]
    public void Entities_OfSameType_WithSameId_AreEqual()
    {
        var id = Guid.NewGuid();

        var first = new EntityA(id);
        var second = new EntityA(id);

        Assert.True(first.Equals(second));
        Assert.True(second.Equals(first));
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void Entities_OfSameType_WithDifferentId_AreNotEqual()
    {
        var first = new EntityA(Guid.NewGuid());
        var second = new EntityA(Guid.NewGuid());

        Assert.False(first.Equals(second));
        Assert.True(first != second);
    }

    [Fact]
    public void Entities_OfDifferentTypes_WithSameId_AreNotEqual()
    {
        var id = Guid.NewGuid();

        var first = new EntityA(id);
        var second = new EntityB(id);

        Assert.False(first.Equals(second));
    }

    [Fact]
    public void Entity_NeverEqualsNull()
    {
        var entity = new EntityA(Guid.NewGuid());

        Assert.False(entity.Equals(null));
        Assert.False(entity == null);
        Assert.True(entity != null);
    }

    [Fact]
    public void EqualsObjectVersion_UsesTypedEquality_WithoutStackOverflow()
    {
        // Regression: the first version of Entity<TId> recursed into itself
        // here and caused a StackOverflowException.
        var id = Guid.NewGuid();

        object first = new EntityA(id);
        object second = new EntityA(id);

        Assert.True(first.Equals(second));
        Assert.True((EntityA)first == (EntityA)second);
    }

    [Fact]
    public void GetHashCode_IsStableAndIdBased()
    {
        var id = Guid.NewGuid();

        Assert.Equal(id.GetHashCode(), new EntityA(id).GetHashCode());
    }
}
