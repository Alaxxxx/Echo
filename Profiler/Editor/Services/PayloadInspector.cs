using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace OpalStudio.Echo.Profiler.Editor.Services
{
      public static class PayloadInspector
      {
            private static readonly Dictionary<Type, FieldInfo[]> FieldCache = new();
            private static readonly Dictionary<Type, PropertyInfo[]> PropertyCache = new();

            public static string Summarize(object payload, int maxFields = 3)
            {
                  if (payload == null)
                  {
                        return string.Empty;
                  }

                  Type type = payload.GetType();
                  FieldInfo[] fields = GetFields(type);
                  PropertyInfo[] props = GetProperties(type);

                  if (fields.Length == 0 && props.Length == 0)
                  {
                        return string.Empty;
                  }

                  StringBuilder sb = new();
                  int shown = 0;

                  for (int i = 0; i < fields.Length && shown < maxFields; i++)
                  {
                        if (shown > 0)
                        {
                              sb.Append(", ");
                        }

                        sb.Append(fields[i].Name);
                        sb.Append('=');
                        AppendValue(sb, fields[i].GetValue(payload));
                        shown++;
                  }

                  for (int i = 0; i < props.Length && shown < maxFields; i++)
                  {
                        if (shown > 0)
                        {
                              sb.Append(", ");
                        }

                        sb.Append(props[i].Name);
                        sb.Append('=');

                        try
                        {
                              AppendValue(sb, props[i].GetValue(payload));
                        }
                        catch (Exception)
                        {
                              sb.Append("<err>");
                        }

                        shown++;
                  }

                  int remaining = fields.Length + props.Length - shown;

                  if (remaining > 0)
                  {
                        sb.Append(", …+");
                        sb.Append(remaining);
                  }

                  return sb.ToString();
            }

            public static IReadOnlyList<(string Key, string Value)> Describe(object payload)
            {
                  if (payload == null)
                  {
                        return Array.Empty<(string, string)>();
                  }

                  Type type = payload.GetType();
                  FieldInfo[] fields = GetFields(type);
                  PropertyInfo[] props = GetProperties(type);

                  List<(string Key, string Value)> result = new(fields.Length + props.Length);

                  for (int i = 0; i < fields.Length; i++)
                  {
                        result.Add((fields[i].Name, FormatValue(fields[i].GetValue(payload))));
                  }

                  for (int i = 0; i < props.Length; i++)
                  {
                        try
                        {
                              result.Add((props[i].Name, FormatValue(props[i].GetValue(payload))));
                        }
                        catch (Exception ex)
                        {
                              result.Add((props[i].Name, $"<error: {ex.GetType().Name}>"));
                        }
                  }

                  return result;
            }

            private static void AppendValue(StringBuilder sb, object value)
            {
                  if (value == null)
                  {
                        sb.Append("null");

                        return;
                  }

                  if (value is string s)
                  {
                        sb.Append('"');
                        sb.Append(s.Length > 32 ? s.Substring(0, 32) + "…" : s);
                        sb.Append('"');

                        return;
                  }

                  if (value is float f)
                  {
                        sb.Append(f.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));

                        return;
                  }

                  if (value is double d)
                  {
                        sb.Append(d.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));

                        return;
                  }

                  sb.Append(value.ToString());
            }

            private static string FormatValue(object value)
            {
                  if (value == null)
                  {
                        return "null";
                  }

                  if (value is string s)
                  {
                        return $"\"{s}\"";
                  }

                  if (value is float f)
                  {
                        return f.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
                  }

                  if (value is double d)
                  {
                        return d.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
                  }

                  return value.ToString();
            }

            private static FieldInfo[] GetFields(Type type)
            {
                  if (!FieldCache.TryGetValue(type, out FieldInfo[] fields))
                  {
                        fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
                        FieldCache[type] = fields;
                  }

                  return fields;
            }

            private static PropertyInfo[] GetProperties(Type type)
            {
                  if (!PropertyCache.TryGetValue(type, out PropertyInfo[] props))
                  {
                        PropertyInfo[] all = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                        List<PropertyInfo> filtered = new(all.Length);

                        for (int i = 0; i < all.Length; i++)
                        {
                              if (all[i].CanRead && all[i].GetIndexParameters().Length == 0)
                              {
                                    filtered.Add(all[i]);
                              }
                        }

                        props = filtered.ToArray();
                        PropertyCache[type] = props;
                  }

                  return props;
            }
      }
}