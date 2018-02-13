using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using netcore_happypath.data.Entities;
using netcore_happypath.services.DatabaseActivities;

namespace netcore_happypath.Controllers
{
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        readonly IUserService _userService;
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public List<User> Get()
        {
            return _userService.GetAll().ToList();
        }

        [HttpGet("{id}")]
        public User Get(Guid id)
        {
            return _userService.Get(id);
        }

        [HttpPost]
        public void Post([FromBody]User item)
        {
            _userService.AddOrUpdate(item);
        }

        [HttpDelete("{id}")]
        public void Delete(Guid id)
        {
            _userService.Delete(id);
        }
    }
}
