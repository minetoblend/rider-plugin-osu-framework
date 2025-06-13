using System.Collections.Generic;
using System.Linq;
using JetBrains.Application;
using JetBrains.Application.DataContext;
using JetBrains.Application.Parts;
using JetBrains.Application.Threading;
using JetBrains.Application.UI.Tooltips;
using JetBrains.Lifetimes;
using JetBrains.ReSharper.Feature.Services.Navigation.ContextNavigation;
using JetBrains.ReSharper.Feature.Services.Occurrences;
using JetBrains.TextControl;
using JetBrains.TextControl.DataContext;
using JetBrains.UI.RichText;
using JetBrains.Util;
using ReSharperPlugin.OsuFramework.DI;

namespace ReSharperPlugin.OsuFramework.Providers;

[ContextNavigationProvider(Instantiation.DemandAnyThreadSafe)]
public class ProviderContextNavigationProvider
    : ContextNavigationProviderBase<ProviderContextSearch, ProviderSearchAction>, INavigateFromHereProvider
{
    private readonly IShellLocks myLocks;
    private readonly ITooltipManager myTooltipManager;

    public ProviderContextNavigationProvider(
        IFeaturePartsContainer manager, IShellLocks myLocks, ITooltipManager myTooltipManager) : base(manager)
    {
        this.myLocks = myLocks;
        this.myTooltipManager = myTooltipManager;
    }

    protected override string GetNavigationMenuTitle(IDataContext dataContext) => "Providers";

    protected override NavigationActionGroup ActionGroup => NavigationActionGroup.APIs;

    protected override void Execute(IDataContext dataContext, IEnumerable<ProviderContextSearch> searches,
        INavigationExecutionHost host)
    {
        var request = searches.SelectNotNull(it => it.CreateSearchRequest(dataContext)).FirstOrDefault();

        if (request == null)
            return;

        ICollection<IOccurrence> occurrences = request.Search();
        if (occurrences == null)
            return;

        if (occurrences.IsEmpty())
        {
            showToolTip(dataContext, "No providers found");
        }
        else
        {
            host.ShowContextPopupMenu(
                dataContext,
                occurrences,
                () => new ProviderSearchDescriptor(request, occurrences),
                OccurrencePresentationOptions.DefaultOptions,
                true,
                request.Title
            );
        }
    }

    private void showToolTip(IDataContext dataContext, string tooltip)
    {
        ITextControl data = dataContext.GetData<ITextControl>(TextControlDataConstants.TEXT_CONTROL);
        if (data != null)
            myTooltipManager.ShowAtCaret((OuterLifetime)Lifetime.Eternal, (RichText)tooltip,
                data, myLocks);
        else
            myTooltipManager.ShowIfPopupWindowContext(tooltip, dataContext);
    }
}