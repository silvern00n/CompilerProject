using System;
using System.IO;

namespace CompilerProject.Models.Helpers
{
    internal class FileManager
    {
        // Checks that the input file is a JavaScript file
        public bool IsJavaScriptFile(string filePath)
        {
            return Path.GetExtension(filePath).ToLower() == ".js";
        }

        // Reads the source file and returns its content
        public string ReadSourceFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return "";
            }

            return File.ReadAllText(filePath);
        }

        // Writes the generated Rust code into an output file
        // If outputDirectory is empty, the output file will be saved next to the JS file
        public bool WriteOutputFile(string content, string inputFilePath, string outputDirectory)
        {
            try
            {
                string outputPath = BuildOutputPath(inputFilePath, outputDirectory);
                File.WriteAllText(outputPath, content);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Builds the output path.
        // If outputDirectory was given, use it.
        // Otherwise, use the input JS file directory.
        private string BuildOutputPath(string inputFilePath, string outputDirectory)
        {
            string finalDirectory = outputDirectory;  

            if (string.IsNullOrWhiteSpace(finalDirectory))
            {
                finalDirectory = Path.GetDirectoryName(inputFilePath);
            }

            if (string.IsNullOrWhiteSpace(finalDirectory))
            {
                finalDirectory = Directory.GetCurrentDirectory();
            }

            // If the output directory does not exist, create it
            if (!Directory.Exists(finalDirectory))
            {
                Directory.CreateDirectory(finalDirectory);
            }

            string newRustFile = Path.GetFileNameWithoutExtension(inputFilePath);

            return Path.Combine(finalDirectory, newRustFile + ".rs");
        }

        public string GetOutputPath(string inputFilePath, string outputDirectory)
        {
            return BuildOutputPath(inputFilePath, outputDirectory);
        }

        public void PrintOutputFile(string inputFilePath, string outputDirectory)
        {
            string outputPath = BuildOutputPath(inputFilePath, outputDirectory);

            if (!File.Exists(outputPath))
            {
                Console.WriteLine("Output file was not found.");
                return;
            }

            string content = File.ReadAllText(outputPath);

            Console.WriteLine("----- Generated Rust Code -----");
            Console.WriteLine(content);
            Console.WriteLine("----- End Of Generated Code -----");
        }
    }
}