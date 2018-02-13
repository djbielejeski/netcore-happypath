using Microsoft.EntityFrameworkCore;
using netcore_happypath.data.Attributes;
using netcore_happypath.data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;
using System.Linq;

namespace netcore_happypath.data.Utilities
{
    public static class EntityToSqlScript
    {
        public static bool IsPropertyDBProperty(PropertyInfo propertyInfo)
        {
            if ((propertyInfo.PropertyType.IsValueType || propertyInfo.PropertyType.IsPrimitive ||
                    propertyInfo.PropertyType == typeof(string) || propertyInfo.PropertyType.IsEnum) &&
                    (propertyInfo.GetSetMethod() != null &&
                    !(propertyInfo.PropertyType.IsGenericType && !(propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    && !propertyInfo.PropertyType.IsArray)))
            {
                NotMappedAttribute notMappedAttribute = propertyInfo.GetCustomAttribute<NotMappedAttribute>();
                ExcludeFromScriptGenerationAttribute excludeFromScriptAttribute = propertyInfo.GetCustomAttribute<ExcludeFromScriptGenerationAttribute>();

                if (notMappedAttribute == null && excludeFromScriptAttribute == null)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
