using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;

namespace EditableJsonFileConfiguration;

/// <inheritdoc />
public class EditableJsonFileConfigurationProvider(EditableJsonFileConfigurationSource source) : FileConfigurationProvider(source)
{
	/// <summary>
	/// Загрузка данных из потока данных
	/// </summary>
	/// <param name="stream">Поток данных</param>
	/// <exception cref="FormatException">Ошибка при загрузке файла</exception>
	public override void Load(Stream stream)
	{
		try
		{
			var root = JsonNode.Parse(stream);

			Data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

			if (root != null)
				FlattenNode(root, parentPath: null);
		}
		catch (Exception ex)
		{
			throw new FormatException("Ошибка при загрузке конфигурации из JSON файла.", ex);
		}
	}

	/// <summary>
	/// Обход JSON-дерева и преобразование его в плоский словарь Data с ключами
	/// </summary>
	/// <param name="node">Текущий нод</param>
	/// <param name="parentPath">Родитель</param>
	private void FlattenNode(JsonNode? node, string? parentPath)
	{
		switch (node)
		{
			case JsonObject jsonObject:
			{
				foreach (var kvp in jsonObject)
				{
					var key = parentPath == null ? kvp.Key : $"{parentPath}:{kvp.Key}";
					FlattenNode(kvp.Value, key);
				}

				break;
			}
			case JsonArray jsonArray:
			{
				int index = 0;
				foreach (var item in jsonArray)
				{
					var key = $"{parentPath}:{index}";
					FlattenNode(item, key);
					index++;
				}

				break;
			}
			case JsonValue jsonValue:
				Data[parentPath ?? ""] = jsonValue.ToString();
				break;
		}
	}

	/// <summary>
	/// Метод для сохранения данных
	/// </summary>
	public void Save()
	{
		var source = (EditableJsonFileConfigurationSource)Source;


		var root = new JsonObject();
		foreach (var kvp in Data)
		{
			var path = kvp.Key.Split(':');
			JsonObject current = root;

			for (int i = 0; i < path.Length; i++)
			{
				var segment = path[i];

				if (i == path.Length - 1)
				{
					current[segment] = TryParseJsonValue(kvp.Value);
				}
				else
				{
					if (current[segment] is null || current[segment].GetType() != typeof(JsonObject))
					{
						current[segment] = new JsonObject();
					}

					current = (JsonObject)current[segment];
				}
			}
		}

		var options = new JsonSerializerOptions { WriteIndented = true };
		var jsonData = root.ToJsonString(options);

		File.WriteAllText(source.Path, jsonData);
	}

	/// <summary>
	/// Преобразование строкового значения в соответствующий тип
	/// </summary>
	/// <param name="value">Значение в строке</param>
	/// <returns>Значение в <see cref="JsonValue"/></returns>
	private JsonValue? TryParseJsonValue(string? value)
	{
		if (value == null)
		{
			return null;
		}

		if (int.TryParse(value, out var intValue))
		{
			return JsonValue.Create(intValue);
		}
		if (double.TryParse(value, out var doubleValue))
		{
			return JsonValue.Create(doubleValue);
		}
		if (bool.TryParse(value, out var boolValue))
		{
			return JsonValue.Create(boolValue);
		}
		if (DateTime.TryParse(value, out var dateTimeValue))
		{
			return JsonValue.Create(dateTimeValue);
		}

		return JsonValue.Create(value);
	}

	/// <summary>
	/// Переопределим Set, чтобы данные сохранялись при изменении
	/// </summary>
	/// <param name="key">Ключ</param>
	/// <param name="value">Значение</param>
	public override void Set(string key, string? value)
	{
		base.Set(key, value);
		Save();
	}
}