using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace Generators
{
    [Generator]
    public class AugmentingGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // Register a factory that can create our custom syntax receiver
            context.RegisterForSyntaxNotifications(() => new MySyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // the generator infrastructure will create a receiver and populate it
            // we can retrieve the populated instance via the context
            MySyntaxReceiver syntaxReceiver = (MySyntaxReceiver)context.SyntaxReceiver;
            // get the recorded user class
            ClassDeclarationSyntax customOptions = syntaxReceiver.ClassToAugment;
            if (customOptions is null)
            {
                // if we didn't find the user class, there is nothing to do
                return;
            }

            // add the generated implementation to the compilation
            SourceText sourceText = SourceText.From($@"
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CustomOptionsNamespace
{{
    public static class CustomOptionsConfigurationExensions
    {{
        public static void Bind<TOptions>(this IConfiguration configuration, string key, TOptions options)
            where TOptions : {customOptions.Identifier}
        {{
            configuration.GetSection(key).Bind(options);
        }}

        public static void Bind<TOptions>(this IConfiguration configuration, TOptions options)
            where TOptions : {customOptions.Identifier}
        {{
            // Generated code
            options.CustomProperty = ""uncomment the following!"";//configuration[nameof(options.CustomProperty)];
        }}
    }}
}}

", Encoding.UTF8);
            context.AddSource("CustomOptions.Generated.cs", sourceText);
        }

        class MySyntaxReceiver : ISyntaxReceiver
        {
            public ClassDeclarationSyntax ClassToAugment { get; private set; }

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                // Business logic to decide what we're interested in goes here
                if (syntaxNode is ClassDeclarationSyntax cds &&
                    cds.Identifier.ValueText == "CustomOptions")
                {
                    ClassToAugment = cds;
                }
            }
        }
    }
}
