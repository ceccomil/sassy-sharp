namespace SassySharp.Helpers;

internal static class Globals
{
  private static DirectoryInfo? _toolFolder;

  internal static DirectoryInfo ToolFolder
  {
    get
    {
      _toolFolder ??= GetToolFolder();

      return _toolFolder;
    }
  }

  private static DirectoryInfo GetToolFolder()
  {
    string assemblyFolder = Path
      .GetDirectoryName(
        Assembly
        .GetExecutingAssembly()
        .Location)!;

    return new DirectoryInfo(
      assemblyFolder);
  }
}
