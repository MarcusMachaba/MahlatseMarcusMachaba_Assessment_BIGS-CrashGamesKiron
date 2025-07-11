using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace DatabaseLayer.Custom.Services
{
    public static class DALConfiguration
    {
        public static IConfiguration Config { get; set; }
        public static string DbConnection => Config["ConnectionStrings:DefaultConnection"];
        public static string ApplicationName => Config["ApplicationName"];
        public static string ApplicationVersion => Config["ApplicationVersion"];
    }
}
