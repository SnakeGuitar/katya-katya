namespace KatyaKatya.Application.Common.Interfaces;

public interface IPasswordService
{
    string Hash(string plainPassword);
    bool Verify(string plainPassword, string hashedPassword);
}
