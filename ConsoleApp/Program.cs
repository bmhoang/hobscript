using System;
using HobScript;

namespace HobScript.ConsoleApp
{
    /// <summary>
    /// Console application demonstrating HobScript functionality
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("HobScript - C# Script Engine Demo");
            Console.WriteLine("=================================");
            Console.WriteLine();

            if (args.Length > 0 && args[0] == "--interactive")
            {
                RunInteractiveMode();
            }
            else
            {
                RunExamples();
            }
        }

        static void RunExamples()
        {
            try
            {
                Example.RunAllExamples();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

        static void RunInteractiveMode()
        {
            Console.WriteLine("Interactive HobScript Mode");
            Console.WriteLine("Type 'help' for available commands, 'exit' to quit");
            Console.WriteLine();

            var engine = new HobScriptEngine();
            string input;

            do
            {
                Console.Write("HobScript> ");
                input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                if (input.ToLower() == "exit")
                    break;

                if (input.ToLower() == "help")
                {
                    ShowHelp();
                    continue;
                }

                if (input.ToLower() == "functions")
                {
                    ShowFunctions(engine);
                    continue;
                }

                if (input.ToLower() == "variables")
                {
                    ShowVariables(engine);
                    continue;
                }

                if (input.ToLower() == "clear")
                {
                    engine.ClearVariables();
                    Console.WriteLine("Variables cleared.");
                    continue;
                }

                try
                {
                    var result = engine.Execute(input);
                    if (result != null)
                    {
                        Console.WriteLine($"Result: {result}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            } while (true);

            Console.WriteLine("Goodbye!");
        }

        static void ShowHelp()
        {
            Console.WriteLine("Available commands:");
            Console.WriteLine("  help       - Show this help message");
            Console.WriteLine("  functions  - List available functions");
            Console.WriteLine("  variables  - List current variables");
            Console.WriteLine("  clear      - Clear all variables");
            Console.WriteLine("  exit       - Exit the program");
            Console.WriteLine();
            Console.WriteLine("Script examples:");
            Console.WriteLine("  x = 10");
            Console.WriteLine("  y = x + 5");
            Console.WriteLine("  print(y)");
            Console.WriteLine("  if (x > 5) { print(\"x is greater than 5\") }");
            Console.WriteLine();
        }

        static void ShowFunctions(HobScriptEngine engine)
        {
            Console.WriteLine("Available functions:");
            foreach (var funcName in engine.GetAvailableFunctions())
            {
                Console.WriteLine($"  {funcName}");
            }
            Console.WriteLine();
        }

        static void ShowVariables(HobScriptEngine engine)
        {
            var variables = engine.GetVariableNames();
            if (!variables.Any())
            {
                Console.WriteLine("No variables defined.");
            }
            else
            {
                Console.WriteLine("Current variables:");
                foreach (var varName in variables)
                {
                    var value = engine.GetVariable(varName);
                    Console.WriteLine($"  {varName} = {value}");
                }
            }
            Console.WriteLine();
        }
    }
}
