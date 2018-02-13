using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace netcore_happypath.data.Utilities
{
    public static class PropertyHelper
    {
        public static PropertyInfo GetPropertyInfoCompilerProtected<T, TProp>(Expression<Func<T, TProp>> propertyExpression, BindingFlags bindingFlags) where T : class
        {
            string propertyName = GetPropertyNameCompilerProtected(propertyExpression);
            if (propertyExpression.Body is UnaryExpression)
            {
                UnaryExpression unaryExpression = (UnaryExpression)propertyExpression.Body;
                MemberExpression memberExpression = (MemberExpression)unaryExpression.Operand;
                propertyName = memberExpression.Member.Name;
            }

            if (propertyExpression.Body is MemberExpression)
            {
                MemberExpression memberExpression = (MemberExpression)propertyExpression.Body;
                propertyName = memberExpression.Member.Name;
            }

            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException(string.Format("Unexpected expression type '{0}'", propertyExpression.GetType().FullName));
            }

            Type type = typeof(T);
            return type.GetProperty(propertyName, bindingFlags);
        }

        public static PropertyInfo GetPropertyInfoCompilerProtected<T>(string propertyName, BindingFlags bindingFlags) where T : class
        {
            Type type = typeof(T);
            return type.GetProperty(propertyName, bindingFlags);
        }

        public static string GetPropertyNameCompilerProtected<T, TProp>(Expression<Func<T, TProp>> propertyExpression) where T : class
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException("propertyExpression");
            }

            if (propertyExpression.Body is UnaryExpression)
            {
                UnaryExpression unaryExpression = (UnaryExpression)propertyExpression.Body;
                MemberExpression memberExpression = (MemberExpression)unaryExpression.Operand;
                return memberExpression.Member.Name;
            }

            if (propertyExpression.Body is MemberExpression)
            {
                MemberExpression memberExpression = (MemberExpression)propertyExpression.Body;
                return memberExpression.Member.Name;
            }

            throw new ArgumentException(string.Format("Unexpected expression type '{0}'", propertyExpression.GetType().FullName));
        }
    }
}
