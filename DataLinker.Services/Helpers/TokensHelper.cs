using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLinker.Services.Helpers
{
    public class TokensHelper
    {
        private TokensHelper() { }

        /// <summary>
        /// Returns a simple unique token that is simply a base 64 encoded
        /// newly generated GUID.
        /// </summary>
        /// <returns>Token string.</returns>
        public static string GenerateToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }
    }
}
