using System.Collections.Generic;
using JetBrains.Application.Progress;
using JetBrains.Application.UI.TreeModels;
using JetBrains.IDE.TreeBrowser;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Occurrences;
using JetBrains.ReSharper.Feature.Services.Tree;
using JetBrains.ReSharper.Resources.Shell;

namespace ReSharperPlugin.OsuFramework.DI;

public sealed class ProvidedInOccurenceBrowserDescriptor : OccurrenceBrowserDescriptor
{
    private readonly TreeSectionModel _model;

    public ProvidedInOccurenceBrowserDescriptor(
        ISolution solution,
        ICollection<IOccurrence> providedInOccurrences,
        IProgressIndicator indicator = null
    ) : base(solution)
    {
        _model = new TreeSectionModel();
        DrawElementExtensions = true;
        
        Title.Value = "Providers for ";

        using (ReadLockCookie.Create())
        {
            SetResults(providedInOccurrences, indicator);
        }
    }

    public override TreeModel Model => _model;

    protected override void SetResults(ICollection<IOccurrence> items, IProgressIndicator indicator = null,
        bool mergeKinds = true)
    {
        base.SetResults(items, indicator, mergeKinds);
        RequestUpdate(UpdateKind.Structure, immediate: true);
    }
}