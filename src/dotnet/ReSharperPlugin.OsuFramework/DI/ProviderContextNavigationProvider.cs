using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Application;
using JetBrains.Application.DataContext;
using JetBrains.Application.Parts;
using JetBrains.Application.Threading;
using JetBrains.Application.UI.Tooltips;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.DataContext;
using JetBrains.ReSharper.Feature.Services.Navigation;
using JetBrains.ReSharper.Feature.Services.Navigation.ContextNavigation;
using JetBrains.ReSharper.Feature.Services.Occurrences;
using JetBrains.ReSharper.Feature.Services.Tree;
using JetBrains.ReSharper.Features.Navigation.Features.FindExtensions;
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
            int num = occurrences.Count;
            OccurrencePopupMenu instance =
                OccurrencePopupMenu.GetInstance(dataContext.GetData(ProjectModelDataConstants.SOLUTION));

            OccurrencePopupMenuOptions popupMenuOptions = new OccurrencePopupMenuOptions(request.Title, false,
                OccurrencePresentationOptions.DefaultOptions, null, DescriptorBuilder);
            popupMenuOptions.ViewportSize = Math.Min(num + 3, popupMenuOptions.ViewportSize);
            IDataContext context = dataContext;
            ICollection<IOccurrence> items = occurrences;
            OccurrencePopupMenuOptions options = popupMenuOptions;
            instance.ShowMenuFromTextControl(context, items, options);
        }

        ProviderSearchDescriptor DescriptorBuilder()
        {
            return new ProviderSearchDescriptor(request, occurrences);
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