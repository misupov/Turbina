using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CSharp.RuntimeBinder;
using Turbina.Engine;

namespace Turbina.StandardNodes
{
    public class ExpressionNode : Node
    {
        private string _latestExpression;
        private Script<object> _script;
        private readonly Dictionary<string, Inlet> _variables = new Dictionary<string, Inlet>();
        private int _counter;

        public Inlet<string> Expression { get; set; }
        public Outlet<dynamic> Result { get; set; }

        public ExpressionNode(Workspace workspace) : base(workspace)
        {
        }

        protected override async Task Operate()
        {
            var expression = await Expression;
            if (expression != _latestExpression)
            {
                _latestExpression = expression;
                RebuildScript();
            }

            try
            {
                var result = await _script.RunAsync(await CreateGlobals());
                Result.Send(result.ReturnValue);
            }
            catch (CompilationErrorException e)
            {
                Exception.Send(e);
            }
        }

        private void RebuildScript()
        {
            var options = ScriptOptions.Default
                .AddReferences(
                    typeof(DynamicObject).GetTypeInfo().Assembly,
                    typeof(CSharpArgumentInfo).GetTypeInfo().Assembly,
                    typeof(ExpandoObject).GetTypeInfo().Assembly)
                .AddImports(typeof(Math).Namespace);

            var usedVars = new HashSet<string>();
            var removeUnused = false;
            foreach (var diagnostic in CSharpScript.Create(_latestExpression, options).Compile())
            {
                if (diagnostic.Id == "CS0103")
                {
                    var missedVariable = diagnostic.Location.SourceTree.GetCompilationUnitRoot().FindToken(diagnostic.Location.SourceSpan.Start).Text;
                    usedVars.Add(missedVariable);
                    removeUnused = true;
                }
            }

            var unusedVars = new HashSet<string>(_variables.Keys);
            unusedVars.ExceptWith(usedVars);

            var sb = new StringBuilder();
            foreach (var variableName in usedVars)
            {
                if (!_variables.TryGetValue(variableName, out Inlet inlet))
                {
                    inlet = Inlets.Create("var" + Interlocked.Increment(ref _counter));
                    ((IObserver<object>) inlet).OnNext(null);
                    _variables.Add(variableName, inlet);
                    inlet.SetAttribute("title", variableName);
                    inlet.SetAttribute("is-title-editable", "true");
                }

                sb.AppendLine($"dynamic {variableName} = (dynamic)__globals__[\"{variableName}\"];");
            }

            if (removeUnused)
            {
                foreach (var variableName in unusedVars)
                {
                    Inlets.Destroy(_variables[variableName]);
                    _variables.Remove(variableName);
                }
            }

            var script = CSharpScript
                .Create(sb.ToString(), options, typeof(Globals))
                .ContinueWith(_latestExpression);
            script.Compile();
            _script = script;
        }

        private async Task<Globals> CreateGlobals()
        {
            var globals = new Globals();
            foreach (var variable in _variables)
            {
                var value = await variable.Value.ReceiveOrTakeLatest();
                var variableName = variable.Value.Attributes["title"];
                globals.__globals__[variableName] = value;
            }
            return globals;
        }

        public class Globals
        {
            // ReSharper disable once InconsistentNaming
            public Dictionary<string, object> __globals__ = new Dictionary<string, object>();
        }
    }
}