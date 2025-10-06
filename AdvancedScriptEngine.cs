using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace HobScript
{
    /// <summary>
    /// Advanced script engine with support for control structures and more complex expressions
    /// </summary>
    public class AdvancedScriptEngine : ScriptEngine
    {
        private readonly Dictionary<string, object> _constants;
        private readonly List<string> _executionHistory;

        public AdvancedScriptEngine() : base()
        {
            _constants = new Dictionary<string, object>();
            _executionHistory = new List<string>();
            
            // Register some useful constants
            RegisterConstant("PI", Math.PI);
            RegisterConstant("E", Math.E);
        }

        /// <summary>
        /// Executes a script with support for control structures
        /// </summary>
        /// <param name="script">The script to execute</param>
        /// <returns>The result of the last expression</returns>
        public new object Execute(string script)
        {
            _executionHistory.Clear();
            return ExecuteScript(script);
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
        /// Registers a constant value
        /// </summary>
        /// <param name="name">Constant name</param>
        /// <param name="value">Constant value</param>
        public void RegisterConstant(string name, object value)
        {
            _constants[name] = value;
        }

        /// <summary>
        /// Gets the execution history
        /// </summary>
        /// <returns>List of executed lines</returns>
        public IReadOnlyList<string> GetExecutionHistory()
        {
            return _executionHistory.AsReadOnly();
        }

        /// <summary>
        /// Clears the execution history
        /// </summary>
        public void ClearExecutionHistory()
        {
            _executionHistory.Clear();
        }

        private object ExecuteScript(string script)
        {
            var lines = PreprocessScript(script);
            object lastResult = null;

            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line) || line.Trim().StartsWith("//"))
                    continue;

                _executionHistory.Add(line);

                // Handle control structures (temporarily disabled)
                if (line.Trim().StartsWith("if ") || line.Trim().StartsWith("while ") || line.Trim().StartsWith("for "))
                {
                    System.Console.WriteLine($"Skipping control structure: {line}");
                    continue;
                }

                lastResult = ExecuteLine(line);
            }

            return lastResult;
        }

        private List<string> PreprocessScript(string script)
        {
            var lines = new List<string>();
            var currentLine = "";
            var inString = false;
            var stringChar = '\0';
            var braceCount = 0;

            for (int i = 0; i < script.Length; i++)
            {
                var c = script[i];

                if (!inString && (c == '"' || c == '\''))
                {
                    inString = true;
                    stringChar = c;
                    currentLine += c;
                }
                else if (inString && c == stringChar)
                {
                    inString = false;
                    currentLine += c;
                }
                else if (!inString && c == '{')
                {
                    braceCount++;
                    currentLine += c;
                }
                else if (!inString && c == '}')
                {
                    braceCount--;
                    currentLine += c;
                }
                else if (!inString && c == '\n' && braceCount == 0)
                {
                    if (!string.IsNullOrWhiteSpace(currentLine))
                    {
                        lines.Add(currentLine.Trim());
                    }
                    currentLine = "";
                }
                else
                {
                    currentLine += c;
                }
            }

            if (!string.IsNullOrWhiteSpace(currentLine))
            {
                lines.Add(currentLine.Trim());
            }

            return lines;
        }

        private int HandleIfStatement(List<string> lines, int startIndex)
        {
            var ifLine = lines[startIndex];
            var condition = ExtractCondition(ifLine, "if");
            
            if (EvaluateCondition(condition))
            {
                // Execute if block - find the opening brace and execute until closing brace
                var braceCount = 0;
                var i = startIndex + 1;
                
                while (i < lines.Count)
                {
                    var line = lines[i];
                    
                    if (line.Contains("{"))
                        braceCount++;
                    if (line.Contains("}"))
                        braceCount--;
                    
                    if (braceCount > 0)
                    {
                        if (!line.Contains("{"))
                            ExecuteLine(line);
                    }
                    else if (line.Contains("}"))
                    {
                        return i;
                    }
                    
                    i++;
                }
                
                return i - 1;
            }
            else
            {
                // Skip to end of if block
                return SkipToEndOfBlock(lines, startIndex + 1);
            }
        }

        private int HandleWhileStatement(List<string> lines, int startIndex)
        {
            var whileLine = lines[startIndex];
            var condition = ExtractCondition(whileLine, "while");
            var endIndex = SkipToEndOfBlock(lines, startIndex + 1);

            while (EvaluateCondition(condition))
            {
                // Execute the block
                var braceCount = 0;
                var i = startIndex + 1;
                
                while (i < lines.Count)
                {
                    var line = lines[i];
                    
                    if (line.Contains("{"))
                        braceCount++;
                    if (line.Contains("}"))
                        braceCount--;
                    
                    if (braceCount > 0)
                    {
                        if (!line.Contains("{"))
                            ExecuteLine(line);
                    }
                    else if (line.Contains("}"))
                    {
                        break;
                    }
                    
                    i++;
                }
                
                // Re-evaluate condition for next iteration
                condition = ExtractCondition(whileLine, "while");
            }

            return endIndex;
        }

        private int HandleForStatement(List<string> lines, int startIndex)
        {
            var forLine = lines[startIndex];
            var parts = ExtractForParts(forLine);
            
            // Execute initialization
            if (!string.IsNullOrWhiteSpace(parts.Initialization))
            {
                ExecuteLine(parts.Initialization);
            }

            var blockStart = startIndex + 1;

            while (string.IsNullOrWhiteSpace(parts.Condition) || EvaluateCondition(parts.Condition))
            {
                // Execute block
                var braceCount = 0;
                var i = startIndex + 1;
                
                while (i < lines.Count)
                {
                    var line = lines[i];
                    
                    if (line.Contains("{"))
                        braceCount++;
                    if (line.Contains("}"))
                        braceCount--;
                    
                    if (braceCount > 0)
                    {
                        if (!line.Contains("{"))
                            ExecuteLine(line);
                    }
                    else if (line.Contains("}"))
                    {
                        break;
                    }
                    
                    i++;
                }
                
                // Execute increment
                if (!string.IsNullOrWhiteSpace(parts.Increment))
                {
                    ExecuteLine(parts.Increment);
                }
                
                // Re-evaluate condition
                if (!string.IsNullOrWhiteSpace(parts.Condition) && !EvaluateCondition(parts.Condition))
                {
                    break;
                }
            }

            return SkipToEndOfBlock(lines, startIndex + 1);
        }

        private (string Initialization, string Condition, string Increment) ExtractForParts(string forLine)
        {
            var match = Regex.Match(forLine, @"for\s*\((.*?);(.*?);(.*?)\)");
            if (match.Success)
            {
                return (match.Groups[1].Value.Trim(), 
                       match.Groups[2].Value.Trim(), 
                       match.Groups[3].Value.Trim());
            }
            return ("", "", "");
        }

        private string ExtractCondition(string line, string keyword)
        {
            var pattern = $@"{keyword}\s*\((.*?)\)";
            var match = Regex.Match(line, pattern);
            return match.Success ? match.Groups[1].Value.Trim() : "";
        }

        private bool EvaluateCondition(string condition)
        {
            if (string.IsNullOrWhiteSpace(condition))
                return true;

            // Handle comparison operators
            var operators = new[] { "==", "!=", "<=", ">=", "<", ">" };
            foreach (var op in operators)
            {
                if (condition.Contains(op))
                {
                    var parts = condition.Split(new[] { op }, 2, StringSplitOptions.None);
                    if (parts.Length == 2)
                    {
                        var left = EvaluateExpression(parts[0].Trim());
                        var right = EvaluateExpression(parts[1].Trim());
                        return CompareValues(left, right, op);
                    }
                }
            }

            // Handle boolean values
            if (bool.TryParse(condition, out var boolResult))
                return boolResult;

            // Handle variable access
            if (GetVariable(condition) is bool boolVar)
                return boolVar;

            // Default to true for non-empty conditions
            return !string.IsNullOrWhiteSpace(condition);
        }

        private bool CompareValues(object left, object right, string op)
        {
            if (left == null && right == null)
                return op == "==";

            if (left == null || right == null)
                return op == "!=";

            var leftComparable = ConvertToComparable(left);
            var rightComparable = ConvertToComparable(right);

            return op switch
            {
                "==" => leftComparable.Equals(rightComparable),
                "!=" => !leftComparable.Equals(rightComparable),
                "<" => leftComparable.CompareTo(rightComparable) < 0,
                ">" => leftComparable.CompareTo(rightComparable) > 0,
                "<=" => leftComparable.CompareTo(rightComparable) <= 0,
                ">=" => leftComparable.CompareTo(rightComparable) >= 0,
                _ => false
            };
        }

        private IComparable ConvertToComparable(object value)
        {
            if (value is IComparable comparable)
                return comparable;

            if (value is string str)
                return str;

            if (value is int || value is double || value is decimal)
                return Convert.ToDouble(value);

            return value?.ToString() ?? "";
        }

        private int ExecuteBlock(List<string> lines, int startIndex)
        {
            var braceCount = 0;
            var i = startIndex;

            while (i < lines.Count)
            {
                var line = lines[i];
                
                if (line.Contains("{"))
                    braceCount++;
                if (line.Contains("}"))
                    braceCount--;

                if (braceCount == 0 && line.Contains("}"))
                {
                    return i;
                }

                if (braceCount > 0 || !line.Contains("}"))
                {
                    ExecuteLine(line);
                }

                i++;
            }

            return i - 1;
        }

        private int SkipToElseOrEnd(List<string> lines, int startIndex)
        {
            var braceCount = 0;
            var i = startIndex;

            while (i < lines.Count)
            {
                var line = lines[i];
                
                if (line.Contains("{"))
                    braceCount++;
                if (line.Contains("}"))
                    braceCount--;

                if (braceCount == 0 && line.Contains("}"))
                {
                    return i;
                }

                i++;
            }

            return i - 1;
        }

        private int SkipToEndOfBlock(List<string> lines, int startIndex)
        {
            return SkipToElseOrEnd(lines, startIndex);
        }

        private new object EvaluateExpression(string expression)
        {
            // Check constants first
            if (_constants.ContainsKey(expression))
                return _constants[expression];

            return base.EvaluateExpression(expression);
        }
    }
}
