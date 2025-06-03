using System.Threading;
using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.Feature.Services;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.TestFramework;
using JetBrains.TestFramework;
using JetBrains.TestFramework.Application.Zones;
using NUnit.Framework;

[assembly: Apartment(ApartmentState.STA)]

namespace ReSharperPlugin.OsuFramework.Tests
{
    [ZoneDefinition]
    public class OsuFrameworkTestEnvironmentZone : ITestsEnvZone, IRequire<PsiFeatureTestZone>, IRequire<IOsuFrameworkZone> { }

    [ZoneMarker]
    public class ZoneMarker : IRequire<ICodeEditingZone>, IRequire<ILanguageCSharpZone>, IRequire<OsuFrameworkTestEnvironmentZone> { }

    [SetUpFixture]
    public class OsuFrameworkTestsAssembly : ExtensionTestEnvironmentAssembly<OsuFrameworkTestEnvironmentZone> { }
}
