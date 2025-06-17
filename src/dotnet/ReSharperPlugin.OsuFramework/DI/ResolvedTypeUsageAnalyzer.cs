using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Feature.Services.InterLineAdornments;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Util;
using ReSharperPlugin.OsuFramework.Providers;

namespace ReSharperPlugin.OsuFramework.DI;

[ElementProblemAnalyzer(typeof(ICreationExpression),
    HighlightingTypes = [typeof(DefaultInterLineAdornmentHighlighting)])]
public class ResolvedTypeUsageAnalyzer : ElementProblemAnalyzer<ICreationExpression>
{
    protected override void Run(
        ICreationExpression element,
        ElementProblemAnalyzerData data,
        IHighlightingConsumer consumer)
    {
        var type = element.ExplicitType();

        var scope = type.GetPsiServices().Symbols.GetSymbolScope(LibrarySymbolScope.FULL, true);

        var drawableTypeElement = scope.GetTypeElementByCLRName(DrawableClrName) as IClass;
        var drawableType = drawableTypeElement != null
            ? TypeFactory.CreateType(drawableTypeElement)
            : null;

        drawableTypeElement.GetResolvedType();


        if (drawableType == null || !type.IsSubtypeOf(drawableType))
            return;


        var resolvedTypes = new HashSet<IType>();

        var clazz = type.GetClassType();

        if (clazz == null)
            return;

        foreach (var property in clazz.Properties)
        {
            var resolvedType = property.GetResolvedType();
            if (resolvedType == null)
                continue;

            resolvedTypes.Add(resolvedType);
        }

        foreach (var method in clazz.Methods)
        {
            if (!method.HasAttributeInstance(ProviderFinder.BackgroundDependencyLoaderAttributeClrName,
                    AttributesSource.Self))
                continue;

            foreach (var parameter in method.Parameters)
            {
                resolvedTypes.Add(parameter.Type);
            }
        }

        if (resolvedTypes.Count > 0)
        {
            var text = resolvedTypes.Count == 1 ? "1 resolved type" : $"{resolvedTypes.Count} resolved types";

            consumer.AddHighlighting(new DefaultInterLineAdornmentHighlighting(element.GetHighlightingRange(), text,
                string.Join(", ", resolvedTypes.Select(s => s.GetPresentableName(element.Language)))));
        }
    }

    private static readonly ClrTypeName DrawableClrName =
        new("osu.Framework.Graphics.Drawable");
}