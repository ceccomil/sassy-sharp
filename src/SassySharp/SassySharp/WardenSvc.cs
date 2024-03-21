namespace SassySharp;

public interface IWardenSvc
{
  DirectoryInfo? AppRootFolder { get; }
  DirectoryInfo[] FullLibPaths { get; }
  bool CleansingRequired { get; }
  FileInfo? DartSassPath { get; }
  FileInfo? Snapshot { get; }

  void Initialize();
  DirectoryInfo GetToolPath();
  FileInfo[] GetFiles(
    string extension,
    bool excludeIfLibs = true);
}

internal sealed class WardenSvc(
  IConfiguration _config,
  ICaptainLogger<WardenSvc> _logger) : IWardenSvc
{
  private static readonly char[] _separator = [';'];

  public DirectoryInfo? AppRootFolder { get; private set; }
  public DirectoryInfo[] FullLibPaths { get; private set; } = [];
  public bool CleansingRequired { get; private set; }
  public FileInfo? DartSassPath { get; private set; }
  public FileInfo? Snapshot { get; private set; }

  private void InitAndValidateAppRoot()
  {
    AppRootFolder = new(
      _config["App-root"]
      ?? throw new NotSupportedException(
        $"--app-root parameter is missing!"));

    if (!AppRootFolder.Exists)
    {
      throw new NotSupportedException(
        $"App root folder '{AppRootFolder
        .FullName}' does not exist!");
    }

    _logger.InformationLog(
      $"App root folder: '{AppRootFolder
      .FullName}'");
  }

  private void InitAndValidateFullLibs()
  {
    var fullLibs = _config["Lib-folders"];

    if (string.IsNullOrWhiteSpace(fullLibs))
    {
      return;
    }

    var libPaths = fullLibs.Split(
      _separator,
      StringSplitOptions.RemoveEmptyEntries);

    FullLibPaths = new DirectoryInfo[libPaths.Length];

    for (var i = 0; i < libPaths.Length; i++)
    {
      FullLibPaths[i] = new(libPaths[i]);

      if (!FullLibPaths[i].Exists)
      {
        throw new NotSupportedException(
          $"Lib folder '{FullLibPaths[i]
          .FullName}' does not exist!");
      }

      _logger.InformationLog(
        $"Lib folder: '{FullLibPaths[i]
        .FullName}'");
    }
  }

  private void InitWindowsDart()
  {
    if (!RuntimeInformation
      .IsOSPlatform(OSPlatform.Windows))
    {
      return;
    }

    var toolPath = GetToolPath();

    DartSassPath = new(
      Path.Combine(
        toolPath.FullName,
        "./Winx64/dart.exe"));

    Snapshot = new(
      Path.Combine(
        toolPath.FullName,
        "./Winx64/sass.snapshot"));
  }

  private void InitLinuxDart()
  {
    if (!RuntimeInformation
      .IsOSPlatform(OSPlatform.Linux))
    {
      return;
    }

    var toolPath = GetToolPath();

    DartSassPath = new(
      Path.Combine(
        toolPath.FullName,
        "./Lnx64/dart"));

    Snapshot = new(
      Path.Combine(
        toolPath.FullName,
        "./Lnx64/sass.snapshot"));
  }

  private void InitMacOsDart()
  {
    if (!RuntimeInformation
      .IsOSPlatform(OSPlatform.OSX))
    {
      return;
    }

    var toolPath = GetToolPath();

    DartSassPath = new(
      Path.Combine(
        toolPath.FullName,
        "./MacOsx64/dart"));

    Snapshot = new(
      Path.Combine(
        toolPath.FullName,
        "./MacOsx64/sass.snapshot"));
  }

  private void InitDart()
  {
    if (RuntimeInformation.OSArchitecture
      is not Architecture.X64)
    {
      throw new NotSupportedException(
        "Only x64 architecture is supported!");
    }

    InitWindowsDart();
    InitLinuxDart();
    InitMacOsDart();

    if (DartSassPath is null || Snapshot is null)
    {
      throw new NotSupportedException(
        "A dart sass version for " +
        "this OS is not found");
    }

    _logger.InformationLog(
      $"Dart Sass : '{DartSassPath}" +
      $"', '{Snapshot}'");
  }

  private static bool IsPartOfTheTree(
    string fileName,
    DirectoryInfo[] dirs)
  {
    if (dirs.Length == 0)
    {
      return false;
    }

    return dirs.Any(x =>
      fileName
      .StartsWith(x.FullName));
  }

  public void Initialize()
  {
    InitAndValidateAppRoot();
    InitAndValidateFullLibs();

    CleansingRequired = _config
      .SwitchIsOn("Clean-css");

    _logger.InformationLog(
      $"Cleansing required: {CleansingRequired}");

    InitDart();

    _logger.InformationLog(
      "Warden service initialized!");
  }

  public DirectoryInfo GetToolPath() => Globals
    .ToolFolder;

  public FileInfo[] GetFiles(
    string extension,
    bool excludeIfLibs = true)
  {
    var files = AppRootFolder!
      .GetFiles(
        $"*.{extension}",
        SearchOption.AllDirectories);

    var resultList = new HashSet<FileInfo>();

    var libs = FullLibPaths;

    if (!excludeIfLibs)
    {
      libs = [];
    }

    foreach (var file in files)
    {
      if (IsPartOfTheTree(file.FullName, libs))
      {
        _logger.InformationLog(
          $"Excluded file: {file}");

        continue;
      }

      _logger.InformationLog(
        $"Found: {file}");

      resultList.Add(file);
    }

    return [.. resultList];
  }
}
