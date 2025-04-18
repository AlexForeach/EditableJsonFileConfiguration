using Microsoft.Extensions.Configuration;

namespace EditableJsonFileConfiguration;

/// <summary>
/// Конфигурация в json формате в виде ключ/значение
/// </summary>
public class EditableJsonFileConfigurationSource : FileConfigurationSource
{
    /// <inheritdoc />
    public override IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        EnsureDefaults(builder); // Устанавливаем дефолтные значения
        return new EditableJsonFileConfigurationProvider(this);
    }
}