using System.Collections.Generic;
using JetBrains.Application.DataContext;
using JetBrains.Application.UI.Actions;
using JetBrains.Application.UI.ActionsRevised.Menu;
using JetBrains.ReSharper.Feature.Services.Navigation.ContextNavigation;
using JetBrains.UI.RichText;

namespace ReSharperPlugin.OsuFramework.Providers;

[Action(nameof(ProviderSearchAction), "Show Providers", Id = 62347092)]
public class ProviderSearchAction : ContextNavigationActionBase<ProviderContextNavigationProvider>
{
    protected override RichText Caption => "Show Providers";

    public override bool Update(IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
    {
        return base.Update(context, presentation, nextUpdate);
    }

    protected override ICollection<ProviderContextNavigationProvider> GetWorkflowProviders()
    {
        return base.GetWorkflowProviders();
    }
}