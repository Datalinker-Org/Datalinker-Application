using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using DataLinker.Database;
using System.Web.Compilation;
using log4net;
using System.Reflection;
using DataLinker.Models;

namespace DataLinker.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            ModelBinders.Binders.Add(new KeyValuePair<Type, IModelBinder>(typeof(ClauseModel), new BaseViewModelBinder()));
            DatabaseConfig.Initialize();
        }

        private void Application_Error(object sender, EventArgs e)
        {
            var ex = Server.GetLastError();
            Log.Error(ex);
        }

        protected void Session_Start() { }

        public void Application_End() { }
    }

    // Used this as example http://stackoverflow.com/questions/7222533/polymorphic-model-binding
    public class BaseViewModelBinder : DefaultModelBinder
    {
        protected override object CreateModel(ControllerContext controllerContext, ModelBindingContext bindingContext, Type modelType)
        {
            var listItemIndexes = bindingContext.ModelName.Where(char.IsDigit).ToArray();
            const int indexOfSectionsList = 0;
            const int indexOfClausesList = 1;
            // Array of type values should contain single element
            const int singleElementIndex = 0;
            var typeValue = bindingContext.ValueProvider.GetValue($"ChildModelType{listItemIndexes[indexOfSectionsList]}{listItemIndexes[indexOfClausesList]}");
            string valueType;
            try
            {
                var value = (string[])typeValue.RawValue;
                valueType = value[singleElementIndex];
            }
            catch (InvalidCastException)
            {
                valueType = (string)typeValue.RawValue;
            }

            //var type = Type.GetType(valueType,true);
            var type = BuildManager.GetType(valueType, true);
            if (!typeof(ClauseModel).IsAssignableFrom(type))
            {
                throw new InvalidOperationException("Bad Type");
            }

            var model = Activator.CreateInstance(type);
            bindingContext.ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, type);
            return model;
        }
    }
}
