using MediatR;
using KatyaKatya.Application.Auth.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace KatyaKatya.Application.Auth.Queries.GetUserByIdQuery
{
    public record GetUserByIdQuery(int UserId) : IRequest<UserDto>;
}
