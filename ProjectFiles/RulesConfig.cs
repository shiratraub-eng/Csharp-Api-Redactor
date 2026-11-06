

//Goal: Read and parse the rules.yaml file into C# objects.
//טעינת חוקים

//using YamlDotNet Library

using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;


namespace ApiRedactor
{
    public class RulesConfig
    {

        // The version of the rules configuration schema

        [YamlMember(Alias = "version")] //תכונה, מקשר בין הגרסא בקובץ ל"גרסא" בקוד שלי
        public int Version { get; set; } 

        //this is property- meant to save data (here its about the version)
        //get and set - access to edit the version

        //--------------------------------------------------------------------------------
        // // Global settings for different redaction actions (mask, partial, hash)

        [YamlMember(Alias = "settings")]
        public Settings? Settings { get; set; }

        //--------------------------------------------------------------------------------
        // List of rules that match based on JSON key names or HTTP header names

        [YamlMember(Alias = "key_rules")]
        public List<KeyRule>? KeyRules { get; set; }

        //--------------------------------------------------------------------------------
        // List of rules that match based on the content/value of strings regardless of the key name
       
        [YamlMember(Alias = "value_rules")]
        public List<ValueRule>? ValueRules { get; set; }

        //--------------------------------------------------------------------------------

   
        /// <param name="filePath">The path to the rules.yaml file.</param>
        /// <returns>A populated instance of RulesConfig.</returns>
        /// 
        /// loads the rules.yaml file and converts it into a C# object
        public static RulesConfig LoadFromFile(string filePath)
        {
            // Verify if the configuration file actually exists on the disk before reading
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Rule file not found at path: {filePath}");
            }

            // Read all the raw text content from the YAML file
            string yamlContent = File.ReadAllText(filePath);


            // Configure the YAML deserializer to match snake_case properties in YAML 
            // with PascalCase properties in C# classes smoothly.
            //for example - in file : key_rules , then it translates to "KeyRules"
               var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();
            // Deserialize the raw YAML string into our C# object model and return it
            return deserializer.Deserialize<RulesConfig>(yamlContent);
            
        }
    }

    //--------------------------------------------------------------------------------
    

    /// <summary>
    /// Holds global parameters for edit redaction actions defined in the YAML file.
    /// Uses Dictionary<string, object> to safely handle mixed data types (strings, integers).
    /// </summary>
   
    public class Settings
    {
        [YamlMember(Alias = "mask")]
        public Dictionary<string, string>? Mask { get; set; }

        [YamlMember(Alias = "partial")]
        public Dictionary<string, object>? Partial { get; set; }
// "object" because it can get both int and strings (and everything else)

        [YamlMember(Alias = "hash")]
        public Dictionary<string, string>? Hash { get; set; }
    }

    //--------------------------------------------------------------------------------

    /// <summary>
    /// Defines a rule that targets a specific key or header name (e.g., 'password', 'Authorization').
    /// </summary>
    public class KeyRule
    {
        // Unique identifier or descriptive name for the rule
        [YamlMember(Alias = "name")]
        public string? Name { get; set; }

        // Where this rule applies: 'headers', 'body', or 'any'
        [YamlMember(Alias = "scope")]
        public string? Scope { get; set; }

        // Match criteria (can contain exact match lists or regex patterns, mapped as generic dictionary)
        [YamlMember(Alias = "match")]
        public Dictionary<string, object>? Match { get; set; }

        // The action to execute when matched: 'mask', 'partial', 'hash', or 'remove'
        [YamlMember(Alias = "action")]
        public string? Action { get; set; }
    }

    //--------------------------------------------------------------------------------

    /// <summary>
    /// Defines a rule that targets sensitive patterns inside text values (e.g., emails, JWT tokens, credit cards).
    /// </summary>
    public class ValueRule
    {
        // Unique name for the value rule
        [YamlMember(Alias = "name")]
        public string? Name { get; set; }

        // Regular expression pattern to search for inside string values
        [YamlMember(Alias = "pattern")]
        public string? Pattern { get; set; }

        // Optional validator type (e.g., 'luhn' for credit card validation)
        [YamlMember(Alias = "validator")]
        public string? Validator { get; set; }

        // The redaction action to apply when a pattern match is found
        [YamlMember(Alias = "action")]
        public string? Action { get; set; }
    }
}