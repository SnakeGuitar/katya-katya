using MediatR;
using MemoryGame.Application.Common.Interfaces;
using MemoryGame.Domain.Common;
using MemoryGame.Domain.Users;

namespace MemoryGame.Application.Profile.Commands.ChangePassword;

/// <summary>
/// Handles <see cref="ChangePasswordCommand"/>: verifies the current password
/// and replaces it with the new hashed password.
/// </summary>
public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes the handler with its dependencies.
    /// </summary>
    public ChangePasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordService passwordService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc/>
    public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId)
            ?? throw new DomainException(DomainErrors.User.NotFound);

        if (!_passwordService.Verify(request.CurrentPassword, user.PasswordHash))
            throw new DomainException(DomainErrors.User.PasswordIncorrect);

        var newPasswordHash = _passwordService.Hash(request.NewPassword);
        user.ChangePassword(newPasswordHash);

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
