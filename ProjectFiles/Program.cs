//The manager.


namespace ApiRedactor
{
    class Program
    {
        // main - activates all the other classes and functions, and runs the program
        static void Main(string[] args)
        {
            try
            {
                // Define the file paths for our configuration and logs
                string rulesPath = "rules.yaml";
                string inputLogPath = "input.ndjson";
                string outputLogPath = "output.ndjson";

                Console.WriteLine("Starting Redactor CLI tool...");

                // Step 1: Load the rules configuration from the YAML file
                Console.WriteLine("Loading rules from rules.yaml...");
                RulesConfig My_config = RulesConfig.LoadFromFile(rulesPath);

                Console.WriteLine($"Successfully loaded {My_config.KeyRules?.Count ?? 0} key rules and {My_config.ValueRules?.Count ?? 0} value rules.");

                // Step 2: build the redaction engine with the rules
                var redactionEngine = new RedactionEngine(My_config);

                // Step 3: Create the log processor with the redaction engine
                var logProcessor = new LogProcessor(redactionEngine);

                // Step 4: Process the input log file and generate the edited output file
                Console.WriteLine($"Processing logs from -{inputLogPath}...");
                logProcessor.ProcessFile(inputLogPath, outputLogPath);

                // Step 5: Print redaction summary per rule to stderr (as required)
                Console.Error.WriteLine("--- Redaction Summary (Per Rule) ---");
                foreach (var kvp in redactionEngine.RuleHitCounts)
                {
                    Console.Error.WriteLine($"Rule '{kvp.Key}': {kvp.Value} hits");
                }

                Console.WriteLine("All done successfully.");
            }
            catch (Exception ex)
            {
                // If anything goes wrong, catch the error and print a clean message
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error : {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}


/*

        static void Main(string[] args)
        {
            try //testing if reading the rules work
            //run in terminal : dotnet run
            {
                string rulesFilePath = "rules.yaml";
                
                Console.WriteLine("loading the file of rules...");
                
                // קריאה לפונקציה שיצרתי שטוענת את הקובץ
                RulesConfig config = RulesConfig.LoadFromFile(rulesFilePath);

                Console.WriteLine("the file loaded successfully!");
                Console.WriteLine($"version of the rules: {config.Version}");
                Console.WriteLine($"number of key rules loaded: {config.KeyRules?.Count ?? 0}");
                Console.WriteLine($"number of value rules loaded: {config.ValueRules?.Count ?? 0}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"error loading the file: {ex.Message}");
            }
        }
*/