using System;
using System.Collections.Generic;
using JetBrains.Application.UI.TreeModels;
using JetBrains.ReSharper.Feature.Services.Navigation.Descriptors;
using JetBrains.ReSharper.Feature.Services.Navigation.Requests;
using JetBrains.ReSharper.Feature.Services.Occurrences;
using JetBrains.ReSharper.Feature.Services.Tree;
using JetBrains.ReSharper.Feature.Services.Tree.SectionsManagement;

namespace ReSharperPlugin.OsuFramework.DI;

public class ProviderSearchDescriptor : SearchDescriptor
{
    public ProviderSearchDescriptor(SearchRequest request, ICollection<IOccurrence> results) : base(request, results)
    {
        KindFilterEnabled = true;
    }

    public override string GetResultsTitle(OccurrenceSection section) => "Providers";


    protected override Func<SearchRequest, IOccurrenceBrowserDescriptor> GetDescriptorFactory() =>
        request => new ProviderSearchDescriptor(request, request.Search());
}