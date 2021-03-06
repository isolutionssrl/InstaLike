﻿using System;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Events;
using MediatR;
using Serilog;

namespace InstaLike.Web.EventHandlers
{
    internal sealed class UserLoggedOutEventHandler : INotificationHandler<UserLoggedOutEvent>
    {
        private readonly ILogger _logger;

        public UserLoggedOutEventHandler(ILogger logger)
        {
            _logger = logger?.ForContext<UserLoggedOutEvent>() ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task Handle(UserLoggedOutEvent notification, CancellationToken cancellationToken)
        {
            _logger.Information("User [{Nickname}({UserID})] has just logged out.", notification.Nickname, notification.UserID);
            return Task.CompletedTask;
        }
    }
}
