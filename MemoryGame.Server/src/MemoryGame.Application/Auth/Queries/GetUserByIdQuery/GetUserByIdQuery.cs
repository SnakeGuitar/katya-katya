using MediatR;
using MemoryGame.Application.Auth.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace MemoryGame.Application.Auth.Queries.GetUserByIdQuery
{
    public record GetUserByIdQuery(int UserId) : IRequest<UserDto>;
}
