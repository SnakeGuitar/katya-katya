using FluentAssertions;
using KatyaKatya.Domain.Common;
using KatyaKatya.Domain.Dating;
using Xunit;

namespace KatyaKatya.Domain.Tests.DatingTests;

public class CharacterTests
{
    [Fact]
    public void Create_SetsCoreFields()
    {
        var character = Character.Create("Katya", "A mysterious classmate.", "katya");

        character.Name.Should().Be("Katya");
        character.Description.Should().Be("A mysterious classmate.");
        character.AssetKey.Should().Be("katya");
        character.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_EmptyName_Throws() =>
        ((Action)(() => Character.Create("", null, "katya"))).Should().Throw<DomainException>();

    [Fact]
    public void Create_EmptyAssetKey_Throws() =>
        ((Action)(() => Character.Create("Katya", null, ""))).Should().Throw<DomainException>();

    [Fact]
    public void Deactivate_SetsInactive()
    {
        var character = Character.Create("Katya", null, "katya");

        character.Deactivate();

        character.IsActive.Should().BeFalse();
    }
}
