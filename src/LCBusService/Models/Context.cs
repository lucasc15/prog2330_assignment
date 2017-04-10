using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LCBusService.Models
{
    public class Context
    {
        private static readonly object padlock = new object();
        private static BusServiceContext _context;
        public static void MakeContext(IConfiguration config)
        {
            // Singleton pattern is overkill I think, instance is made in Startup.cs
            // so should be availble everywhere.  Static context / _context ensure only
            // one variable of context can exist
            if (_context == null) {
                lock (padlock) {
                    if (_context == null)
                    {
                        string connectionString = config["Database:connection"];
                        var optionsBuilder = new DbContextOptionsBuilder<BusServiceContext>();
                        optionsBuilder.UseSqlServer(connectionString);
                        _context = new BusServiceContext(optionsBuilder.Options);
                    }
                }
            }
        }
        public static BusServiceContext GetContext()
        {
            return _context;
        }
    }
}
