using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HobScript
{
    /// <summary>
    /// Main HobScript engine that combines all functionality
    /// </summary>
    public class HobScriptEngine : AdvancedScriptEngine
    {
        private readonly FunctionRegistry _functionRegistry;
        private readonly Dictionary<string, object> _globalVariables;

        public HobScriptEngine() : base()
        {
            _functionRegistry = new FunctionRegistry();
            _globalVariables = new Dictionary<string, object>();
            
            // Register default functions
            RegisterDefaultFunctions();
        }

        /// <summary>
        /// Gets the function registry
        /// </summary>
        public FunctionRegistry FunctionRegistry => _functionRegistry;

        /// <summary>
        /// Registers a custom function
        /// </summary>
        /// <param name="name">Function name</param>
        /// <param name="function">Function delegate</param>
        /// <param name="description">Optional description</param>
        public void RegisterFunction(string name, Delegate function, string description = null)
        {
            _functionRegistry.RegisterFunction(name, function, description);
            base.RegisterFunction(name, function);
        }

        /// <summary>
        /// Registers a function from a method info
        /// </summary>
        /// <param name="name">Function name</param>
        /// <param name="method">Method info</param>
        /// <param name="target">Target object for instance methods</param>
        /// <param name="description">Optional description</param>
        public void RegisterFunction(string name, System.Reflection.MethodInfo method, object target = null, string description = null)
        {
            _functionRegistry.RegisterFunction(name, method, target, description);
            var function = method.IsStatic 
                ? Delegate.CreateDelegate(GetDelegateType(method), method)
                : Delegate.CreateDelegate(GetDelegateType(method), target, method);
            base.RegisterFunction(name, function);
        }

        /// <summary>
        /// Registers all public static methods from a type
        /// </summary>
        /// <param name="type">Type to register methods from</param>
        /// <param name="prefix">Optional prefix for function names</param>
        public void RegisterType(Type type, string prefix = null)
        {
            _functionRegistry.RegisterType(type, prefix);
            
            var methods = type.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .Where(m => !m.IsSpecialName && m.DeclaringType == type);

            foreach (var method in methods)
            {
                var functionName = string.IsNullOrEmpty(prefix) ? method.Name : $"{prefix}.{method.Name}";
                var function = Delegate.CreateDelegate(GetDelegateType(method), method);
                base.RegisterFunction(functionName, function);
            }
        }

        /// <summary>
        /// Registers all public instance methods from an object
        /// </summary>
        /// <param name="target">Object to register methods from</param>
        /// <param name="prefix">Optional prefix for function names</param>
        public void RegisterObject(object target, string prefix = null)
        {
            _functionRegistry.RegisterObject(target, prefix);
            
            var type = target.GetType();
            var methods = type.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Where(m => !m.IsSpecialName && m.DeclaringType == type);

            foreach (var method in methods)
            {
                var functionName = string.IsNullOrEmpty(prefix) ? method.Name : $"{prefix}.{method.Name}";
                var function = Delegate.CreateDelegate(GetDelegateType(method), target, method);
                base.RegisterFunction(functionName, function);
            }
        }

        /// <summary>
        /// Gets function help information
        /// </summary>
        /// <param name="name">Function name</param>
        /// <returns>Help string</returns>
        public string GetFunctionHelp(string name)
        {
            return _functionRegistry.GetFunctionHelp(name);
        }

        /// <summary>
        /// Gets all available function names
        /// </summary>
        /// <returns>Collection of function names</returns>
        public IEnumerable<string> GetAvailableFunctions()
        {
            return _functionRegistry.GetFunctionNames();
        }

        /// <summary>
        /// Sets a global variable that persists across script executions
        /// </summary>
        /// <param name="name">Variable name</param>
        /// <param name="value">Variable value</param>
        public void SetGlobalVariable(string name, object value)
        {
            _globalVariables[name] = value;
        }

        /// <summary>
        /// Gets a global variable
        /// </summary>
        /// <param name="name">Variable name</param>
        /// <returns>Variable value or null if not found</returns>
        public object GetGlobalVariable(string name)
        {
            return _globalVariables.TryGetValue(name, out var value) ? value : null;
        }

        /// <summary>
        /// Gets all global variable names
        /// </summary>
        /// <returns>Collection of global variable names</returns>
        public IEnumerable<string> GetGlobalVariableNames()
        {
            return _globalVariables.Keys;
        }

        /// <summary>
        /// Clears all global variables
        /// </summary>
        public void ClearGlobalVariables()
        {
            _globalVariables.Clear();
        }

        /// <summary>
        /// Executes a script with access to both local and global variables
        /// </summary>
        /// <param name="script">The script to execute</param>
        /// <returns>The result of the last expression</returns>
        public new object Execute(string script)
        {
            // Copy global variables to local scope
            foreach (var kvp in _globalVariables)
            {
                SetVariable(kvp.Key, kvp.Value);
            }

            var result = base.Execute(script);

            // Update global variables from local scope
            foreach (var varName in GetVariableNames())
            {
                if (_globalVariables.ContainsKey(varName))
                {
                    _globalVariables[varName] = GetVariable(varName);
                }
            }

            return result;
        }

        /// <summary>
        /// Executes a script asynchronously
        /// </summary>
        /// <param name="script">The script to execute</param>
        /// <returns>Task with the result</returns>
        public override async Task<object> ExecuteAsync(string script, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => Execute(script), cancellationToken);
        }

        /// <summary>
        /// Creates a new script context with isolated variables
        /// </summary>
        /// <returns>New script context</returns>
        public ScriptContext CreateContext()
        {
            return new ScriptContext(this);
        }

        private void RegisterDefaultFunctions()
        {
            // Register math functions
            RegisterFunction("abs", new Func<double, double>(Math.Abs), "Returns the absolute value of a number");
            RegisterFunction("max", new Func<double, double, double>(Math.Max), "Returns the larger of two numbers");
            RegisterFunction("min", new Func<double, double, double>(Math.Min), "Returns the smaller of two numbers");
            RegisterFunction("sqrt", new Func<double, double>(Math.Sqrt), "Returns the square root of a number");
            RegisterFunction("pow", new Func<double, double, double>(Math.Pow), "Returns a number raised to a power");
            RegisterFunction("sin", new Func<double, double>(Math.Sin), "Returns the sine of an angle");
            RegisterFunction("cos", new Func<double, double>(Math.Cos), "Returns the cosine of an angle");
            RegisterFunction("tan", new Func<double, double>(Math.Tan), "Returns the tangent of an angle");
            RegisterFunction("floor", new Func<double, double>(Math.Floor), "Returns the largest integer less than or equal to a number");
            RegisterFunction("ceil", new Func<double, double>(Math.Ceiling), "Returns the smallest integer greater than or equal to a number");
            RegisterFunction("round", new Func<double, double>(Math.Round), "Rounds a number to the nearest integer");

            // Register string functions
            RegisterFunction("length", new Func<string, int>(s => s?.Length ?? 0), "Returns the length of a string");
            RegisterFunction("upper", new Func<string, string>(s => s?.ToUpper() ?? ""), "Converts a string to uppercase");
            RegisterFunction("lower", new Func<string, string>(s => s?.ToLower() ?? ""), "Converts a string to lowercase");
            RegisterFunction("trim", new Func<string, string>(s => s?.Trim() ?? ""), "Removes leading and trailing whitespace");

            // Register utility functions
            RegisterFunction("type", new Func<object, string>(o => o?.GetType().Name ?? "null"), "Returns the type name of an object");
            RegisterFunction("isNull", new Func<object, bool>(o => o == null), "Checks if an object is null");
        }

        private Type GetDelegateType(System.Reflection.MethodInfo method)
        {
            var parameters = method.GetParameters();
            var parameterTypes = parameters.Select(p => p.ParameterType).ToArray();
            var returnType = method.ReturnType;

            if (returnType == typeof(void))
            {
                return parameterTypes.Length switch
                {
                    0 => typeof(Action),
                    1 => typeof(Action<>).MakeGenericType(parameterTypes),
                    2 => typeof(Action<,>).MakeGenericType(parameterTypes),
                    3 => typeof(Action<,,>).MakeGenericType(parameterTypes),
                    4 => typeof(Action<,,,>).MakeGenericType(parameterTypes),
                    _ => typeof(Delegate)
                };
            }
            else
            {
                return parameterTypes.Length switch
                {
                    0 => typeof(Func<>).MakeGenericType(returnType),
                    1 => typeof(Func<,>).MakeGenericType(parameterTypes.Concat(new[] { returnType }).ToArray()),
                    2 => typeof(Func<,,>).MakeGenericType(parameterTypes.Concat(new[] { returnType }).ToArray()),
                    3 => typeof(Func<,,,>).MakeGenericType(parameterTypes.Concat(new[] { returnType }).ToArray()),
                    4 => typeof(Func<,,,,>).MakeGenericType(parameterTypes.Concat(new[] { returnType }).ToArray()),
                    _ => typeof(Delegate)
                };
            }
        }
    }

    /// <summary>
    /// Script context for isolated script execution
    /// </summary>
    public class ScriptContext
    {
        private readonly HobScriptEngine _engine;
        private readonly Dictionary<string, object> _localVariables;

        internal ScriptContext(HobScriptEngine engine)
        {
            _engine = engine;
            _localVariables = new Dictionary<string, object>();
        }

        /// <summary>
        /// Executes a script in this context
        /// </summary>
        /// <param name="script">The script to execute</param>
        /// <returns>The result of the last expression</returns>
        public object Execute(string script)
        {
            // Copy local variables to engine
            foreach (var kvp in _localVariables)
            {
                _engine.SetVariable(kvp.Key, kvp.Value);
            }

            var result = _engine.Execute(script);

            // Update local variables from engine
            foreach (var varName in _engine.GetVariableNames())
            {
                _localVariables[varName] = _engine.GetVariable(varName);
            }

            return result;
        }

        /// <summary>
        /// Sets a variable in this context
        /// </summary>
        /// <param name="name">Variable name</param>
        /// <param name="value">Variable value</param>
        public void SetVariable(string name, object value)
        {
            _localVariables[name] = value;
        }

        /// <summary>
        /// Gets a variable from this context
        /// </summary>
        /// <param name="name">Variable name</param>
        /// <returns>Variable value or null if not found</returns>
        public object GetVariable(string name)
        {
            return _localVariables.TryGetValue(name, out var value) ? value : null;
        }

        /// <summary>
        /// Clears all variables in this context
        /// </summary>
        public void ClearVariables()
        {
            _localVariables.Clear();
        }
    }
}
