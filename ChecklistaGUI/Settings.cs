using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Checklist
{
    public static class Settings
    {
        private const string settings_file = @"\\Mtdb001\va_data$\Filedata\ProgramData\Vision\PublishedScripts\Settings.xml";

        public static string RESULT_SERVER = getFromXml(settings_file, "RESULT_SERVER");
        public static string RESULT_USERNAME = getFromXml(settings_file, "RESULT_USERNAME");
        public static string RESULT_PASSWORD = getFromXml(settings_file, "RESULT_PASSWORD");

        private static string getFromXml(string file, string varible)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(file);

            return doc.SelectSingleNode("Settings/" + varible).InnerText;
        }
    }
}
