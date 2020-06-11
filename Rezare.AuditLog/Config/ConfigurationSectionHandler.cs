using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Rezare.AuditLog.Config
{
    public class ConfigurationSectionHandler : IConfigurationSectionHandler
    {
        /// <summary>
        /// Returns the configuration section from the config file.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="configContext"></param>
        /// <param name="section"></param>
        /// <returns>XmlNode with configuration settings.</returns>
        public object Create(object parent, object configContext, XmlNode section)
        {
            return section;
        }
    }
}
