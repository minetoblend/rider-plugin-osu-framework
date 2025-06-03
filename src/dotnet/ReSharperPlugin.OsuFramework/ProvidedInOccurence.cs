using JetBrains.Annotations;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Occurrences;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharperPlugin.OsuFramework;

public class ProvidedInOccurence : RangeOccurrence
{
    public ProvidedInOccurence([NotNull] IPsiSourceFile sourceFile, DocumentRange documentRange, OccurrenceType occurrenceType, OccurrencePresentationOptions options) : base(sourceFile, documentRange, occurrenceType, options)
    {
    }

    public ProvidedInOccurence([NotNull] IReference reference, [CanBeNull] IDeclaredElement target, OccurrenceType occurrenceType) : base(reference, target, occurrenceType)
    {
    }

    protected ProvidedInOccurence([NotNull] ITreeNode treeNode, OccurrenceType occurrenceKind) : base(treeNode, occurrenceKind)
    {
    }

    protected ProvidedInOccurence([NotNull] IPsiSourceFile sourceFile, DocumentRange documentRange, OccurrencePresentationOptions options) : base(sourceFile, documentRange, options)
    {
    }

    public ProvidedInOccurence([NotNull] IPsiSourceFile sourceFile, DocumentRange documentRange) : base(sourceFile, documentRange)
    {
    }
}