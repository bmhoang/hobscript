using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HobScript
{
    /// <summary>
    /// Registry for managing custom functions in the script engine
    /// </summary>
    public class FunctionRegistry
    {
        private readonly Dictionary<string, FunctionInfo> _functions;

        public FunctionRegistry()
        {
            _functions = new Dictionary<string, FunctionInfo>();
        }

        /// <summary>
        /// Registers a function with the given name and delegate
        /// </summary>
        /// <param name="name">Function name</param>
        /// <param name="function">Function delegate</param>
        /// <param name="description">Optional description</param>
        public void RegisterFunction(string name, Delegate function, string description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Function name cannot be null or empty", nameof(name));

            if (function == null)
                throw new ArgumentNullException(nameof(function));

            var functionInfo = new FunctionInfo
            {
                Name = name,
                Delegate = function,
                Description = description ?? $"Custom function: {name}",
                ParameterCount = function.Method.GetParameters().Length,
                ParameterTypes = function.Method.GetParameters().Select(p => p.ParameterType).ToArray(),
                ReturnType = function.Method.ReturnType
            };

            _functions[name] = functionInfo;
        }

        /// <summary>
        /// Registers a function from a method info
        /// </summary>
        /// <param name="name">Function name</param>
        /// <param name="method">Method info</param>
        /// <param name="target">Target object for instance methods</param>
        /// <param name="description">Optional description</param>
        public void RegisterFunction(string name, MethodInfo method, object target = null, string description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Function name cannot be null or empty", nameof(name));

            if (method == null)
                throw new ArgumentNullException(nameof(method));

            var function = method.IsStatic 
                ? Delegate.CreateDelegate(GetDelegateType(method), method)
                : Delegate.CreateDelegate(GetDelegateType(method), target, method);

            RegisterFunction(name, function, description);
        }

        /// <summary>
        /// Registers all public static methods from a type
        /// </summary>
        /// <param name="type">Type to register methods from</param>
        /// <param name="prefix">Optional prefix for function names</param>
        public void RegisterType(Type type, string prefix = null)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => !m.IsSpecialName && m.DeclaringType == type);

            foreach (var method in methods)
            {
                var functionName = string.IsNullOrEmpty(prefix) ? method.Name : $"{prefix}.{method.Name}";
                RegisterFunction(functionName, method, description: GetMethodDescription(method));
            }
        }

        /// <summary>
        /// Registers all public instance methods from an object
        /// </summary>
        /// <param name="target">Object to register methods from</param>
        /// <param name="prefix">Optional prefix for function names</param>
        public void RegisterObject(object target, string prefix = null)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            var type = target.GetType();
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => !m.IsSpecialName && m.DeclaringType == type);

            foreach (var method in methods)
            {
                var functionName = string.IsNullOrEmpty(prefix) ? method.Name : $"{prefix}.{method.Name}";
                RegisterFunction(functionName, method, target, GetMethodDescription(method));
            }
        }

        /// <summary>
        /// Gets a function by name
        /// </summary>
        /// <param name="name">Function name</param>
        /// <returns>Function info or null if not found</returns>
        public FunctionInfo GetFunction(string name)
        {
            return _functions.TryGetValue(name, out var function) ? function : null;
        }

        /// <summary>
        /// Gets all registered function names
        /// </summary>
        /// <returns>Collection of function names</returns>
        public IEnumerable<string> GetFunctionNames()
        {
            return _functions.Keys;
        }

        /// <summary>
        /// Gets all registered functions
        /// </summary>
        /// <returns>Collection of function info</returns>
        public IEnumerable<FunctionInfo> GetFunctions()
        {
            return _functions.Values;
        }

        /// <summary>
        /// Checks if a function is registered
        /// </summary>
        /// <param name="name">Function name</param>
        /// <returns>True if function is registered</returns>
        public bool IsFunctionRegistered(string name)
        {
            return _functions.ContainsKey(name);
        }

        /// <summary>
        /// Removes a function from the registry
        /// </summary>
        /// <param name="name">Function name</param>
        /// <returns>True if function was removed</returns>
        public bool UnregisterFunction(string name)
        {
            return _functions.Remove(name);
        }

        /// <summary>
        /// Clears all registered functions
        /// </summary>
        public void Clear()
        {
            _functions.Clear();
        }

        /// <summary>
        /// Gets function help information
        /// </summary>
        /// <param name="name">Function name</param>
        /// <returns>Help string</returns>
        public string GetFunctionHelp(string name)
        {
            var function = GetFunction(name);
            if (function == null)
                return $"Function '{name}' not found";

            var help = $"{name}(";
            var parameters = function.ParameterTypes;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (i > 0) help += ", ";
                help += $"{parameters[i].Name}";
            }
            help += $") -> {function.ReturnType.Name}";
            
            if (!string.IsNullOrEmpty(function.Description))
                help += $"\nDescription: {function.Description}";

            return help;
        }

        private Type GetDelegateType(MethodInfo method)
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

        private string GetMethodDescription(MethodInfo method)
        {
            var description = method.Name;
            var parameters = method.GetParameters();
            if (parameters.Length > 0)
            {
                description += $"({string.Join(", ", parameters.Select(p => $"{p.ParameterType.Name} {p.Name}"))})";
            }
            else
            {
                description += "()";
            }
            description += $" -> {method.ReturnType.Name}";
            return description;
        }
    }

    /// <summary>
    /// Information about a registered function
    /// </summary>
    public class FunctionInfo
    {
        public string Name { get; set; }
        public Delegate Delegate { get; set; }
        public string Description { get; set; }
        public int ParameterCount { get; set; }
        public Type[] ParameterTypes { get; set; }
        public Type ReturnType { get; set; }
    }
}
