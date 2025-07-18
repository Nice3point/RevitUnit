# Revit Injector

This repository contains a tool for injecting dlls into Revit process.

## Installation

Add a new Nuget source:

```text
<configuration>
  <packageSources>
    <add key="Nice3point" value="https://nuget.pkg.github.com/Nice3point/index.json" />
  </packageSources>
</configuration>
```

Install Injector as a nuget package:

```text
<PackageReference Include="Nice3point.Revit.Injector" Version="$(RevitVersion).*"/>
```

## Usages

```csharp
var injector = new Injector();

// Injects the current dll into the Revit process and returns the application instance
var application = injector.InjectApplication();

//Properly cleans up and ejects the application from the Revit process
injector.EjectApplication();
```