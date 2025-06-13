using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharperPlugin.OsuFramework.DI;

public record ProviderSearchContext(
    IType Type,
    IDeclaration Target,
    ISolution Solution,
    PsiLanguageType LanguageType
);