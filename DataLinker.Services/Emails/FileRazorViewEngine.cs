using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Postal;

namespace DataLinker.Services.Emails
{
    /// <summary>
    /// A view engine that uses the Razor engine to render a templates loaded directly from the
    /// file system. This means it will work outside of ASP.NET. This implementation is based on
    /// <see cref="FileSystemRazorViewEngine"/> and is modified to cope with nesting views under sub 
    /// directories. A side-effect of the simplistic view searching algorithm means that email
    /// views need to be uniquely named (as if they were in one directory).
    /// </summary>
    public class FileRazorViewEngine : IViewEngine
    {
        private string _viewPathRoot;

        /// <summary>
        /// Creates a new <see cref="FileRazorViewEngine"/> that finds views under the given path.
        /// </summary>
        /// <param name="viewPathRoot">The root directory that contains views.</param>
        public FileRazorViewEngine(string viewPathRoot)
        {
            _viewPathRoot = viewPathRoot;
        }

        /// <summary>
        /// Builds a list of possible paths for a view.
        /// </summary>
        /// <param name="path">View file name.</param>
        /// <returns>List of possible file locations.</returns>
        IList<string> GetViewFullPaths(string path)
        {
            var paths = new List<string>();
            paths.Add(Path.Combine(_viewPathRoot, path));

            foreach(var directory in Directory.GetDirectories(_viewPathRoot, "*", SearchOption.AllDirectories))
            {
                paths.Add(Path.Combine(directory, path));
            }

            return paths;
        }

        /// <summary>
        /// Tries to find a razor view (.cshtml or .vbhtml files).
        /// </summary>
        public ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            var possibleFilenames = new List<string>();

            if (!partialViewName.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase)
                && !partialViewName.EndsWith(".vbhtml", StringComparison.OrdinalIgnoreCase))
            {
                possibleFilenames.Add(partialViewName + ".cshtml");
                possibleFilenames.Add(partialViewName + ".vbhtml");
            }
            else
            {
                possibleFilenames.Add(partialViewName);
            }

            var possibleFullPaths = possibleFilenames.SelectMany(GetViewFullPaths).ToArray();

            var existingPath = possibleFullPaths.FirstOrDefault(File.Exists);

            if (existingPath != null)
            {
                return new ViewEngineResult(new FileSystemRazorView(existingPath), this);
            }
            else
            {
                return new ViewEngineResult(possibleFullPaths);
            }
        }

        /// <summary>
        /// Tries to find a razor view (.cshtml or .vbhtml files).
        /// </summary>
        public ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            return FindPartialView(controllerContext, viewName, useCache);
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public void ReleaseView(ControllerContext controllerContext, IView view)
        {
            // Nothing to do here - FileSystemRazorView does not need disposing.
        }
    }
}