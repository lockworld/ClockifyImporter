using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockifyImport.ConsoleApp
{
    internal static class Core
    {
        internal static void UserMessage(string msg, bool pressEnter=true)
        {
            Console.WriteLine(msg);
            if (pressEnter)
            {
                Console.WriteLine("Press [Enter] to continue");
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Gets the AppSettings value for the specified configuration item. When a "configurationPrefix" is included, the application tries to find a setting with that prefix. This allows settings for any number of  different  configurations (TEST, PILOT, etc.) to be saved in the config file and accessed by updating the "Configuration" appSettings key.
        /// </summary>
        /// <param name="configName">The name of the appSettings key you wish to retrieve</param>
        /// <param name="configurationPrefix">Leave blank for global or default settings. Add a custom value to allow the application to be run with different configurations</param>
        /// <returns></returns>
        internal static string GetConfigValue(string configName, string configurationPrefix="")
        {
            return ConfigurationManager.AppSettings[configurationPrefix + configName] ?? "";
        }

        /// <summary>
        /// Gets the ConnectionString value for the named connection. When a "configurationPrefix" is included, the application tries to find a ConnectionString with that prefix. This allows for any number of  different  connections (TEST, PILOT, etc.) to be saved in the config file and accessed by updating the "Configuration" appSettings key.
        /// </summary>
        /// <param name="configName">The name of the ConnectionString you wish to retrieve</param>
        /// <param name="configurationPrefix">Leave blank for the primary ConnectionString. Add a custom value to allow the application to be run with different configurations</param>
        /// <returns></returns>
        internal static string GetConnectionString(string configName, string configurationPrefix="")
        {
            return ConfigurationManager.ConnectionStrings[configurationPrefix + configName].ToString();
        }
    }
}
