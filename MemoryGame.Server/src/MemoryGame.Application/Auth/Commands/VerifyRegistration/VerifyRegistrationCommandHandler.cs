using MediatR;
using MemoryGame.Application.Common.Interfaces;
using MemoryGame.Domain.Common;
using MemoryGame.Domain.Users;
using MemoryGame.Domain.Users.ValueObjects;

namespace MemoryGame.Application.Auth.Commands.VerifyRegistration;

/// <summary>
/// Maneja <see cref="VerifyRegistrationCommand"/>: valida que el PIN sea correcto
/// y no haya expirado sin consumir el registro pendiente.
/// </summary>
public class VerifyRegistrationCommandHandler : IRequestHandler<VerifyRegistrationCommand, bool>
{
    private readonly IPendingRegistrationRepository _pendingRegistrationRepository;

    /// <summary>
    /// Inicializa el handler con sus dependencias.
    /// </summary>
    public VerifyRegistrationCommandHandler(IPendingRegistrationRepository pendingRegistrationRepository)
    {
        _pendingRegistrationRepository = pendingRegistrationRepository;
    }

    /// <inheritdoc/>
    public async Task<bool> Handle(VerifyRegistrationCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);

        var pending = await _pendingRegistrationRepository.GetByEmailAsync(email)
            ?? throw new DomainException("Registration not found.");

        if (!pending.ValidatePin(request.Pin))
            throw new DomainException("Invalid or expired PIN.");

        return true;
    }
}
