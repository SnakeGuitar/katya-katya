using MemoryGame.Domain.Common;
using MemoryGame.Domain.Users.ValueObjects;
using Hasher = BCrypt.Net.BCrypt;
using Xunit;
using MemoryGame.Domain.Users;

namespace MemoryGame.Tests;

public class PendingRegistrationTests
{
    // Method Create()
    // Attribute validation tests.
    [Fact]
    public void Create_EmailIsValid_ReturnNewPendingRegistration()
    {
        // Arrange
        string email = "johnsmith@outlook.com";
        string pin = "123456";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");
        TimeSpan validity = new TimeSpan(1, 0 , 0 , 0); // One day.

        // Act
        PendingRegistration pendingRegistration = PendingRegistration.Create(email, pin, hashedPassword, validity);

        // Assert
        Assert.Equal(email, pendingRegistration.Email.Value);
    }

    [Fact]
    public void Create_PinIsValid_ReturnNewPendingRegistration()
    {
        // Arrange
        string email = "johnsmith@outlook.com";
        string pin = "123456";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");
        TimeSpan validity = new TimeSpan(1, 0 , 0 , 0);

        // Act
        PendingRegistration pendingRegistration = PendingRegistration.Create(email, pin, hashedPassword, validity);

        // Assert
        Assert.Equal(pin, pendingRegistration.Pin);
    }

    [Fact]
    public void Create_HashedPasswordIsValid_ReturnNewPendingRegistration()
    {
        // Arrange
        string email = "johnsmith@outlook.com";
        string pin = "123456";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");
        TimeSpan validity = new TimeSpan(1, 0 , 0 , 0);

        // Act
        PendingRegistration pendingRegistration = PendingRegistration.Create(email, pin, hashedPassword, validity);

        // Assert
        Assert.Equal(hashedPassword, pendingRegistration.HashedPassword);
    }

