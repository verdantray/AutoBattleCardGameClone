using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ProjectABC.Data
{
    public enum JsonType
    {
        String,
        Integer,
        Float,
        Bool,
        Null,
        Object,
        Array,
    }

    [Serializable]
    public class JsonValue
    {
        public JsonType type = JsonType.Object;
        public string strValue;
        public long intValue;
        public double floatValue;
        public bool boolValue;
        public JsonObject obj = new JsonObject();
        public List<JsonValue> arr = new List<JsonValue>();
        
        public JsonValue() { }

        public JsonValue(JsonType type)
        {
            SetType(type);
        }

        public void SetType(JsonType jsonType)
        {
            type = jsonType;
            switch (jsonType)
            {
                case JsonType.String:
                    strValue ??= string.Empty;
                    break;
                case JsonType.Integer:
                    intValue = 0;
                    break;
                case JsonType.Float:
                    floatValue = 0.0;
                    break;
                case JsonType.Bool:
                    boolValue = false;
                    break;
                case JsonType.Object:
                    obj ??= new JsonObject();
                    break;
                case JsonType.Array:
                    arr ??= new List<JsonValue>();
                    break;
                default:
                case JsonType.Null:
                    // nothing
                    break;
            }
        }
    }

    [Serializable]
    public class JsonObject : IReadOnlyList<JsonField>
    {
        public List<JsonField> fields = new List<JsonField>();

        #region inherits of IReadOnlyList<JsonField>

        public IEnumerator<JsonField> GetEnumerator() => fields.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => fields.GetEnumerator();

        public int Count => fields.Count;

        public JsonField this[int index] => fields[index];

        #endregion
    }

    [Serializable]
    public class JsonField
    {
        public string key = "key";
        public JsonValue value = new JsonValue(JsonType.String);
    }

    public static class JsonValueParser
    {
        public static string WriteJson(this JsonValue value, bool pretty)
        {
            var builder =  new StringBuilder();
            WriteNode(value, builder, pretty, 0);
            
            return builder.ToString();
        }

        private static void WriteNode(JsonValue value, StringBuilder builder, bool pretty, int indent)
        {
            switch (value.type)
            {
                case JsonType.String:
                    builder.Append('"').Append(Escape(value.strValue ?? string.Empty)).Append('"');
                    break;
                case JsonType.Integer:
                    builder.Append(value.intValue.ToString(CultureInfo.InvariantCulture));
                    break;
                case JsonType.Float:
                    builder.Append(value.floatValue.ToString(CultureInfo.InvariantCulture));
                    break;
                case JsonType.Bool:
                    builder.Append(value.boolValue ? "true" : "false");
                    break;
                case JsonType.Null:
                    builder.Append("null");
                    break;
                case JsonType.Object:
                    builder.Append('{');
                    
                    if (value.obj == null || value.obj.fields.Count == 0)
                    {
                        builder.Append('}');
                        break;
                    }

                    if (pretty)
                    {
                        builder.Append('\n');
                    }
                    
                    for (int i = 0; i < value.obj.fields.Count; i++)
                    {
                        var jsonField = value.obj.fields[i];
                        if (pretty)
                        {
                            builder.Append(new string(' ', (indent+1)*2));
                        }
                        
                        builder.Append('"').Append(Escape(jsonField.key ?? string.Empty)).Append('"').Append(':');
                        if (pretty)
                        {
                            builder.Append(' ');
                        }
                        
                        WriteNode(jsonField.value, builder, pretty, indent+1);
                        
                        if (i < value.obj.fields.Count - 1)
                        {
                            builder.Append(',');
                        }

                        if (pretty)
                        {
                            builder.Append('\n');
                        }
                    }

                    if (pretty)
                    {
                        builder.Append(new string(' ', indent*2));
                    }
                    
                    builder.Append('}');
                    break;
                case JsonType.Array:
                    builder.Append('[');
                    if (value.arr == null || value.arr.Count == 0)
                    {
                        builder.Append(']');
                        break;
                    }

                    if (pretty)
                    {
                        builder.Append('\n');
                    }
                    
                    for (int i = 0; i < value.arr.Count; i++)
                    {
                        if (pretty)
                        {
                            builder.Append(new string(' ', (indent+1)*2));
                        }
                        
                        WriteNode(value.arr[i], builder, pretty, indent+1);
                        
                        if (i < value.arr.Count - 1)
                        {
                            builder.Append(',');
                        }

                        if (pretty)
                        {
                            builder.Append('\n');
                        }
                    }

                    if (pretty)
                    {
                        builder.Append(new string(' ', indent*2));
                    }
                    
                    builder.Append(']');
                    break;
            }
        }
        
        private static string Escape(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            
            var sb = new StringBuilder();
            
            foreach (var ch in s)
            {
                switch (ch)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (char.IsControl(ch))
                        {
                            sb.AppendFormat("\\u{0:X4}", (int)ch);
                        }
                        else
                        {
                            sb.Append(ch);
                        }
                        break;
                }
            }
            
            return sb.ToString();
        }

        public static JsonValue ParseToJsonValue(this string jsonText)
        {
            JToken token = JToken.Parse(jsonText);
            return ConvertFromJToken(token);
        }

        public static JsonValue ConvertFromJToken(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.String:
                    return new JsonValue(JsonType.String) { strValue = token.Value<string>() };
                case JTokenType.Integer:
                    return new JsonValue(JsonType.Integer) { intValue = token.Value<long>() };
                case JTokenType.Float:
                    return new JsonValue(JsonType.Float) { floatValue = token.Value<double>() };
                case JTokenType.Boolean:
                    return new JsonValue(JsonType.Bool) { boolValue = token.Value<bool>() };
                case JTokenType.Null:
                case JTokenType.Undefined:
                    return new JsonValue(JsonType.Null);
                case JTokenType.Object:
                {
                    var obj = new JsonObject();
                    foreach (var prop in ((JObject)token).Properties())
                    {
                        obj.fields.Add(new JsonField
                        {
                            key = prop.Name,
                            value = ConvertFromJToken(prop.Value)
                        });
                    }
                    return new JsonValue(JsonType.Object) { obj = obj };
                }
                case JTokenType.Array:
                {
                    var list = new List<JsonValue>();
                    foreach (var el in (JArray)token)
                    {
                        list.Add(ConvertFromJToken(el));
                    }
                    return new JsonValue(JsonType.Array) { arr = list };
                }
                default:
                // Fallback: stringify
                    return new JsonValue(JsonType.String) { strValue = token.ToString(Formatting.None) };
            }
        }
    }
}