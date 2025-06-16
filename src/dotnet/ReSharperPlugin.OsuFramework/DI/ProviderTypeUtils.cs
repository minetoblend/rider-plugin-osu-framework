using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ReSharper.Feature.Services.CSharp.PredictiveDebugger;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Impl;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;

namespace ReSharperPlugin.OsuFramework.DI;

public static class ProviderTypeUtils
{
    private static ClrTypeName ResolvedAttributeClrName = new ClrTypeName("osu.Framework.Allocation.ResolvedAttribute");

    public static bool HasResolvedAttribute(this IAttributesSet holder) =>
        holder.HasAttributeInstance(ResolvedAttributeClrName, AttributesSource.Self);

    [CanBeNull]
    public static IAttributeInstance GetResolvedAttribute(this IAttributesSet holder) =>
        holder.GetAttributeInstances(ResolvedAttributeClrName, AttributesSource.Self).FirstOrDefault();

    [CanBeNull]
    public static IType GetResolvedType(this ITypeMember member)
    {
        var attribute = member.GetAttributeInstances(ResolvedAttributeClrName, AttributesSource.Self).FirstOrDefault();

        if (attribute == null)
            return null;

        return attribute.NamedParameter("Type").TypeValue ??
               attribute.PositionParameter(0).TypeValue ??
               member.Type();
    }

    public static IList<IDeclaration> GetDeclarations(this IType type) =>
        type?.GetScalarType()?.Resolve().DeclaredElement?.GetDeclarations() ?? ImmutableList<IDeclaration>.Empty;

    public static bool GetProvidedType(this IAttribute attribute,
        [MaybeNullWhen(false)] out IType type,
        [MaybeNullWhen(false)] out IDeclaration declaration,
        out bool isExplicit)
    {
        declaration = attribute.FindParentOf<IDeclaration>();
        if (declaration == null)
        {
            type = null;
            isExplicit = false;
            return false;
        }

        var attributeInstance = attribute.GetAttributeInstance();
        if (attributeInstance.NamedParameter("Type").TypeValue is { } namedValue)
        {
            type = namedValue;
            isExplicit = true;
            return true;
        }

        if (attributeInstance.PositionParameter(0).TypeValue is { } positionalValue)
        {
            type = positionalValue;
            isExplicit = true;
            return true;
        }

        type = declaration switch
        {
            IPropertyDeclaration property => property.Type,
            IFieldDeclaration field => field.Type,
            IClassDeclaration clazz => clazz.DeclaredElement.Type(),
            _ => null
        };
        isExplicit = false;
        return type != null;
    }

    [CanBeNull]
    public static IType GetResolvedType(this IPropertyDeclaration declaration)
    {
        var attribute = declaration.Attributes.FirstOrDefault(attr =>
            attr.GetAttributeInstance().GetAttributeType().GetClrName().Equals(ResolvedAttributeClrName));

        if (attribute == null)
            return null;


        attribute.GetProvidedType(out IType type, out _, out _);

        return type;
    }
}