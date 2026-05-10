using CompilerProject.Controllers;
using CompilerProject.Models;
using CompilerProject.Views;

namespace CompilerProject
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            CompilerModel model = new CompilerModel();
            CompilerView view = new CompilerView();
            CompilerController controller = new CompilerController(model, view);

            controller.Run();
        }
    }
}
