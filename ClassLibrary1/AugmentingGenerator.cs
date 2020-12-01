using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Generators
{
    [Generator]
    public class AugmentingGenerator : ISourceGenerator
    {
        public Dictionary<string, Type> TypesToBind { get; private set; }

        public void Initialize(GeneratorInitializationContext context)
        {
            // Register a factory that can create our custom syntax receiver
            context.RegisterForSyntaxNotifications(() => new MySyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            //if (!System.Diagnostics.Debugger.IsAttached)
            //    System.Diagnostics.Debugger.Launch();
            //System.Diagnostics.Debugger.Break();

            MySyntaxReceiver syntaxReceiver = (MySyntaxReceiver)context.SyntaxReceiver;

            var metadataLoadContext = new MetadataLoadContext(context.Compilation);
            var assembly = metadataLoadContext.MainAssembly;

            foreach ((MemberAccessExpressionSyntax _, ArgumentSyntax ArgSyntax) bindOverload in syntaxReceiver.BindOverloads)
            {
                SemanticModel compilationSemanticModel = context.Compilation.GetSemanticModel(bindOverload.ArgSyntax.SyntaxTree);

                ITypeSymbol typeSymbol = compilationSemanticModel.GetTypeInfo(bindOverload.ArgSyntax.Expression).Type;

                var accessibility = typeSymbol.DeclaredAccessibility;
                if (accessibility == Accessibility.Private)
                {
                    // TODO support
                    continue;
                }

                Type type = new TypeWrapper(typeSymbol, metadataLoadContext);

                if (type.IsGenericType)
                {
                    // TODO support
                    continue;
                }

                if (!(TypesToBind ??= new Dictionary<string, Type>()).ContainsKey(type.FullName))
                    TypesToBind[type.FullName] = type;
            }

            if (TypesToBind == null)
            {
                return;
            }

            foreach (KeyValuePair<string, Type> pair in TypesToBind)
            {
                Type type = pair.Value;
                if (type.FullName == typeof(object).FullName)
                {
                    // Bind overloads for object already available in the framework
                    continue;
                }

                StringBuilder sb = new();

                sb.Append(@$"using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace {type.Namespace}
{{
    public static class {type.Name}ConfigurationExensions
    {{
        public static void Bind(this IConfiguration configuration, string key, {type.Name} instance)
        {{
            Console.WriteLine(""first overload"");
            configuration.GetSection(key).Bind(instance);
        }}

        public static void Bind(this IConfiguration configuration, {type.Name} instance)
        {{
            Console.WriteLine(""second overload"");
            configuration.Bind(instance, o => {{ }});
        }}

        public static void Bind(this IConfiguration configuration, {type.Name} instance, Action<BinderOptions> configureOptions)
        {{
            Console.WriteLine(""third overload, has simplified logic, TODO: complete binding recursively for nested type."");");

                foreach (PropertyInfo property in type.GetProperties())
                {
                    if (!property.GetType().IsPublic)
                    {
                        // TODO support
                        continue;
                    }

                    sb.AppendLine(@$"
            instance.{property.Name} = configuration.GetValue<{property.PropertyType.FullName}>(""{property.Name}"");");
                }

                sb.Append(@"
        }
    }
}
");
                context.AddSource($"{type.Name}.generated.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
            }
        }
        

        class MySyntaxReceiver : ISyntaxReceiver
        {
            public List<(MemberAccessExpressionSyntax, ArgumentSyntax)> BindOverloads { get; } = new();
            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is MemberAccessExpressionSyntax 
                    { 
                        Name: SimpleNameSyntax { Identifier: { ValueText: "Bind" } },
                        Parent: InvocationExpressionSyntax { ArgumentList: { Arguments: { } arguments } }
                    } mapBindCall)
                {
                    // TODO cleanup condition
                    if (arguments.Count == 1)
                    {
                        BindOverloads.Add((mapBindCall, arguments[0]));
                    }
                    else if (arguments.Count == 2)
                    {
                        if (arguments[1].Expression is SimpleLambdaExpressionSyntax)
                        {
                            BindOverloads.Add((mapBindCall, arguments[0]));
                        }
                        else
                        {
                            // Generating Bind overloads for the second argument type
                            BindOverloads.Add((mapBindCall, arguments[1]));
                        }
                    }
                }
            }
        }
    }
}
