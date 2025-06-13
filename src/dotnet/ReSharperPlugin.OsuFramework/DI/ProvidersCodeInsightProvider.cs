#if RIDER
using System.Collections.Generic;
using System.Linq;
using JetBrains.Application;
using JetBrains.Application.DataContext;
using JetBrains.Application.Parts;
using JetBrains.Application.Progress;
using JetBrains.Application.UI.Actions.ActionManager;
using JetBrains.IDE.UI;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ProjectModel;
using JetBrains.RdBackend.Common.Features.CodeInsights.Providers;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Daemon.UsageChecking;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Feature.Services.Resources;
using JetBrains.ReSharper.Psi;
using JetBrains.Rider.Model;
using JetBrains.UI.Icons;
using ReSharperPlugin.OsuFramework.DI;

namespace ReSharperPlugin.OsuFramework.Providers;

[ShellComponent(Instantiation.DemandAnyThreadSafe)]
[HighlightingSource]
public class ProvidersCodeInsightProvider(
    CodeVisionSettingsModel codeVisionSettingsModel,
    IconHostBase iconHostBase,
    DataContexts dataContexts,
    IActionManager actionManager
)
    : ContextNavigationCodeInsightsProviderBase<ProviderSearchAction, ProviderContextNavigationProvider>(
        codeVisionSettingsModel,
        iconHostBase,
        dataContexts,
        actionManager
    )
{
    public override string ProviderId => nameof(ProvidersCodeInsightProvider);

    public override string DisplayName => "Providers";

    public override ICollection<CodeVisionRelativeOrdering> RelativeOrderings => [new CodeVisionRelativeOrderingLast()];

    protected override IconId IconId => CodeInsightsThemedIcons.InsightReference.Id;

    public override bool IsAvailableFor(IDeclaredElement declaredElement, ElementId? elementId) =>
        declaredElement is IProperty property && property.HasResolvedAttribute();

    protected override string Noun(IDeclaredElement element, int count) => count == 1 ? "provider" : "providers";


    protected override int GetBaseCount(SolutionAnalysisService swa, IGlobalUsageChecker usageChecker,
        IDeclaredElement element, ElementId? elementId) => 0;

    protected override int GetOwnCount(SolutionAnalysisService swa, IGlobalUsageChecker usageChecker,
        IDeclaredElement element,
        ElementId? elementId)
    {
        if (element is not IProperty property) return 0;

        var type = property.GetResolvedType();

        if (type == null)
            return 0;

        return ProviderFinder.SearchForProviders(type, NullProgressIndicator.Create()).Count();
    }

    private static readonly ClrTypeName ResolvedAttributeClrName = new("osu.Framework.Allocation.ResolvedAttribute");
}
#endif