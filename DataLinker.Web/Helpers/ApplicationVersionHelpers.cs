using System.Globalization;
using System.Reflection;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;

namespace DataLinker.Web.Helpers
{
    public static class ApplicationVersionHelpers
    {
        private const string AssemblyRevisionNumberKey = "AssemblyRevisionNumber";

        public static string AssemblyRevisionNumber(this HtmlHelper helper)
        {
#if (DEBUG) 
            return string.Empty;
#else
            if (HttpRuntime.Cache[AssemblyRevisionNumberKey] == null)
            {
                var assembly = Assembly.GetExecutingAssembly();
                var assemblyRevisionNumber = assembly.GetName().Version.Revision.ToString(CultureInfo.InvariantCulture);

                HttpRuntime.Cache.Insert(AssemblyRevisionNumberKey, assemblyRevisionNumber,
                    new CacheDependency(assembly.Location));
            }

            return HttpRuntime.Cache[AssemblyRevisionNumberKey] as string;
#endif
        }
    }
}