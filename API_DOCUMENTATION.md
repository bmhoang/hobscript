# HobScript API Documentation

This document provides detailed API reference for the HobScript library.

## Table of Contents

- [HobScriptEngine Class](#hobscriptengine-class)
- [ScriptContext Class](#scriptcontext-class)
- [FunctionInfo Class](#functioninfo-class)
- [ScriptException Class](#scriptexception-class)
- [FunctionRegistry Class](#functionregistry-class)
- [AdvancedScriptEngine Class](#advancedscriptengine-class)
- [ScriptEngine Class](#scriptengine-class)

## HobScriptEngine Class

The main entry point for the HobScript library. Provides comprehensive script execution capabilities.

### Constructor

```csharp
public HobScriptEngine()
```

Creates a new instance of the HobScriptEngine with default settings.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `GlobalVariables` | `Dictionary<string, object>` | Global variables accessible across all contexts |
| `FunctionRegistry` | `FunctionRegistry` | Registry for custom functions |

### Methods

#### Execute

```csharp
public object Execute(string script)
public Task<object> ExecuteAsync(string script, CancellationToken cancellationToken = default)
```

Executes a script string and returns the result of the last expression.

**Parameters:**
- `script` (string): The script to execute

**Returns:**
- `object`: The result of the last expression in the script
- `Task<object>`: Async execution result

**Example:**
```csharp
var engine = new HobScriptEngine();
var result = engine.Execute("x = 10; y = 20; x + y"); // Returns 30

// Async
var asyncResult = await engine.ExecuteAsync("x = 10\ny = 20\nx + y");
```

#### ExecuteFile

```csharp
public object ExecuteFile(string filePath)
public object ExecuteFile(string filePath, string workingDirectory)
public Task<object> ExecuteFileAsync(string filePath, CancellationToken cancellationToken = default)
```

Loads and executes a script file.

**Parameters:**
- `filePath` (string): Path to the script file
- `workingDirectory` (string): Working directory for relative imports (optional)

**Returns:**
- `object`: The result of the last expression in the script
- `Task<object>`: Async execution result

**Example:**
```csharp
var engine = new HobScriptEngine();
var result = engine.ExecuteFile("my_script.hob");
var result2 = engine.ExecuteFile("scripts/main.hob", "/path/to/scripts");

// Async file execution
var asyncResult = await engine.ExecuteFileAsync("my_script.hob");
```

#### Variable Management

```csharp
public object GetVariable(string name)
public void SetVariable(string name, object value)
public IEnumerable<string> GetVariableNames()
public bool HasVariable(string name)
```

Manages script variables.

**Parameters:**
- `name` (string): Variable name
- `value` (object): Variable value

**Example:**
```csharp
var engine = new HobScriptEngine();
engine.Execute("x = 10; y = 20");

var x = engine.GetVariable("x"); // Returns 10
var y = engine.GetVariable("y"); // Returns 20

engine.SetVariable("z", 30);
var z = engine.GetVariable("z"); // Returns 30

var allVars = engine.GetVariableNames(); // Returns ["x", "y", "z"]
bool hasX = engine.HasVariable("x"); // Returns true
```

#### Global Variables

```csharp
public void SetGlobalVariable(string name, object value)
public object GetGlobalVariable(string name)
```

Manages global variables that persist across script executions.

**Example:**
```csharp
var engine = new HobScriptEngine();
engine.SetGlobalVariable("appName", "HobScript");
engine.SetGlobalVariable("version", "1.0.0");

engine.Execute("print(appName + ' ' + version)"); // Output: HobScript 1.0.0
```

#### Function Registration

```csharp
public void RegisterFunction(string name, Delegate function, string description = null)
public void RegisterObject(object obj, string prefix = "")
public void RegisterType(Type type, string prefix = "")
```

Registers custom functions for use in scripts.

**Parameters:**
- `name` (string): Function name
- `function` (Delegate): Function delegate
- `description` (string): Function description (optional)
- `obj` (object): Object instance to register
- `prefix` (string): Prefix for method names (optional)
- `type` (Type): Type to register static methods from

**Example:**
```csharp
var engine = new HobScriptEngine();

// Register simple function
engine.RegisterFunction("add", new Func<int, int, int>((a, b) => a + b), "Adds two numbers");

// Register object methods
var mathUtils = new MathUtils();
engine.RegisterObject(mathUtils, "Math");

// Register static methods
engine.RegisterType(typeof(System.Math), "Math");
```

#### Context Management

```csharp
public ScriptContext CreateContext(string name = null)
```

Creates an isolated script context.

**Parameters:**
- `name` (string): Context name (optional)

**Returns:**
- `ScriptContext`: New script context

**Example:**
```csharp
var engine = new HobScriptEngine();
var context1 = engine.CreateContext("Context1");
var context2 = engine.CreateContext("Context2");

context1.Execute("x = 10");
context2.Execute("x = 20");

var x1 = context1.GetVariable("x"); // Returns 10
var x2 = context2.GetVariable("x"); // Returns 20
```

#### Function Information

```csharp
public IEnumerable<FunctionInfo> GetRegisteredFunctions()
public bool HasFunction(string name)
```

Gets information about registered functions.

**Example:**
```csharp
var engine = new HobScriptEngine();
engine.RegisterFunction("add", new Func<int, int, int>((a, b) => a + b));

var functions = engine.GetRegisteredFunctions();
foreach (var func in functions)
{
    Console.WriteLine($"{func.Name}: {func.Description}");
}

bool hasAdd = engine.HasFunction("add"); // Returns true
```

#### File Management

```csharp
public void ClearFileCache()
public bool IsFileLoaded(string filePath)
```

Manages file loading and caching.

**Example:**
```csharp
var engine = new HobScriptEngine();
engine.ExecuteFile("script.hob");

bool isLoaded = engine.IsFileLoaded("script.hob"); // Returns true
engine.ClearFileCache();
```

## ScriptContext Class

Represents an isolated script execution context with its own variable scope.

### Constructor

```csharp
public ScriptContext(string name, HobScriptEngine engine)
```

Creates a new script context.

**Parameters:**
- `name` (string): Context name
- `engine` (HobScriptEngine): Parent engine

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Name` | `string` | Context name |
| `Variables` | `Dictionary<string, object>` | Context variables |

### Methods

#### Execute

```csharp
public object Execute(string script)
```

Executes a script in this context.

**Parameters:**
- `script` (string): The script to execute

**Returns:**
- `object`: The result of the last expression

#### Variable Management

```csharp
public object GetVariable(string name)
public void SetVariable(string name, object value)
public IEnumerable<string> GetVariableNames()
public bool HasVariable(string name)
```

Manages variables in this context.

## FunctionInfo Class

Contains information about a registered function.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Name` | `string` | Function name |
| `Description` | `string` | Function description |
| `ParameterCount` | `int` | Number of parameters |
| `ReturnType` | `Type` | Return type |
| `Function` | `Delegate` | Function delegate |

### Constructor

```csharp
public FunctionInfo(string name, Delegate function, string description = null)
```

Creates a new FunctionInfo instance.

## ScriptException Class

Exception thrown when script execution errors occur.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `LineNumber` | `int` | Line number where error occurred |
| `ScriptLine` | `string` | Script line that caused the error |

### Constructors

```csharp
public ScriptException(string message)
public ScriptException(string message, int lineNumber)
public ScriptException(string message, int lineNumber, string scriptLine)
```

Creates a new ScriptException.

## FunctionRegistry Class

Manages the registration and retrieval of custom functions.

### Methods

#### Registration

```csharp
public void RegisterFunction(string name, Delegate function, string description = null)
public void RegisterObject(object obj, string prefix = "")
public void RegisterType(Type type, string prefix = "")
```

Registers functions for use in scripts.

#### Retrieval

```csharp
public Delegate GetFunction(string name)
public bool HasFunction(string name)
public IEnumerable<FunctionInfo> GetRegisteredFunctions()
```

Gets information about registered functions.

#### Management

```csharp
public void UnregisterFunction(string name)
public void Clear()
```

Manages function registry.

## AdvancedScriptEngine Class

Extends ScriptEngine with advanced features like constants and control structures.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Constants` | `Dictionary<string, object>` | Named constants |

### Methods

#### Constants

```csharp
public void DefineConstant(string name, object value)
public object GetConstant(string name)
public bool HasConstant(string name)
```

Manages named constants.

**Example:**
```csharp
var engine = new AdvancedScriptEngine();
engine.DefineConstant("PI", 3.14159);
engine.DefineConstant("E", 2.71828);

engine.Execute("area = PI * radius * radius");
```

#### Control Structures

```csharp
public object ExecuteScript(string script)
```

Executes scripts with support for control structures (if/else, while, for).

**Note:** Control structures are currently disabled for stability.

## ScriptEngine Class

Core script execution engine providing basic functionality.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Variables` | `Dictionary<string, object>` | Script variables |
| `Functions` | `Dictionary<string, Delegate>` | Registered functions |

### Methods

#### Core Execution

```csharp
public object Execute(string script)
public object ExecuteLine(string line)
```

Executes scripts and individual lines.

#### Variable Management

```csharp
public object GetVariable(string name)
public void SetVariable(string name, object value)
public IEnumerable<string> GetVariableNames()
public bool HasVariable(string name)
```

Manages script variables.

#### Function Management

```csharp
public void RegisterFunction(string name, Delegate function)
public Delegate GetFunction(string name)
public bool HasFunction(string name)
```

Manages script functions.

#### Expression Evaluation

```csharp
protected object EvaluateExpression(string expression)
private object EvaluateArithmeticExpression(string expression)
```

Evaluates expressions and arithmetic operations.

#### File Operations

```csharp
public object ExecuteFile(string filePath)
public object ExecuteFile(string filePath, string workingDirectory)
private string ReadFileContent(string filePath)
public void ClearFileCache()
```

Handles file loading and execution.

#### Import System

```csharp
private void HandleImport(string importLine)
private string ResolveFilePath(string fileName)
```

Manages script imports and file resolution.

#### Script-Defined Functions

```csharp
private void HandleFunctionDefinition(string functionLine, string[] lines, ref int currentIndex)
private Delegate CreateFunctionDelegate(string functionName, string[] parameters, string[] bodyLines)
private object ExecuteFunctionBody(string[] bodyLines, object[] args = null, string[] parameters = null)
```

Handles function definitions within scripts.

## Usage Examples

### Basic Script Execution

```csharp
var engine = new HobScriptEngine();
var result = engine.Execute(@"
    x = 10
    y = 20
    z = x + y
    print(z)
");
```

### Custom Function Registration

```csharp
var engine = new HobScriptEngine();
engine.RegisterFunction("add", new Func<int, int, int>((a, b) => a + b));
engine.RegisterFunction("greet", new Action<string>(name => Console.WriteLine($"Hello, {name}!")));

engine.Execute(@"
    result = add(5, 3)
    greet(""World"")
");
```

### File Loading and Imports

```csharp
var engine = new HobScriptEngine();
engine.ExecuteFile("main.hob"); // Loads main.hob and any imported files
```

### Context Isolation

```csharp
var engine = new HobScriptEngine();
var context1 = engine.CreateContext("Context1");
var context2 = engine.CreateContext("Context2");

context1.Execute("x = 10");
context2.Execute("x = 20");

// Variables are isolated between contexts
var x1 = context1.GetVariable("x"); // 10
var x2 = context2.GetVariable("x"); // 20
```

### Runtime Execution

```csharp
var engine = new HobScriptEngine();

// Load initial script
engine.ExecuteFile("base_functions.hob");

// Execute additional code at runtime
engine.Execute(@"
    result = calculate_something(10, 20)
    print(""Result: "" + result)
");

// Access variables from C#
var result = engine.GetVariable("result");
```

## Error Handling

### Common Exceptions

- `ScriptException`: Script execution errors
- `FileNotFoundException`: Script file not found
- `ArgumentException`: Invalid function parameters
- `InvalidOperationException`: Invalid script operations

### Example Error Handling

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
catch (FileNotFoundException ex)
{
    Console.WriteLine($"File not found: {ex.FileName}");
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected error: {ex.Message}");
}
```

## Performance Considerations

### File Caching

Files are automatically cached to improve performance. Use `ClearFileCache()` to clear the cache when needed.

### Variable Access

Variable access is O(1) for both reading and writing operations.

### Function Lookup

Function lookup is O(1) for registered functions.

### Memory Usage

Script contexts are isolated, so memory usage scales with the number of active contexts and variables.

## Thread Safety

The HobScriptEngine is not thread-safe. If you need to use it from multiple threads, consider:

1. Creating separate engine instances for each thread
2. Using locks around engine operations
3. Using script contexts for isolation

## Best Practices

1. **Use contexts for isolation**: Create separate contexts for different script executions
2. **Register functions once**: Register custom functions once at startup
3. **Handle exceptions**: Always wrap script execution in try-catch blocks
4. **Clear file cache**: Clear file cache when files are modified
5. **Use type-safe variable access**: Check variable types before casting
6. **Minimize global variables**: Use local variables when possible
7. **Use descriptive function names**: Make function names clear and descriptive
