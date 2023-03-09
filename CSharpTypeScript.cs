using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace aeronology_tech_exam_no1
{
    public static class CSharpTypeScript
    {
        public class TypeScriptClassDefinition {
            public string Name { get; } 

            public List<TypeScriptProperty> Properties { get; }

            internal TypeScriptClassDefinition(string name, List<TypeScriptProperty> properties)
            {
                Name = name;
                Properties = properties;
            }

            public override string ToString() => $"export interface {Name} {{\n\t{string.Join("\n\t", Properties.Select(x => $"{x.Name}: {x.Type};"))}\n}}";
        }

        public class TypeScriptProperty
        {
            public string Name { get; set; }
            public string Type { get; set; }
        }


        private static readonly Dictionary<string, string> typeMap = new()
        {
            ["long"] = "number",
            ["int"] = "number",
        };

        private static string SnakeCase(string value)
        {
            if(string.IsNullOrEmpty(value)) return value;

            return value.Substring(0, 1).ToLower() + value[1..];
        }

        public static List<TypeScriptClassDefinition> ConvertCSharpObjectToTypeScript(string classDefinition) => DoConvertCSharpObjectToTypeScript(classDefinition, null);

        private static List<TypeScriptClassDefinition> DoConvertCSharpObjectToTypeScript(string classDefinition, List<TypeScriptClassDefinition> classes)
        {
            var lines = Regex.Split(classDefinition, @"\r\n|\n")
                                .Select(l => Regex.Replace(l, @"\{.*(get;.*set;|get;|set;).*\}", "").Trim())
                                .ToArray();

            var className = lines[0].Replace("public class ", "").Trim();

            if(classes != null && classes.Any(x => x.Name == className))
            {
                throw new Exception($"Interface '{className}' already exists in the list.");
            }

            var typescriptClassDefinitions = new List<TypeScriptClassDefinition>();

            var typescriptProps = new List<TypeScriptProperty>();

            // iterate each properties
            for (int i = 1; i < lines.Length; i++)
            {
                var line = Regex.Replace(lines[i], @"\{.*(get;.*set;|get;|set;).*\}", "").Trim();

                if (line.StartsWith("public class"))
                {
                    var nestedClassDefinition = new List<string>();
                    for (int j = i; j < lines.Length; j++)
                    {
                        nestedClassDefinition.Add(lines[j]);

                        if (lines[j] == "}")
                        {
                            i = j;
                            break;
                        }
                    }

                    // recursive iteration for nested class
                    typescriptClassDefinitions.AddRange(DoConvertCSharpObjectToTypeScript(string.Join('\n', nestedClassDefinition), typescriptClassDefinitions));
                }
                else if (line.StartsWith("public"))
                {
                    var parts = line.Split(' ');

                    var propertyName = SnakeCase(parts[2]);
                    var propertyType = parts[1];

                    if (typescriptProps.Any(x => Regex.IsMatch(x.Name, $@"{propertyName}(\?)?")))
                    {
                        throw new Exception($"Duplicate property '{propertyName}' for '{className}'.");
                    }

                    if (propertyType.EndsWith("?"))
                    {
                        propertyName += "?";
                        propertyType = propertyType[0..^1];
                    }


                    if (propertyType.StartsWith("List"))
                    {
                        propertyType = propertyType.Replace("List<", "").Replace(">", "");

                        if (typeMap.TryGetValue(propertyType, out var type))
                        {
                            propertyType = type;
                        }

                        propertyType += "[]";
                    }
                    else
                    {
                        if (typeMap.TryGetValue(propertyType, out var type))
                        {
                            propertyType = type;
                        }
                    }

                    
                    typescriptProps.Add(new TypeScriptProperty { Name = propertyName, Type = propertyType });
                }

            }


            typescriptClassDefinitions.Insert(0, new TypeScriptClassDefinition(className, typescriptProps));

            return typescriptClassDefinitions;
        }
    }

   
}
