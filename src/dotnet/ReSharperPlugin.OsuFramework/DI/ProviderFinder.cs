using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ReSharper.Feature.Services.CSharp.PredictiveDebugger;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Search;
using JetBrains.Util;
using ReSharperPlugin.OsuFramework.Providers;

namespace ReSharperPlugin.OsuFramework.DI;

public static class ProviderFinder
{
    private static readonly ClrTypeName CachedAttributeClrName =
        new("osu.Framework.Allocation.CachedAttribute");

    private static readonly ClrTypeName ResolvedAttributeClrName =
        new("osu.Framework.Allocation.ResolvedAttribute");

    private static readonly ClrTypeName DependencyContainerClrName =
        new("osu.Framework.Allocation.DependencyContainer");

    private static readonly ClrTypeName BackgroundDependencyLoaderAttributeClrName =
        new("osu.Framework.Allocation.BackgroundDependencyLoaderAttribute");


    public static IEnumerable<ProvideInformation> SearchForProviders(
        [NotNull] IType expectedType,
        [NotNull] IProgressIndicator progressIndicator
    )
    {
        progressIndicator ??= NullProgressIndicator.Create();

        using (var p = progressIndicator.CreateSubProgress(1))
        {
            foreach (var result in searchForCachedAttributeProviders(expectedType, p))
                yield return result;
        }

        using (var p = progressIndicator.CreateSubProgress(1))
        {
            foreach (var result in searchForCacheAsProviders(expectedType, p))
                yield return result;
        }
    }

    private static IEnumerable<ProvideInformation> searchForCachedAttributeProviders(
        IType expectedType,
        IProgressIndicator progressIndicator
    )
    {
        var cacheAttrType = getCachedAttributeType(expectedType.GetPsiServices());

        if (cacheAttrType == null)
            yield break;

        var references = expectedType.GetPsiServices().ParallelFinder
            .FindReferences(cacheAttrType, cacheAttrType.GetSearchDomain(), progressIndicator);

        foreach (var reference in references)
        {
            var attribute = reference.GetTreeNode().Parent as IAttribute;
            if (attribute == null)
                continue;

            if (attribute?.GetProvidedType(out var providedType, out var declaration, out bool isExplicit) != true)
                continue;

            if (!providedType.Equals(expectedType))
                continue;

            yield return new ProvideInformation(attribute, ProvideType.CacheAs)
            {
                Explicit = isExplicit,
                Declaration = declaration,
            };

            if (declaration is IClassDeclaration { DeclaredElement: { } typeElement })
            {
                var elements = new List<ITypeElement>();

                var symbolScope = typeElement.GetPsiServices().Symbols
                    .GetSymbolScope(LibrarySymbolScope.FULL, true);

                using var subProgress = progressIndicator.CreateSubProgress(0.05);
                declaration.GetPsiServices().ParallelFinder
                    .FindInheritors(typeElement, symbolScope, elements.ConsumeDeclaredElements(),
                        subProgress);

                foreach (var element in elements)
                {
                    if (element.GetDeclarations().FirstOrDefault() is IClassDeclaration elementDeclaration)
                    {
                        yield return new ProvideInformation(elementDeclaration.NameIdentifier,
                            ProvideType.InheritedCachedAttribute)
                        {
                            Explicit = isExplicit,
                            Declaration = elementDeclaration
                        };
                    }
                }
            }
        }
    }

    private static IEnumerable<ProvideInformation> searchForCacheAsProviders(
        IType expectedType,
        IProgressIndicator progressIndicator
    )
    {
        var psiServices = expectedType.GetPsiServices();

        var dependencyContainerType = getDependencyContainerType(psiServices);

        if (dependencyContainerType == null)
            yield break;

        var cacheAsMethods = dependencyContainerType.Methods.Where(
                method => method.ShortName == "CacheAs" && method.TypeParametersCount == 1)
            .ToList();

        if (cacheAsMethods.IsEmpty())
            yield break;

        var references = expectedType.GetPsiServices().ParallelFinder
            .FindReferences(cacheAsMethods, cacheAsMethods.First().GetSearchDomain(), progressIndicator);

        foreach (var invocation in references.SelectNotNull(it =>
                     it.GetTreeNode().FindParentOf<IInvocationExpression>()))
        {
            var argumentType =
                invocation.TypeArguments.FirstOrDefault()?.ToIType() ??
                invocation.Arguments.FirstOrDefault()?.Value?.Type();

            var typeDeclaration = argumentType?.GetDeclarations()?.FirstOrDefault();

            if (argumentType?.Equals(expectedType) == true)
            {
                yield return new ProvideInformation(invocation, ProvideType.CacheAs)
                {
                    Declaration = typeDeclaration,
                    Explicit = true,
                };
            }
        }
    }

    [CanBeNull]
    private static ITypeElement getCachedAttributeType(IPsiServices psiServices) =>
        psiServices.Symbols.GetSymbolScope(LibrarySymbolScope.FULL, true)
            .GetTypeElementsByCLRName(CachedAttributeClrName).FirstOrDefault();

    [CanBeNull]
    private static ITypeElement getResolvedAttributeType(IPsiServices psiServices) =>
        psiServices.Symbols.GetSymbolScope(LibrarySymbolScope.FULL, true)
            .GetTypeElementsByCLRName(CachedAttributeClrName).FirstOrDefault();

    [CanBeNull]
    private static ITypeElement getBackgroundDependencyLoaderAttributeType(IPsiServices psiServices) =>
        psiServices.Symbols.GetSymbolScope(LibrarySymbolScope.FULL, true)
            .GetTypeElementsByCLRName(CachedAttributeClrName).FirstOrDefault();

    [CanBeNull]
    private static ITypeElement getDependencyContainerType(IPsiServices psiServices) =>
        psiServices.Symbols.GetSymbolScope(LibrarySymbolScope.FULL, true)
            .GetTypeElementsByCLRName(DependencyContainerClrName).FirstOrDefault();
}