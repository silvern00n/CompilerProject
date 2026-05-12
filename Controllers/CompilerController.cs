using CompilerProject.Models;
using CompilerProject.Models.DataStructures;
using CompilerProject.Views;
using System;
using System.IO;
using System.Collections.Generic;

namespace CompilerProject.Controllers
{
    internal class CompilerController
    {
        private CompilerModel _model;
        private CompilerView _view;

        public CompilerController(CompilerModel model, CompilerView view)
        {
            _model = model;
            _view = view;
        }

        // Main controller function
        public int Run()
        {
            _view.ShowWelcomeMessage();

            bool runAgain = true;

            while (runAgain)
            {
                // Stage 1 — get input file path from the user
                string inputPath = _view.GetInputPath();

                if (string.IsNullOrEmpty(inputPath))
                {
                    _view.ShowError("No file path was entered.");
                    return 1;
                }

                // Stage 2 — get optional output folder path
                string outputDirectory = _view.GetOutputDirectory();    

                // Stage 3 — run the compiler
                _view.ShowStatus("Starting compilation process...");

                int result = _model.RunCompiler(inputPath, outputDirectory);

                // Stage 4 — show results
                List<ErrorModel> errors = _model.GetErrors();

                if (result == 0)
                {
                    _view.ShowSuccess("Compilation finished successfully.");
                    _view.ShowOutputPath(_model.GetOutputPath());

                    if (errors.Count > 0)
                    {
                        _view.ShowErrors(errors);
                    }
                }
                else
                {
                    _view.ShowError("Compilation failed.");
                    _view.ShowErrors(errors);
                }

                // Stage 5 — ask whether to run again
                runAgain = _view.AskToRunAgain();

                if (runAgain)
                {
                    Console.Clear();
                }
            }
            Console.WriteLine("Have a nice day!");

            return 0;
        }
    }
}