using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Navigation.Requests;
using JetBrains.ReSharper.Feature.Services.Occurrences;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Refactorings.WorkflowNew;
using JetBrains.Util;

namespace ReSharperPlugin.OsuFramework.DI;

public sealed class ProviderSearchRequest(ProviderSearchContext context) : SearchRequest
{
    DeclaredElementEnvoy<IDeclaredElement> target = new DeclaredElementEnvoy<IDeclaredElement>(context.Target.DeclaredElement);
    
    public override ICollection<IOccurrence> Search(IProgressIndicator progressIndicator)
    {
        return ProviderFinder.SearchForProviders(context.Type, progressIndicator)
            .SelectNotNull(s => s.Usage)
            .Select(s => (IOccurrence)new ReferenceOccurrence(s, OccurrenceType.Occurrence))
            .ToList();
    }

    public override string Title => "Providers";

    public override ISolution Solution => context.Solution;

    public override ICollection SearchTargets => new List<object>();
}