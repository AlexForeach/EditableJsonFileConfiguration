using Microsoft.Extensions.Configuration;

namespace EditableJsonFileConfiguration;

/// <summary>
/// Класс с методом расширения для <see cref="IConfigurationBuilder"/>
/// </summary>
public static class EditableJsonConfigurationExtensions
{
    /// <summary>
    /// Добавление провайдера в <see cref="IConfigurationBuilder"/>
    /// </summary>
    /// <param name="builder"><see cref="IConfigurationBuilder"/></param>
    /// <param name="path">Путь к файлу конфигураций</param>
    /// <returns><see cref="IConfigurationBuilder"/></returns>
    public static IConfigurationBuilder AddEditableJsonFile(this IConfigurationBuilder builder, string path)
    {
        var source = new EditableJsonFileConfigurationSource
        {
            Path = path,
            Optional = false,
            ReloadOnChange = true
        };

        builder.Add(source);
        return builder;
    }
}