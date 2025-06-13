using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharperPlugin.OsuFramework.DI;

public class ProvideInformation(ITreeNode treeNode, ProvideType type)
{
    public ITreeNode TreeNode => treeNode;

    public ProvideType Type => type;

    [CanBeNull] public IDeclaration Declaration;

    public bool Explicit;
}

public enum ProvideType
{
    CachedAttribute,
    CacheAs,
    InheritedCachedAttribute,
}