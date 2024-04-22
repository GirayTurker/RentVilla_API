using System.Reflection;

namespace RentVilla_API.Helpers
{
    public static class JsonSerialization
    {
        public static bool ShouldSerializeProperty<T>(T obj, string propertyName)
        {
            PropertyInfo propertyInfo = typeof(T).GetProperty(propertyName);
            if (propertyInfo == null)
            {
                throw new ArgumentException($"Property '{propertyName}' not found in type '{typeof(T).Name}'.");
            }

            object propertyValue = propertyInfo.GetValue(obj);
            return propertyValue != null;
        }
    }
}
