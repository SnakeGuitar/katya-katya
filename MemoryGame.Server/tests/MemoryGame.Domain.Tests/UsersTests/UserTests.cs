using Xunit;
using MemoryGame.Domain.Users;
using Hasher = BCrypt.Net.BCrypt;
using MemoryGame.Domain.Common;

namespace MemoryGame.Tests;

public class UserTests
{
    // Method CreateRegistered() (and indirectly, ValidateUsername())
    // Attribute validation tests.
    // TODO: Find a way to implement unit tests for private methods.
    [Fact]
    public void CreateRegistered_UsernameIsValid_ReturnNewUser()
    {
        // Arrange
        string username = "xX_kingOfSpace_Xx";
        string email = "johnsmith@outlook.com";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");

        // Act
        User user = User.CreateRegistered(username, email, hashedPassword);

        // Assert
        Assert.Equal(username, user.Username);
    }

    [Fact]
    public void CreateRegistered_EmailIsValid_ReturnNewUser()
    {
        // Arrange
        string username = "xX_kingOfSpace_Xx";
        string email = "johnsmith@outlook.com";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");

        // Act
        User user = User.CreateRegistered(username, email, hashedPassword);

        // Assert
        Assert.Equal(email, user.Email.Value);
    }

    [Fact]
    public void CreateRegistered_HashedPasswordIsValid_ReturnNewUser()
    {
        // Arrange
        string username = "xX_kingOfSpace_Xx";
        string email = "johnsmith@outlook.com";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");

        // Act
        User user = User.CreateRegistered(username, email, hashedPassword);

        // Assert
        Assert.Equal(hashedPassword, user.PasswordHash);
    }

