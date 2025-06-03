using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ReSharper.Daemon.CSharp.Stages.Resolve;
using JetBrains.ReSharper.Feature.Services.CSharp.PredictiveDebugger;
using JetBrains.ReSharper.Feature.Services.Occurrences;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Search;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.TestFramework.Components.Settings;
using JetBrains.Util;

namespace ReSharperPlugin.OsuFramework;

public static class ProviderNavigator
{
    public static readonly ClrTypeName CachedAttributeClrName =
        new ClrTypeName("osu.Framework.Allocation.CachedAttribute");

    public static readonly ClrTypeName ResolvedAttributeClrName =
        new ClrTypeName("osu.Framework.Allocation.ResolvedAttribue");

    public static readonly ClrTypeName DependencyContainerClrName =
        new ClrTypeName("osu.Framework.Allocation.DependencyContainer");

    [CanBeNull]
    public static IType GetProvidedType(IPropertyDeclaration declaration)
    {
        return !declaration.Attributes.Any(attr => attr.Name.ShortName == "Resolved")
            ? null
            : declaration.Type;
    }

    public static IEnumerable<IOccurrence> GetValueProviders(IPropertyDeclaration declaration, IType valueType)
    {
        var psiServices = declaration.GetPsiServices();

        var classDeclarations = GetProvidedThroughAttribute(psiServices, valueType);


        return classDeclarations.Concat(GetProvidedThroughCacheAs(psiServices, valueType));
    }

    private static IEnumerable<IOccurrence> GetProvidedThroughCacheAs(IPsiServices psiServices, IType expectedType)
    {
        var scope = psiServices.Symbols.GetSymbolScope(LibrarySymbolScope.FULL, true);

        var dependencyContainerType = GetDependencyContainerType(scope);

        var cacheAsMethods = dependencyContainerType.Methods.Where(
                method => method.ShortName == "CacheAs" && method.TypeParametersCount == 1)
            .SelectNotNull(method => method.GetSingleDeclaration()?.DeclaredElement)
            .ToList();

        var searchDomain = cacheAsMethods.FirstOrDefault()?.GetSearchDomain();

        if (searchDomain != null)
        {
            var references =
                psiServices.ParallelFinder.FindReferences(cacheAsMethods, searchDomain, NullProgressIndicator.Create());

            foreach (var invocation in references
                         .SelectNotNull(it => it.GetTreeNode().FindParentOf<IInvocationExpression>()))
            {
                var type =
                    invocation.TypeArguments.FirstOrDefault()?.ToIType() ??
                    invocation.Arguments.FirstOrDefault()?.Value?.Type();

                if (type != null && type.Equals(expectedType))
                {
                    yield return CreateOccurence(invocation);
                }
            }
        }
    }

    private static IEnumerable<IOccurrence> GetProvidedThroughAttribute(IPsiServices psiServices, IType valueType)
    {
        var scope = psiServices.Symbols.GetSymbolScope(LibrarySymbolScope.FULL, true);

        var attributeElement = GetCachedAttributeType(scope);
        if (attributeElement == null)
            return [];

        var references = psiServices.ParallelFinder.FindAllReferences(attributeElement);

        return references
            .SelectNotNull(GetOccurence).ToList();

        [CanBeNull]
        IOccurrence GetOccurence(IReference reference)
        {
            var attribute = reference.GetTreeNode().Parent as IAttribute;
            if (attribute == null)
                return null;

            if (GetTypeArgument(attribute, valueType) is { } argumentType)
            {
                if (valueType.Equals(argumentType))
                    return CreateOccurence(attribute);

                return null;
            }


            var parentDeclaration = attribute.FindParentOf<IAttributeSectionList>()?.Parent;

            return parentDeclaration switch
            {
                IPropertyDeclaration propertyDeclaration
                    when propertyDeclaration.Type.Equals(valueType)
                    => CreateOccurence(attribute),
                IClassDeclaration classDeclaration
                    when classDeclaration.DeclaredElement.Type()?.Equals(valueType) == true
                    => CreateOccurence(attribute),
                _ => null
            };
        }
    }

    private static ProvidedInOccurence CreateOccurence(ITreeNode node)
    {
        var sourceFile = node.GetSourceFile();

        if (sourceFile != null)
        {
            return new ProvidedInOccurence(
                sourceFile,
                node.GetDocumentRange(),
                OccurrenceType.Occurrence,
                new OccurrencePresentationOptions()
            );
        }

        return null;
    }

    private static IType GetTypeArgument(IAttribute attribute, IType expectedType)
    {
        var typeArgument = attribute.Arguments.FirstOrDefault(it => it.ArgumentName == "Type")
                           ?? attribute.Arguments.FirstOrDefault(it => !it.IsNamedArgument);

        return typeArgument?.Value is ITypeofExpression typeofExpression ? typeofExpression.ArgumentType : null;
    }

    // private static IEnumerable<IOccurrence>
    //     GetProvidedThroughField(IReference[] references, IType valueType) =>
    //     references
    //         .SelectNotNull(r => r.GetMultipleFieldDeclaration())
    //         .Where(r => r.HasType(valueType))
    //         .SelectNotNull(r => r.Declarators.FirstOrDefault()?.DeclaredElement)
    //         .Select(r => new ProvidedInOccurence(r))
    //         .ToList();


    [CanBeNull]
    private static ITypeElement GetCachedAttributeType(ISymbolScope scope) =>
        scope.GetTypeElementsByCLRName(CachedAttributeClrName).FirstOrDefault();

    private static ITypeElement GetDependencyContainerType(ISymbolScope scope) =>
        scope.GetTypeElementsByCLRName(DependencyContainerClrName).FirstOrDefault();

    public static ITypeElement GetResolvedAttributeType(ISymbolScope scope) =>
        scope.GetTypeElementsByCLRName(ResolvedAttributeClrName).FirstOrDefault();

    private static IMultipleFieldDeclaration GetMultipleFieldDeclaration(this IReference reference) =>
        GetMultipleFieldDeclaration(reference.GetTreeNode());

    [CanBeNull]
    private static IMultipleFieldDeclaration GetMultipleFieldDeclaration([CanBeNull] this ITreeNode node)
    {
        while (node != null)
        {
            if (node is IMultipleFieldDeclaration declaration)
                return declaration;

            node = node.Parent;
        }

        return null;
    }

    [CanBeNull]
    private static IClassDeclaration GetClassDeclaration(this IReference reference)
    {
        var node = reference.GetTreeNode();

        while (node != null)
        {
            if (node is IClassDeclaration declaration)
                return declaration;

            node = node.Parent;
        }

        return null;
    }

    // private static bool HasType(this IMultipleFieldDeclaration field, IType type) =>
    //     field.
    //     field.TypeUsage is IUserTypeUsage userTypeUsage &&
    //     userTypeUsage.ScalarTypeName.QualifiedName == type.QualifiedName;

    private static bool HasType(this IClassDeclaration classDeclaration, IReferenceName type) =>
        classDeclaration.NameIdentifier.Name == type.NameIdentifier.Name;
}