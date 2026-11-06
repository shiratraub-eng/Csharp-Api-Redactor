//The Goal: The "brain" of the program. - מנוע ההסתרה
//It receives text or a JSON object, applies the rules 
//(Regex checks, Luhn validation for credit cards),
//and returns the information with the data masked.

using System;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace ApiRedactor
{
    /// <summary>
    /// The RedactionEngine is responsible for applying the rules from rules.yaml 
    /// onto the log lines read from the input.ndjson file.
    /// </summary>
    public class RedactionEngine
    {
        private readonly RulesConfig Rule_config;

        // Tracks how many times each rule was triggered by its Name
        //Dictionary to store the hit counts for each rule by its name
        public Dictionary<string, int> RuleHitCounts { get; } = new();

        /// <summary>
        /// Constructor receives the loaded configuration rules so the engine can use them.
        /// </summary>
        public RedactionEngine(RulesConfig config)
        {
            Rule_config = config;
            
            // Initialize hit counts dictionary for all rules so every rule starts at 0
            if (Rule_config.KeyRules != null)
            {
                foreach (var rule in Rule_config.KeyRules)
                {
                    if (!string.IsNullOrEmpty(rule.Name)) RuleHitCounts[rule.Name] = 0;
                }
            }
            if (Rule_config.ValueRules != null)
            {
                foreach (var rule in Rule_config.ValueRules)
                {
                    if (!string.IsNullOrEmpty(rule.Name)) RuleHitCounts[rule.Name] = 0;
                }
            }
        }

        // -------------
        //RedactLine - receives a line of text (JSON) and returns the edited version
        public string RedactLine(string jsonLine)
        {
            // If the line is empty or whitespace, return it as is
            if (string.IsNullOrWhiteSpace(jsonLine))
            {
                return jsonLine;
            }

            try
            {
                // process the raw JSON string into a JsonNode tree
                JsonNode? jsonNode = JsonNode.Parse(jsonLine);

                if (jsonNode == null)
                {
                    return jsonLine; // Return original if parsing failed
                }

                //call the recursive function to process the JSON tree
                ProcessJsonNode(jsonNode);

                // Convert the modified JSON tree back to a string and return it
                return jsonNode.ToJsonString();
            }
            catch (Exception)
            {
                // If the line is not valid JSON, return it unmodified or handle as raw text
                return jsonLine;
            }
        }

        /// <summary>
        /// Helper method to recursively walk through JSON objects and arrays.
        /// searches for fields that need to be hidden (according to the rules), and replaces them with masking characters (****).
        /// </summary>
        private void ProcessJsonNode(JsonNode node)
        {
            if (node is JsonObject My_jsonObject)
            {
                foreach (var kvp in My_jsonObject)
                {
                    string key = kvp.Key;
                    JsonNode? value = kvp.Value;
                    //for example: age is the key and 25 is the value.

                    if (value != null)
                    {
                        //1. Check if this key matches any KeyRules (e.g., 'password', 'credit_card')
                        string? matchedRuleName = GetMatchedKeyRuleName(key);
                        if (matchedRuleName != null)
                        {
                            My_jsonObject[key] = "****"; // Apply standard mask action
                            
                            // Increment hit count for this rule
                            if (RuleHitCounts.ContainsKey(matchedRuleName))
                            {
                                //add 1 to the count of how many times this rule was triggered
                                RuleHitCounts[matchedRuleName]++;
                            }
                        }
                        else //doesnt match any key rule, so we check the value
                        {
                            // 2. If the value is a string, check ValueRules (Regex patterns)
                            if (value is JsonValue jsonVal && jsonVal.TryGetValue<string>(out string? My_strVal) && My_strVal != null) //checks if value is string
                            {
                                string editedValue = ApplyValueRules(My_strVal); //calls function
                                if (editedValue != My_strVal) //if it was changed, we update the value in the JSON object
                                {
                                    My_jsonObject[key] = editedValue; // Update with the redacted string value!!!
                                }
                            }
                            else
                            {
                                // 3. If it's a nested object or array, recurse deeper
                                ProcessJsonNode(value);
                            }
                        }
                    }
                }
            }
            else if (node is JsonArray My_jsonArray)
            {
                for (int i = 0; i < My_jsonArray.Count; i++)
                {
                    var item = My_jsonArray[i];
                    if (item != null)
                    {
                        ProcessJsonNode(item);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if a given key matches any of the KeyRules and returns the rule name if matched.
        /// </summary>
        private string? GetMatchedKeyRuleName(string keyName)
        {
            if (Rule_config.KeyRules == null) 
                return null;

            //read the rules from the config and check if the key matches any of them
            foreach (var Myrule in Rule_config.KeyRules)
            {
                // Check if the rule has an exact match configuration for keys
                if (Myrule.Name != null && Myrule.Match.TryGetValue("keys", out object? My_keysObj))
                //check if the rule has a "keys" entry in its match dictionary
                {
                    if (My_keysObj is System.Text.Json.JsonElement jsonElement && jsonElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                    //check if the "keys" entry is a JSON array and its a JsonElement
                    {
                        foreach (var CurrentVal in jsonElement.EnumerateArray())
                        {
                            if (CurrentVal.GetString()?.Equals(keyName, StringComparison.OrdinalIgnoreCase) == true)
                            {
                                return Myrule.Name;
                            }
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Applies ValueRules (Regex patterns) to string values.
        /// </summary>
        private string ApplyValueRules(string inputVal)
        {
            if (Rule_config.ValueRules == null) 
                return inputVal;

            string result = inputVal;
            foreach (var rule in Rule_config.ValueRules)
            {
                if (!string.IsNullOrEmpty(rule.Pattern))
                {
                    try
                    {
                        // Check if pattern matches before replacing to track hits accurately
                        if (Regex.IsMatch(result, rule.Pattern))
                        {
                            // Replace matches of the regex pattern with a redaction mask
                            result = Regex.Replace(result, rule.Pattern, "****");

                            // Increment hit count for this rule name
                            if (!string.IsNullOrEmpty(rule.Name) && RuleHitCounts.ContainsKey(rule.Name))
                            {
                                RuleHitCounts[rule.Name]++;
                            }
                        }
                    }
                    catch (ArgumentException)
                    {
                        // Handle invalid regex patterns safely
                    }
                }
            }

            return result;
        }
    }
}