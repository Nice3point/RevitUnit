# Package Management

The solution uses centralized NuGet package management. All versions live in `Directory.Packages.props` (`ManagePackageVersionsCentrally=true`, with floating and transitive pinning enabled). Renovate (`renovate.json`) bumps versions automatically, so manual version edits are rare.

## Rules

* Define every package version in `Directory.Packages.props`. Do not add `<Version>` to individual `PackageReference` items.
* Keep Revit-version-specific packages conditional on `$(RevitVersion)`. The Revit API package floats to `$(RevitVersion).*`, and the per-version packages are pinned with a `$(RevitVersion)` condition.
* Keep shared dependency versions unconditional unless they truly vary by Revit version.
* Use `GlobalPackageReference` only for solution-wide packages such as the polyfill.
* The Revit API and injector references in the framework are `PrivateAssets="all"`, so they stay build-time only. The Release build repacks the injector into the package, so the shipped framework carries no runtime dependency a consumer must add.

## Add a Dependency

1. Add the package version to `Directory.Packages.props`.
2. Add a versionless `PackageReference` to the project that uses it.
3. Keep the scope narrow. The shipped framework stays dependency-light. A consumer already references TUnit and the Revit API, so prefer those before introducing a new one.
