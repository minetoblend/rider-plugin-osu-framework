using System.Linq;
using JetBrains.Annotations;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Util;

namespace ReSharperPlugin.OsuFramework.Providers;

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
}