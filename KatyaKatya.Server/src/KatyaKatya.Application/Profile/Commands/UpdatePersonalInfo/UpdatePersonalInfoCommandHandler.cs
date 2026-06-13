using MediatR;
using KatyaKatya.Application.Common.Interfaces;
using KatyaKatya.Domain.Common;
using KatyaKatya.Domain.Users;

namespace KatyaKatya.Application.Profile.Commands.UpdatePersonalInfo;

/// <summary>
/// Handles <see cref="UpdatePersonalInfoCommand"/>: updates the user's first and last name.
/// </summary>
public class UpdatePersonalInfoCommandHandler : IRequestHandler<UpdatePersonalInfoCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes the handler with its dependencies.
    /// </summary>
    public UpdatePersonalInfoCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc/>
    public async Task<Unit> Handle(UpdatePersonalInfoCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId)
            ?? throw new DomainException(DomainErrors.User.NotFound);

        user.UpdatePersonalInfo(request.Name, request.LastName);
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
