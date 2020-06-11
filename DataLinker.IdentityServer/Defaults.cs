using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLinker.IdentityServer
{
    class Defaults
    {
        /// <summary>
        /// The default path for embedding ID server
        /// </summary>
        public const string DefaultPath = "/core";

        /// <summary>
        /// The default path for callbacks
        /// </summary>
        public const string DefaultCallbackPath = DefaultPath;

        /// <summary>
        /// The default path for the admin module
        /// </summary>
        public const string DefaultAdminPath = DefaultPath + "/admin";
    }
}
