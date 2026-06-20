# Project Structure

Nice3point.TUnit.Revit runs TUnit tests inside the Revit process. The solution separates the shipped framework, the test project that dogfoods it, and the build. Keep each piece of code in the project that owns its responsibility.

## Solution Groups

* **`/source`**: the shipped NuGet package.
    * The framework is everything a consumer references at test time: the base test classes for the application lifecycle and the Revit API, and the executor that marshals tests onto the Revit thread. It is the only package-producing project.
    * The executor and its thread infrastructure live in the `Executors` namespace. The thread host and the parallel limiter stay file-scoped, never public.
* **`/tests`**: the test project. It dogfoods the framework, so it doubles as the reference for how a consumer writes Revit tests.
* **`/build`**: the ModularPipelines build that compiles, tests, packages, and publishes.
* **Root**: build and package configuration, the README and CHANGELOG, the agent guidelines, and the CI workflows.

## Change Placement

* A base test class that consumers inherit goes alongside the other base classes at the framework root.
* The executor and the thread infrastructure it owns go under `Executors`.
* Helpers for the Revit thread or the injection lifecycle stay internal unless they belong to the consumer's contract.
* Coverage for a framework change goes in the test project as a test that runs on the Revit thread.
