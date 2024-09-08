using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MiniFB.SourceGeneration;

partial class SourceGenerator
{
    private const string LibraryIdentifierFormat = @"_Lib{0}";
    private const string SymbolIdentifierFormat = @"_Sym{0}";
    private const string ConditionLocalFormat = @"b{0}";

    private const string GeneratedImportsOutputFileName = $"{nameof(MiniFB)}.{nameof(SourceGeneration)}.NativeImports.g.cs";

    private static readonly DiagnosticDescriptor mCompilationDoesNotAllowUnsafeDescriptor = new(
        id: $"{DiagnosticDescriptorIdPrefix}0001",
        title: "Compilation does not allow unsafe regions",
        messageFormat: "The compilation does not allow for usage of unsafe regions/blocks. This is needed in order for the generated source code to work. Allow it by adding \"<AllowUnsafeBlocks>true</AllowUnsafeBlocks>\" to the project file, or by compiling with the \"-unsafe\" option.",
        category: DiagnosticDescriptorCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    private static readonly SymbolDisplayFormat DefaultTypeSymbolDisplayFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        delegateStyle: SymbolDisplayDelegateStyle.NameOnly,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier | SymbolDisplayMiscellaneousOptions.CollapseTupleTypes
    );

    private static void GenerateNativeImportsSource(SourceProductionContext spc, (Compilation compilation, (ImmutableArray<ImportSymbolData> symbolImports, (ImmutableArray<ImportSymbolData> conditionalSymbolImports, (ImmutableArray<ImportFunctionData> functionImports, ImmutableArray<ImportFunctionData> conditionalFunctionImports)))) data)
    {
        var (compilation, (symbolImports, (conditionalSymbolImports, (functionImports, conditionalFunctionImports)))) = data;

        if (symbolImports.IsDefaultOrEmpty && conditionalSymbolImports.IsDefaultOrEmpty && functionImports.IsDefaultOrEmpty && conditionalFunctionImports.IsDefaultOrEmpty)
        {
            return;
        }

        if (compilation is not CSharpCompilation { Options.AllowUnsafe: true })
        {
            spc.ReportDiagnostic(Diagnostic.Create(mCompilationDoesNotAllowUnsafeDescriptor,
                location: default
            ));

            return;
        }

        var libraries = new Dictionary<ITypeSymbol, (Dictionary<string, List<ImportData>> unconditionalImports, Dictionary<ITypeSymbol, Dictionary<string, List<ImportData>>> conditionalImports)>(SymbolEqualityComparer.Default);
        var buildTree = new BuildTree();

        foreach (var import in symbolImports.AsEnumerable<ImportData>().Concat(conditionalSymbolImports.AsEnumerable<ImportData>()).Concat(functionImports.AsEnumerable<ImportData>()).Concat(conditionalFunctionImports.AsEnumerable<ImportData>()))
        {
            if (buildTree.TryAddImportData(import, spc, compilation))
            {
                if (!libraries.TryGetValue(import.ImportLibraryType, out var library))
                {
                    libraries.Add(key: import.ImportLibraryType, library = (unconditionalImports: [], conditionalImports: new(SymbolEqualityComparer.Default)));
                }

                Dictionary<string, List<ImportData>> importsBySymbolName;
                if (import.ConditionType is null)
                {
                    importsBySymbolName = library.unconditionalImports;
                }
                else
                {
                    if (!library.conditionalImports.TryGetValue(import.ConditionType, out importsBySymbolName))
                    {
                        library.conditionalImports.Add(key: import.ConditionType, importsBySymbolName = []);
                    }
                }

                if (!importsBySymbolName.TryGetValue(import.SymbolName, out var imports))
                {
                    importsBySymbolName.Add(key: import.SymbolName, imports = []);
                }

                imports.Add(import);
            }
        }

        buildTree.Consolidate();

        if (buildTree.IsEmpty)
        {
            return;
        }

        var builder = new StringBuilder();

        builder.Append($$"""
            #nullable enable

            """);

        var libraryId = 0;

        foreach (var (importLibraryType, (unconditionalImports, conditionalImports)) in libraries)
        {
            if (unconditionalImports.Count is not > 0 && conditionalImports.Sum(static t => t.Value.Count) is not > 0)
            {
                continue;
            }

            var libraryIdentifier = string.Format(CultureInfo.InvariantCulture, LibraryIdentifierFormat, libraryId++);
            var importLibraryTypeName = importLibraryType.ToDisplayString(DefaultTypeSymbolDisplayFormat);

            builder.Append($$"""

                [global::System.CodeDom.Compiler.GeneratedCode("{{Tool.Name}}", "{{Tool.Version}}")]
                file static class {{libraryIdentifier}}
                {
                """);

            var symbolId = 0;

            foreach (var (symbolName, imports) in unconditionalImports)
            {
                var symbolIdentifier = string.Format(CultureInfo.InvariantCulture, SymbolIdentifierFormat, symbolId++);

                foreach (var import in imports)
                {
                    import.LibraryImplementationName = libraryIdentifier;
                    import.SymbolName = symbolIdentifier;
                }

                builder.Append($$"""

                        internal static readonly global::System.IntPtr {{symbolIdentifier}};

                    """);
            }

            foreach (var (_, importsBySymbolName) in conditionalImports)
            {
                foreach (var (symbolName, imports) in importsBySymbolName)
                {
                    var symbolIdentifier = string.Format(CultureInfo.InvariantCulture, SymbolIdentifierFormat, symbolId++);

                    foreach (var import in imports)
                    {
                        import.LibraryImplementationName = libraryIdentifier;
                        import.SymbolName = symbolIdentifier;
                    }

                    builder.Append($$"""

                            internal static readonly global::System.IntPtr {{symbolIdentifier}};

                        """);
                }
            }

            builder.Append($$"""

                    [global::System.Runtime.CompilerServices.SkipLocalsInit]
                    [global::System.Runtime.CompilerServices.ModuleInitializer]
                    internal static void ModuleInitializer()
                    {
                        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining | global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
                        [global::System.Runtime.CompilerServices.SkipLocalsInit]
                        static bool tryLoadLibrary(out global::System.IntPtr libraryHandle)
                        {
                            do
                            {
                                var (libraryName, searchPath) = global::{{NativeImportAttributesNamespaceName}}.{{NativeImportLibraryTypeName}}.{{NativeImportLibraryGetLibraryNameAndSearchPathMethodName}}<{{importLibraryTypeName}}>();

                                global::System.Runtime.ExceptionServices.ExceptionDispatchInfo? info;

                                if (!string.IsNullOrWhiteSpace(libraryName))
                                {
                                    try
                                    {
                                        libraryHandle = global::System.Runtime.InteropServices.NativeLibrary.Load(libraryName, typeof(global::{{libraryIdentifier}}).Assembly, searchPath);

                                        return true;
                                    }
                                    catch (global::System.Exception exception)
                                    {
                                        info = global::System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception);
                                    }
                                }
                                else
                                {
                                    info = null;
                                }

                                if (!global::{{NativeImportAttributesNamespaceName}}.{{NativeImportLibraryTypeName}}.{{NativeImportLibraryHandleLibraryImportErrorMethodName}}<{{importLibraryTypeName}}>(libraryName, searchPath, info))
                                {
                                    global::System.Runtime.CompilerServices.Unsafe.SkipInit(out libraryHandle);

                                    return false;
                                }
                            }
                            while (true);
                        }

                        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining | global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
                        [global::System.Runtime.CompilerServices.SkipLocalsInit]
                        static bool tryLoadSymbol(global::System.IntPtr libraryHandle, string? symbolName, scoped ref readonly global::System.IntPtr symbolHandle)
                        {
                            global::System.Runtime.ExceptionServices.ExceptionDispatchInfo? info;

                            if (!string.IsNullOrWhiteSpace(symbolName))
                            {
                                try
                                {
                                    global::System.Runtime.CompilerServices.Unsafe.AsRef(in symbolHandle) = global::System.Runtime.InteropServices.NativeLibrary.GetExport(libraryHandle, symbolName);

                                    return true;
                                }
                                catch (global::System.Exception exception)
                                {
                                    info = global::System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception);
                                }
                            }
                            else
                            {
                                info = null;
                            }

                            return global::{{NativeImportAttributesNamespaceName}}.{{NativeImportLibraryTypeName}}.{{NativeImportLibraryHandleSymbolImportErrorMethodName}}<{{importLibraryTypeName}}>(symbolName, info);
                        }

                """);

            int conditionId;

            if (unconditionalImports.Count is not > 0)
            {
                conditionId = 0;
                foreach (var (conditionType, importsBySymbolName) in conditionalImports)
                {
                    if (importsBySymbolName.Count is not > 0)
                    {
                        continue;
                    }

                    builder.Append($$"""
                        
                                var {{string.Format(CultureInfo.InvariantCulture, ConditionLocalFormat, conditionId++)}} = global::{{NativeImportAttributesNamespaceName}}.{{NativeImportConditionTypeName}}.{{NativeImportConditionEvaluateMethodName}}<{{conditionType.ToDisplayString(DefaultTypeSymbolDisplayFormat)}}>();
                        """);
                }

                if (conditionId is > 0)
                {
                    builder.Append($$"""


                                if (!({{string.Join(" || ", Enumerable.Range(0, conditionId).Select(static id => string.Format(CultureInfo.InvariantCulture, ConditionLocalFormat, id)))}}))
                                {
                                    return;
                                }

                        """);
                }
            }

            builder.Append($$"""

                        if (!tryLoadLibrary(out var handle))
                        {
                            return;
                        }

                """);

            foreach (var (symbolName, imports) in unconditionalImports)
            {
                if (imports is not [{ SymbolName: var symbolIdentifier }, ..])
                {
                    continue;
                }

                builder.Append($$"""

                            if (!tryLoadSymbol(handle, "{{symbolName}}", in {{symbolIdentifier}}))
                            {
                                return;
                            }

                    """);
            }

            conditionId = 0;
            foreach (var (conditionType, importsBySymbolName) in conditionalImports)
            {
                if (importsBySymbolName.Count is not > 0)
                {
                    continue;
                }

                builder.Append($$"""

                            if ({{unconditionalImports.Count switch
                {
                    > 0 => $"global::{NativeImportAttributesNamespaceName}.{NativeImportConditionTypeName}.{NativeImportConditionEvaluateMethodName}<{conditionType.ToDisplayString(DefaultTypeSymbolDisplayFormat)}>()",
                    _ => string.Format(CultureInfo.InvariantCulture, ConditionLocalFormat, conditionId++)
                }}})
                            {
                    """);

                foreach (var (symbolName, imports) in importsBySymbolName)
                {
                    if (imports is not [{ SymbolName: var symbolIdentifier }, ..])
                    {
                        continue;
                    }

                    builder.Append($$"""

                                    if (!tryLoadSymbol(handle, "{{symbolName}}", in {{symbolIdentifier}}))
                                    {
                                        return;
                                    }

                        """);
                }

                builder.Append("""
                            }

                    """);
            }

            builder.Append("""
                    }
                }

                """);
        }

        buildTree.Print(builder, compilation);

        builder.Append("""

            #nullable restore
            """);

        spc.AddSource(GeneratedImportsOutputFileName, SourceText.From(text: builder.ToString(), encoding: Encoding.UTF8));
    }
}
