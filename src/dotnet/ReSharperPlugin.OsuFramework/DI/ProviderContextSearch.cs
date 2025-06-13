using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.Application.DataContext;
using JetBrains.Application.Parts;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ReSharper.Feature.Services.Navigation;
using JetBrains.ReSharper.Feature.Services.Navigation.ContextNavigation;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharperPlugin.OsuFramework.DI;

namespace ReSharperPlugin.OsuFramework.Providers;

[ShellFeaturePart(Instantiation.DemandAnyThreadSafe)]
public class ProviderContextSearch : IContextSearch
{
    public bool IsAvailable(IDataContext dataContext) => createContext(dataContext) != null;

    public bool IsContextApplicable(IDataContext dataContext) =>
        ContextNavigationUtil.CheckDefaultApplicability<CSharpLanguage>(dataContext);

    [CanBeNull]
    private ProviderSearchContext createContext(IDataContext context)
    {
        var property =
            ContextNavigationUtil.GetSelectedLanguageSpecificTreeNode<IPropertyDeclaration, CSharpLanguage>(context);

        var attribute = property?.DeclaredElement
            ?.GetAttributeInstances(
                new ClrTypeName("osu.Framework.Allocation.ResolvedAttribute"),
                AttributesSource.Self
            )
            .FirstOrDefault();

        var type = attribute?.NamedParameter("Type").TypeValue
                   ?? attribute?.PositionParameter(0).TypeValue
                   ?? property?.Type;

        if (type == null)
            return null;

        return new ProviderSearchContext(type, property, type.GetPsiServices().Solution, property.Language);
    }

    public ProviderSearchRequest CreateSearchRequest(IDataContext dataContext)
    {
        var context = createContext(dataContext);

        return context != null ? new ProviderSearchRequest(context) : null;
    }
}