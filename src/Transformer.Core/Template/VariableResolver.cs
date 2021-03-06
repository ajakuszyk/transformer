using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Transformer.Core.Logging;
using Transformer.Core.Model;

namespace Transformer.Core.Template
{
    /// <summary>
    /// Variable Syntax:
    /// 
    /// ${host}             without default value
    /// ${host:localhost}   with default value localhost
    /// 
    /// Conditionals:
    /// <!--[if ${condition}]-->
    /// gugus
    /// <!--[endif]-->
    /// 
    /// if condition variable is true, on, enabled or 1 -> gugus will be used, otherwise not.
    ///
    /// <!--[if empty ${variable}]--> will be used if variable does not exists or is empty
    /// <!--[if not empty ${variable}]--> will be used if variable exists and is not empty
    /// </summary>
    public class VariableResolver
    {
        private IList<Variable> Variables { get; set; }
        private static readonly string[] TrueStrings = { "TRUE", "ON", "1", "ENABLED" };
        private static readonly ILog Log = LogManager.GetLogger(typeof(VariableResolver));
        public IList<VariableUsage> VariableUsageList { get; private set; }

        public static readonly Regex VariableRegex = new Regex(@"\$\{(?<Name>[^\}]+)\}", RegexOptions.Compiled);
        private static readonly Regex NormalizeRegex = new Regex(@"\r\n|\n\r|\n|\r", RegexOptions.Compiled);
        private static readonly Regex ConditionalOpenRegex = new Regex(@"<!--\s*\[if (?<negate>not)?\s*(?<empty>empty)?\s*(?<expression>[^\]]*)]\s*-->", RegexOptions.Compiled);
        private static readonly Regex ConditionalCloseRegex = new Regex(@"<!--\s*\[endif]\s*-->", RegexOptions.Compiled);

        public VariableResolver(IList<Variable> variables)
        {
            Variables = variables;
            VariableUsageList = new List<VariableUsage>();
        }

        public string TransformVariables(string content)
        {
            var transformedVariables = VariableRegex.Replace(content, ReplaceVariables);
            
            return ParseConditional(transformedVariables);
        }

        private string ReplaceVariables(Match match)
        {
            var variableUsage = new VariableUsage(match.Groups["Name"].Value, Variables);

            VariableUsageList.Add(variableUsage);

            var parsed = variableUsage.GetValueOrDefault(Variables);

            while (VariableRegex.IsMatch(parsed))
            {
                parsed = TransformVariables(parsed);
            }

            return parsed;
        }

        public string Resolve(string variableName)
        {
            var variable = Variables.FirstOrDefault(v => v.Name.ToUpperInvariant() == variableName.ToUpperInvariant());

            if (variable == null)
                return null;

            var parsed = variable.Value;

            while (VariableRegex.IsMatch(parsed))
            {
                parsed = TransformVariables(parsed);
            }

            return parsed;
        }

        private string ParseConditional(string input)
        {
            // normalize line endings in order to be able to split line by line
            var normalized = NormalizeRegex.Replace(input, e => "\r\n");

            var transformed = new StringBuilder();
            bool lastCondition = false;
            bool insideCondition = false;

            foreach (var line in normalized.Split(new [] { System.Environment.NewLine}, StringSplitOptions.None))
            {
                var conditionalStatementMatch = ConditionalOpenRegex.Match(line);

                if (conditionalStatementMatch.Success)
                {
                    var variableBoolValue = TrueStrings.Contains(conditionalStatementMatch.Groups["expression"].Value.ToUpperInvariant());
                    var variableEmpty = string.IsNullOrWhiteSpace(conditionalStatementMatch.Groups["expression"]?.Value);

                    if (conditionalStatementMatch.Groups["negate"].Success)
                    {
                        lastCondition = conditionalStatementMatch.Groups["empty"].Success ? !variableEmpty : !variableBoolValue;
                    }
                    else
                    {
                        lastCondition = conditionalStatementMatch.Groups["empty"].Success ? variableEmpty : variableBoolValue;
                    }

                    insideCondition = true;
                }
                else if(ConditionalCloseRegex.IsMatch(line))
                {
                    // <!-- [endif] -->
                    insideCondition = false;
                }
                else if (lastCondition || !insideCondition)
                {
                    transformed.AppendLine(line);
                }
            }

            return transformed.ToString().Replace("_$_", "$").TrimEnd();
        }
    }
}