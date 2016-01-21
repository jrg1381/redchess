using System;
using System.Web.Mvc;

namespace RedChess.ControllerTests
{
    static class PropertyUtils
    {
        public static T ExtractPropertyValue<T>(JsonResult result, string propertyName)
        {
            var res = result.Data;
            var propertyInfo = res.GetType().GetProperty(propertyName);
            if(propertyInfo == null)
                throw new ArgumentException("Property '" + propertyName + "' not found on JsonResult");
            var value = propertyInfo.GetValue(res, null);
            return (T)value;
        }

        public static T ExtractPropertyValue<T>(object result, string propertyName)
        {
            var propertyInfo = result.GetType().GetProperty(propertyName);
            if (propertyInfo == null)
                throw new ArgumentException("Property '" + propertyName + "' not found on object");
            var value = propertyInfo.GetValue(result, null);
            return (T)value;
        }
    }
}