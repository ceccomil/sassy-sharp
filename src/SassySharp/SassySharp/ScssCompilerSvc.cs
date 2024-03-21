namespace SassySharp;

public interface IScssCompilerSvc
{
  Task RunCompiler(
    CancellationToken cancellationToken);
}

internal sealed class ScssCompilerSvc(
  IWardenSvc _wardenSvc,
  ICaptainLogger<ScssCompilerSvc> _logger) : IScssCompilerSvc
{
  private const string NO_SOURCE_MAP = "--no-source-map";
  private const string STOP_ON_ERROR = "--stop-on-error";
  private const string MIN = "--style=compressed";
  private const string NO_WARN = "--quiet";

  private string GetCssFileName(FileInfo scss)
  {
    var name = scss
      .Name
      .Remove(
        scss.Name.Length -
        scss.Extension.Length);

    var fileName = Path.Combine(
      scss.Directory!.FullName,
      $"{name}.css");

    var fi = new FileInfo(fileName);

    _logger.InformationLog(
      $"Compiled file: {fi.FullName}");

    return fi.FullName;
  }

  private void CompileFolder(
    DirectoryInfo path)
  {
    using var p = new Process()
    {
      StartInfo = new ProcessStartInfo(
      _wardenSvc.DartSassPath!.FullName,
      [
        _wardenSvc.Snapshot!.FullName,
        NO_SOURCE_MAP,
        STOP_ON_ERROR,
        MIN,
        NO_WARN,
        path.FullName
      ])
    };

    p.Start();
    p.WaitForExit(30_0000);

    if (p.ExitCode != 0)
    {
      throw new ApplicationException(
        $"{path} cannot be compiled!");
    }

    _logger.InformationLog(
      $"{path.FullName} has been compiled");
  }

  private void CompileFile(
    FileInfo file)
  {
    var resultName = GetCssFileName(
      file);

    using var p = new Process()
    {
      StartInfo = new ProcessStartInfo(
      _wardenSvc.DartSassPath!.FullName,
      [
        _wardenSvc.Snapshot!.FullName,
        file.FullName,
        resultName,
        NO_SOURCE_MAP,
        STOP_ON_ERROR,
        MIN
      ])
    };

    p.Start();
    p.WaitForExit(10_0000);

    if (p.ExitCode != 0)
    {
      throw new ApplicationException(
        $"{file} cannot be compiled!");
    }
  }

  public async Task RunCompiler(
    CancellationToken cancellationToken)
  {
    await Task.Run(() =>
    {
      _logger.InformationLog(
        "Begin Scss compilation");

      _logger.InformationLog(
          "Searching files " +
          $"from: {_wardenSvc
            .AppRootFolder!
            .FullName}");

      var files = _wardenSvc.GetFiles("scss");

      foreach (var lib in _wardenSvc.FullLibPaths)
      {
        if (cancellationToken.IsCancellationRequested)
        {
          break;
        }

        CompileFolder(lib);
      }

      foreach (var file in files)
      {
        if (cancellationToken.IsCancellationRequested)
        {
          break;
        }

        CompileFile(file);
      }
    },
    cancellationToken);
  }
}
