using Microsoft.AspNetCore.Http;
using netcore_happypath.data.Models;
using netcore_happypath.data.Types;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace netcore_happypath.services.Services
{
    public interface ICurrentUserService
    {
        UserPrincipalViewModel CurrentUser { get; }
    }

    public class CurrentUserService : ICurrentUserService
    {
        public CurrentUserService()
        {
            this.CurrentUser = new UserPrincipalViewModel
            {
                EmailAddress = "djbielejeski@gmail.com",
                UserId = Guid.NewGuid(),
                UserType = UserType.Admin
            };
        }
        public UserPrincipalViewModel CurrentUser { get; set; }
    }
}
