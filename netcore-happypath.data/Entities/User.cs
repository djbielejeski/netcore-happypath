using System;
using System.Collections.Generic;
using System.Text;

namespace netcore_happypath.data.Entities
{
    public class User : BaseEntity<Guid>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
    }
}
