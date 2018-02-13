using netcore_happypath.data.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace netcore_happypath.data.Models
{
    public class UserPrincipalViewModel
    {
        public string EmailAddress { get; set; }
        public UserType UserType { get; set; }
        public Guid UserId { get; set; }
    }
}
