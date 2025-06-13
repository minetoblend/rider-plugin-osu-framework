using JetBrains.Application.Parts;
using JetBrains.Application.UI.Controls.JetPopupMenu;
using JetBrains.Application.UI.Icons.CommonThemedIcons;
using JetBrains.ReSharper.Feature.Services.Occurrences;
using JetBrains.ReSharper.Feature.Services.Presentation;
using JetBrains.ReSharper.Feature.Services.Resources;

namespace ReSharperPlugin.OsuFramework.DI;

[OccurrencePresenter(Instantiation.DemandAnyThreadSafe)]
public class ProviderOccurencePresenter : RangeOccurrencePresenter
{
    public override bool IsApplicable(IOccurrence occurrence)
    {
        return occurrence is ProvidedInOccurrence;
    }


    public override bool Present(IMenuItemDescriptor descriptor, IOccurrence occurrence,
        OccurrencePresentationOptions options)
    {
        if (!base.Present(descriptor, occurrence, options))
            return false;

        if (occurrence is ProvidedInOccurrence providedInOccurrence)
        {
            switch (providedInOccurrence.ProvideType)
            {
                case ProvideType.CachedAttribute:
                    descriptor.Icon = ServicesNavigationThemedIcons.UsageAttribute.Id;
                    break;

                case ProvideType.CacheAs:
                    descriptor.Icon = ServicesNavigationThemedIcons.UsageInvocation.Id;
                    break;

                case ProvideType.InheritedCachedAttribute:
                    descriptor.Icon = ServicesNavigationThemedIcons.UsageExtendedType.Id;
                    break;
            }
        }

        return true;
    }
}