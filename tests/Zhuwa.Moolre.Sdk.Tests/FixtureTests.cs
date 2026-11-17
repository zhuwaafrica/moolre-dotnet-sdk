using System.Text.Json;
using Xunit;

namespace Zhuwa.Moolre;

/// <summary>
/// Guards the documented Moolre example bodies in <c>Fixtures</c>: every file must stay valid JSON,
/// because the contract tests that come next parse them.
/// </summary>
public sealed class FixtureTests
{
    private static readonly string FixtureRoot = Path.Combine(AppContext.BaseDirectory, "Fixtures");

    public static TheoryData<string> FixturePaths()
    {
        var data = new TheoryData<string>();
        foreach (var path in Directory.EnumerateFiles(FixtureRoot, "*.json", SearchOption.AllDirectories).Order(StringComparer.Ordinal))
        {
            data.Add(Path.GetRelativePath(FixtureRoot, path));
        }

        return data;
    }

    [Fact]
    public void FixtureFolder_AfterBuild_ContainsEveryDocumentedExample()
    {
        const int DocumentedExampleCount = 60;

        var actual = Directory.EnumerateFiles(FixtureRoot, "*.json", SearchOption.AllDirectories).Count();

        Assert.Equal(DocumentedExampleCount, actual);
    }

    [Theory]
    [MemberData(nameof(FixturePaths))]
    public void Fixture_ParsedAsJson_IsValid(string relativePath)
    {
        var json = File.ReadAllText(Path.Combine(FixtureRoot, relativePath));

        using var document = JsonDocument.Parse(json);

        Assert.True(document.RootElement.ValueKind is JsonValueKind.Object);
    }
}
