//using Microsoft.Extensions.Configuration;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;

//namespace ExpenseDemo.Helpers
//{
//    public class ConfigHelper
//    {
//        public const string AppSettingsFileName = "appsettings.json";

//        static ConfigHelper()
//        {
//            InitializeBuilder();
//        }

//        public static IConfigurationRoot Configuration { get; set; }

//        private static void InitializeBuilder()
//        {
//            var builder = new ConfigurationBuilder()
//                .AddEnvironmentVariables()
//               .SetBasePath(Directory.GetCurrentDirectory())
//               .AddJsonFile(AppSettingsFileName);
//            var config = builder.Build();
//            Configuration = builder.Build();
//        }
//    }
//}
