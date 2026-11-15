using System.Text.RegularExpressions;
using Xunit;

namespace Zhuwa.Moolre;

/// <summary>
/// Keeps the source tree in the agreed layout (copied from the Square .NET SDK; see CONTRIBUTING.md, "Where code goes").
/// Each test reads the source files, so a file added in the wrong folder or namespace fails here instead of in review.
/// </summary>
public sealed partial class FolderConventionTests
{
    private static readonly string SourceRoot = FindSourceRoot();

    // The API area folders. A new area must be added here and to CONTRIBUTING.md.
    private static readonly string[] Areas = ["Accounts", "Payments", "Reference", "Sales", "Sms", "Transfers", "WhatsApp", "Webhooks"];

    [Fact]
    public void ProjectRoot_HoldsOnlyTheClientAndItsFactory()
    {
        var files = Directory.GetFiles(SourceRoot, "*.cs").Select(Path.GetFileName).Order(StringComparer.Ordinal);

        Assert.Equal(["MoolreClient.cs", "MoolreClientFactory.cs"], files);
    }

    [Fact]
    public void RequestTypes_LiveOnlyInAnAreaRequestsFolder()
    {
        var misplaced = SourceFiles()
            .Where(f => Path.GetFileName(f).EndsWith("Request.cs", StringComparison.Ordinal))
            .Where(f => !IsInAreaRequestsFolder(f));

        Assert.Empty(misplaced);
    }

    [Fact]
    public void AreaRequestsFolders_HoldOnlyRequestTypesInTheAreaNamespace()
    {
        foreach (var area in Areas)
        {
            var folder = Path.Combine(SourceRoot, area, "Requests");
            if (!Directory.Exists(folder))
            {
                continue;
            }

            foreach (var file in Directory.GetFiles(folder, "*.cs"))
            {
                Assert.EndsWith("Request.cs", Path.GetFileName(file), StringComparison.Ordinal);
                AssertNamespace($"Zhuwa.Moolre.{area}", file);
            }
        }
    }

    [Fact]
    public void ResultTypes_LiveOnlyInTypes()
    {
        var misplaced = SourceFiles()
            .Where(f => Path.GetFileName(f).EndsWith("Result.cs", StringComparison.Ordinal))
            .Where(f => Folder(f) != "Types");

        Assert.Empty(misplaced);
    }

    [Theory]
    [InlineData("Types", "Zhuwa.Moolre")]
    [InlineData("Core/Public", "Zhuwa.Moolre")]
    [InlineData("Core", "Zhuwa.Moolre.Core")]
    [InlineData("DependencyInjection", "Zhuwa.Moolre.DependencyInjection")]
    public void SharedFolders_UseTheirAgreedNamespace(string folder, string expectedNamespace)
    {
        var files = Directory.GetFiles(Path.Combine(SourceRoot, folder), "*.cs");

        Assert.All(files, f => AssertNamespace(expectedNamespace, f));
    }

    [Fact]
    public void AreaFolderRoots_HoldOnlyTheClientImplementationAndMapperInTheAreaNamespace()
    {
        foreach (var area in Areas)
        {
            foreach (var file in Directory.GetFiles(Path.Combine(SourceRoot, area), "*.cs"))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                Assert.True(
                    name.StartsWith("Moolre", StringComparison.Ordinal)
                    || name.StartsWith("DefaultMoolre", StringComparison.Ordinal)
                    || name.EndsWith("Mapper", StringComparison.Ordinal),
                    $"{area}/{name}.cs is not a client, implementation or mapper; results and models go in Types/, requests in {area}/Requests/.");
                AssertNamespace($"Zhuwa.Moolre.{area}", file);
            }
        }
    }

    [Fact]
    public void TopLevelFolders_AreOnlyTheAgreedOnes()
    {
        string[] allowed = [.. Areas, "Core", "DependencyInjection", "Types"];

        var folders = Directory.GetDirectories(SourceRoot)
            .Select(Path.GetFileName)
            .Where(n => n is not ("bin" or "obj") && !n!.StartsWith('.'));

        Assert.All(folders, f => Assert.Contains(f, allowed));
    }

    // Walks up from the test output folder to the repository (the folder holding the .slnx file).
    private static string FindSourceRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Zhuwa.Moolre.slnx")))
        {
            directory = directory.Parent;
        }

        return directory is null
            ? throw new InvalidOperationException("Could not find the repository root (Zhuwa.Moolre.slnx).")
            : Path.Combine(directory.FullName, "src", "Zhuwa.Moolre.Sdk");
    }

    private static IEnumerable<string> SourceFiles() =>
        Directory.EnumerateFiles(SourceRoot, "*.cs", SearchOption.AllDirectories)
            .Where(f => !Folder(f).StartsWith("obj", StringComparison.Ordinal) && !Folder(f).StartsWith("bin", StringComparison.Ordinal));

    // The file's folder relative to the project, with forward slashes ("" for the project root).
    private static string Folder(string file) =>
        Path.GetRelativePath(SourceRoot, Path.GetDirectoryName(file)!).Replace('\\', '/') is "." ? string.Empty
            : Path.GetRelativePath(SourceRoot, Path.GetDirectoryName(file)!).Replace('\\', '/');

    private static bool IsInAreaRequestsFolder(string file)
    {
        var parts = Folder(file).Split('/');
        return parts.Length == 2 && Areas.Contains(parts[0]) && parts[1] == "Requests";
    }

    // Checks a file's namespace, naming the file on failure. Placeholder files for code not written yet hold only a
    // "Not implemented yet" comment and no namespace; they are skipped until they get real code.
    private static void AssertNamespace(string expected, string file)
    {
        var actual = NamespaceOf(file);
        if (actual is null && File.ReadAllText(file).Contains("Not implemented yet", StringComparison.Ordinal))
        {
            return;
        }

        Assert.True(expected == actual, $"{Path.GetRelativePath(SourceRoot, file)} declares namespace '{actual}', expected '{expected}'.");
    }

    private static string? NamespaceOf(string file) =>
        NamespaceDeclaration().Match(File.ReadAllText(file)) is { Success: true } match ? match.Groups[1].Value : null;

    [GeneratedRegex(@"^namespace\s+([\w.]+)\s*;", RegexOptions.Multiline)]
    private static partial Regex NamespaceDeclaration();
}
