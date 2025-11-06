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
    /// 非可空 DateTime：读兼容多格式，写固定 "yyyy-MM-dd HH:mm:ss"
    public sealed class DateTimeTextConverter : JsonConverter<DateTime>
    {
        private readonly string _format;
        private static readonly string[] _readFormats =
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

        public DateTimeTextConverter(string format = "yyyy-MM-dd HH:mm:ss") => _format = format;

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var s = reader.GetString();
                if (string.IsNullOrWhiteSpace(s)) return default;

                if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt))
                    return dt;

                if (DateTime.TryParseExact(s, _readFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                    return dt;

                return DateTime.ParseExact(s!, _format, CultureInfo.InvariantCulture);
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                var v = reader.GetInt64();
                return v > 1_000_000_000_000
                    ? DateTimeOffset.FromUnixTimeMilliseconds(v).LocalDateTime
                    : DateTimeOffset.FromUnixTimeSeconds(v).LocalDateTime;
            }

            return default;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString(_format, CultureInfo.InvariantCulture));
    }

    /// 可空 DateTime?：空串/空值 => null
    public sealed class NullableDateTimeTextConverter : JsonConverter<DateTime?>
    {
        private readonly DateTimeTextConverter _inner = new DateTimeTextConverter();

        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null) return null;

            if (reader.TokenType == JsonTokenType.String)
            {
                var s = reader.GetString();
                if (string.IsNullOrWhiteSpace(s)) return null;
            }

            return _inner.Read(ref reader, typeof(DateTime), options);
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue) _inner.Write(writer, value.Value, options);
            else writer.WriteNullValue();
        }
    }
}
