﻿using Investager.Core.Constants;
using Investager.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Investager.Api.Policies
{
    public class AuthenticatedUserHandler : AuthorizationHandler<AuthenticatedUserRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthenticatedUserHandler(IHttpContextAccessor httpContextAccessor, IJwtTokenService jwtTokenService)
        {
            _httpContextAccessor = httpContextAccessor;
            _jwtTokenService = jwtTokenService;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthenticatedUserRequirement requirement)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            httpContext.Request.Headers.TryGetValue("Authorization", out var authorization);

            var tokenString = authorization[0].Split(" ")[1];
            var token = _jwtTokenService.Validate(tokenString);

            if (token != null)
            {
                httpContext.Items[HttpContextKeys.UserId] = token.Subject;
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
