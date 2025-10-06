using System;
using System.Collections.Generic;

namespace HobScript
{
    /// <summary>
    /// Example usage of the HobScript engine
    /// </summary>
    public class Example
    {
        public static void RunBasicExample()
        {
            System.Console.WriteLine("=== Basic HobScript Example ===");
            
            var engine = new HobScriptEngine();
            
            // Basic variable assignment and arithmetic
            var script1 = @"
                x = 10
                y = 20
                z = x + y
                print(z)
            ";
            
            System.Console.WriteLine("Executing: Basic arithmetic");
            engine.Execute(script1);
            
            // String operations
            var script2 = @"
                name = ""HobScript""
                greeting = ""Hello "" + name + ""!""
                print(greeting)
                print(length(greeting))
            ";
            
            System.Console.WriteLine("\nExecuting: String operations");
            engine.Execute(script2);
            
            // Function calls
            var script3 = @"
                result = sqrt(16)
                print(result)
                print(max(10, 20))
                print(min(10, 20))
            ";
            
            System.Console.WriteLine("\nExecuting: Math functions");
            engine.Execute(script3);
        }

        public static void RunAdvancedExample()
        {
            System.Console.WriteLine("\n=== Advanced HobScript Example ===");
            
            var engine = new HobScriptEngine();
            
            // Control structures
            var script1 = @"
                x = 5
                if (x > 3) {
                    print(""x is greater than 3"")
                }
                
                for (i = 1; i <= 5; i = i + 1) {
                    print(i)
                }
                
                counter = 0
                while (counter < 3) {
                    print(""Counter: "" + counter)
                    counter = counter + 1
                }
            ";
            
            System.Console.WriteLine("Executing: Control structures");
            engine.Execute(script1);
        }

        public static void RunCustomFunctionExample()
        {
            System.Console.WriteLine("\n=== Custom Function Example ===");
            
            var engine = new HobScriptEngine();
            
            // Register custom functions
            engine.RegisterFunction("add", new Func<int, int, int>((a, b) => a + b), "Adds two numbers");
            engine.RegisterFunction("multiply", new Func<int, int, int>((a, b) => a * b), "Multiplies two numbers");
            engine.RegisterFunction("greet", new Action<string>(name => System.Console.WriteLine($"Hello, {name}!")), "Greets a person");
            
            var script = @"
                result1 = add(5, 3)
                result2 = multiply(4, 6)
                print(result1)
                print(result2)
                greet(""World"")
            ";
            
            System.Console.WriteLine("Executing: Custom functions");
            engine.Execute(script);
        }

        public static void RunObjectRegistrationExample()
        {
            System.Console.WriteLine("\n=== Object Registration Example ===");
            
            var engine = new HobScriptEngine();
            
            // Create a custom object
            var mathUtils = new MathUtils();
            engine.RegisterObject(mathUtils, "Math");
            
            var script = @"
                result1 = Math.Add(10, 20)
                result2 = Math.Multiply(5, 6)
                result3 = Math.Power(2, 8)
                print(result1)
                print(result2)
                print(result3)
            ";
            
            System.Console.WriteLine("Executing: Object methods");
            engine.Execute(script);
        }

        public static void RunGlobalVariablesExample()
        {
            System.Console.WriteLine("\n=== Global Variables Example ===");
            
            var engine = new HobScriptEngine();
            
            // Set global variables
            engine.SetGlobalVariable("appName", "HobScript");
            engine.SetGlobalVariable("version", "1.0.0");
            
            var script1 = @"
                print(""Application: "" + appName)
                print(""Version: "" + version)
                userCount = 100
            ";
            
            System.Console.WriteLine("Executing: First script with global variables");
            engine.Execute(script1);
            
            var script2 = @"
                print(""User count: "" + userCount)
                print(""App: "" + appName)
            ";
            
            System.Console.WriteLine("Executing: Second script (userCount should be available)");
            engine.Execute(script2);
        }

