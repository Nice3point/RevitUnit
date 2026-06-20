# Documentation

These rules govern every piece of prose the package ships: XML doc comments, `README.md`, and `CHANGELOG.md`. Each format adds its own rules on top of the shared set.

A public-surface change updates the README, the CHANGELOG, and the affected XML docs in the same commit. Documentation that lags the code is a defect.

## Shared Prose Rules

* **State what, not how.** Describe observable behavior and contract, never the implementation. A summary survives an implementation rewrite unchanged.
* **Plain technical English.** No corporate jargon, no marketing tone.
* **No filler.** Omit obvious statements. State only what a reader cannot infer from the signature.
* **Third-person present indicative.** Write "Marshals the test onto the Revit thread", not "Marshalling the test onto the Revit thread". No `-ing` verb form for what a member does.
* **One sentence per line.** Break at sentence boundaries, never at a fixed character width.
* **No dashes or semicolons.** Use separate sentences or commas.

## XML Doc Comments

* Document every public member with a `<summary>` that states what it does.
* **`<summary>` describes the member, not its parameters.** Parameters belong in `<param>`, the return value in `<returns>`, and thrown exceptions in `<exception>`. Do not restate the signature in prose.
* Add `<remarks>` for a non-trivial constraint, such as the Revit thread the executor runs on.
* Reference another type or member with `<see cref="..."/>` so renames stay tracked.

## README

The README is the consumer-facing manual. It is where a consumer learns how to write a Revit test with this framework, so every consumer-facing feature has a usage example under its matching section. Cover the first test, the executor registration, the application and document patterns, and the Revit environment configuration. Keep examples copy-pasteable with C# syntax highlighting. The docs under `/docs` describe how to author the framework, not how to consume it, so the two never duplicate.

## CHANGELOG

Document every change in the current version section, not only the major ones. The existing entries set the format: a short bullet per change, with a fuller subsection and a before-and-after code block for a change a consumer must act on, such as a renamed member or a new configuration attribute. Provide a migration example for any breaking change or deprecation.
