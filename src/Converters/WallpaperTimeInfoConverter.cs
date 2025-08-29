using System.Text.Json;
using System.Text.Json.Serialization;
using BingWallpaperCollector.Models;

namespace BingWallpaperCollector.Converters;

/// <summary>
/// WallpaperTimeInfo 的自定义 JSON 转换器
/// </summary>
public sealed class WallpaperTimeInfoConverter : JsonConverter<WallpaperTimeInfo>
{
  public override WallpaperTimeInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    if (reader.TokenType != JsonTokenType.StartObject)
    {
      throw new JsonException("Expected StartObject token");
    }

    string? startDate = null;
    string? fullStartDate = null;
    string? endDate = null;
    DateOnly? parsedStartDate = null;
    DateOnly? parsedEndDate = null;
    DateTime? fullStartDateTime = null;

    while (reader.Read())
    {
      if (reader.TokenType == JsonTokenType.EndObject)
      {
        break;
      }

      if (reader.TokenType != JsonTokenType.PropertyName)
      {
        throw new JsonException("Expected PropertyName token");
      }

      string? propertyName = reader.GetString();
      reader.Read();

      switch (propertyName?.ToLowerInvariant())
      {
        case "startdate":
          parsedStartDate = reader.TokenType == JsonTokenType.String ?
              JsonSerializer.Deserialize<DateOnly?>(ref reader, options) : null;
          break;
        case "enddate":
          parsedEndDate = reader.TokenType == JsonTokenType.String ?
              JsonSerializer.Deserialize<DateOnly?>(ref reader, options) : null;
          break;
        case "fullstartdatetime":
          fullStartDateTime = reader.TokenType == JsonTokenType.String ?
              JsonSerializer.Deserialize<DateTime?>(ref reader, options) : null;
          break;
        case "originaltimefields":
          var originalFields = JsonSerializer.Deserialize<BingApiTimeFields>(ref reader, options);
          if (originalFields != null)
          {
            startDate = originalFields.StartDate;
            fullStartDate = originalFields.FullStartDate;
            endDate = originalFields.EndDate;
          }
          break;
      }
    }

    return new WallpaperTimeInfo
    {
      StartDate = parsedStartDate,
      EndDate = parsedEndDate,
      FullStartDateTime = fullStartDateTime,
      OriginalTimeFields = new BingApiTimeFields
      {
        StartDate = startDate ?? string.Empty,
        FullStartDate = fullStartDate ?? string.Empty,
        EndDate = endDate ?? string.Empty
      }
    };
  }

  public override void Write(Utf8JsonWriter writer, WallpaperTimeInfo value, JsonSerializerOptions options)
  {
    ArgumentNullException.ThrowIfNull(writer);
    ArgumentNullException.ThrowIfNull(value);

    writer.WriteStartObject();

    if (value.StartDate.HasValue)
    {
      writer.WritePropertyName("StartDate");
      JsonSerializer.Serialize(writer, value.StartDate.Value, options);
    }

    if (value.EndDate.HasValue)
    {
      writer.WritePropertyName("EndDate");
      JsonSerializer.Serialize(writer, value.EndDate.Value, options);
    }

    if (value.FullStartDateTime.HasValue)
    {
      writer.WritePropertyName("FullStartDateTime");
      JsonSerializer.Serialize(writer, value.FullStartDateTime.Value, options);
    }

    writer.WritePropertyName("OriginalTimeFields");
    JsonSerializer.Serialize(writer, value.OriginalTimeFields, options);

    writer.WriteEndObject();
  }
}
