using MemoryGame.Domain.Common;
using MemoryGame.Domain.Users.ValueObjects;
using Xunit;

namespace MemoryGame.Tests;

public class EmailTests
{
    // Method Create()
    // Attribute validation tests.
    [Fact]
    public void Create_ValueIsValid_ReturnNewEmail()
    {
        // Arrange
        string value = "johnsmith@outlook.com";

        // Act
        Email email = Email.Create(value);

        // Assert
        Assert.Equal(value, email.Value);
    }

    // Exception throw tests.
    [Fact]
    public void Create_ValueIsNull_ThrowDomainException()
    {
        // Arrange
        // No value defined.

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            Email.Create(null)
        );
    }

    [Fact]
    public void Create_ValueIsWhiteSpace_ThrowDomainException()
    {
        // Arrange
        string value = " ";

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            Email.Create(value)
        );
    }

    [Fact]
    public void Create_ValueIsTooLong_ThrowDomainException()
    {
        // Arrange
        string value = "IDontHaveToTellYouThingsAreBadEverybodyKnowsThingsAreBad@gmail.com";

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            Email.Create(value)
        );
    }

    [Fact]
    public void Create_ValueDoesNotContainAtSign_ThrowDomainException()
    {
        // Arrange
        string value = "oopsIForgorhotmail.com";

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            Email.Create(value)
        );
    }

    [Fact]
    public void Create_ValueDoesNotContainUsername_ThrowDomainException()
    {
        // Arrange
        string value = "@yahoo.com";

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            Email.Create(value)
        );
    }

    [Fact]
    public void Create_ValueDoesNotContainMailService_ThrowDomainException()
    {
        // Arrange
        string value = "realAddressTrustMeBro.net";

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            Email.Create(value)
        );
    }

    [Fact]
    public void Create_ValueDoesNotContainDomain_ThrowDomainException()
    {
        // Arrange
        string value = "sayMyName@aol";

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            Email.Create(value)
        );
    }
}