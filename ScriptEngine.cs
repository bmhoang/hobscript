using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Text.RegularExpressions;

namespace HobScript
{
    /// <summary>
    /// Main script engine class that executes C#-like scripts with custom functions
    /// </summary>
    public class ScriptEngine
    {
        private readonly Dictionary<string, object> _variables;
        private readonly Dictionary<string, Delegate> _functions;
        private readonly Dictionary<string, Type> _types;
        private readonly HashSet<string> _loadedFiles = new HashSet<string>();
        private readonly Dictionary<string, string> _fileCache = new Dictionary<string, string>();

        public ScriptEngine()
        {
            _variables = new Dictionary<string, object>();
            _functions = new Dictionary<string, Delegate>();
            _types = new Dictionary<string, Type>();
            
            // Register default functions
            RegisterDefaultFunctions();
        }

        /// <summary>
        /// Executes a script string
        /// </summary>
        /// <param name="script">The script to execute</param>
        /// <returns>The result of the last expression</returns>
        public object Execute(string script)
        {
            if (string.IsNullOrWhiteSpace(script))
                return null;

            var lines = script.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            object lastResult = null;

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("//"))
                    continue;

                // Handle import statements
                if (trimmedLine.StartsWith("import "))
                {
                    HandleImport(trimmedLine);
                    continue;
                }

                // Handle function definitions
                if (trimmedLine.StartsWith("function "))
                {
                    HandleFunctionDefinition(trimmedLine, lines, ref i);
                    continue;
                }

                lastResult = ExecuteLine(trimmedLine);
            }

