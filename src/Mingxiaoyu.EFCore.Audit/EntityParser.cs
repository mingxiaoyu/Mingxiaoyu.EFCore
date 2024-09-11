using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mingxiaoyu.EFCore.Audit
{
    public static class EntityParser
    {
        public static T? ParseEntity<T>(string value) where T : class, new()
        {
            var entity = new T();
            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Split the input based on common delimiters
            var parts = value.Split(new[] { ", " }, StringSplitOptions.None);
            foreach (var part in parts)
            {
                var keyValue = part.Split(':', 2);
                if (keyValue.Length == 2)
                {
                    var propertyName = keyValue[0].Trim();
                    var propertyValue = keyValue[1].Trim();

                    var property = properties.FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
                    if (property != null)
                    {
                        object convertedValue = null;

                        // Handle different types
                        if (property.PropertyType == typeof(Guid))
                        {
                            convertedValue = Guid.Parse(propertyValue);
                        }
                        else if (property.PropertyType == typeof(DateTime))
                        {
                            convertedValue = DateTime.Parse(propertyValue);
                        }
                        else if (property.PropertyType == typeof(bool))
                        {
                            convertedValue = bool.Parse(propertyValue);
                        }
                        else if (property.PropertyType == typeof(decimal))
                        {
                            convertedValue = decimal.Parse(propertyValue);
                        }
                        else if (property.PropertyType == typeof(int))
                        {
                            convertedValue = int.Parse(propertyValue);
                        }
                        else if (property.PropertyType == typeof(double))
                        {
                            convertedValue = double.Parse(propertyValue);
                        }
                        else if (property.PropertyType == typeof(string))
                        {
                            convertedValue = propertyValue;
                        }

                        if (convertedValue != null)
                        {
                            property.SetValue(entity, convertedValue);
                        }
                    }
                }
            }

            return entity;
        }

        public static List<T> ParseEntityList<T>(List<AuditLog> auditLogs) where T : class, new()
        {
            var entities = new List<T>();
            foreach (var log in auditLogs)
            {
                if (string.IsNullOrEmpty(log.NewValue))
                    continue;

                var entity = ParseEntity<T>(log.NewValue);
                entities.Add(entity);
            }
            return entities;
        }
    }
}
