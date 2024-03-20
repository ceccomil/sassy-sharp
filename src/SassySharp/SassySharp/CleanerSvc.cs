namespace SassySharp;

public interface ICleanerSvc
{
  Task Clean(
    CancellationToken cancellationToken);
}

internal sealed class CleanerSvc(
  IWardenSvc _wardenSvc,
  ICaptainLogger<CleanerSvc> _logger) : ICleanerSvc
{
  private async Task CleanCss(
    CancellationToken cancellationToken)
  {
    _logger.InformationLog(
      "Searching files " +
      $"from: {_wardenSvc
        .AppRootFolder!
        .FullName}");

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

        _logger.InformationLog(
          $"{file} has been deleted");
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

    _logger.InformationLog(
      "Deleting all css files");

    await CleanCss(cancellationToken);
  }
}
