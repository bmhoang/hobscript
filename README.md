# HobScript

A powerful C# script engine library that can execute C#-like scripts with custom functions, variable storage, file loading, and import capabilities. The library is compatible with both .NET Core and .NET Framework.

## Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Basic Usage](#basic-usage)
- [Variable Management](#variable-management)
- [Custom Functions](#custom-functions)
- [File Loading and Imports](#file-loading-and-imports)
- [Script-Defined Functions](#script-defined-functions)
- [Script Contexts](#script-contexts)
- [Built-in Functions](#built-in-functions)
- [Runtime Execution](#runtime-execution)
- [Async and Parallel Execution](#async-and-parallel-execution)
- [Error Handling](#error-handling)
- [Advanced Features](#advanced-features)
- [Examples](#examples)
- [API Reference](#api-reference)
- [Requirements](#requirements)
- [License](#license)

## Documentation

- **[Quick Start Guide](QUICK_START.md)** - Get up and running in minutes
- **[Complete Examples](EXAMPLES.md)** - Comprehensive examples for all features
- **[API Documentation](API_DOCUMENTATION.md)** - Detailed API reference

## Features

- **C#-like Syntax**: Execute scripts using C# keywords and syntax
- **Custom Functions**: Register your own functions for use in scripts
- **Variable Storage**: Built-in variable storage system for reading and writing values
- **File Loading**: Load and execute scripts from files
- **Import System**: Import other script files with circular dependency detection
- **Script-Defined Functions**: Define functions directly within scripts
- **Control Structures**: Support for if/else, while loops, and for loops (coming soon)
- **Math Functions**: Built-in mathematical functions (sqrt, pow, sin, cos, etc.)
- **String Functions**: String manipulation functions (length, upper, lower, trim)
- **Global Variables**: Persistent variables across script executions
- **Script Contexts**: Isolated execution contexts
- **Runtime Execution**: Load scripts first, then execute additional code
- **Cross-Platform**: Works with both .NET Core and .NET Framework
- **Type Safety**: Full type information and safe variable access

## Installation

The library is a .NET Standard 2.0 library that can be used in both .NET Core and .NET Framework projects.

### .NET Core / .NET 5+
```xml
<PackageReference Include="HobScript" Version="1.0.0" />
```

### .NET Framework
```xml
<PackageReference Include="HobScript" Version="1.0.0" />
```

## Quick Start

```csharp
using HobScript;

// Create a new script engine
var engine = new HobScriptEngine();

// Execute a simple script
var result = engine.Execute(@"
    x = 10
    y = 20
    z = x + y
    print(z)
");

// Read variables after execution
var x = engine.GetVariable("x");  // Returns: 10
var y = engine.GetVariable("y");  // Returns: 20
var z = engine.GetVariable("z");  // Returns: 30
```

## Basic Usage

### Variable Assignment and Arithmetic

```csharp
var engine = new HobScriptEngine();

var script = @"
    x = 10
    y = 20
    z = x + y
    print(z)  // Output: 30
";

engine.Execute(script);
```

### String Operations

```csharp
var script = @"
    name = ""HobScript""
    greeting = ""Hello "" + name + ""!""
    print(greeting)  // Output: Hello HobScript!
    print(length(greeting))  // Output: 13
";

engine.Execute(script);
```

### Arithmetic Expressions

```csharp
var script = @"
    a = 10
    b = 5
    result = a + b * 2 - 3
    print(result)  // Output: 17
";

engine.Execute(script);
```

## Variable Management

### Reading Variables

```csharp
var engine = new HobScriptEngine();

engine.Execute(@"
    user_id = 12345
    user_name = ""Alice Johnson""
    user_age = 28
    user_salary = 75000.50
    is_employee = true
");

// Read individual variables
var userId = engine.GetVariable("user_id");        // Returns: 12345
var userName = engine.GetVariable("user_name");    // Returns: "Alice Johnson"
var userAge = engine.GetVariable("user_age");      // Returns: 28
var userSalary = engine.GetVariable("user_salary"); // Returns: 75000.5
var isEmployee = engine.GetVariable("is_employee"); // Returns: true

// List all variables
var variableNames = engine.GetVariableNames();
foreach (var varName in variableNames)
{
    var value = engine.GetVariable(varName);
    Console.WriteLine($"{varName} = {value} ({value?.GetType().Name})");
}
```

### Setting Variables

```csharp
var engine = new HobScriptEngine();

// Set variables from C#
engine.SetVariable("x", 10);
engine.SetVariable("name", "HobScript");
engine.SetVariable("isActive", true);

// Use in script
engine.Execute(@"
    print(x)        // Output: 10
    print(name)     // Output: HobScript
    print(isActive) // Output: True
");
```

### Type-Safe Variable Reading

```csharp
var engine = new HobScriptEngine();
engine.Execute("age = 25; salary = 50000.0; name = \"John\"");

// Type-safe reading
var age = engine.GetVariable("age");
if (age is int intAge)
{
    Console.WriteLine($"Age: {intAge}");
}

var salary = engine.GetVariable("salary");
if (salary is double doubleSalary)
{
    Console.WriteLine($"Salary: {doubleSalary:C}");
}
```

### Global Variables

```csharp
var engine = new HobScriptEngine();

// Set global variables that persist across script executions
engine.SetGlobalVariable("appName", "HobScript");
engine.SetGlobalVariable("version", "1.0.0");

var script1 = @"
    print(""Application: "" + appName)
    print(""Version: "" + version)
    userCount = 100
";

engine.Execute(script1);

var script2 = @"
    print(""User count: "" + userCount)  // userCount is still available
    print(""App: "" + appName)
";

engine.Execute(script2);
```

## Custom Functions

### Registering Simple Functions

```csharp
var engine = new HobScriptEngine();

// Register custom functions
engine.RegisterFunction("add", new Func<int, int, int>((a, b) => a + b), "Adds two numbers");
engine.RegisterFunction("greet", new Action<string>(name => Console.WriteLine($"Hello, {name}!")), "Greets a person");
engine.RegisterFunction("multiply", new Func<double, double, double>((a, b) => a * b), "Multiplies two numbers");

var script = @"
    result = add(5, 3)
    print(result)  // Output: 8
    greet(""World"")  // Output: Hello, World!
    product = multiply(4.5, 2.0)
    print(product)  // Output: 9
";

engine.Execute(script);
```

### Registering Object Methods

```csharp
public class MathUtils
{
    public int Add(int a, int b) => a + b;
    public int Multiply(int a, int b) => a * b;
    public double Power(double baseValue, double exponent) => Math.Pow(baseValue, exponent);
    public double Sqrt(double value) => Math.Sqrt(value);
}

var engine = new HobScriptEngine();
var mathUtils = new MathUtils();

// Register all methods from an object
engine.RegisterObject(mathUtils, "Math");

var script = @"
    result1 = Math.Add(10, 20)
    result2 = Math.Multiply(5, 6)
    result3 = Math.Power(2, 8)
    result4 = Math.Sqrt(16)
    print(result1)  // Output: 30
    print(result2)  // Output: 30
    print(result3)  // Output: 256
    print(result4)  // Output: 4
";

engine.Execute(script);
```

### Registering Static Methods

```csharp
var engine = new HobScriptEngine();

// Register all static methods from a type
engine.RegisterType(typeof(Math), "Math");

var script = @"
    result = Math.Sqrt(16)
    print(result)  // Output: 4
";

engine.Execute(script);
```

## File Loading and Imports

### Loading Script Files

```csharp
var engine = new HobScriptEngine();

// Load and execute a script file
var result = engine.ExecuteFile("my_script.hob");

// Load with custom working directory
var result2 = engine.ExecuteFile("scripts/main.hob", "/path/to/scripts");
```

### Import System

Create a utility script file `math_utils.hob`:
```hobscript
// Math utility functions
pi = 3.14159
e = 2.71828

// Function to calculate area of circle
function calculate_circle_area(radius)
{
    return pi * radius * radius
}

// Function to calculate factorial
function factorial(n)
{
    if (n <= 1)
        return 1
    else
        return n * factorial(n - 1)
}
```

Create a main script `main.hob`:
```hobscript
// Import utility functions
import "math_utils.hob"

print("=== Math Utilities Test ===")
radius = 10
area = calculate_circle_area(radius)
print("Area of circle with radius " + radius + ": " + area)

num = 5
fact = factorial(num)
print("Factorial of " + num + ": " + fact)
```

Execute the main script:
```csharp
var engine = new HobScriptEngine();
engine.ExecuteFile("main.hob");
```

### File Caching

```csharp
var engine = new HobScriptEngine();

// Files are automatically cached for performance
engine.ExecuteFile("large_script.hob");

// Clear cache if needed
engine.ClearFileCache();
```

## Script-Defined Functions

### Defining Functions in Scripts

```csharp
var engine = new HobScriptEngine();

var script = @"
    // Define a function
    function greet(name)
    {
        return ""Hello, "" + name + ""!""
    }
    
    // Define a function with multiple parameters
    function calculate_tax(amount, rate)
    {
        return amount * rate / 100
    }
    
    // Use the functions
    greeting = greet(""World"")
    print(greeting)
    
    tax = calculate_tax(100, 10)
    print(""Tax: $"" + tax)
";

engine.Execute(script);
```

### Function Parameters and Return Values

```csharp
var script = @"
    // Function with no parameters
    function get_current_time()
    {
        return ""2024-01-15 10:30:00""
    }
    
    // Function with one parameter
    function square(x)
    {
        return x * x
    }
    
    // Function with multiple parameters
    function calculate_rectangle_area(width, height)
    {
        return width * height
    }
    
    // Use the functions
    time = get_current_time()
    print(""Current time: "" + time)
    
    squared = square(5)
    print(""5 squared: "" + squared)
    
    area = calculate_rectangle_area(10, 20)
    print(""Rectangle area: "" + area)
";

engine.Execute(script);
```

## Script Contexts

Create isolated execution contexts for different scripts:

```csharp
var engine = new HobScriptEngine();

// Create isolated contexts
var context1 = engine.CreateContext("Context1");
var context2 = engine.CreateContext("Context2");

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

context1.Execute(script1);  // Variables are isolated
context2.Execute(script2);  // Variables are isolated

// Read variables from specific contexts
var x1 = context1.GetVariable("x");  // Returns: 10
var x2 = context2.GetVariable("x");  // Returns: 100
```

## Built-in Functions

### Math Functions

- `abs(x)` - Absolute value
- `max(a, b)` - Maximum of two numbers
- `min(a, b)` - Minimum of two numbers
- `sqrt(x)` - Square root
- `pow(x, y)` - Power function
- `sin(x)`, `cos(x)`, `tan(x)` - Trigonometric functions
- `floor(x)`, `ceil(x)`, `round(x)` - Rounding functions

### String Functions

- `length(s)` - String length
- `upper(s)` - Convert to uppercase
- `lower(s)` - Convert to lowercase
- `trim(s)` - Remove leading/trailing whitespace

### Utility Functions

- `print(x)` - Print to console
- `read()` - Read from console
- `type(x)` - Get type name
- `isNull(x)` - Check if null

### Example Usage

```csharp
var script = @"
    // Math functions
    result1 = sqrt(16)
    result2 = pow(2, 3)
    result3 = max(10, 20)
    print(result1)  // Output: 4
    print(result2)  // Output: 8
    print(result3)  // Output: 20
    
    // String functions
    text = ""  Hello World  ""
    print(length(text))  // Output: 15
    print(upper(text))   // Output:   HELLO WORLD  
    print(trim(text))    // Output: Hello World
";

engine.Execute(script);
```

## Runtime Execution

### Load Script First, Then Execute More

```csharp
var engine = new HobScriptEngine();

// Step 1: Load initial script (defines functions and variables)
engine.ExecuteFile("initial_functions.hob");

// Step 2: Execute additional code at runtime
var runtimeScript = @"
    // Use the loaded functions
    greeting = greet(""World"")
    print(greeting)
    
    // Calculate some values
    tax1 = calculate_tax(100, 10)
    tax2 = calculate_tax(250, 15)
    
    print(""Tax on $100 at 10%: $"" + tax1)
    print(""Tax on $250 at 15%: $"" + tax2)
    
    // Create new variables
    total_tax = tax1 + tax2
    print(""Total tax: $"" + total_tax)
";

engine.Execute(runtimeScript);

// Step 3: Access variables from C#
var totalTax = engine.GetVariable("total_tax");
Console.WriteLine($"Total tax from script: ${totalTax}");
```

## Async and Parallel Execution

Run scripts asynchronously and in parallel by using the async APIs. Create separate engine instances per concurrent script.

### Execute script strings asynchronously

```csharp
var e1 = new HobScriptEngine();
var e2 = new HobScriptEngine();

var t1 = e1.ExecuteAsync("a = 1 + 2\na");
var t2 = e2.ExecuteAsync("b = 10 * 3\nb");

var results = await Task.WhenAll(t1, t2);
// results[0] == 3, results[1] == 30
```

### Execute files asynchronously

```csharp
var e1 = new ScriptEngine();
var e2 = new ScriptEngine();

var t1 = e1.ExecuteFileAsync("main_script.hob");
var t2 = e2.ExecuteFileAsync("runtime_execution_example.hob");

await Task.WhenAll(t1, t2);
```

### Cancellation support

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

var engine = new HobScriptEngine();
await engine.ExecuteAsync("x = 42\nx", cts.Token);
```

Notes:
- Async methods wrap execution with Task.Run and use async I/O for files.
- Use separate engine instances for parallel runs; engines are not thread-safe.

### Interactive Script Execution

```csharp
var engine = new HobScriptEngine();

// Load base functions
engine.ExecuteFile("base_functions.hob");

// Interactive execution
while (true)
{
    Console.Write("HobScript> ");
    var input = Console.ReadLine();
    
    if (input?.ToLower() == "exit")
        break;
    
    try
    {
        var result = engine.Execute(input);
        if (result != null)
            Console.WriteLine($"Result: {result}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}
```

## Error Handling

### Script Exceptions

```csharp
try
{
    var result = engine.Execute("invalid syntax");
}
catch (ScriptException ex)
{
    Console.WriteLine($"Script error: {ex.Message}");
    Console.WriteLine($"Line: {ex.LineNumber}");
}
```

### Function Not Found

```csharp
try
{
    engine.Execute("result = unknown_function(10)");
}
catch (ScriptException ex)
{
    Console.WriteLine($"Function not found: {ex.Message}");
}
```

### Variable Access Errors

```csharp
try
{
    var value = engine.GetVariable("nonexistent_variable");
    if (value == null)
    {
        Console.WriteLine("Variable not found");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error accessing variable: {ex.Message}");
}
```

## Advanced Features

### Function Registry

```csharp
var engine = new HobScriptEngine();

// Get all registered functions
var functions = engine.GetRegisteredFunctions();
foreach (var func in functions)
{
    Console.WriteLine($"{func.Name}: {func.Description}");
}

// Check if function exists
if (engine.HasFunction("add"))
{
    Console.WriteLine("Add function is available");
}
```

### Script Context Management

```csharp
var engine = new HobScriptEngine();

// Create and manage multiple contexts
var contexts = new List<ScriptContext>();
for (int i = 0; i < 5; i++)
{
    contexts.Add(engine.CreateContext($"Context{i}"));
}

// Execute different scripts in different contexts
for (int i = 0; i < contexts.Count; i++)
{
    contexts[i].Execute($"x = {i * 10}; print(\"Context {i}: x = \" + x)");
}
```

### File Management

```csharp
var engine = new HobScriptEngine();

// Load multiple files
engine.ExecuteFile("config.hob");
engine.ExecuteFile("functions.hob");
engine.ExecuteFile("main.hob");

// Clear file cache
engine.ClearFileCache();

// Check if file is loaded
if (engine.IsFileLoaded("config.hob"))
{
    Console.WriteLine("Config file is loaded");
}
```

## Examples

### Complete Example Application

```csharp
using HobScript;

class Program
{
    static void Main()
    {
        var engine = new HobScriptEngine();
        
        // Register custom functions
        engine.RegisterFunction("log", new Action<string>(Console.WriteLine), "Log message");
        engine.RegisterFunction("add", new Func<int, int, int>((a, b) => a + b), "Add two numbers");
        
        // Load configuration
        engine.ExecuteFile("config.hob");
        
        // Execute main script
        var result = engine.Execute(@"
            app_name = ""My Application""
            version = ""1.0.0""
            
            log(""Starting "" + app_name + "" v"" + version)
            
            // Process some data
            data = [1, 2, 3, 4, 5]
            sum = 0
            
            for (i = 0; i < length(data); i = i + 1)
            {
                sum = add(sum, data[i])
            }
            
            log(""Sum: "" + sum)
            log(""Average: "" + (sum / length(data)))
        ");
        
        // Read results
        var appName = engine.GetVariable("app_name");
        var version = engine.GetVariable("version");
        var sum = engine.GetVariable("sum");
        
        Console.WriteLine($"Application: {appName} {version}");
        Console.WriteLine($"Total sum: {sum}");
    }
}
```

## API Reference

### HobScriptEngine Class

#### Methods

| Method | Description | Parameters | Returns |
|--------|-------------|------------|---------|
| `Execute(string script)` | Execute a script string | `script` - The script to execute | `object` - Last expression result |
| `ExecuteFile(string filePath)` | Load and execute a script file | `filePath` - Path to script file | `object` - Last expression result |
| `ExecuteFile(string filePath, string workingDirectory)` | Load and execute a script file with working directory | `filePath` - Path to script file, `workingDirectory` - Working directory | `object` - Last expression result |
| `GetVariable(string name)` | Get a variable value | `name` - Variable name | `object` - Variable value |
| `SetVariable(string name, object value)` | Set a variable value | `name` - Variable name, `value` - Variable value | `void` |
| `GetVariableNames()` | Get all variable names | None | `IEnumerable<string>` - Variable names |
| `HasVariable(string name)` | Check if variable exists | `name` - Variable name | `bool` - True if exists |
| `SetGlobalVariable(string name, object value)` | Set a global variable | `name` - Variable name, `value` - Variable value | `void` |
| `GetGlobalVariable(string name)` | Get a global variable | `name` - Variable name | `object` - Variable value |
| `RegisterFunction(string name, Delegate function, string description)` | Register a custom function | `name` - Function name, `function` - Function delegate, `description` - Function description | `void` |
| `RegisterObject(object obj, string prefix)` | Register all methods from an object | `obj` - Object instance, `prefix` - Method prefix | `void` |
| `RegisterType(Type type, string prefix)` | Register all static methods from a type | `type` - Type, `prefix` - Method prefix | `void` |
| `CreateContext(string name)` | Create a script context | `name` - Context name | `ScriptContext` - New context |
| `GetRegisteredFunctions()` | Get all registered functions | None | `IEnumerable<FunctionInfo>` - Function information |
| `HasFunction(string name)` | Check if function exists | `name` - Function name | `bool` - True if exists |
| `ClearFileCache()` | Clear file cache | None | `void` |
| `IsFileLoaded(string filePath)` | Check if file is loaded | `filePath` - File path | `bool` - True if loaded |

### ScriptContext Class

#### Methods

| Method | Description | Parameters | Returns |
|--------|-------------|------------|---------|
| `Execute(string script)` | Execute a script in this context | `script` - The script to execute | `object` - Last expression result |
| `GetVariable(string name)` | Get a variable value from this context | `name` - Variable name | `object` - Variable value |
| `SetVariable(string name, object value)` | Set a variable value in this context | `name` - Variable name, `value` - Variable value | `void` |
| `GetVariableNames()` | Get all variable names in this context | None | `IEnumerable<string>` - Variable names |

### FunctionInfo Class

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Name` | `string` | Function name |
| `Description` | `string` | Function description |
| `ParameterCount` | `int` | Number of parameters |
| `ReturnType` | `Type` | Return type |

## Requirements

- .NET 6.0 or .NET Framework 4.8
- C# 9.0 or later

## License

This project is open source and available under the MIT License.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Support

For support, please open an issue on the GitHub repository.