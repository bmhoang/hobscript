# HobScript Quick Start Guide

Get up and running with HobScript in minutes!

## Installation

### .NET Core / .NET 5+
```bash
dotnet add package HobScript
```

### .NET Framework
```bash
Install-Package HobScript
```

## 1. Basic Script Execution

```csharp
using HobScript;

var engine = new HobScriptEngine();

// Execute a simple script
var result = engine.Execute(@"
    x = 10
    y = 20
    z = x + y
    print(z)  // Output: 30
");

// Read variables after execution
var x = engine.GetVariable("x");  // Returns: 10
var y = engine.GetVariable("y");  // Returns: 20
var z = engine.GetVariable("z");  // Returns: 30
```

## 2. Custom Functions

```csharp
var engine = new HobScriptEngine();

// Register a custom function
engine.RegisterFunction("add", new Func<int, int, int>((a, b) => a + b));

// Use in script
engine.Execute(@"
    result = add(5, 3)
    print(result)  // Output: 8
");
```

## 3. File Loading

Create a script file `hello.hob`:
```hobscript
name = "HobScript"
print("Hello, " + name + "!")
```

Load and execute:
```csharp
var engine = new HobScriptEngine();
engine.ExecuteFile("hello.hob");  // Output: Hello, HobScript!
```

## 4. Script-Defined Functions

```csharp
var engine = new HobScriptEngine();

engine.Execute(@"
    // Define a function
    function greet(name)
    {
        return "Hello, " + name + "!"
    }
    
    // Use the function
    message = greet("World")
    print(message)  // Output: Hello, World!
");
```

## 5. Import System

Create `math_utils.hob`:
```hobscript
function add(a, b)
{
    return a + b
}

function multiply(a, b)
{
    return a * b
}
```

Create `main.hob`:
```hobscript
import "math_utils.hob"

result1 = add(10, 20)
result2 = multiply(5, 6)
print("Sum: " + result1)      // Output: Sum: 30
print("Product: " + result2)  // Output: Product: 30
```

Execute:
```csharp
var engine = new HobScriptEngine();
engine.ExecuteFile("main.hob");
```

## 6. Runtime Execution

```csharp
var engine = new HobScriptEngine();

// Load initial script
engine.ExecuteFile("base_functions.hob");

// Execute additional code at runtime
engine.Execute(@"
    result = calculate_something(10, 20)
    print("Result: " + result)
");

// Access variables from C#
var result = engine.GetVariable("result");
Console.WriteLine($"Script result: {result}");
```

## 7. Object Registration

```csharp
public class MathUtils
{
    public int Add(int a, int b) => a + b;
    public int Multiply(int a, int b) => a * b;
}

var engine = new HobScriptEngine();
var mathUtils = new MathUtils();

// Register object methods
engine.RegisterObject(mathUtils, "Math");

engine.Execute(@"
    result = Math.Add(10, 20)
    print(result)  // Output: 30
");
```

## 8. Script Contexts

```csharp
var engine = new HobScriptEngine();

// Create isolated contexts
var context1 = engine.CreateContext("Context1");
var context2 = engine.CreateContext("Context2");

context1.Execute("x = 10");
context2.Execute("x = 20");

// Variables are isolated
var x1 = context1.GetVariable("x");  // Returns: 10
var x2 = context2.GetVariable("x");  // Returns: 20
```

## 9. Error Handling

```csharp
var engine = new HobScriptEngine();

try
{
    engine.Execute("invalid syntax");
}
catch (ScriptException ex)
{
    Console.WriteLine($"Script error: {ex.Message}");
    Console.WriteLine($"Line: {ex.LineNumber}");
}
```

## 10. Complete Example

```csharp
using HobScript;

class Program
{
    static void Main()
    {
        var engine = new HobScriptEngine();
        
        // Register custom functions
        engine.RegisterFunction("log", new Action<string>(Console.WriteLine));
        engine.RegisterFunction("add", new Func<int, int, int>((a, b) => a + b));
        
        // Load configuration
        engine.ExecuteFile("config.hob");
        
        // Execute main script
        engine.Execute(@"
            app_name = "My Application"
            version = "1.0.0"
            
            log("Starting " + app_name + " v" + version)
            
            // Process some data
            sum = add(10, 20)
            log("Sum: " + sum)
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

## Next Steps

- Read the [full documentation](README.md) for detailed features
- Check the [API reference](API_DOCUMENTATION.md) for complete method listings
- Run the examples: `dotnet run --project HobScript.Console.csproj`

## Common Patterns

### Configuration Loading
```csharp
var engine = new HobScriptEngine();
engine.ExecuteFile("config.hob");
var dbConnection = engine.GetVariable("database_connection");
var apiKey = engine.GetVariable("api_key");
```

### Dynamic Calculations
```csharp
var engine = new HobScriptEngine();
engine.RegisterFunction("calculate", new Func<double, double, double>((x, y) => x * y + 10));

var result = engine.Execute("result = calculate(5, 3)");
var value = engine.GetVariable("result"); // Returns: 25
```

### User Input Processing
```csharp
var engine = new HobScriptEngine();
engine.RegisterFunction("read", new Func<string>(() => Console.ReadLine()));

engine.Execute(@"
    print("Enter your name:")
    name = read()
    print("Hello, " + name + "!")
");
```

### Batch Processing
```csharp
var engine = new HobScriptEngine();
engine.ExecuteFile("processing_functions.hob");

foreach (var item in data)
{
    engine.SetVariable("current_item", item);
    engine.Execute("result = process_item(current_item)");
    var result = engine.GetVariable("result");
    // Process result...
}
```

Happy scripting with HobScript! 🚀
