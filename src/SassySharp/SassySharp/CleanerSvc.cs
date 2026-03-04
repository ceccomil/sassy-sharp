namespace SassySharp;

public interface ICleanerSvc
{
  Task Clean(CancellationToken cancellationToken);
}

internal sealed class CleanerSvc(
  IWardenSvc _wardenSvc,
  ILogger<CleanerSvc> _logger) : ICleanerSvc
{
  private async Task CleanCss(
    CancellationToken cancellationToken)
  {
    _logger.LogInformation(
      "Searching files from: {FullName}",
      _wardenSvc.AppRootFolder!.FullName);

    await Task.Run(() =>
    {
      var files = _wardenSvc.GetFiles(
        "css",
        excludeIfLibs: false);

      foreach (var file in files)
      {
        if (cancellationToken.IsCancellationRequested)
        {
          break;
        }

        file.Delete();

        _logger.LogInformation(
          "{File} has been deleted",
          file);
      }
    },
    cancellationToken);
  }

  public async Task Clean(
    CancellationToken cancellationToken)
  {
    if (cancellationToken.IsCancellationRequested)
    {
      await Task.FromCanceled(
        cancellationToken);

      return;
    }

    _logger.LogInformation(
      "Deleting all css files");

    await CleanCss(cancellationToken);
  }
}