    // Exception throw tests.
    [Fact]
    public void CreateRegistered_UsernameIsNull_ThrowDomainException()
    {
        // Arrange
        string email = "johnsmith@outlook.com";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            User.CreateRegistered(null, email, hashedPassword)
        );
    }

    [Fact]
    public void CreateRegistered_UsernameIsWhiteSpace_ThrowDomainException()
    {
        // Arrange
        string username = " ";
        string email = "johnsmith@outlook.com";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            User.CreateRegistered(username, email, hashedPassword)
        );
    }

    [Fact]
    public void CreateRegistered_UsernameIsTooLong_ThrowDomainException()
    {
        // Arrange
        string username = "I_Have_Walked_The_Edge_Of_The_Abbyss";
        string email = "johnsmith@outlook.com";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            User.CreateRegistered(username, email, hashedPassword)
        );
    }


    // Method CreateGuest()
    // Attribute validation tests.
    [Fact]
    public void CreateGuest_UsernameIsValid_ReturnNewUser()
    {
        // Arrange
        string username = "xX_kingOfSpace_Xx";

        // Act
        User user = User.CreateGuest(username);

        // Assert
        Assert.Equal(username, user.Username);
    }


    // Method ChangeUsername()
    // Attribute validation tests.
    [Fact]
    public void ChangeUsername_UsernameIsValid_UpdateUsername()
    {
        // Arrange
        string username = "xX_kingOfSpace_Xx";
        string email = "johnsmith@outlook.com";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");

        User user = User.CreateRegistered(username, email, hashedPassword);

        string newUsername = "Pillar Of Autumn";

        // Act
        user.ChangeUsername(newUsername);

        // Assert
        Assert.Equal(newUsername, user.Username);
    }


    // Method UpdatePersonalInfo()
    // Attribute validation tests.
    [Fact]
    public void UpdatePersonalInfo_NameIsValid_UpdatePersonalInfo()
    {
        // Arrange
        string username = "xX_kingOfSpace_Xx";
        string email = "johnsmith@outlook.com";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");

        User user = User.CreateRegistered(username, email, hashedPassword);

        string name = "John";
        string lastName = "Smith";

        // Act
        user.UpdatePersonalInfo(name, lastName);

        // Assert
        Assert.Equal(name, user.Name);
    }

    [Fact]
    public void UpdatePersonalInfo_LastNameIsValid_UpdatePersonalInfo()
    {
        // Arrange
        string username = "xX_kingOfSpace_Xx";
        string email = "johnsmith@outlook.com";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");

        User user = User.CreateRegistered(username, email, hashedPassword);

        string name = "John";
        string lastName = "Smith";

        // Act
        user.UpdatePersonalInfo(name, lastName);

        // Assert
        Assert.Equal(lastName, user.LastName);
    }

    // Exception throw tests.
    [Fact]
    public void UpdatePersonalInfo_NameIsNull_ThrowDomainException()
    {
        // Arrange
        string username = "xX_kingOfSpace_Xx";
        string email = "johnsmith@outlook.com";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");

        User user = User.CreateRegistered(username, email, hashedPassword);

        string lastName = "Smith";

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            user.UpdatePersonalInfo(null, lastName)
        );
    }

    [Fact]
    public void UpdatePersonalInfo_NameIsWhiteSpace_ThrowDomainException()
    {
        // Arrange
        string username = "xX_kingOfSpace_Xx";
        string email = "johnsmith@outlook.com";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");

        User user = User.CreateRegistered(username, email, hashedPassword);

        string name = " ";
        string lastName = "Smith";

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            user.UpdatePersonalInfo(name, lastName)
        );
    }

    [Fact]
    public void UpdatePersonalInfo_NameIsTooLong_ThrowDomainException()
    {
        // Arrange
        string username = "xX_kingOfSpace_Xx";
        string email = "johnsmith@outlook.com";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");

        User user = User.CreateRegistered(username, email, hashedPassword);

        string name = "Adolph Blaine Charles David Earl Frederick Gerald Hubert Irvin John Kenneth";
        string lastName = "Smith";

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            user.UpdatePersonalInfo(name, lastName)
        );
    }

    [Fact]
    public void UpdatePersonalInfo_LastNameIsNull_ThrowDomainException()
    {
        // Arrange
        string username = "xX_kingOfSpace_Xx";
        string email = "johnsmith@outlook.com";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");

        User user = User.CreateRegistered(username, email, hashedPassword);

        string name = "John";

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            user.UpdatePersonalInfo(name, null)
        );
    }

    [Fact]
    public void UpdatePersonalInfo_LastNameIsWhiteSpace_ThrowDomainException()
    {
        // Arrange
        string username = "xX_kingOfSpace_Xx";
        string email = "johnsmith@outlook.com";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");

        User user = User.CreateRegistered(username, email, hashedPassword);

        string name = "John";
        string lastName = " ";

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            user.UpdatePersonalInfo(name, lastName)
        );
    }

    [Fact]
    public void UpdatePersonalInfo_LastNameIsTooLong_ThrowDomainException()
    {
        // Arrange
        string username = "xX_kingOfSpace_Xx";
        string email = "johnsmith@outlook.com";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");

        User user = User.CreateRegistered(username, email, hashedPassword);

        string name = "Hubert";
        string lastName = "John Kenneth Lloyd Martin Nero Oliver Paul Quincy Randolph Sherman Thomas Uncas Victor William Xerxes Yancy Zeus";

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            user.UpdatePersonalInfo(name, lastName)
        );
    }


    // Method ChangePassword()
    // Attribute validation tests.
    [Fact]
    public void ChangePassword_UserIsRegistered_UpdatePassword()
    {
        // Arrange
        string username = "xX_kingOfSpace_Xx";
        string email = "johnsmith@outlook.com";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");

        User user = User.CreateRegistered(username, email, hashedPassword);

        string newHashedPassword = Hasher.HashPassword("YetAnotherUnbreakableSecret_456");

        // Act
        user.ChangePassword(newHashedPassword);

        // Assert
        Assert.Equal(newHashedPassword, user.PasswordHash);
    }

    // Exception throw tests.
    [Fact]
    public void ChangePassword_UserIsGuest_ThrowDomainException()
    {
        // Arrange
        string username = "xX_kingOfSpace_Xx";

        User user = User.CreateGuest(username);

        string hashedPassword = Hasher.HashPassword("UnfathomablyUselessSecret_67");

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            user.ChangePassword(hashedPassword)
        );
    }


    // Method VerifyEmail()
    // Attribute validation tests.
    [Fact]
    public void VerifyEmail_EmailIsNotVerified_VerifyEmail()
    {
        // Arrange
        string username = "xX_kingOfSpace_Xx";
        string email = "johnsmith@outlook.com";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");

        User user = User.CreateRegistered(username, email, hashedPassword);

        // Act
        user.VerifyEmail();

        // Assert
        Assert.True(user.VerifiedEmail);
    }

    // Exception throw tests.
    [Fact]
    public void VerifyEmail_EmailIsAlreadyVerified_ThrowDomainException()
    {
        // Arrange
        string username = "xX_kingOfSpace_Xx";
        string email = "johnsmith@outlook.com";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");

        User user = User.CreateRegistered(username, email, hashedPassword);

        user.VerifyEmail();

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            user.VerifyEmail()
        );
    }

    [Fact]
    public void VerifyEmail_UserIsGuest_ThrowDomainException()
    {
        // Arrange
        string username = "xX_kingOfSpace_Xx";

        User user = User.CreateGuest(username);

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            user.VerifyEmail()
        );
    }


    // Method PromoteFromGuest()
    // Attribute validation tests.
    [Fact]
    public void PromoteFromGuest_EmailIsValid_PromotedToRegistered()
    {
        // Arrange
        string username = "xX_kingOfSpace_Xx";

        User user = User.CreateGuest(username);

        string email = "johnsmith@outlook.com";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");

        // Act
        user.PromoteFromGuest(email, hashedPassword);

        // Assert
        Assert.Equal(email, user.Email.Value);
    }

    [Fact]
    public void PromoteFromGuest_HashedPasswordIsValid_PromotedToRegistered()
    {
        // Arrange
        string username = "xX_kingOfSpace_Xx";

        User user = User.CreateGuest(username);

        string email = "johnsmith@outlook.com";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");

        // Act
        user.PromoteFromGuest(email, hashedPassword);

        // Assert
        Assert.Equal(hashedPassword, user.PasswordHash);
    }

    // Exception throw tests.
    [Fact]
    public void PromoteFromGuest_UserIsNotGuest_ThrowDomainException()
    {
        // Arrange
        string username = "xX_kingOfSpace_Xx";
        string email = "johnsmith@outlook.com";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");

        User user = User.CreateRegistered(username, email, hashedPassword);

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            user.PromoteFromGuest(email, hashedPassword)
        );
    }


    // Method UpdateAvatar()
    // Attribute validation tests.
    [Fact]
    public void UpdateAvatar_ImageIsValid_UpdateAvatarImage()
    {
        // Arrange
        string username = "xX_kingOfSpace_Xx";
        string email = "johnsmith@outlook.com";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");

        User user = User.CreateRegistered(username, email, hashedPassword);

        string testProjectPath = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;

        byte[] image = File.ReadAllBytes(testProjectPath + "/resources/mistah.jpg");

        // Act
        user.UpdateAvatar(image);

        // Assert
        Assert.Equal(image, user.Avatar);
    }

    // Exception throw tests.
    [Fact]
    public void UpdateAvatar_ImageIsNull_ThrowDomainException()
    {
        // Arrange
        string username = "xX_kingOfSpace_Xx";
        string email = "johnsmith@outlook.com";
        string hashedPassword = Hasher.HashPassword("UnbreakableSecret_123");

        User user = User.CreateRegistered(username, email, hashedPassword);

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            user.UpdateAvatar(null)
        );
    }
}