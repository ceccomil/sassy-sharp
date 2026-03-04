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
        "Single execution started!");

      _wardenSvc.Initialize();

      if (_wardenSvc.CleansingRequired)
      {
        await _cleanerSvc
          .Clean(cancellationToken);

        _appHandler.Exit();
      }

      await _scssCompilerSvc
        .RunCompiler(cancellationToken);

      _appHandler.Exit();
    }
    catch (Exception ex)
    {
      _logger.LogError(
        ex,
        "An error occurred.");

      _appHandler.Exit(ErrorCode.CriticalError);
    }
  }
}
