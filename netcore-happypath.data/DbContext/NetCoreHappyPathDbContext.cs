using Microsoft.EntityFrameworkCore;
using netcore_happypath.data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace netcore_happypath.data
{
    public class NetCoreHappyPathDbContext : DbContext
    {
        public DbSet<User> UserEntities { get; set; }
        public NetCoreHappyPathDbContext(DbContextOptions<NetCoreHappyPathDbContext> options) : base(options)
        {
        }
    }
}
