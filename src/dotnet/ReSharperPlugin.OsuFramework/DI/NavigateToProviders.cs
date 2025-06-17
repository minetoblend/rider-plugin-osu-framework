using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application.DataContext;
using JetBrains.Application.Parts;
using JetBrains.Application.Progress;
using JetBrains.Application.UI.DataContext;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Navigation.ContextNavigation;
using JetBrains.ReSharper.Feature.Services.Navigation.ExecutionHosting;
using JetBrains.ReSharper.Feature.Services.Occurrences;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharperPlugin.OsuFramework.DI;

[ContextNavigationProvider(Instantiation.DemandAnyThreadSafe)]
public class NavigateToProviders : INavigateFromHereProvider
{
    public IEnumerable<ContextNavigation> CreateWorkflow(IDataContext dataContext)
    {
        var declaration = dataContext.GetSelectedTreeNode<IPropertyDeclaration>();
        if (declaration == null)
            yield break;

        var resolvedType = declaration.GetResolvedType();
        if (resolvedType == null)
            yield break;


        yield return new ContextNavigation("Providers", null, NavigationActionGroup.Blessed, () =>
        {
            var occurrences = ProviderFinder.SearchForProviders(resolvedType, NullProgressIndicator.Create())
                .Select(s => (IOccurrence)new ProvidedInOccurrence(s))
                .ToList();

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