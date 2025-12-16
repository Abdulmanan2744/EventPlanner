using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Security
{
    public static class AppSettingsManager
    {
        public static void SaveKey(string key)
        {
            var filePath = "appsettings.json";

            // Load existing appsettings.json
            var json = File.ReadAllText(filePath);
            var jsonObj = JObject.Parse(json);

            // Update the JWT Key
            jsonObj["Jwt"]["Key"] = key;

            // Save back to appsettings.json
            File.WriteAllText(filePath, jsonObj.ToString());
        }
    }
}
