using MediatR;
using MemoryGame.Application.Common.Interfaces;
using MemoryGame.Domain.Common;
using MemoryGame.Domain.Users;

namespace MemoryGame.Application.Profile.Commands.ChangeUsername;

/// <summary>
/// Handles <see cref="ChangeUsernameCommand"/>: validates the new username is not taken
/// and applies the change.
/// </summary>
public class ChangeUsernameCommandHandler : IRequestHandler<ChangeUsernameCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes the handler with its dependencies.
    /// </summary>
    public ChangeUsernameCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc/>
    public async Task<Unit> Handle(ChangeUsernameCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId)
            ?? throw new DomainException("User not found.");

        if (await _userRepository.ExistsByUsernameAsync(request.NewUsername))
            throw new DomainException("Username already taken.");

        user.ChangeUsername(request.NewUsername);
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
