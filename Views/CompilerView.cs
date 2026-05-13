using System;
using CompilerProject.Models.DataStructures;
using System;
using System.Collections.Generic;

namespace CompilerProject.Views
{
    internal class CompilerView
    {
        // Shows the welcome message
        public void ShowWelcomeMessage()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n╔══════════════════════════════════════╗");
            Console.WriteLine("║   JavaScript to Rust Compiler        ║");
            Console.WriteLine("║   Final Project - Tomer Lupo         ║");
            Console.WriteLine("╚══════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }

        // Gets the input file path from the user
        public string GetInputPath()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Enter path to JavaScript file: ");
            Console.ResetColor();

            return Console.ReadLine()?.Trim() ?? "";
        }

        // Shows process status
        public void ShowStatus(string message)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"  ► {message}");
            Console.ResetColor();
        }

        // Shows success message
        public void ShowSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n✔ {message}");
            Console.ResetColor();
        }

        // Shows error message
        public void ShowError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n✘ {message}");
            Console.ResetColor();
        }

        // Shows output file path
        public void ShowOutputPath(string path)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  Output file saved at: {path}");
            Console.ResetColor();
        }

        // Shows all collected errors
        public void ShowErrors(List<ErrorModel> errors)
        {
            if (errors.Count == 0)
            {
                return;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n═══════════════ Errors ═══════════════");
            Console.ResetColor();

            foreach (ErrorModel error in errors)
            {
                ConsoleColor color = error.Severity == ErrorSeverity.Critical
                    ? ConsoleColor.Red
                    : ConsoleColor.Yellow;

                Console.ForegroundColor = color;
                Console.WriteLine($"  {error}");
                Console.ResetColor();
            }

            Console.WriteLine("══════════════════════════════════════");
        }

        // Asks whether the user wants to compile another file
        public bool AskToRunAgain()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("\nCompile another file? (yes/no): ");
            Console.ResetColor();

            string answer = Console.ReadLine()?.Trim().ToLower() ?? "";

            return answer == "yes" || answer == "y";
        }

        public string GetOutputDirectory()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Enter output folder path for Rust file, or press Enter to use the JavaScript file location: ");
            Console.ResetColor();

            return Console.ReadLine()?.Trim() ?? "";
        }

        public void showGoodbyeMessage()
        {
            Console.WriteLine("\n══════════════════════════════════════");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("\nThank you for using me\nHave a nice day!\nGoodbye\n\n");
            Console.ResetColor();
            return;
        }
    }
}