        public static void RunContextExample()
        {
            System.Console.WriteLine("\n=== Script Context Example ===");
            
            var engine = new HobScriptEngine();
            
            // Create isolated contexts
            var context1 = engine.CreateContext();
            var context2 = engine.CreateContext();
            
            var script1 = @"
                x = 10
                y = 20
                print(""Context 1 - x: "" + x + "", y: "" + y)
            ";
            
            var script2 = @"
                x = 100
                y = 200
                print(""Context 2 - x: "" + x + "", y: "" + y)
            ";
            
            System.Console.WriteLine("Executing: Context 1");
            context1.Execute(script1);
            
            System.Console.WriteLine("Executing: Context 2");
            context2.Execute(script2);
            
            System.Console.WriteLine("Executing: Context 1 again (variables should be preserved)");
            context1.Execute("print(\"Context 1 - x: \" + x + \", y: \" + y)");
        }

        public static void RunAllExamples()
        {
            try
            {
                RunBasicExample();
                RunAdvancedExample();
                RunCustomFunctionExample();
                RunObjectRegistrationExample();
                RunGlobalVariablesExample();
                RunContextExample();
                RunFileLoadingExample();
                RunRuntimeExecutionExample();
                RunVariableReadingExample();
                
                System.Console.WriteLine("\n=== All examples completed successfully! ===");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error running examples: {ex.Message}");
            }
        }

        /// <summary>
        /// Demonstrates file loading and import functionality
        /// </summary>
        public static void RunFileLoadingExample()
        {
            System.Console.WriteLine("\n=== File Loading Example ===");
            
            var engine = new HobScriptEngine();
            
            try
            {
                System.Console.WriteLine("Executing: Simple file loading");
                var result = engine.ExecuteFile("test_file_loading.hob");
                System.Console.WriteLine($"File execution result: {result}");
                
                System.Console.WriteLine("\nExecuting: Simple test without functions");
                engine.ExecuteFile("simple_test.hob");
                
                System.Console.WriteLine("\nExecuting: File with imports");
                engine.ExecuteFile("main_script.hob");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error running file loading examples: {ex.Message}");
            }
        }

        /// <summary>
        /// Demonstrates loading a script first, then executing additional code at runtime
        /// </summary>
        public static void RunRuntimeExecutionExample()
        {
            System.Console.WriteLine("\n=== Runtime Execution Example ===");
            
            var engine = new HobScriptEngine();
            
            try
            {
                // Step 1: Load the initial script (defines functions and variables)
                System.Console.WriteLine("Step 1: Loading initial script...");
                engine.ExecuteFile("runtime_execution_example.hob");
                System.Console.WriteLine("Initial script loaded successfully!");
                
                // Step 2: Execute additional code at runtime using the loaded functions and variables
                System.Console.WriteLine("\nStep 2: Executing runtime code...");
                
                // Use the loaded variables
                var companyName = engine.GetVariable("company_name");
                var version = engine.GetVariable("version");
                System.Console.WriteLine($"Company: {companyName}, Version: {version}");
                
                // Execute additional script code that uses the loaded functions
                var runtimeScript = @"
                    // Use the loaded functions
                    greeting = greet(""World"")
                    print(greeting)
                    
                    // Calculate some taxes
                    tax1 = calculate_tax(100, 10)
                    tax2 = calculate_tax(250, 15)
                    
                    print(""Tax on $100 at 10%: $"" + tax1)
                    print(""Tax on $250 at 15%: $"" + tax2)
                    
                    // Create some new variables
                    total_tax = tax1 + tax2
                    print(""Total tax: $"" + total_tax)
                ";
                
                engine.Execute(runtimeScript);
                
                // Step 3: Show that variables persist between executions
                System.Console.WriteLine("\nStep 3: Variables persist between executions...");
                var totalTax = engine.GetVariable("total_tax");
                System.Console.WriteLine($"Total tax from previous execution: ${totalTax}");
                
                // Execute more runtime code
                var moreRuntimeScript = @"
                    // Use existing variables and create new ones
                    discount = total_tax * 0.1
                    final_amount = total_tax - discount
                    
                    print(""Discount (10%): $"" + discount)
                    print(""Final amount: $"" + final_amount)
                ";
                
                engine.Execute(moreRuntimeScript);
                
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error in runtime execution example: {ex.Message}");
            }
        }

