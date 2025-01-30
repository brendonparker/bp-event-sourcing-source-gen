using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace CustomEventSourcing.SourceGenerators;

[Generator]
internal class CustomEventSourcingIncrementalSourceGenerator : IIncrementalGenerator
{
    private const string AllowConstructionFromEventsAttribute =
        "CustomEventSourcing.AllowConstructionFromEventsAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static ctx => ctx.AddSource(
            "AllowConstructionFromEventsAttribute.g.cs",
            SourceText.From(SourceGenerationHelper.Attribute, Encoding.UTF8)));

        context.RegisterPostInitializationOutput(static ctx => ctx.AddSource(
            "EventSourceFactory.g.cs",
            SourceText.From(SourceGenerationHelper.EventSourceFactory, Encoding.UTF8)));

        context.RegisterPostInitializationOutput(static ctx => ctx.AddSource(
            "EventSourceFactoryRegistrations.g.cs",
            SourceText.From(SourceGenerationHelper.Registrations, Encoding.UTF8)));

        var applyMethods = context.CompilationProvider
            .Select(static (compilation, _) => FindApplyMethods(compilation).ToImmutableArray());

        context.RegisterSourceOutput(applyMethods,
            (spc, source) => GenerateCodeFromAttributes2(source, spc));
    }

    private static IEnumerable<ApplyMethod> FindApplyMethods(Compilation compilation)
    {
        foreach (var attr in compilation.Assembly.GetAttributes().Where(attr =>
                     attr.AttributeClass?.ToDisplayString() == AllowConstructionFromEventsAttribute))
        {
            if (attr.ConstructorArguments[0].Value is not INamedTypeSymbol type) continue;

            var typeName = type.ToDisplayString();

            var singleParamCtor = type.Constructors.FirstOrDefault(x => x.Parameters.Length == 1);
            if (singleParamCtor != null)
            {
                yield return new ApplyMethod(typeName, "", singleParamCtor.Parameters[0].Type.ToDisplayString(),
                    IsConstructor: true);
            }

            foreach (var applyMethod in
                     type
                         .GetMembers("Apply")
                         .OfType<IMethodSymbol>()
                         .Where(x => x.Parameters.Length == 1 && x.DeclaredAccessibility == Accessibility.Public))
            {
                yield return new ApplyMethod(typeName, "Apply", applyMethod.Parameters[0].Type.ToDisplayString());
            }
        }
    }


    private void GenerateCodeFromAttributes2(ImmutableArray<ApplyMethod> applyMethods, SourceProductionContext context)
    {
        foreach (var group in applyMethods.GroupBy(x => x.TypeName))
        {
            var typeName = group.Key;
            var className = group.Key.Replace('.', '_');

            var sb = new StringBuilder();
            sb.AppendLine(
                $$"""
                  #nullable enable

                  using System.Text.Json;
                  using CustomEventSourcing.Lib;

                  namespace CustomEventSourcing.Generated;

                  public static partial class EventSourceFactoryRegistrations
                  {
                      public static {{typeName}} {{className}}_FromEvents(IReadOnlyList<Event> events)
                      {
                          {{typeName}}? result = null;
                          foreach(var @event in events)
                          {
                              switch (@event.DotnetType)
                              {
                  """);

            foreach (var method in group)
            {
                if (method.IsConstructor)
                {
                    sb.AppendLine(
                        $$"""
                                          case "{{method.ParameterType}}":
                                              if (result != null) throw new InvalidOperationException("Object already constructed.");
                                              result = new {{typeName}}(JsonSerializer.Deserialize<{{method.ParameterType}}>(@event.Data)!);
                                              break;
                          """);
                }
                else
                {
                    sb.AppendLine(
                        $$"""
                                          case "{{method.ParameterType}}":
                                              if (result == null) throw new InvalidOperationException("Object has not been constructed yet"); 
                                              result.{{method.MethodName}}(JsonSerializer.Deserialize<{{method.ParameterType}}>(@event.Data)!);
                                              break;
                          """);
                }
            }

            sb.AppendLine(
                $$"""
                              }
                          }
                          
                          return result ?? throw new InvalidOperationException();
                      }
                  }    
                  """);

            context.AddSource($"EventSourceFactoryRegistrations_{className}.g.cs",
                SourceText.From(sb.ToString(), Encoding.UTF8));
        }

        var sb2 = new StringBuilder();
        sb2.AppendLine(
            $$"""
              using CustomEventSourcing.Generated;

              namespace CustomEventSourcing.Lib;

              public static partial class EventSourceFactory
              {
                  private static bool isInitialized = false;
                  private static void Init()
                  {
                      if (isInitialized) return;
                      isInitialized = true;
              """);

        foreach (var typeName in applyMethods.Select(x => x.TypeName).Distinct())
        {
            string className = typeName.Replace('.', '_');

            sb2.AppendLine(
                $$"""
                          RegisterIfNotRegistered<{{typeName}}>(
                              EventSourceFactoryRegistrations.{{className}}_FromEvents
                          );
                  """);
        }

        sb2.AppendLine(
            $$"""
                  }
              }
              """);


        context.AddSource($"EventSourceFactory_Init.g.cs",
            SourceText.From(sb2.ToString(), Encoding.UTF8));
    }
}

public record struct ApplyMethod(
    string TypeName,
    string MethodName,
    string ParameterType,
    bool IsConstructor = false);