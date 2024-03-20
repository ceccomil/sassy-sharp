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
