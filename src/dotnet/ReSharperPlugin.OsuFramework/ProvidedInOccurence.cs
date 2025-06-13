using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.Occurrences;
using ReSharperPlugin.OsuFramework.DI;

namespace ReSharperPlugin.OsuFramework;

public class ProvidedInOccurence : ReferenceOccurrence
{
    public ProvidedInOccurence([NotNull] ProvideInformation provideInformation, OccurrenceType occurrenceType = OccurrenceType.Occurrence) :
        base(provideInformation.TreeNode, occurrenceType, calculateOccurrenceKinds(provideInformation))
    {
    }

    private static ICollection<OccurrenceKind> calculateOccurrenceKinds(ProvideInformation provideInformation)
    {
        switch (provideInformation.Type)
        {
            case ProvideType.CachedAttribute:
                return [OccurrenceKind.Attribute];
            case ProvideType.CacheAs:
                return [OccurrenceKind.DirectUsage];
            case ProvideType.InheritedCachedAttribute:
                return [OccurrenceKind.Attribute, OccurrenceKind.ExtendedType];
            default:
                return [OccurrenceKind.Other];
        }
    }
}