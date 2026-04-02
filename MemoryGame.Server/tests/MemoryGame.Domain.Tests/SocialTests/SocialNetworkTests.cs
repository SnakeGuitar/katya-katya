using MemoryGame.Domain.Common;
using MemoryGame.Domain.Social;
using Xunit;

namespace MemoryGame.Tests;

public class SocialNetworkTests
{
    // Method Create()
    // Attribute validation tests.
    [Fact]
    public void Create_UserIdIsValid_ReturnNewSocialNetwork()
    {
        // Arrange
        int userId = 1;
        string account = "Totally Not a Bot";

        // Act
        SocialNetwork socialNetwork = SocialNetwork.Create(userId, account);

        // Assert
        Assert.Equal(userId, socialNetwork.UserId);
    }

    [Fact]
    public void Create_AccountNameIsValid_ReturnNewSocialNetwork()
    {
        // Arrange
        int userId = 1;
        string account = "Totally Not a Bot";

        // Act
        SocialNetwork socialNetwork = SocialNetwork.Create(userId, account);

        // Assert
        Assert.Equal(account, socialNetwork.Account);
    }

    // Exception throw tests.
    [Fact]
    public void Create_AccountNameIsNull_ThrowDomainException()
    {
        // Arrange
        int userId = 1;

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            SocialNetwork.Create(userId, null)
        );
    }

    [Fact]
    public void Create_AccountNameIsWhiteSpace_ThrowDomainException()
    {
        // Arrange
        int userId = 1;
        string account = " ";

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            SocialNetwork.Create(userId, account)
        );
    }

    [Fact]
    public void Create_AccountNameIsTooLong_ThrowDomainException()
    {
        // Arrange
        int userId = 1;
        string account = "Gordon Freeman, PhD in Theoretical Physics and the One Free Man, Saviour of Humanity";

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            SocialNetwork.Create(userId, account)
        );
    }


    // Method UpdateAccount()
    // Attribute validation tests.
    [Fact]
    public void UpdateAccount_NewAccountNameIsValid_UpdateAccountName()
    {
        // Arrange
        int userId = 1;
        string oldAccountName = "Totally Not a Bot";
        string newAccountName = "One Hundred Percent a Bot";

        SocialNetwork socialNetwork = SocialNetwork.Create(userId, oldAccountName);

        // Act
        socialNetwork.UpdateAccount(newAccountName);

        // Assert
        Assert.Equal(newAccountName, socialNetwork.Account);
    }

    // Exception throw tests.
    [Fact]
    public void UpdateAccount_NewAccountNameIsNull_ThrowDomainException()
    {
        // Arrange
        int userId = 1;
        string oldAccountName = "Totally Not a Bot";

        SocialNetwork socialNetwork = SocialNetwork.Create(userId, oldAccountName);

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            socialNetwork.UpdateAccount(null)
        );
    }

    [Fact]
    public void UpdateAccount_NewAccountNameIsWhiteSpace_ThrowDomainException()
    {
        // Arrange
        int userId = 1;
        string oldAccountName = "Totally Not a Bot";
        string newAccountName = " ";

        SocialNetwork socialNetwork = SocialNetwork.Create(userId, oldAccountName);

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            socialNetwork.UpdateAccount(newAccountName)
        );
    }

    [Fact]
    public void UpdateAccount_NewAccountNameIsTooLong_ThrowDomainException()
    {
        // Arrange
        int userId = 1;
        string oldAccountName = "Totally Not a Bot";
        string newAccountName = "The Gun [that] Pointed at the Head of the Universe...";

        SocialNetwork socialNetwork = SocialNetwork.Create(userId, oldAccountName);

        // Assert
        Assert.Throws<DomainException>(() =>
            // Act
            socialNetwork.UpdateAccount(newAccountName)
        );
    }
}