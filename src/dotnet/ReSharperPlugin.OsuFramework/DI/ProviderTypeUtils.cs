using JetBrains.Metadata.Reader.Impl;
using JetBrains.ReSharper.Psi;

namespace ReSharperPlugin.OsuFramework.Providers;

public static class ProviderTypeUtils
{
    public static bool HasResolvedAttribute(this IAttributesSet holder)
    {
        return holder.HasAttributeInstance(new ClrTypeName("osu.Framework.Allocation.ResolvedAttribute"),
            AttributesSource.Self);
    }
}