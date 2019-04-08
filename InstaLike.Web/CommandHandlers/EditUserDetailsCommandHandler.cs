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
    public sealed class EditUserDetailsCommandHandler : IRequestHandler<EditUserDetailsCommand, Result>
    {
        private readonly ISession _session;
        private readonly ILogger _logger;

        public EditUserDetailsCommandHandler(ISession session, ILogger logger)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            this._logger = logger?.ForContext<EditUserDetailsCommand>() ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result> Handle(EditUserDetailsCommand request, CancellationToken cancellationToken)
        {
            var nicknameValidationResult = Nickname.Create(request.Nickname);
            var eMailValidationResult = Email.Create(request.Email);
            var fullNameValidationResult = FullName.Create(request.Name, request.Surname);

            var validationResult = Result.Combine(nicknameValidationResult, eMailValidationResult, fullNameValidationResult);
            if (validationResult.IsFailure)
            {
                _logger.Warning("Tried to update user profile for user {UserID} ({Nickname}) but some data were not valid: {WarningMessage}",
                    request.UserID,
                    request.Nickname,
                    validationResult.Error);
                return Result.Fail(validationResult.Error);
            }

            using (var tx = _session.BeginTransaction())
            {
                try
                {
                    var userToUpdate = await _session.GetAsync<User>(request.UserID);
                    userToUpdate.ChangeNickname(nicknameValidationResult.Value);
                    userToUpdate.ChangeEmailAddress(eMailValidationResult.Value);
                    userToUpdate.ChangeFullName(fullNameValidationResult.Value);
                    userToUpdate.UpdateBiography(request.Bio);
                    userToUpdate.SetProfilePicture((Picture)request.ProfilePicture);

                    await _session.UpdateAsync(userToUpdate);
                    await tx.CommitAsync();
                    _logger.Information("Successfully updated user profile for user {UserID} ({Nickname})",
                        request.UserID,
                        request.Nickname);
                    return Result.Ok(userToUpdate.ID);
                }
                catch (ADOException ex)
                {
                    await tx.RollbackAsync();
                    _logger.Error("Error updating user profile for user {UserID} ({Nickname}). Error message: {ErrorMessage}",
                        request.UserID,
                        request.Nickname,
                        ex.Message);
                    throw;
                }
            }
        }
    }
}
