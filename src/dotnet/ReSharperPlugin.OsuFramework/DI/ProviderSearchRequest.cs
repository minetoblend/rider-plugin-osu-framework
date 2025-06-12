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
    public override ICollection<IOccurrence> Search(IProgressIndicator progressIndicator)
    {
        return ProviderFinder.SearchForProviders(context.Type, progressIndicator)
            .SelectNotNull(s => s.Declaration.DeclaredElement)
            .Select(s => (IOccurrence)new DeclaredElementOccurrence(s))
            .ToList();
    }

    public override string Title => "Providers";

    public override ISolution Solution => context.Solution;

    public override ICollection SearchTargets => new List<object>();
}