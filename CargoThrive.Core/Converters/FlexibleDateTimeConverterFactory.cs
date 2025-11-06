using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CargoThrive.Core.Converters
{
    /// <summary>
    /// 支持 DateTime 与 DateTime? 的灵活 JSON 转换器
    /// - 支持多种常见时间格式（本地时间 / UTC / ISO 8601）
    /// - 空字符串自动转为 null
    /// - 统一输出格式（yyyy-MM-dd HH:mm:ss）
    /// </summary>
    public class FlexibleDateTimeConverterFactory : JsonConverterFactory
    {
        private readonly string _targetFormat;

        public FlexibleDateTimeConverterFactory(string targetFormat = "yyyy-MM-dd HH:mm:ss")
        {
            _targetFormat = targetFormat;
        }

        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(DateTime) || typeToConvert == typeof(DateTime?);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            if (typeToConvert == typeof(DateTime))
                return new DateTimeConverter(_targetFormat);

            return new NullableDateTimeConverter(_targetFormat);
        }

        private sealed class DateTimeConverter : JsonConverter<DateTime>
        {
            private readonly string _format;

            private static readonly string[] _supportedFormats = new[]
            {
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-ddTHH:mm:ss",
                "yyyy-MM-ddTHH:mm:ss.fff",
                "yyyy-MM-ddTHH:mm:ssZ",
                "yyyy-MM-ddTHH:mm:ss.fffZ",
                "yyyy-MM-dd",
                "MM/dd/yyyy HH:mm:ss",
                "MM/dd/yyyy",
                "O"
            };

            public DateTimeConverter(string format) => _format = format;

            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    var s = reader.GetString();

                    if (string.IsNullOrWhiteSpace(s))
                        return default;

                    // 支持通用 ISO8601 / 本地时间 / UTC
                    if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt))
                        return dt;

                    if (DateTime.TryParseExact(s, _supportedFormats, CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out dt))
                        return dt;

                    // 如果仍不匹配，强行按目标格式解析
                    return DateTime.ParseExact(s, _format, CultureInfo.InvariantCulture);
                }

                if (reader.TokenType == JsonTokenType.Number)
                {
                    var val = reader.GetInt64();
                    // 判断是秒还是毫秒
                    return val > 1_000_000_000_000
                        ? DateTimeOffset.FromUnixTimeMilliseconds(val).LocalDateTime
                        : DateTimeOffset.FromUnixTimeSeconds(val).LocalDateTime;
                }

                return default;
            }

            public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString(_format, CultureInfo.InvariantCulture));
            }
        }

        private sealed class NullableDateTimeConverter : JsonConverter<DateTime?>
        {
            private readonly DateTimeConverter _inner;

            public NullableDateTimeConverter(string format)
            {
                _inner = new DateTimeConverter(format);
            }

            public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Null)
                    return null;

                if (reader.TokenType == JsonTokenType.String)
                {
                    var s = reader.GetString();
                    if (string.IsNullOrWhiteSpace(s))
                        return null;
                }

                return _inner.Read(ref reader, typeof(DateTime), options);
            }

            public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
            {
                if (value.HasValue)
                    _inner.Write(writer, value.Value, options);
                else
                    writer.WriteNullValue();
            }
        }
    }
}
