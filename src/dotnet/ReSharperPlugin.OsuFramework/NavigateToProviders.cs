using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application.DataContext;
using JetBrains.Application.Parts;
using JetBrains.Application.UI.DataContext;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.CSharp.CompleteStatement;
using JetBrains.ReSharper.Feature.Services.CSharp.PredictiveDebugger;
using JetBrains.ReSharper.Feature.Services.Navigation.ContextNavigation;
using JetBrains.ReSharper.Feature.Services.Navigation.ExecutionHosting;
using JetBrains.ReSharper.Feature.Services.Occurrences;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Search;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;

namespace ReSharperPlugin.OsuFramework;

[ContextNavigationProvider(Instantiation.DemandAnyThreadSafe)]
public class NavigateToProviders : INavigateFromHereProvider
{
    public IEnumerable<ContextNavigation> CreateWorkflow(IDataContext dataContext)
    {
        var declaration = dataContext.GetSelectedTreeNode<IPropertyDeclaration>();
        if (declaration == null)
            yield break;

        var resolvedType = ProviderNavigator.GetProvidedType(declaration);
        if (resolvedType == null)
            yield break;


        var scope = declaration.GetPsiServices().Symbols.GetSymbolScope(LibrarySymbolScope.FULL, true);

        var element = scope.GetTypeElementsByCLRName("osu.Framework.Allocation.CachedAttribute").FirstOrDefault();
        if (element == null)
            yield break;


        yield return new ContextNavigation("Providers", null, NavigationActionGroup.Blessed, () =>
        {
            var occurrences = ProviderNavigator.GetValueProviders(declaration, resolvedType).ToList();

            var solution = declaration.GetSolution();

            var navigationExecutionHost = solution.GetComponent<DefaultNavigationExecutionHost>();

            var window = dataContext.GetData(UIDataConstants.PopupWindowContextSource);

            navigationExecutionHost.ShowGlobalPopupMenu(
                solution,
                occurrences,
                activate: true,
                windowContext: window,
                descriptorBuilder: () =>
                    new ProvidedInOccurenceBrowserDescriptor(solution, occurrences),
                options: new OccurrencePresentationOptions(),
                skipMenuIfSingleEnabled: true,
                title: "Go to Provider"
            );
        });
    }


    [CanBeNull]
    private static IMultipleFieldDeclaration GetMultipleFieldDeclaration([CanBeNull] ITreeNode node)
    {
        while (node != null)
        {
            if (node is IMultipleFieldDeclaration declaration)
                return declaration;

            node = node.Parent;
        }

        return null;
    }
}