            return lastResult;
        }

        /// <summary>
        /// Executes a script string asynchronously.
        /// </summary>
        /// <param name="script">The script to execute</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A task producing the result of the last expression</returns>
        public virtual Task<object> ExecuteAsync(string script, CancellationToken cancellationToken = default)
        {
            return Task.Run(() => Execute(script), cancellationToken);
        }

        /// <summary>
        /// Executes a single line of script
        /// </summary>
        /// <param name="line">The line to execute</param>
        /// <returns>The result of the expression</returns>
        public object ExecuteLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return null;

            // Handle variable assignment first (before function calls)
            if (line.Contains("=") && !line.Contains("==") && !line.Contains("!="))
            {
                return HandleVariableAssignment(line);
            }

            // Handle function calls
            if (line.Contains("(") && line.Contains(")"))
            {
                return HandleFunctionCall(line);
            }

            // Handle variable access
            if (_variables.ContainsKey(line))
            {
                return _variables[line];
            }

            // Handle literals
            return ParseLiteral(line);
        }

        /// <summary>
        /// Registers a custom function
        /// </summary>
        /// <param name="name">Function name</param>
        /// <param name="function">Function delegate</param>
        public void RegisterFunction(string name, Delegate function)
        {
            _functions[name] = function;
        }

        /// <summary>
        /// Registers a custom type
        /// </summary>
        /// <param name="name">Type name</param>
        /// <param name="type">Type object</param>
        public void RegisterType(string name, Type type)
        {
            _types[name] = type;
        }

        /// <summary>
        /// Gets a variable value
        /// </summary>
        /// <param name="name">Variable name</param>
        /// <returns>Variable value or null if not found</returns>
        public object GetVariable(string name)
        {
            return _variables.TryGetValue(name, out var value) ? value : null;
        }

        /// <summary>
        /// Sets a variable value
        /// </summary>
        /// <param name="name">Variable name</param>
        /// <param name="value">Variable value</param>
        public void SetVariable(string name, object value)
        {
            _variables[name] = value;
        }

        /// <summary>
        /// Gets all variable names
        /// </summary>
        /// <returns>Collection of variable names</returns>
        public IEnumerable<string> GetVariableNames()
        {
            return _variables.Keys;
        }

        /// <summary>
        /// Clears all variables
        /// </summary>
        public void ClearVariables()
        {
            _variables.Clear();
        }

        private void RegisterDefaultFunctions()
        {
            // Register print function
            RegisterFunction("print", new Action<object>(Print));
            
            // Register read function
            RegisterFunction("read", new Func<string>(Read));
        }

        private void Print(object value)
        {
            System.Console.WriteLine(value?.ToString() ?? "null");
        }

        private string Read()
        {
            return System.Console.ReadLine() ?? "";
        }

        private object HandleVariableAssignment(string line)
        {
            var parts = line.Split(new char[] { '=' }, 2);
            if (parts.Length != 2)
                throw new ScriptException($"Invalid assignment: {line}");

            var varName = parts[0].Trim();
            var expression = parts[1].Trim();

            var value = EvaluateExpression(expression);
            _variables[varName] = value;
            return value;
        }

        private object HandleFunctionCall(string line)
        {
            var match = Regex.Match(line, @"([\w.]+)\s*\((.*)\)");
            if (!match.Success)
                throw new ScriptException($"Invalid function call: {line}");

            var functionName = match.Groups[1].Value;
            var argsString = match.Groups[2].Value;

            if (!_functions.TryGetValue(functionName, out var function))
                throw new ScriptException($"Function '{functionName}' not found");

            var args = ParseArguments(argsString);
            return function.DynamicInvoke(args);
        }

        private object[] ParseArguments(string argsString)
        {
            if (string.IsNullOrWhiteSpace(argsString))
                return new object[0];

            var args = new List<object>();
            var currentArg = "";
            var parenCount = 0;
            var inString = false;
            var stringChar = '\0';

            for (int i = 0; i < argsString.Length; i++)
            {
                var c = argsString[i];

                if (!inString && (c == '"' || c == '\''))
                {
                    inString = true;
                    stringChar = c;
                    currentArg += c;
                }
                else if (inString && c == stringChar)
                {
                    inString = false;
                    currentArg += c;
                }
                else if (!inString && c == '(')
                {
                    parenCount++;
                    currentArg += c;
                }
                else if (!inString && c == ')')
                {
                    parenCount--;
                    currentArg += c;
                }
                else if (!inString && c == ',' && parenCount == 0)
                {
                    args.Add(EvaluateExpression(currentArg.Trim()));
                    currentArg = "";
                }
                else
                {
                    currentArg += c;
                }
            }

            if (!string.IsNullOrWhiteSpace(currentArg))
            {
                args.Add(EvaluateExpression(currentArg.Trim()));
            }

            return args.ToArray();
        }

        protected object EvaluateExpression(string expression)
        {
            // Handle function calls in expressions
            if (expression.Contains("(") && expression.Contains(")"))
            {
                return HandleFunctionCall(expression);
            }

            // Handle arithmetic expressions
            if (expression.Contains("+") || expression.Contains("-") || expression.Contains("*") || expression.Contains("/"))
            {
                return EvaluateArithmeticExpression(expression);
            }

            // Handle variable access
            if (_variables.ContainsKey(expression))
            {
                return _variables[expression];
            }

            // Handle literals
            return ParseLiteral(expression);
        }

        private object EvaluateArithmeticExpression(string expression)
        {
            // Simple arithmetic evaluation for basic operations
            var operators = new[] { "+", "-", "*", "/" };
            
            foreach (var op in operators)
            {
                if (expression.Contains(op))
                {
                    var parts = expression.Split(new string[] { op }, 2, StringSplitOptions.None);
                    if (parts.Length == 2)
                    {
                        var left = EvaluateExpression(parts[0].Trim());
                        var right = EvaluateExpression(parts[1].Trim());
                        
                        return PerformArithmeticOperation(left, right, op);
                    }
                }
            }
            
            return expression;
        }

        private object PerformArithmeticOperation(object left, object right, string operation)
        {
            try
            {
                var leftNum = Convert.ToDouble(left);
                var rightNum = Convert.ToDouble(right);
                
                return operation switch
                {
                    "+" => leftNum + rightNum,
                    "-" => leftNum - rightNum,
                    "*" => leftNum * rightNum,
                    "/" => rightNum != 0 ? leftNum / rightNum : 0,
                    _ => 0
                };
            }
            catch
            {
                // If conversion fails, try string concatenation for +
                if (operation == "+")
                {
                    return (left?.ToString() ?? "") + (right?.ToString() ?? "");
                }
                return 0;
            }
        }

        private object ParseLiteral(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            // Handle string literals
            if ((value.StartsWith("\"") && value.EndsWith("\"")) || 
                (value.StartsWith("'") && value.EndsWith("'")))
            {
                return value.Substring(1, value.Length - 2);
            }

            // Handle boolean literals
            if (value.Equals("true", StringComparison.OrdinalIgnoreCase))
                return true;
            if (value.Equals("false", StringComparison.OrdinalIgnoreCase))
                return false;

            // Handle null
            if (value.Equals("null", StringComparison.OrdinalIgnoreCase))
                return null;

            // Handle numbers
            if (int.TryParse(value, out var intValue))
                return intValue;
            if (double.TryParse(value, out var doubleValue))
                return doubleValue;

            // Handle decimal
            if (decimal.TryParse(value, out var decimalValue))
                return decimalValue;

            // If it's a variable name that doesn't exist, return the name as string
            return value;
        }

        /// <summary>
        /// Executes a script from a file
        /// </summary>
        /// <param name="filePath">Path to the script file</param>
        /// <returns>The result of the last expression</returns>
        public object ExecuteFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ScriptException("File path cannot be null or empty");

            var fullPath = Path.GetFullPath(filePath);
            
            if (!File.Exists(fullPath))
                throw new ScriptException($"Script file not found: {fullPath}");

            // Check for circular imports
            if (_loadedFiles.Contains(fullPath))
                throw new ScriptException($"Circular import detected: {fullPath}");

            _loadedFiles.Add(fullPath);

            try
            {
                var script = ReadFileContent(fullPath);
                return Execute(script);
            }
            finally
            {
                _loadedFiles.Remove(fullPath);
            }
        }

        /// <summary>
        /// Executes a script from a file asynchronously.
        /// </summary>
        /// <param name="filePath">Path to the script file</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A task producing the result of the last expression</returns>
        public virtual async Task<object> ExecuteFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ScriptException("File path cannot be null or empty");

            var fullPath = Path.GetFullPath(filePath);

            if (!File.Exists(fullPath))
                throw new ScriptException($"Script file not found: {fullPath}");

            if (_loadedFiles.Contains(fullPath))
                throw new ScriptException($"Circular import detected: {fullPath}");

            _loadedFiles.Add(fullPath);

            try
            {
                var script = await ReadFileContentAsync(fullPath, cancellationToken).ConfigureAwait(false);
                return await ExecuteAsync(script, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _loadedFiles.Remove(fullPath);
            }
        }

        /// <summary>
        /// Executes a script from a file with a specific working directory
        /// </summary>
        /// <param name="filePath">Path to the script file</param>
        /// <param name="workingDirectory">Working directory for relative imports</param>
        /// <returns>The result of the last expression</returns>
        public object ExecuteFile(string filePath, string workingDirectory)
        {
            var originalDirectory = Directory.GetCurrentDirectory();
            try
            {
                if (!string.IsNullOrWhiteSpace(workingDirectory))
                {
                    Directory.SetCurrentDirectory(workingDirectory);
                }
                return ExecuteFile(filePath);
            }
            finally
            {
                Directory.SetCurrentDirectory(originalDirectory);
            }
        }

        /// <summary>
        /// Reads file content with caching
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>File content</returns>
        private string ReadFileContent(string filePath)
        {
            if (_fileCache.TryGetValue(filePath, out var cachedContent))
            {
                return cachedContent;
            }

            var content = File.ReadAllText(filePath);
            _fileCache[filePath] = content;
            return content;
        }

        /// <summary>
        /// Reads file content with caching (async).
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>File content</returns>
        private async Task<string> ReadFileContentAsync(string filePath, CancellationToken cancellationToken)
        {
            if (_fileCache.TryGetValue(filePath, out var cachedContent))
            {
                return cachedContent;
            }

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
            using (var reader = new StreamReader(stream))
            {
                var content = await reader.ReadToEndAsync().ConfigureAwait(false);
                _fileCache[filePath] = content;
                return content;
            }
        }

        /// <summary>
        /// Clears the file cache
        /// </summary>
        public void ClearFileCache()
        {
            _fileCache.Clear();
        }

        /// <summary>
        /// Handles import statements
        /// </summary>
        /// <param name="importLine">The import line to process</param>
        private void HandleImport(string importLine)
        {
            // Parse import statement: import "filename" or import filename
            var match = Regex.Match(importLine, @"import\s+[""']?([^""']+)[""']?");
            if (!match.Success)
                throw new ScriptException($"Invalid import statement: {importLine}");

            var fileName = match.Groups[1].Value.Trim();
            
            // Resolve file path
            var filePath = ResolveFilePath(fileName);
            
            // Execute the imported file
            ExecuteFile(filePath);
        }

        /// <summary>
        /// Handles function definitions in scripts
        /// </summary>
        /// <param name="functionLine">The function definition line</param>
        /// <param name="lines">All script lines</param>
        /// <param name="currentIndex">Current line index (will be updated)</param>
        private void HandleFunctionDefinition(string functionLine, string[] lines, ref int currentIndex)
        {
            // Parse function definition: function name(params) { ... }
            var match = Regex.Match(functionLine, @"function\s+(\w+)\s*\(([^)]*)\)");
            if (!match.Success)
                throw new ScriptException($"Invalid function definition: {functionLine}");

            var functionName = match.Groups[1].Value;
            var parameters = match.Groups[2].Value.Split(',').Select(p => p.Trim()).Where(p => !string.IsNullOrEmpty(p)).ToArray();

            // Find the function body (skip to next line and find opening brace)
            currentIndex++;
            var bodyLines = new List<string>();
            var braceCount = 0;
            var inFunction = false;

            while (currentIndex < lines.Length)
            {
                var line = lines[currentIndex].Trim();
                
                if (line.Contains("{"))
                {
                    inFunction = true;
                    braceCount++;
                }
                
                if (inFunction)
                {
                    if (line.Contains("{"))
                        braceCount++;
                    if (line.Contains("}"))
                        braceCount--;
                    
                    if (braceCount == 0)
                        break;
                    
                    // Skip the opening brace line
                    if (!line.Contains("{"))
                    {
                        bodyLines.Add(line);
                    }
                }
                
                currentIndex++;
            }

            // Create a delegate for the function
            var functionDelegate = CreateFunctionDelegate(functionName, parameters, bodyLines.ToArray());
            _functions[functionName] = functionDelegate;
        }

        /// <summary>
        /// Creates a delegate for a script function
        /// </summary>
        /// <param name="functionName">Name of the function</param>
        /// <param name="parameters">Function parameters</param>
        /// <param name="bodyLines">Function body lines</param>
        /// <returns>Delegate representing the function</returns>
        private Delegate CreateFunctionDelegate(string functionName, string[] parameters, string[] bodyLines)
        {
            // Create a delegate that matches the number of parameters
            if (parameters.Length == 0)
            {
                return new Func<object>(() =>
                {
                    return ExecuteFunctionBody(bodyLines);
                });
            }
            else if (parameters.Length == 1)
            {
                return new Func<object, object>(arg1 =>
                {
                    return ExecuteFunctionBody(bodyLines, new[] { arg1 }, parameters);
                });
            }
            else if (parameters.Length == 2)
            {
                return new Func<object, object, object>((arg1, arg2) =>
                {
                    return ExecuteFunctionBody(bodyLines, new[] { arg1, arg2 }, parameters);
                });
            }
            else if (parameters.Length == 3)
            {
                return new Func<object, object, object, object>((arg1, arg2, arg3) =>
                {
                    return ExecuteFunctionBody(bodyLines, new[] { arg1, arg2, arg3 }, parameters);
                });
            }
            else
            {
                // For more than 3 parameters, use the generic approach
                return new Func<object[], object>(args =>
                {
                    return ExecuteFunctionBody(bodyLines, args, parameters);
                });
            }
        }

        /// <summary>
        /// Executes the function body with the given arguments
        /// </summary>
        /// <param name="bodyLines">Function body lines</param>
        /// <param name="args">Function arguments</param>
        /// <param name="parameters">Parameter names</param>
        /// <returns>Function result</returns>
        private object ExecuteFunctionBody(string[] bodyLines, object[] args = null, string[] parameters = null)
        {
            // Store original variables
            var originalVariables = new Dictionary<string, object>(_variables);
            
            try
            {
                // Set parameter values
                if (args != null && parameters != null)
                {
                    for (int i = 0; i < parameters.Length && i < args.Length; i++)
                    {
                        _variables[parameters[i]] = args[i];
                    }
                }

                // Execute function body
                object result = null;
                foreach (var line in bodyLines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
                        continue;
                    
                    if (line.StartsWith("return "))
                    {
                        var returnExpression = line.Substring(7).Trim();
                        result = EvaluateExpression(returnExpression);
                        break;
                    }
                    
                    ExecuteLine(line);
                }

                return result;
            }
            finally
            {
                // Restore original variables
                _variables.Clear();
                foreach (var kvp in originalVariables)
                {
                    _variables[kvp.Key] = kvp.Value;
                }
            }
        }

        /// <summary>
        /// Resolves a file path for import
        /// </summary>
        /// <param name="fileName">The file name to resolve</param>
        /// <returns>Full path to the file</returns>
        private string ResolveFilePath(string fileName)
        {
            // If it's already an absolute path, use it as is
            if (Path.IsPathRooted(fileName))
            {
                return Path.GetFullPath(fileName);
            }

            // Try relative to current directory
            var currentDir = Directory.GetCurrentDirectory();
            var relativePath = Path.Combine(currentDir, fileName);
            
            if (File.Exists(relativePath))
            {
                return Path.GetFullPath(relativePath);
            }

            // Try with .hob extension
            var withExtension = fileName.EndsWith(".hob") ? fileName : fileName + ".hob";
            var withExtensionPath = Path.Combine(currentDir, withExtension);
            
            if (File.Exists(withExtensionPath))
            {
                return Path.GetFullPath(withExtensionPath);
            }

            // If not found, throw exception
            throw new ScriptException($"Import file not found: {fileName}");
        }
    }

    /// <summary>
    /// Exception thrown by the script engine
    /// </summary>
    public class ScriptException : Exception
    {
        public ScriptException(string message) : base(message) { }
        public ScriptException(string message, Exception innerException) : base(message, innerException) { }
    }
}
