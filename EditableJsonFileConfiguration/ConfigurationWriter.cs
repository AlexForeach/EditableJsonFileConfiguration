using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace EditableJsonFileConfiguration;

/// <summary>
/// Класс для записи объекта в конфигурацию
/// </summary>
public static class ConfigurationWriter
{
    /// <summary>
    /// Записать объект в <see cref="IConfiguration"/> по ключу
    /// </summary>
    /// <param name="configuration">Конфигурация</param>
    /// <param name="keyPrefix">Ключ</param>
    /// <param name="obj">Объект</param>
    public static void AddObjectToConfiguration<T>(IConfiguration configuration, string keyPrefix, T? obj)
        where T : IEditableOptions
    {
        if (obj is null)
            return;

        Type objType = obj.GetType();
        var properties = objType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var value = property.GetValue(obj);
            var key = string.IsNullOrEmpty(keyPrefix) ? property.Name : $"{keyPrefix}:{property.Name}";

            if (value == null)
            {
                configuration[key] = null;
            }
            else if (IsSimpleType(property.PropertyType))
            {
                configuration[key] = Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (typeof(IEnumerable<T>).IsAssignableFrom(property.PropertyType) && property.PropertyType != typeof(string))
            {
                int index = 0;
                foreach (var item in (IEnumerable<T>)value)
                {
                    AddObjectToConfiguration(configuration, $"{key}:{index}", item);
                    index++;
                }
            }
            else if (value is T castValue)
            {
                AddObjectToConfiguration(configuration, key, castValue);
            }
            else
                throw new ArgumentException($"Not allow type {nameof(value)}");
        }
    }

    static bool IsSimpleType(Type type)
    {
        var simpleTypes = new HashSet<Type>
        {
            typeof(string), typeof(char), typeof(bool), typeof(byte), typeof(sbyte),
            typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long),
            typeof(ulong), typeof(float), typeof(double), typeof(decimal), typeof(DateTime),
            typeof(DateTimeOffset), typeof(TimeSpan), typeof(Guid)
        };

        return simpleTypes.Contains(type) || type.IsEnum;
    }
}