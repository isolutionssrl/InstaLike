﻿using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Commands;
using InstaLike.Core.Domain;
using MediatR;
using NHibernate;
using Serilog;

namespace InstaLike.Web.CommandHandlers
{
    internal class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<int>>
    {
        private readonly ISession _session;
        private readonly ILogger _logger;

        public RegisterUserCommandHandler(ISession session, ILogger logger)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _logger = logger?.ForContext<RegisterUserCommand>() ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<int>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var nicknameValidationResult = Nickname.Create(request.Nickname);
            var eMailValidationResult = Email.Create(request.Email);
            var passwordValidationResult = Password.Create(request.Password);
            var fullNameValidationResult = FullName.Create(request.Name, request.Surname);

            var validationResult = Result.Combine(nicknameValidationResult, eMailValidationResult, passwordValidationResult, fullNameValidationResult);
            if (validationResult.IsFailure)
            {
                _logger.Warning("Error during registration of user {Nickname}: Error message: {ErrorMessage}",
                    request.Nickname,
                    validationResult.Error);
                return Result.Fail<int>(validationResult.Error);
            }

            var userToRegister = new User(
                nicknameValidationResult.Value, 
                fullNameValidationResult.Value,
                passwordValidationResult.Value, 
                eMailValidationResult.Value,
                request.Biography);

            if (request.ProfilePicture != null)
            {
                userToRegister.SetProfilePicture((Picture)request.ProfilePicture);
            }

            using (var tx = _session.BeginTransaction())
            {
                try
                {
                    await _session.SaveAsync(userToRegister);
                    await tx.CommitAsync();

                    _logger.Information("User [{Nickname}({UserID})] has just registered.",
                        request.Nickname,
                        userToRegister.ID);

                    return Result.Ok(userToRegister.ID);
                }
                catch (ADOException ex)
                {
                    await tx.RollbackAsync();

                    _logger.Error("Error during registration of user {Nickname}. Error message: {ErrorMessage}",
                        request.Nickname,
                        ex.Message);

                    return Result.Fail<int>(ex.Message);
                }
            }
        }
    }
}
