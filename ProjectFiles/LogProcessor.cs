//The goal: to read the `input.ndjson` file line by line (using streaming),
//pass each line to the redaction engine (קובץ מנוע ההתסרה), and write out the result.
// עיבוד קובץ אינפוט

using System;
using System.IO;

namespace ApiRedactor
{
    /// <summary>
    /// The LogProcessor handles reading NDJSON log files line by line,
    /// passing each line through the RedactionEngine, and writing the output.
    /// </summary>
    
    public class LogProcessor
    {
        private readonly RedactionEngine _redactionEngine;

        /// <summary>
        /// Constructor receives an instance of the RedactionEngine.
        /// </summary>
         
        public LogProcessor(RedactionEngine redactionEngine_1)
        {
            _redactionEngine = redactionEngine_1;
        }

        /// <summary>
        /// Processes the input log file line by line and writes redacted logs to the output file.
        /// </summary>
        
        public void ProcessFile(string inputFilePath, string outputFilePath)
        {
            if (!File.Exists(inputFilePath))
            {
                throw new FileNotFoundException($"Input log file not found at path: {inputFilePath}");
            }

            // Read the input file line by line and write to output
            using (var My_reader = new StreamReader(inputFilePath))
            using (var My_writer = new StreamWriter(outputFilePath))
            {
                string? line;
                while ((line = My_reader.ReadLine()) != null)
                {
                    // edit sensitive data in the current line (sends the line to the redaction engine)
                    string redactedLine = _redactionEngine.RedactLine(line);

                    // Write the processed line to the output file
                    My_writer.WriteLine(redactedLine);
                }
            }

            Console.WriteLine($"Processing complete! Redacted logs saved to: {outputFilePath}");
        }
    }
}