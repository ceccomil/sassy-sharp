var hostRunner = new HostRunner()
{
  ServicesConfig = services =>
  {
    services
      .Configure<LoggerFilters>(x =>
      {
        x.Add(new("", LogLevel.Warning));
        x.Add(new("SassySharp", LogLevel.Information));
      })
      .AddDefaultLogger()
      .Configure<CaptainLoggerOptions>(x =>
      {
        var path = Path.Combine(
          Globals.ToolFolder.FullName,
          "Logs/sassy-sharp.log");

        var dir = new DirectoryInfo(path);

        x.FilePath = dir.FullName;
      })
      .AddSingleton<IWardenSvc, WardenSvc>()
      .AddSingleton<IScssCompilerSvc, ScssCompilerSvc>()
      .AddSingleton<ICleanerSvc, CleanerSvc>();

    return Task.CompletedTask;
  }
};

await hostRunner
  .CreateAndRun<EntryPointSvc>(
    args,
    "Bootstrapping");
