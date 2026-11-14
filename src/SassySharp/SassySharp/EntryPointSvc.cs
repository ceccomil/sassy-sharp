namespace SassySharp;

internal sealed class EntryPointSvc(
  IAppHandler _appHandler,
  IWardenSvc _wardenSvc,
  ICleanerSvc _cleanerSvc,
  IScssCompilerSvc _scssCompilerSvc,
  ILogger<EntryPointSvc> _logger) : IExecutor
{
  public async Task MainTask(
    CancellationToken cancellationToken)
  {
    try
    {
      _logger.LogInformation(
        "Single execution started! SassySharp {Version}", 
        GetVersion());

      _wardenSvc.Initialize();

      if (_wardenSvc.CleansingRequired)
      {
        await _cleanerSvc
          .Clean(cancellationToken);

        await _appHandler.Exit();

        return;
      }

      await _scssCompilerSvc
        .RunCompiler(cancellationToken);

      await _appHandler.Exit();
    }
    catch (Exception ex)
    {
      _logger.LogError(
        ex,
        "An error occurred.");

      await _appHandler.Exit(ErrorCode.CriticalError);
    }
  }

  private static string GetVersion()
  {
    //var msixVersion = TryGetMsixPackageVersion();

    //if (!string.IsNullOrWhiteSpace(msixVersion))
    //{
    //  return msixVersion;
    //}

    var version = "v unknown";

    var assembly = typeof(EntryPointSvc).Assembly;

    var fileVersion = assembly
      .GetCustomAttribute<AssemblyFileVersionAttribute>()?
      .Version;

    if (!string.IsNullOrWhiteSpace(fileVersion))
    {
      version = $"v {fileVersion}";
    }

    return version;
  }


}
