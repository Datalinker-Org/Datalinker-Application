using System;

namespace DataLinker.WebApi
{
    public class SwaggerGroupNameAttribute : Attribute
    {
        public readonly string Name;

        public SwaggerGroupNameAttribute(string name)
        {
            Name = name;
        }
    }

    public static class GroupNames
    {
        public const string Provider = "For Provider";
        public const string Consumer = "For Consumer";
    }
}