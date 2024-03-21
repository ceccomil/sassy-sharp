# sassy-sharp
Sass compilation for .NET Web applications

### Installation

`dotnet tool install --global SassySharp`

### Update

`dotnet tool update --global SassySharp`

### Usage

Cpmpile all `.scss` files in the current directory and subdirectories:

`sassy-sharp --app-folder <path>`

Add libraries to the compilation (compile the entire folder):

`sassy-sharp --app-folder <path> --lib-folders "<folder1>;<folder2>;etc..."`

### Clean all compiled files

`sassy-sharp --app-folder <path> --clean-css`

### Example

[Blazor App](./src/Example/)

### MSBuild targets example

[Example.csproj](./src/Example/Example/Example.csproj)

```xml

  <Target Name="SassySharpCompile" AfterTargets="PreBuildEvent" >
    <Exec Command="sassy-sharp --app-root $(ProjectDir) --lib-folders $(ProjectDir)wwwroot\css\bootstrap;$(ProjectDir)wwwroot\css\fontawesome" />
  </Target>

  <Target Name="SassySharpClean" AfterTargets="AfterClean" >
    <Exec Command="sassy-sharp --app-root $(ProjectDir) --clean-css" />
  </Target>

```