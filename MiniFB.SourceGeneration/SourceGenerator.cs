﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace MiniFB.SourceGeneration;

[Generator(LanguageNames.CSharp)]
internal sealed partial class SourceGenerator : IIncrementalGenerator
{
	private const string DiagnosticDescriptorIdPrefix = "MFBSRCGEN";
	private const string DiagnosticDescriptorCategory = $"{nameof(MiniFB)}.{nameof(SourceGeneration)}";

	private static readonly (string Name, string Version) Tool = typeof(SourceGenerator).Assembly.GetName() switch { var assemblyName => (assemblyName.Name, assemblyName.Version.ToString(3)) };

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		context.RegisterPostInitializationOutput(GenerateNativeImportAttributesSource);

		context.RegisterImplementationSourceOutput(
			source: context.CompilationProvider
				.Combine(context.SyntaxProvider.ForAttributeWithMetadataName($"{NativeImportAttributesNamespaceName}.{NativeImportSymbolAttributeTypeName}`1",
					predicate: static (node, _) => node is MethodDeclarationSyntax,
					transform: static (gasc, cancellationToken) => new ImportSymbolData(gasc, cancellationToken)
				)
					.Where(static import => import.ImportLibraryType is not null && !string.IsNullOrWhiteSpace(import.SymbolName))
					.Collect()
					.Combine(context.SyntaxProvider.ForAttributeWithMetadataName($"{NativeImportAttributesNamespaceName}.{NativeImportSymbolAttributeTypeName}`2",
						predicate: static (node, _) => node is MethodDeclarationSyntax,
						transform: static (gasc, cancellationToken) => new ImportSymbolData(gasc, cancellationToken)
					)
						.Where(static import => import.ImportLibraryType is not null && !string.IsNullOrWhiteSpace(import.SymbolName))
						.Collect()
						.Combine(context.SyntaxProvider.ForAttributeWithMetadataName($"{NativeImportAttributesNamespaceName}.{NativeImportFunctionAttributeTypeName}`1",
							predicate: static (node, _) => node is MethodDeclarationSyntax,
							transform: static (gasc, cancellationToken) => new ImportFunctionData(gasc, cancellationToken)
						)
							.Where(static import => import.ImportLibraryType is not null && !string.IsNullOrWhiteSpace(import.SymbolName))
							.Collect()
							.Combine(context.SyntaxProvider.ForAttributeWithMetadataName($"{NativeImportAttributesNamespaceName}.{NativeImportFunctionAttributeTypeName}`2",
								predicate: static (node, _) => node is MethodDeclarationSyntax,
								transform: static (gasc, cancellationToken) => new ImportFunctionData(gasc, cancellationToken)
							)
								.Where(static import => import.ImportLibraryType is not null && !string.IsNullOrWhiteSpace(import.SymbolName))
								.Collect()
							)
						)
					)
				),
			action: GenerateNativeImportsSource
		);
	}
}