    // Exception throw tests.
    [Fact]
    public void Create_PinIsNull_ThrowDomainException()
    {
        // Arrange
        string email = "johnsmith@outlook.com";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");
        TimeSpan validity = new TimeSpan(1, 0 , 0 , 0);

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            PendingRegistration.Create(email, null, hashedPassword, validity)
        );
    }

    [Fact]
    public void Create_PinIsWhiteSpace_ThrowDomainException()
    {
        // Arrange
        string email = "johnsmith@outlook.com";
        string pin = " ";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");
        TimeSpan validity = new TimeSpan(1, 0 , 0 , 0);

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            PendingRegistration.Create(email, pin, hashedPassword, validity)
        );
    }

    [Fact]
    public void Create_PinIsTooLong_ThrowDomainException()
    {
        // Arrange
        string email = "johnsmith@outlook.com";
        string pin = "1234567890123";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");
        TimeSpan validity = new TimeSpan(1, 0 , 0 , 0);

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            PendingRegistration.Create(email, pin, hashedPassword, validity)
        );
    }

    [Fact]
    public void Create_PinIsTooShort_ThrowDomainException()
    {
        // Arrange
        string email = "johnsmith@outlook.com";
        string pin = "123";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");
        TimeSpan validity = new TimeSpan(1, 0 , 0 , 0);

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            PendingRegistration.Create(email, pin, hashedPassword, validity)
        );
    }

    [Fact]
    public void Create_PinDoesNotContainNumericCharacters_ThrowDomainException()
    {
        // Arrange
        string email = "johnsmith@outlook.com";
        string pin = "abcdef";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");
        TimeSpan validity = new TimeSpan(1, 0 , 0 , 0);

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            PendingRegistration.Create(email, pin, hashedPassword, validity)
        );
    }

    [Fact]
    public void Create_PinContainsAlphabeticalCharacters_ThrowDomainException()
    {
        // Arrange
        string email = "johnsmith@outlook.com";
        string pin = "123abc";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");
        TimeSpan validity = new TimeSpan(1, 0 , 0 , 0);

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            PendingRegistration.Create(email, pin, hashedPassword, validity)
        );
    }


    // Method CreateForUpgrade()
    // Attribute validation tests.
    [Fact]
    public void CreateForUpgrade_EmailIsValid_ReturnNewPendingRegistration()
    {
        // Arrange
        Email email = Email.Create("johnsmith@outlook.com");
        string pin = "123456";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");
        int userId = 1;

        // Act
        PendingRegistration pendingRegistration = PendingRegistration.CreateForUpgrade(email, pin, hashedPassword, userId);

        // Assert
        Assert.Equal(email, pendingRegistration.Email);
    }

    [Fact]
    public void CreateForUpgrade_PinIsValid_ReturnNewPendingRegistration()
    {
        // Arrange
        Email email = Email.Create("johnsmith@outlook.com");
        string pin = "123456";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");
        int userId = 1;

        // Act
        PendingRegistration pendingRegistration = PendingRegistration.CreateForUpgrade(email, pin, hashedPassword, userId);

        // Assert
        Assert.Equal(pin, pendingRegistration.Pin);
    }

    [Fact]
    public void CreateForUpgrade_HashedPasswordIsValid_ReturnNewPendingRegistration()
    {
        // Arrange
        Email email = Email.Create("johnsmith@outlook.com");
        string pin = "123456";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");
        int userId = 1;

        // Act
        PendingRegistration pendingRegistration = PendingRegistration.CreateForUpgrade(email, pin, hashedPassword, userId);

        // Assert
        Assert.Equal(hashedPassword, pendingRegistration.HashedPassword);
    }

    // Exception throw tests.
    [Fact]
    public void CreateForUpgrade_PinIsNull_ThrowDomainException()
    {
        // Arrange
        Email email = Email.Create("johnsmith@outlook.com");
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");
        int userId = 1;

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            PendingRegistration.CreateForUpgrade(email, null, hashedPassword, userId)
        );
    }

    [Fact]
    public void CreateForUpgrade_PinIsWhiteSpace_ThrowDomainException()
    {
        // Arrange
        Email email = Email.Create("johnsmith@outlook.com");
        string pin = " ";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");
        int userId = 1;

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            PendingRegistration.CreateForUpgrade(email, pin, hashedPassword, userId)
        );
    }

    [Fact]
    public void CreateForUpgrade_PinIsTooLong_ThrowDomainException()
    {
        // Arrange
        Email email = Email.Create("johnsmith@outlook.com");
        string pin = "1234567890123";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");
        int userId = 1;

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            PendingRegistration.CreateForUpgrade(email, pin, hashedPassword, userId)
        );
    }

    [Fact]
    public void CreateForUpgrade_PinIsTooShort_ThrowDomainException()
    {
        // Arrange
        Email email = Email.Create("johnsmith@outlook.com");
        string pin = "123";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");
        int userId = 1;

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            PendingRegistration.CreateForUpgrade(email, pin, hashedPassword, userId)
        );
    }

    [Fact]
    public void CreateForUpgrade_PinDoesNotContainNumericCharacters_ThrowDomainException()
    {
        // Arrange
        Email email = Email.Create("johnsmith@outlook.com");
        string pin = "abcdef";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");
        int userId = 1;

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            PendingRegistration.CreateForUpgrade(email, pin, hashedPassword, userId)
        );
    }

    [Fact]
    public void CreateForUpgrade_PinContainsAlphabeticalCharacters_ThrowDomainException()
    {
        // Arrange
        Email email = Email.Create("johnsmith@outlook.com");
        string pin = "123abc";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");
        int userId = 1;

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            PendingRegistration.CreateForUpgrade(email, pin, hashedPassword, userId)
        );
    }


    // Method UpdatePin()
    // Attribute validation tests.
    [Fact]
    public void UpdatePin_PinIsValid_UpdatePendingRegistrationPin()
    {
        // Arrange
        string email = "johnsmith@outlook.com";
        string pin = "123456";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");
        TimeSpan validity = new TimeSpan(1, 0, 0, 0);

        PendingRegistration pendingRegistration = PendingRegistration.Create(email, pin, hashedPassword, validity);

        string newPin = "098765";

        // Act
        pendingRegistration.UpdatePin(newPin);

        // Assert
        Assert.Equal(newPin, pendingRegistration.Pin);
    }

    // Exception throw tests.
    [Fact]
    public void UpdatePin_PinIsNull_ThrowDomainException()
    {
        // Arrange
        string email = "johnsmith@outlook.com";
        string pin = "123456";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");
        TimeSpan validity = new TimeSpan(1, 0, 0, 0);

        PendingRegistration pendingRegistration = PendingRegistration.Create(email, pin, hashedPassword, validity);

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            pendingRegistration.UpdatePin(null)
        );
    }

    [Fact]
    public void UpdatePin_PinIsWhiteSpace_ThrowDomainException()
    {
        // Arrange
        string email = "johnsmith@outlook.com";
        string pin = "123456";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");
        TimeSpan validity = new TimeSpan(1, 0, 0, 0);

        PendingRegistration pendingRegistration = PendingRegistration.Create(email, pin, hashedPassword, validity);

        string newPin = " ";

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            pendingRegistration.UpdatePin(newPin)
        );
    }

    [Fact]
    public void UpdatePin_PinIsTooLong_ThrowDomainException()
    {
        // Arrange
        string email = "johnsmith@outlook.com";
        string pin = "123456";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");
        TimeSpan validity = new TimeSpan(1, 0, 0, 0);

        PendingRegistration pendingRegistration = PendingRegistration.Create(email, pin, hashedPassword, validity);

        string newPin = "09876543210987";

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            pendingRegistration.UpdatePin(newPin)
        );
    }

    [Fact]
    public void UpdatePin_PinIsTooShort_ThrowDomainException()
    {
        // Arrange
        string email = "johnsmith@outlook.com";
        string pin = "123456";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");
        TimeSpan validity = new TimeSpan(1, 0, 0, 0);

        PendingRegistration pendingRegistration = PendingRegistration.Create(email, pin, hashedPassword, validity);

        string newPin = "098";

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            pendingRegistration.UpdatePin(newPin)
        );
    }

    [Fact]
    public void UpdatePin_PinDoesNotContainNumericalCharacters_ThrowDomainException()
    {
        // Arrange
        string email = "johnsmith@outlook.com";
        string pin = "123456";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");
        TimeSpan validity = new TimeSpan(1, 0, 0, 0);

        PendingRegistration pendingRegistration = PendingRegistration.Create(email, pin, hashedPassword, validity);

        string newPin = "abcdef";

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            pendingRegistration.UpdatePin(newPin)
        );
    }

    [Fact]
    public void UpdatePin_PinDoesContainsAlphabeticalCharacters_ThrowDomainException()
    {
        // Arrange
        string email = "johnsmith@outlook.com";
        string pin = "123456";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");
        TimeSpan validity = new TimeSpan(1, 0, 0, 0);

        PendingRegistration pendingRegistration = PendingRegistration.Create(email, pin, hashedPassword, validity);

        string newPin = "098abc";

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            pendingRegistration.UpdatePin(newPin)
        );
    }


    // Method IsExpired()
    // Attribute validation tests.
    [Fact]
    public void IsExpired_Expired_ReturnTrue()
    {
        // Arrange
        string email = "johnsmith@outlook.com";
        string pin = "123456";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");
        TimeSpan validity = new TimeSpan(0); // No time.

        PendingRegistration pendingRegistration = PendingRegistration.Create(email, pin, hashedPassword, validity);

        // Act
        bool expired = pendingRegistration.IsExpired();

        // Assert
        Assert.True(expired);
    }

    [Fact]
    public void IsExpired_NotExpired_ReturnFalse()
    {
        // Arrange
        string email = "johnsmith@outlook.com";
        string pin = "123456";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");
        TimeSpan validity = new TimeSpan(1, 0, 0, 0); // One day.

        PendingRegistration pendingRegistration = PendingRegistration.Create(email, pin, hashedPassword, validity);

        // Act
        bool expired = pendingRegistration.IsExpired();

        // Assert
        Assert.False(expired);
    }

    // Method ValidatePin()
    // Attribute validation tests.
    [Fact]
    public void ValidatePin_PinIsValid_ReturnTrue()
    {
        // Arrange
        string email = "johnsmith@outlook.com";
        string pin = "123456";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");
        TimeSpan validity = new TimeSpan(1, 0, 0, 0);

        PendingRegistration pendingRegistration = PendingRegistration.Create(email, pin, hashedPassword, validity);

        // Act
        bool isValid = pendingRegistration.ValidatePin("123456");

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void ValidatePin_PinIsNotValid_ReturnFalse()
    {
        // Arrange
        string email = "johnsmith@outlook.com";
        string pin = "123456";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");
        TimeSpan validity = new TimeSpan(1, 0, 0, 0);

        PendingRegistration pendingRegistration = PendingRegistration.Create(email, pin, hashedPassword, validity);

        // Act
        bool isValid = pendingRegistration.ValidatePin("098765");

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ValidatePin_Expired_ReturnFalse()
    {
        // Arrange
        string email = "johnsmith@outlook.com";
        string pin = "123456";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");
        TimeSpan validity = new TimeSpan(0); // No time.

        PendingRegistration pendingRegistration = PendingRegistration.Create(email, pin, hashedPassword, validity);

        // Act
        bool isValid = pendingRegistration.ValidatePin("123456");

        // Assert
        Assert.False(isValid);
    }

}