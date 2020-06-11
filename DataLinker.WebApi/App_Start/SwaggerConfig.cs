using System.Web.Http;
using WebActivatorEx;
using DataLinker.WebApi;
using Swashbuckle.Application;
using Swashbuckle.Swagger;
using System.Web.Http.Description;
using System.Linq;
using System.Collections.Generic;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace DataLinker.WebApi
{
    public class SwaggerConfig
    {
        public static void Register()
        {
            GlobalConfiguration.Configuration
                .EnableSwagger(c =>
                {
                    c.SingleApiVersion("v2", "DataLinker API")
                    .Description("API for DataLinker Web Application")
                    .Contact(cc => cc.Name("Rezare Systems LTD.").Email("sergey.molchanov@rezare.co.nz"));
                    c.IncludeXmlComments(GetXmlCommentsPath());
                    c.OperationFilter<AddAuthorizationHeaderParameterOperationFilter>();
                    c.GroupActionsBy(GetGroupName);
                    c.DocumentFilter<HideSomeDocsFilter>();
                })
                .EnableSwaggerUi(c => { });
        }

        private static string GetXmlCommentsPath()
        {
            return $@"{System.AppDomain.CurrentDomain.BaseDirectory}\bin\DataLinker.WebApi.xml";
        }

        private static string GetGroupName(ApiDescription apiDescription)
        {
            // Controller level
            var scopes =
                apiDescription.ActionDescriptor.ControllerDescriptor
                    .GetCustomAttributes<SwaggerGroupNameAttribute>().Select(i => i.Name)
                    .Distinct()
                    .ToList();
            if (scopes.Any())
            {
                return string.Join(",", scopes);
            }
            // Action level
            scopes =
                apiDescription.ActionDescriptor.GetCustomAttributes<SwaggerGroupNameAttribute>().Select(i => i.Name)
                    .Distinct()
                    .ToList();
            if (scopes.Any())
            {
                return string.Join(",", scopes);
            }
            // Default: by controller name
            return apiDescription.ActionDescriptor.ControllerDescriptor.ControllerName;
        }

        private class HideSomeDocsFilter : IDocumentFilter
        {
            public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
            {
                var obsoletePathes = swaggerDoc.paths.Where(route => route.Key.Contains("/api/")).ToList();
                foreach (var route in obsoletePathes)
                {
                    swaggerDoc.paths.Remove(route.Key);
                }
            }
        }

        public class AddAuthorizationHeaderParameterOperationFilter : IOperationFilter
        {
            public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
            {
                // Operation ID
                if (operation.parameters != null)
                {
                    operation.operationId += "By";
                    foreach (var parm in operation.parameters)
                    {
                        operation.operationId += $"{parm.name}";
                    }
                }

                // Authentication
                if (operation.parameters == null)
                {
                    operation.parameters = new List<Parameter>();
                }

                operation.parameters.Add(new Parameter
                {
                    name = "Authorization",
                    @in = "header",
                    description = "Application access token",
                    required = true,
                    type = "string"
                });
            }
        }
    }
}
