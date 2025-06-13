using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ReSharper.Feature.Services.CSharp.PredictiveDebugger;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.CSharp.Impl;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Search;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.Util;

namespace ReSharperPlugin.OsuFramework.DI;

public static class ProviderFinder
{
    public static IEnumerable<ProviderEntry> SearchForProviders(
        IType expectedType,
        [CanBeNull] IProgressIndicator progressIndicator = null
    )
    {
        progressIndicator ??= NullProgressIndicator.Create();

        var cacheAttrType = getCachedAttributeType(expectedType.GetPsiServices());

        if (cacheAttrType == null)
            yield break;

        IReference[] references;

        using (var subProgress = progressIndicator.CreateSubProgress(1))
            references = expectedType.GetPsiServices().ParallelFinder
                .FindReferences(cacheAttrType, cacheAttrType.GetSearchDomain(), subProgress);


        foreach (var reference in references)
        {
            var attribute = reference.GetTreeNode().Parent as IAttribute;
            if (attribute == null)
                continue;

            var declaration = attribute.FindParentOf<ICSharpDeclaration>();
            if (declaration == null)
                continue;

            var attributeInstance = attribute.GetAttributeInstance();

            if (attributeInstance.NamedParameter("Type").TypeValue?.Equals(expectedType) == true)
            {
                yield return new ProviderEntry(attribute);
                continue;
            }

            if (attributeInstance.PositionParameter(0).TypeValue?.Equals(expectedType) == true)
            {
                yield return new ProviderEntry(attribute);
                continue;
            }

            var declarationType = declaration switch
            {
                IPropertyDeclaration property => property.Type,
                IFieldDeclaration field => field.Type,
                IClassDeclaration clazz => clazz.DeclaredElement.Type(),
                _ => null
            };

            if (declarationType?.Equals(expectedType) == true)
                yield return new ProviderEntry(attribute);
        }

        var scope = expectedType.GetPsiServices().Symbols.GetSymbolScope(LibrarySymbolScope.FULL, true);

        var dependencyContainerType = GetDependencyContainerType(scope);

        var cacheAsMethods = dependencyContainerType.Methods.Where(
                method => method.ShortName == "CacheAs" && method.TypeParametersCount == 1)
            .ToList();

        var searchDomain = cacheAsMethods.FirstOrDefault()?.GetSearchDomain();

        if (searchDomain != null)
        {
            using (var subProcess = progressIndicator.CreateSubProgress(1))
                references = expectedType.GetPsiServices().ParallelFinder
                    .FindReferences(cacheAsMethods, searchDomain, subProcess);

            foreach (var invocation in references.SelectNotNull(it =>
                         it.GetTreeNode().FindParentOf<IInvocationExpression>()))
            {
                var argumentType =
                    invocation.TypeArguments.FirstOrDefault()?.ToIType() ??
                    invocation.Arguments.FirstOrDefault()?.Value?.Type();

                if (argumentType?.Equals(expectedType) == true)
                    yield return new ProviderEntry(invocation);
            }
        }
    }

    public readonly record struct ProviderEntry(ITreeNode Usage);

    private static readonly ClrTypeName CachedAttributeClrName = new("osu.Framework.Allocation.CachedAttribute");

    private static readonly ClrTypeName ResolvedAttributeClrName = new("osu.Framework.Allocation.ResolvedAttribute");

    public static readonly ClrTypeName DependencyContainerClrName = new("osu.Framework.Allocation.DependencyContainer");

    [CanBeNull]
    private static ITypeElement getCachedAttributeType(IPsiServices psiServices) =>
        psiServices.Symbols.GetSymbolScope(LibrarySymbolScope.FULL, true)
            .GetTypeElementsByCLRName(CachedAttributeClrName).FirstOrDefault();

    [CanBeNull]
    private static ITypeElement getResolvedAttributeType(IPsiServices psiServices) =>
        psiServices.Symbols.GetSymbolScope(LibrarySymbolScope.FULL, true)
            .GetTypeElementsByCLRName(CachedAttributeClrName).FirstOrDefault();

    private static ITypeElement GetDependencyContainerType(ISymbolScope scope) =>
        scope.GetTypeElementsByCLRName(DependencyContainerClrName).FirstOrDefault();
}