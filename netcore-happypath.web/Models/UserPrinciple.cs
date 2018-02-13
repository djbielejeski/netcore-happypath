using netcore_happypath.data.Models;
using netcore_happypath.data.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace netcore_happypath.web.Models
{
    public interface IUserPrincipal : IPrincipal
    {
        string EmailAddress { get; }
        bool IsAuthenticated { get; }
        Guid UserId { get; set; }
    }

    public class UserPrincipal : IUserPrincipal
    {
        public IIdentity Identity { get; private set; }

        public UserPrincipal(UserPrincipalViewModel userData)
        {
            this.Identity = new GenericIdentity(userData.EmailAddress);
            this.UserType = userData.UserType;
            this.UserId = userData.UserId;
        }

        public bool IsInRole(string role)
        {
            return Identity != null && 
                    Identity.IsAuthenticated &&
                    !string.IsNullOrWhiteSpace(role) && 
                    this.UserType.ToString() == role;
        }

        public string EmailAddress
        {
            get
            {
                if (Identity != null && Identity.IsAuthenticated)
                {
                    return Identity.Name;
                }

                return string.Empty;
            }
        }
        public UserType UserType { get; set; }
        public Guid UserId { get; set; }

        public bool IsAuthenticated
        {
            get
            {
                if (this.Identity != null)
                {
                    return this.Identity.IsAuthenticated;
                }

                return false;
            }
        }
    }
}
