using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.Occurrences;
using ReSharperPlugin.OsuFramework.DI;

namespace ReSharperPlugin.OsuFramework;

public class ProvidedInOccurrence : ReferenceOccurrence
{
    private readonly ProvideInformation provideInformation;

    public ProvidedInOccurrence([NotNull] ProvideInformation provideInformation,
        OccurrenceType occurrenceType = OccurrenceType.Occurrence) :
        base(provideInformation.TreeNode, occurrenceType, calculateOccurrenceKinds(provideInformation))
    {
        this.provideInformation = provideInformation;

        switch (provideInformation.Type)
        {
            case ProvideType.InheritedCachedAttribute:
                PresentationOptions = new OccurrencePresentationOptions
                {
                    ContainerStyle = ContainerDisplayStyle.ContainingTypeWithArrow,
                    TextDisplayStyle = TextDisplayStyle.Identifier,
                    IconDisplayStyle = IconDisplayStyle.OccurrenceEntityType,
                };
                break;
            case ProvideType.CacheAs:
                PresentationOptions = new OccurrencePresentationOptions
                {
                    TextDisplayStyle = TextDisplayStyle.IdentifierAndContext,
                    IconDisplayStyle = IconDisplayStyle.OccurrenceEntityType,
                };
                break;
        }
    }

    private static readonly OccurrenceKind CachedOccurenceKind = OccurrenceKind.CreateSemantic("[Cached]", true);
    private static readonly OccurrenceKind CacheAsOccurenceKind = OccurrenceKind.CreateSemantic("CacheAs()", true);

    private static readonly OccurrenceKind InheritedOccurenceKind =
        OccurrenceKind.CreateSemantic("[Cached] inherited", true);

    private static ICollection<OccurrenceKind> calculateOccurrenceKinds(ProvideInformation provideInformation)
    {
        switch (provideInformation.Type)
        {
            case ProvideType.CachedAttribute:
                return [CachedOccurenceKind];
            case ProvideType.CacheAs:
                return [CacheAsOccurenceKind];
            case ProvideType.InheritedCachedAttribute:
                return [InheritedOccurenceKind];
            default:
                return [OccurrenceKind.Other];
        }
    }

    public ProvideType ProvideType => provideInformation.Type;
}