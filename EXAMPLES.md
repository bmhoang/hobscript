# HobScript Examples

This document provides comprehensive examples demonstrating all features of the HobScript library.

## Table of Contents

- [Basic Examples](#basic-examples)
- [Variable Management](#variable-management)
- [Custom Functions](#custom-functions)
- [File Loading and Imports](#file-loading-and-imports)
- [Script-Defined Functions](#script-defined-functions)
- [Script Contexts](#script-contexts)
- [Runtime Execution](#runtime-execution)
- [Error Handling](#error-handling)
- [Advanced Examples](#advanced-examples)
- [Real-World Examples](#real-world-examples)

## Basic Examples

### Simple Arithmetic

```csharp
var engine = new HobScriptEngine();

var script = @"
    x = 10
    y = 20
    z = x + y
    print(z)  // Output: 30
";

engine.Execute(script);
var result = engine.GetVariable("z"); // Returns: 30
```

### String Operations

```csharp
var engine = new HobScriptEngine();

var script = @"
    name = ""HobScript""
    greeting = ""Hello "" + name + ""!""
    print(greeting)  // Output: Hello HobScript!
    print(length(greeting))  // Output: 13
";

engine.Execute(script);
```

### Complex Expressions

```csharp
var engine = new HobScriptEngine();

var script = @"
    a = 10
    b = 5
    c = 3
    result = (a + b) * c - 2
    print(result)  // Output: 43
";

engine.Execute(script);
```

## Variable Management

### Reading and Writing Variables

```csharp
var engine = new HobScriptEngine();

// Set variables from C#
engine.SetVariable("user_id", 12345);
engine.SetVariable("user_name", "Alice Johnson");
engine.SetVariable("is_active", true);

// Execute script that uses these variables
engine.Execute(@"
    print(""User ID: "" + user_id)
    print(""User Name: "" + user_name)
    print(""Active: "" + is_active)
    
    // Create new variables
    last_login = ""2024-01-15""
    login_count = 42
");

// Read all variables
var allVars = engine.GetVariableNames();
foreach (var varName in allVars)
{
    var value = engine.GetVariable(varName);
    Console.WriteLine($"{varName} = {value} ({value?.GetType().Name})");
}
```

### Type-Safe Variable Access

```csharp
var engine = new HobScriptEngine();

engine.Execute(@"
    user_id = 12345
    user_salary = 75000.50
    user_name = ""John Doe""
    is_employee = true
");

// Type-safe reading
var userId = engine.GetVariable("user_id");
if (userId is int intUserId)
{
    Console.WriteLine($"User ID (int): {intUserId}");
}

var salary = engine.GetVariable("user_salary");
if (salary is double doubleSalary)
{
    Console.WriteLine($"Salary (double): {doubleSalary:C}");
}

var name = engine.GetVariable("user_name");
if (name is string stringName)
{
    Console.WriteLine($"Name (string): {stringName}");
}

var isEmployee = engine.GetVariable("is_employee");
if (isEmployee is bool boolIsEmployee)
{
    Console.WriteLine($"Is Employee (bool): {boolIsEmployee}");
}
```

### Global Variables

```csharp
var engine = new HobScriptEngine();

// Set global variables
engine.SetGlobalVariable("app_name", "HobScript");
engine.SetGlobalVariable("version", "1.0.0");
engine.SetGlobalVariable("debug_mode", true);

// First script execution
engine.Execute(@"
    print(""Application: "" + app_name)
    print(""Version: "" + version)
    print(""Debug Mode: "" + debug_mode)
    
    // Set some local variables
    user_count = 100
    last_update = ""2024-01-15""
");

// Second script execution (global variables persist)
engine.Execute(@"
    print(""App: "" + app_name)  // Still available
    print(""Users: "" + user_count)  // Still available
    print(""Last Update: "" + last_update)  // Still available
");
```

## Custom Functions

### Simple Function Registration

```csharp
var engine = new HobScriptEngine();

// Register basic functions
engine.RegisterFunction("add", new Func<int, int, int>((a, b) => a + b), "Adds two numbers");
engine.RegisterFunction("multiply", new Func<double, double, double>((a, b) => a * b), "Multiplies two numbers");
engine.RegisterFunction("greet", new Action<string>(name => Console.WriteLine($"Hello, {name}!")), "Greets a person");
engine.RegisterFunction("log", new Action<string>(message => Console.WriteLine($"[LOG] {message}")), "Logs a message");

engine.Execute(@"
    result1 = add(10, 20)
    result2 = multiply(3.5, 2.0)
    print(""Sum: "" + result1)      // Output: Sum: 30
    print(""Product: "" + result2)  // Output: Product: 7
    greet(""World"")                // Output: Hello, World!
    log(""Script executed"")        // Output: [LOG] Script executed
");
```

### Object Method Registration

```csharp
public class MathUtils
{
    public int Add(int a, int b) => a + b;
    public int Subtract(int a, int b) => a - b;
    public int Multiply(int a, int b) => a * b;
    public double Divide(double a, double b) => a / b;
    public double Power(double baseValue, double exponent) => Math.Pow(baseValue, exponent);
    public double Sqrt(double value) => Math.Sqrt(value);
}

public class StringUtils
{
    public string Upper(string input) => input.ToUpper();
    public string Lower(string input) => input.ToLower();
    public string Reverse(string input) => new string(input.Reverse().ToArray());
    public int Length(string input) => input.Length;
}

var engine = new HobScriptEngine();
var mathUtils = new MathUtils();
var stringUtils = new StringUtils();

// Register object methods
engine.RegisterObject(mathUtils, "Math");
engine.RegisterObject(stringUtils, "String");

engine.Execute(@"
    // Math operations
    sum = Math.Add(10, 20)
    diff = Math.Subtract(50, 30)
    product = Math.Multiply(5, 6)
    quotient = Math.Divide(20, 4)
    power = Math.Power(2, 8)
    sqrt = Math.Sqrt(16)
    
    print(""Sum: "" + sum)           // Output: Sum: 30
    print(""Difference: "" + diff)   // Output: Difference: 20
    print(""Product: "" + product)   // Output: Product: 30
    print(""Quotient: "" + quotient) // Output: Quotient: 5
    print(""Power: "" + power)       // Output: Power: 256
    print(""Square Root: "" + sqrt)  // Output: Square Root: 4
    
    // String operations
    text = ""Hello World""
    upper = String.Upper(text)
    lower = String.Lower(text)
    reversed = String.Reverse(text)
    textLength = String.Length(text)
    
    print(""Original: "" + text)     // Output: Original: Hello World
    print(""Upper: "" + upper)       // Output: Upper: HELLO WORLD
    print(""Lower: "" + lower)       // Output: Lower: hello world
    print(""Reversed: "" + reversed) // Output: Reversed: dlroW olleH
    print(""Length: "" + textLength) // Output: Length: 11
");
```

### Static Method Registration

```csharp
var engine = new HobScriptEngine();

// Register static methods from System.Math
engine.RegisterType(typeof(Math), "Math");

engine.Execute(@"
    result1 = Math.Sqrt(16)
    result2 = Math.Pow(2, 3)
    result3 = Math.Max(10, 20)
    result4 = Math.Min(5, 15)
    result5 = Math.Abs(-10)
    result6 = Math.Round(3.14159, 2)
    
    print(""Square Root: "" + result1)  // Output: Square Root: 4
    print(""Power: "" + result2)        // Output: Power: 8
    print(""Max: "" + result3)          // Output: Max: 20
    print(""Min: "" + result4)          // Output: Min: 5
    print(""Absolute: "" + result5)     // Output: Absolute: 10
    print(""Rounded: "" + result6)      // Output: Rounded: 3.14
");
```

## File Loading and Imports

### Simple File Loading

Create `config.hob`:
```hobscript
// Application configuration
app_name = "HobScript Application"
version = "1.0.0"
debug_mode = true
max_users = 1000
database_url = "localhost:5432"
api_key = "abc123def456"
```

Load and use:
```csharp
var engine = new HobScriptEngine();

// Load configuration file
engine.ExecuteFile("config.hob");

// Read configuration values
var appName = engine.GetVariable("app_name");
var version = engine.GetVariable("version");
var debugMode = engine.GetVariable("debug_mode");
var maxUsers = engine.GetVariable("max_users");
var dbUrl = engine.GetVariable("database_url");
var apiKey = engine.GetVariable("api_key");

Console.WriteLine($"App: {appName} {version}");
Console.WriteLine($"Debug: {debugMode}, Max Users: {maxUsers}");
Console.WriteLine($"DB: {dbUrl}, API Key: {apiKey}");
```

### Import System

Create `math_utils.hob`:
```hobscript
// Math utility functions
pi = 3.14159
e = 2.71828

function add(a, b)
{
    return a + b
}

function subtract(a, b)
{
    return a - b
}

function multiply(a, b)
{
    return a * b
}

function divide(a, b)
{
    return a / b
}

function power(base, exp)
{
    return base ^ exp
}

function factorial(n)
{
    if (n <= 1)
        return 1
    else
        return n * factorial(n - 1)
}

function fibonacci(n)
{
    if (n <= 1)
        return n
    else
        return fibonacci(n - 1) + fibonacci(n - 2)
}
```

Create `string_utils.hob`:
```hobscript
// String utility functions
function reverse_string(s)
{
    // Simplified for demonstration
    return "reversed_" + s
}

function concatenate(s1, s2)
{
    return s1 + s2
}

function get_length(s)
{
    return length(s)
}
```

Create `main.hob`:
```hobscript
// Main script that imports utility modules
import "math_utils.hob"
import "string_utils.hob"

print("=== Math Utilities Test ===")
radius = 10
area = pi * radius * radius
print("Area of circle with radius " + radius + ": " + area)

num = 5
fact = factorial(num)
print("Factorial of " + num + ": " + fact)

fib_num = 7
fib = fibonacci(fib_num)
print("Fibonacci of " + fib_num + ": " + fib)

print("\n=== String Utilities Test ===")
str1 = "Hello"
str2 = "World"
combined = concatenate(str1, str2)
print("Concatenated string: " + combined)

original = "HobScript"
reversed = reverse_string(original)
print("Reversed string: " + reversed)
```

Execute:
```csharp
var engine = new HobScriptEngine();
engine.ExecuteFile("main.hob");
```

## Script-Defined Functions

### Basic Function Definitions

```csharp
var engine = new HobScriptEngine();

engine.Execute(@"
    // Function with no parameters
    function get_greeting()
    {
        return ""Hello, World!""
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
    
    // Function with string operations
    function format_name(first, last)
    {
        return ""Mr. "" + first + "" "" + last
    }
    
    // Use the functions
    greeting = get_greeting()
    print(greeting)  // Output: Hello, World!
    
    squared = square(5)
    print(""5 squared: "" + squared)  // Output: 5 squared: 25
    
    area = calculate_rectangle_area(10, 20)
    print(""Rectangle area: "" + area)  // Output: Rectangle area: 200
    
    full_name = format_name(""John"", ""Doe"")
    print(full_name)  // Output: Mr. John Doe
");
```

### Recursive Functions

```csharp
var engine = new HobScriptEngine();

engine.Execute(@"
    // Recursive factorial function
    function factorial(n)
    {
        if (n <= 1)
            return 1
        else
            return n * factorial(n - 1)
    }
    
    // Recursive fibonacci function
    function fibonacci(n)
    {
        if (n <= 1)
            return n
        else
            return fibonacci(n - 1) + fibonacci(n - 2)
    }
    
    // Test factorial
    for (i = 1; i <= 5; i = i + 1)
    {
        fact = factorial(i)
        print(""Factorial of "" + i + "": "" + fact)
    }
    
    // Test fibonacci
    for (i = 0; i <= 8; i = i + 1)
    {
        fib = fibonacci(i)
        print(""Fibonacci of "" + i + "": "" + fib)
    }
");
```

### Complex Function Examples

```csharp
var engine = new HobScriptEngine();

engine.Execute(@"
    // Function to calculate compound interest
    function compound_interest(principal, rate, time)
    {
        return principal * power(1 + rate, time)
    }
    
    // Function to check if a number is prime
    function is_prime(n)
    {
        if (n <= 1)
            return false
        if (n <= 3)
            return true
        if (n % 2 == 0 || n % 3 == 0)
            return false
        
        i = 5
        while (i * i <= n)
        {
            if (n % i == 0 || n % (i + 2) == 0)
                return false
            i = i + 6
        }
        return true
    }
    
    // Function to find the greatest common divisor
    function gcd(a, b)
    {
        while (b != 0)
        {
            temp = b
            b = a % b
            a = temp
        }
        return a
    }
    
    // Test compound interest
    principal = 1000
    rate = 0.05
    time = 10
    final_amount = compound_interest(principal, rate, time)
    print(""Principal: $"" + principal + "", Rate: "" + (rate * 100) + ""%, Time: "" + time + "" years"")
    print(""Final Amount: $"" + final_amount)
    
    // Test prime checking
    numbers = [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20]
    for (i = 0; i < length(numbers); i = i + 1)
    {
        num = numbers[i]
        prime = is_prime(num)
        if (prime)
            print(""Number "" + num + "" is prime"")
        else
            print(""Number "" + num + "" is not prime"")
    }
    
    // Test GCD
    a = 48
    b = 18
    gcd_result = gcd(a, b)
    print(""GCD of "" + a + "" and "" + b + "": "" + gcd_result)
");
```

## Script Contexts

### Basic Context Usage

```csharp
var engine = new HobScriptEngine();

// Create multiple contexts
var context1 = engine.CreateContext("User1");
var context2 = engine.CreateContext("User2");
var context3 = engine.CreateContext("Admin");

// Execute different scripts in different contexts
context1.Execute(@"
    user_id = 1
    user_name = ""Alice""
    balance = 1000
    print(""User1 - ID: "" + user_id + "", Name: "" + user_name + "", Balance: $"" + balance)
");

context2.Execute(@"
    user_id = 2
    user_name = ""Bob""
    balance = 2500
    print(""User2 - ID: "" + user_id + "", Name: "" + user_name + "", Balance: $"" + balance)
");

context3.Execute(@"
    admin_id = 999
    admin_name = ""Admin""
    permissions = [""read"", ""write"", ""delete""]
    print(""Admin - ID: "" + admin_id + "", Name: "" + admin_name + "", Permissions: "" + permissions)
");

// Variables are isolated between contexts
var aliceBalance = context1.GetVariable("balance");  // Returns: 1000
var bobBalance = context2.GetVariable("balance");    // Returns: 2500
var adminId = context3.GetVariable("admin_id");      // Returns: 999

Console.WriteLine($"Alice's balance: ${aliceBalance}");
Console.WriteLine($"Bob's balance: ${bobBalance}");
Console.WriteLine($"Admin ID: {adminId}");
```

### Context with Shared Functions

```csharp
var engine = new HobScriptEngine();

// Register shared functions
engine.RegisterFunction("log", new Action<string>(message => Console.WriteLine($"[LOG] {message}")));
engine.RegisterFunction("add", new Func<int, int, int>((a, b) => a + b));

// Create contexts
var context1 = engine.CreateContext("Process1");
var context2 = engine.CreateContext("Process2");

// Both contexts can use shared functions
context1.Execute(@"
    x = 10
    y = 20
    result = add(x, y)
    log(""Process1 calculated: "" + result)
");

context2.Execute(@"
    a = 5
    b = 15
    result = add(a, b)
    log(""Process2 calculated: "" + result)
");

// Each context has its own variables
var result1 = context1.GetVariable("result");  // Returns: 30
var result2 = context2.GetVariable("result");  // Returns: 20
```

## Runtime Execution

### Load Script First, Then Execute More

```csharp
var engine = new HobScriptEngine();

// Step 1: Load initial script with functions and variables
engine.ExecuteFile("base_functions.hob");

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

// Step 4: Execute more runtime code
var moreRuntimeScript = @"
    // Use existing variables and create new ones
    discount = total_tax * 0.1
    final_amount = total_tax - discount
    
    print(""Discount (10%): $"" + discount)
    print(""Final amount: $"" + final_amount)
";

engine.Execute(moreRuntimeScript);

// Step 5: Read final results
var finalAmount = engine.GetVariable("final_amount");
Console.WriteLine($"Final amount: ${finalAmount}");
```

### Interactive Script Execution

```csharp
var engine = new HobScriptEngine();

// Load base functions and configuration
engine.ExecuteFile("base_functions.hob");
engine.ExecuteFile("config.hob");

Console.WriteLine("HobScript Interactive Mode");
Console.WriteLine("Type 'exit' to quit, 'help' for available functions");

while (true)
{
    Console.Write("HobScript> ");
    var input = Console.ReadLine();
    
    if (input?.ToLower() == "exit")
        break;
    
    if (input?.ToLower() == "help")
    {
        var functions = engine.GetRegisteredFunctions();
        Console.WriteLine("Available functions:");
        foreach (var func in functions)
        {
            Console.WriteLine($"  {func.Name}: {func.Description}");
        }
        continue;
    }
    
    if (input?.ToLower() == "vars")
    {
        var variables = engine.GetVariableNames();
        Console.WriteLine("Current variables:");
        foreach (var varName in variables)
        {
            var value = engine.GetVariable(varName);
            Console.WriteLine($"  {varName} = {value}");
        }
        continue;
    }
    
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

### Basic Error Handling

```csharp
var engine = new HobScriptEngine();

try
{
    engine.Execute("invalid syntax here");
}
catch (ScriptException ex)
{
    Console.WriteLine($"Script error: {ex.Message}");
    Console.WriteLine($"Line: {ex.LineNumber}");
    if (!string.IsNullOrEmpty(ex.ScriptLine))
        Console.WriteLine($"Script line: {ex.ScriptLine}");
}
```

### Function Not Found Handling

```csharp
var engine = new HobScriptEngine();

try
{
    engine.Execute("result = unknown_function(10, 20)");
}
catch (ScriptException ex)
{
    Console.WriteLine($"Function not found: {ex.Message}");
    // Suggest similar functions
    var functions = engine.GetRegisteredFunctions();
    Console.WriteLine("Available functions:");
    foreach (var func in functions)
    {
        Console.WriteLine($"  {func.Name}");
    }
}
```

### File Loading Error Handling

```csharp
var engine = new HobScriptEngine();

try
{
    engine.ExecuteFile("nonexistent_file.hob");
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"File not found: {ex.FileName}");
    Console.WriteLine("Please check the file path and try again.");
}
catch (ScriptException ex)
{
    Console.WriteLine($"Script error in file: {ex.Message}");
    Console.WriteLine($"Line: {ex.LineNumber}");
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected error: {ex.Message}");
}
```

### Comprehensive Error Handling

```csharp
var engine = new HobScriptEngine();

try
{
    // Register some functions
    engine.RegisterFunction("add", new Func<int, int, int>((a, b) => a + b));
    
    // Execute script with potential errors
    engine.Execute(@"
        x = 10
        y = 20
        z = add(x, y)
        print(""Sum: "" + z)
        
        // This will cause an error
        result = divide(x, y)  // Function not found
    ");
}
catch (ScriptException ex)
{
    Console.WriteLine($"Script execution error:");
    Console.WriteLine($"  Message: {ex.Message}");
    Console.WriteLine($"  Line: {ex.LineNumber}");
    
    if (ex.Message.Contains("not found"))
    {
        Console.WriteLine("Available functions:");
        var functions = engine.GetRegisteredFunctions();
        foreach (var func in functions)
        {
            Console.WriteLine($"  {func.Name}");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected error: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
}
```

## Advanced Examples

### Configuration Management System

```csharp
public class ConfigurationManager
{
    private readonly HobScriptEngine _engine;
    
    public ConfigurationManager()
    {
        _engine = new HobScriptEngine();
        LoadConfiguration();
    }
    
    private void LoadConfiguration()
    {
        try
        {
            _engine.ExecuteFile("config.hob");
        }
        catch (FileNotFoundException)
        {
            // Create default configuration
            _engine.Execute(@"
                app_name = ""Default App""
                version = ""1.0.0""
                debug_mode = false
                max_connections = 100
                timeout_seconds = 30
            ");
        }
    }
    
    public T GetValue<T>(string key)
    {
        var value = _engine.GetVariable(key);
        if (value is T typedValue)
            return typedValue;
        
        throw new InvalidOperationException($"Configuration value '{key}' is not of type {typeof(T).Name}");
    }
    
    public void SetValue(string key, object value)
    {
        _engine.SetVariable(key, value);
    }
    
    public void SaveConfiguration()
    {
        var variables = _engine.GetVariableNames();
        var configContent = new StringBuilder();
        
        foreach (var varName in variables)
        {
            var value = _engine.GetVariable(varName);
            configContent.AppendLine($"{varName} = {FormatValue(value)}");
        }
        
        File.WriteAllText("config.hob", configContent.ToString());
    }
    
    private string FormatValue(object value)
    {
        if (value is string str)
            return $"\"{str}\"";
        if (value is bool boolVal)
            return boolVal.ToString().ToLower();
        return value?.ToString() ?? "null";
    }
}

// Usage
var config = new ConfigurationManager();
var appName = config.GetValue<string>("app_name");
var maxConnections = config.GetValue<int>("max_connections");
var debugMode = config.GetValue<bool>("debug_mode");

Console.WriteLine($"App: {appName}, Max Connections: {maxConnections}, Debug: {debugMode}");
```

### Dynamic Calculation Engine

```csharp
public class CalculationEngine
{
    private readonly HobScriptEngine _engine;
    
    public CalculationEngine()
    {
        _engine = new HobScriptEngine();
        RegisterMathFunctions();
    }
    
    private void RegisterMathFunctions()
    {
        _engine.RegisterFunction("sqrt", new Func<double, double>(Math.Sqrt));
        _engine.RegisterFunction("pow", new Func<double, double, double>(Math.Pow));
        _engine.RegisterFunction("sin", new Func<double, double>(Math.Sin));
        _engine.RegisterFunction("cos", new Func<double, double>(Math.Cos));
        _engine.RegisterFunction("tan", new Func<double, double>(Math.Tan));
        _engine.RegisterFunction("log", new Func<double, double>(Math.Log));
        _engine.RegisterFunction("exp", new Func<double, double>(Math.Exp));
    }
    
    public double Calculate(string expression, Dictionary<string, double> variables = null)
    {
        try
        {
            // Set variables if provided
            if (variables != null)
            {
                foreach (var kvp in variables)
                {
                    _engine.SetVariable(kvp.Key, kvp.Value);
                }
            }
            
            // Execute the expression
            var result = _engine.Execute(expression);
            
            if (result is double doubleResult)
                return doubleResult;
            if (result is int intResult)
                return intResult;
            if (result is float floatResult)
                return floatResult;
            
            throw new InvalidOperationException("Expression did not return a numeric result");
        }
        catch (ScriptException ex)
        {
            throw new InvalidOperationException($"Calculation error: {ex.Message}", ex);
        }
    }
    
    public double EvaluateFunction(string functionName, params double[] parameters)
    {
        var paramString = string.Join(", ", parameters);
        var expression = $"{functionName}({paramString})";
        return Calculate(expression);
    }
}

// Usage
var calc = new CalculationEngine();

// Simple calculations
var result1 = calc.Calculate("10 + 20 * 3");  // 70
var result2 = calc.Calculate("sqrt(16) + pow(2, 3)");  // 12

// Calculations with variables
var variables = new Dictionary<string, double>
{
    ["x"] = 5,
    ["y"] = 10,
    ["z"] = 2
};
var result3 = calc.Calculate("x * y + z", variables);  // 52

// Function evaluations
var result4 = calc.EvaluateFunction("sin", Math.PI / 2);  // 1
var result5 = calc.EvaluateFunction("pow", 2, 8);  // 256

Console.WriteLine($"Results: {result1}, {result2}, {result3}, {result4}, {result5}");
```

### Template Engine

```csharp
public class TemplateEngine
{
    private readonly HobScriptEngine _engine;
    
    public TemplateEngine()
    {
        _engine = new HobScriptEngine();
        RegisterTemplateFunctions();
    }
    
    private void RegisterTemplateFunctions()
    {
        _engine.RegisterFunction("format", new Func<object, string, string>((value, format) => 
            string.Format($"{{0:{format}}}", value)));
        _engine.RegisterFunction("upper", new Func<string, string>(s => s.ToUpper()));
        _engine.RegisterFunction("lower", new Func<string, string>(s => s.ToLower()));
        _engine.RegisterFunction("pad", new Func<string, int, string>((s, width) => s.PadRight(width)));
    }
    
    public string ProcessTemplate(string template, Dictionary<string, object> data)
    {
        try
        {
            // Set template data
            foreach (var kvp in data)
            {
                _engine.SetVariable(kvp.Key, kvp.Value);
            }
            
            // Process template
            var result = _engine.Execute(template);
            return result?.ToString() ?? "";
        }
        catch (ScriptException ex)
        {
            throw new InvalidOperationException($"Template processing error: {ex.Message}", ex);
        }
    }
}

// Usage
var templateEngine = new TemplateEngine();

var template = @"
    name = upper(first_name) + "" "" + upper(last_name)
    email = lower(email_address)
    phone = format(phone_number, ""(000) 000-0000"")
    
    result = ""Name: "" + name + ""\n""
    result = result + ""Email: "" + email + ""\n""
    result = result + ""Phone: "" + phone + ""\n""
    result = result + ""Age: "" + age + "" years old""
";

var data = new Dictionary<string, object>
{
    ["first_name"] = "John",
    ["last_name"] = "Doe",
    ["email_address"] = "JOHN.DOE@EXAMPLE.COM",
    ["phone_number"] = 1234567890,
    ["age"] = 30
};

var result = templateEngine.ProcessTemplate(template, data);
Console.WriteLine(result);
```

## Real-World Examples

### Web API Configuration

```csharp
public class ApiConfiguration
{
    private readonly HobScriptEngine _engine;
    
    public ApiConfiguration()
    {
        _engine = new HobScriptEngine();
        LoadConfiguration();
    }
    
    private void LoadConfiguration()
    {
        _engine.ExecuteFile("api_config.hob");
    }
    
    public string GetApiUrl() => _engine.GetVariable("api_url")?.ToString();
    public string GetApiKey() => _engine.GetVariable("api_key")?.ToString();
    public int GetTimeout() => (int)_engine.GetVariable("timeout_seconds");
    public bool IsDebugMode() => (bool)_engine.GetVariable("debug_mode");
    public string[] GetAllowedOrigins() => _engine.GetVariable("allowed_origins") as string[];
    
    public void UpdateConfiguration(string key, object value)
    {
        _engine.SetVariable(key, value);
    }
}
```

### Data Processing Pipeline

```csharp
public class DataProcessor
{
    private readonly HobScriptEngine _engine;
    
    public DataProcessor()
    {
        _engine = new HobScriptEngine();
        RegisterProcessingFunctions();
    }
    
    private void RegisterProcessingFunctions()
    {
        _engine.RegisterFunction("validate", new Func<object, bool>(ValidateData));
        _engine.RegisterFunction("transform", new Func<object, object>(TransformData));
        _engine.RegisterFunction("log", new Action<string>(Console.WriteLine));
    }
    
    private bool ValidateData(object data)
    {
        // Validation logic
        return data != null;
    }
    
    private object TransformData(object data)
    {
        // Transformation logic
        return data;
    }
    
    public void ProcessData(IEnumerable<object> data)
    {
        _engine.Execute(@"
            processed_count = 0
            error_count = 0
            
            for (i = 0; i < data_length; i = i + 1)
            {
                item = data[i]
                
                if (validate(item))
                {
                    transformed = transform(item)
                    processed_count = processed_count + 1
                    log(""Processed item: "" + transformed)
                }
                else
                {
                    error_count = error_count + 1
                    log(""Validation failed for item: "" + item)
                }
            }
            
            log(""Processing complete. Processed: "" + processed_count + "", Errors: "" + error_count)
        ");
    }
}
```

These examples demonstrate the full power and flexibility of the HobScript library for various real-world scenarios!
