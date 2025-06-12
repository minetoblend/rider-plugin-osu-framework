using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ReSharper.Feature.Services.CSharp.PredictiveDebugger;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Search;
using JetBrains.ReSharper.Psi.Util;

namespace ReSharperPlugin.OsuFramework.DI;

public static class ProviderFinder
{
    public static IEnumerable<ProviderEntry> SearchForProviders(
        IType type,
        [CanBeNull] IProgressIndicator progressIndicator
    )
    {
        var cacheAttrType = getCachedAttributeType(type.GetPsiServices());

        if (cacheAttrType == null)
            yield break;

        var references = type.GetPsiServices().ParallelFinder.FindAllReferences(cacheAttrType);

        foreach (var reference in references)
        {
            var field = reference.GetTreeNode().FindParentOf<IFieldDeclaration>();

            if (field?.DeclaredElement != null)
            {
                var cachedAttributes =
                    field.DeclaredElement.GetAttributeInstances(CachedAttributeClrName, AttributesSource.Self);

                foreach (var attribute in cachedAttributes)
                {
                    var providedType = attribute.NamedParameter("Type").TypeValue
                                       ?? attribute.PositionParameter(0).TypeValue
                                       ?? field.Type;

                    if (providedType.Equals(type))
                        yield return new ProviderEntry(field);
                }
            }

            var property = reference.GetTreeNode().FindParentOf<IPropertyDeclaration>();

            if (property?.DeclaredElement != null)
            {
                var cachedAttributes =
                    property.DeclaredElement.GetAttributeInstances(CachedAttributeClrName, AttributesSource.Self);

                foreach (var attribute in cachedAttributes)
                {
                    var providedType = attribute.NamedParameter("Type").TypeValue
                                       ?? attribute.PositionParameter(0).TypeValue
                                       ?? property.Type;

                    if (providedType.Equals(type))
                        yield return new ProviderEntry(property);
                }
            }

            var classDeclaration = reference.GetTreeNode().FindParentOf<IClassDeclaration>();

            if (classDeclaration?.DeclaredElement != null)
            {
                var cachedAttributes =
                    classDeclaration.DeclaredElement.GetAttributeInstances(CachedAttributeClrName,
                        AttributesSource.Self);

                foreach (var attribute in cachedAttributes)
                {
                    var providedType = attribute.NamedParameter("Type").TypeValue
                                       ?? attribute.PositionParameter(0).TypeValue
                                       ?? classDeclaration.DeclaredElement.Type();

                    if (providedType?.Equals(type) == true)
                        yield return new ProviderEntry(classDeclaration);
                }
            }
        }
    }

    public readonly record struct ProviderEntry(ICSharpDeclaration Declaration);

    private static readonly ClrTypeName CachedAttributeClrName = new("osu.Framework.Allocation.CachedAttribute");

    private static readonly ClrTypeName ResolvedAttributeClrName = new("osu.Framework.Allocation.ResolvedAttribute");

    [CanBeNull]
    private static ITypeElement getCachedAttributeType(IPsiServices psiServices) =>
        psiServices.Symbols.GetSymbolScope(LibrarySymbolScope.FULL, true)
            .GetTypeElementsByCLRName(CachedAttributeClrName).FirstOrDefault();

    [CanBeNull]
    private static ITypeElement getResolvedAttributeType(IPsiServices psiServices) =>
        psiServices.Symbols.GetSymbolScope(LibrarySymbolScope.FULL, true)
            .GetTypeElementsByCLRName(CachedAttributeClrName).FirstOrDefault();
}