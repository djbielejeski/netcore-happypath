using Microsoft.Extensions.Logging;
using netcore_happypath.data.Entities;
using netcore_happypath.data.Session;
using netcore_happypath.services.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace netcore_happypath.services.DatabaseActivities
{
    public interface IUserService : IBaseEntityService<User, Guid>
    {

    }
    public class UserService : BaseEntityService<User, Guid>, IUserService
    {
        readonly ICurrentUserService _currentUserService;
        public UserService(INetCoreHappyPathSession session, ICurrentUserService currentUserService, ILogger<BaseEntityService<User, Guid>> logger)
            : base(session, currentUserService, logger)
        {
            _currentUserService = currentUserService;
        }

        protected override bool IsAuthorized(User originalEntity, User newEntity)
        {
            // TODO - If current user is not admin dont let the user update/delete/add the item
            return true;
        }
    }
}
