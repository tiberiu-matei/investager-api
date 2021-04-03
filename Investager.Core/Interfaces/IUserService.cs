﻿using Investager.Core.Dtos;
using System.Threading.Tasks;

namespace Investager.Core.Interfaces
{
    public interface IUserService
    {
        Task RegisterUser(RegisterUserDto registerUserDto);
    }
}