        /// <summary>
        /// Demonstrates different ways to read variables after script execution
        /// </summary>
        public static void RunVariableReadingExample()
        {
            System.Console.WriteLine("\n=== Variable Reading Example ===");
            
            var engine = new HobScriptEngine();
            
            try
            {
                // Method 1: Execute script and read individual variables
                System.Console.WriteLine("Method 1: Reading individual variables...");
                engine.Execute(@"
                    user_id = 12345
                    user_name = ""Alice Johnson""
                    user_age = 28
                    user_salary = 75000.50
                    is_employee = true
                ");
                
                var userId = engine.GetVariable("user_id");
                var userName = engine.GetVariable("user_name");
                var userAge = engine.GetVariable("user_age");
                var userSalary = engine.GetVariable("user_salary");
                var isEmployee = engine.GetVariable("is_employee");
                
                System.Console.WriteLine($"User ID: {userId} ({userId?.GetType().Name})");
                System.Console.WriteLine($"User Name: {userName} ({userName?.GetType().Name})");
                System.Console.WriteLine($"User Age: {userAge} ({userAge?.GetType().Name})");
                System.Console.WriteLine($"User Salary: {userSalary} ({userSalary?.GetType().Name})");
                System.Console.WriteLine($"Is Employee: {isEmployee} ({isEmployee?.GetType().Name})");
                
                // Method 2: List all variables
                System.Console.WriteLine("\nMethod 2: Listing all variables...");
                var variableNames = engine.GetVariableNames();
                System.Console.WriteLine("All variables in engine:");
                foreach (var varName in variableNames)
                {
                    var value = engine.GetVariable(varName);
                    System.Console.WriteLine($"  {varName} = {value} ({value?.GetType().Name})");
                }
                
                // Method 3: Load from file and read variables
                System.Console.WriteLine("\nMethod 3: Loading from file and reading variables...");
                engine.ExecuteFile("variable_reading_example.hob");
                
                var fileUserId = engine.GetVariable("user_id");
                var fileUserName = engine.GetVariable("user_name");
                var fileUserEmail = engine.GetVariable("user_email");
                var fileDepartment = engine.GetVariable("department");
                
                System.Console.WriteLine($"From file - User ID: {fileUserId}");
                System.Console.WriteLine($"From file - User Name: {fileUserName}");
                System.Console.WriteLine($"From file - User Email: {fileUserEmail}");
                System.Console.WriteLine($"From file - Department: {fileDepartment}");
                
                // Method 4: Read variables with type safety
                System.Console.WriteLine("\nMethod 4: Type-safe variable reading...");
                var count = engine.GetVariable("user_age");
                if (count is int intCount)
                {
                    System.Console.WriteLine($"Age as integer: {intCount}");
                }
                
                var salary = engine.GetVariable("user_salary");
                if (salary is double doubleSalary)
                {
                    System.Console.WriteLine($"Salary as double: {doubleSalary:C}");
                }
                
                var name = engine.GetVariable("user_name");
                if (name is string stringName)
                {
                    System.Console.WriteLine($"Name as string: {stringName}");
                }
                
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error in variable reading example: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Sample utility class for object registration example
    /// </summary>
    public class MathUtils
    {
        public int Add(int a, int b) => a + b;
        public int Multiply(int a, int b) => a * b;
        public double Power(double baseValue, double exponent) => Math.Pow(baseValue, exponent);
        public double SquareRoot(double value) => Math.Sqrt(value);
        public string GetInfo() => "MathUtils v1.0";
    }
}
