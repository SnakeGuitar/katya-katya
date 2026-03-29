using MediatR;
using MemoryGame.Application.Auth.DTOs;
using MemoryGame.Domain.Users;
using MemoryGame.Domain.Common;

namespace MemoryGame.Application.Auth.Queries.GetUserByIdQuery
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
    {
        private readonly IUserRepository _userRepository;

        public GetUserByIdQueryHandler(
            IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId) 
                ?? throw new DomainException(DomainErrors.User.NotFound);

            return new UserDto(
            user.Id,
            user.Username,
            user.Email.Value,
            user.IsGuest,
            user.VerifiedEmail
        );
        }
    }
}
