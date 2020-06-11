using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DataLinker.Web.Helpers
{
    public static class PathHelpers
    {
        public static string ScriptsPath(this HtmlHelper helper, string pathWithoutScripts)
        {
            var fullPath = "";
            
#if (DEBUG)
            var scriptsPath = "~/Scripts/";
            fullPath = VirtualPathUtility.ToAbsolute(scriptsPath + pathWithoutScripts);
#else
            var rootUrl = ConfigurationManager.AppSettings["DataLinkerHost"];
            var scriptsPath = $"{rootUrl}Scripts/";
            fullPath = scriptsPath + pathWithoutScripts + "?v=" + helper.AssemblyRevisionNumber();
#endif
            return fullPath;
        }

        public static string StylesPath(this HtmlHelper helper, string pathWithoutStyles)
        {
            var fullPath = "";
#if (DEBUG)
            var stylesPath = "~/Styles/";
            fullPath = VirtualPathUtility.ToAbsolute(stylesPath + pathWithoutStyles);
#else
            var rootUrl = ConfigurationManager.AppSettings["DataLinkerHost"];
            var stylesPath = $"{rootUrl}Styles/";
            fullPath = stylesPath + pathWithoutStyles + "?v=" + helper.AssemblyRevisionNumber();
#endif
            return fullPath;
        }
    }

    public static class DateTimeFormat
    {
        public static string ToDisplayDate(this DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd");
        }
    }
}