# agents.md — InsaneOne.Core (repository root)

> Language convention: this file is always written in English, regardless of the language used
> in the conversation that edits it.

## What this repository is

This is the git repository for **InsaneOne Core** (`com.insaneone.core`) — a Unity Package
Manager (UPM) package distributed via a git URL. The repo root is *not* a Unity project; it's
meant to be embedded as a package inside some Unity project's `Assets/` (or referenced via
`Packages/manifest.json` → git URL), which is why almost every file here has a Unity `.meta`
sibling.

The actual package code lives one level down, in [`InsaneOne/Core/`](InsaneOne/Core/) — **see
[`InsaneOne/Core/agents.md`](InsaneOne/Core/agents.md) for the code structure, assembly
definitions, and coding conventions.** This file only covers what's specific to the repo root.

## Root-level layout

```
package.json          UPM manifest: name, version, Unity version requirement, dependencies
README.md             public-facing feature overview/usage examples (keep in sync with
                       new features — this is what users see on GitHub / Package Manager)
LICENSE_MIT            dual-licensed under MIT ...
LICENSE_APACHE_2_0      ... and Apache 2.0 — don't remove either without being asked
.gitmodules            declares InsaneOne/Core/Sources/Architect/Injection as a submodule
                       (InsaneOne.Core.Injection, extracted into its own repo)
.gitignore
InsaneOne/Core/        the actual package source — see its own agents.md
```

Every tracked file/folder under Unity's purview (basically everything except `.gitmodules`,
`.gitignore`, and licenses) has a matching `<name>.meta` file holding a Unity-assigned GUID.
When adding, renaming, or deleting a file, keep its `.meta` in sync (add one for new files,
rename/delete together with the original) — losing or duplicating a GUID breaks references in
any Unity project that consumes this package.

## Submodule

`InsaneOne/Core/Sources/Architect/Injection` is a **git submodule** pointing at
`InsaneOne.Core.Injection` (a standalone, dependency-free DI container also usable outside this
package). Don't edit files under that path as if they were regular repo files:
- Changes need to be made and committed inside the submodule's own repo, then the submodule
  pointer here bumped (`git submodule update --remote` / manual commit of the new SHA).
- A plain `git status` here will show the submodule as a single changed pointer, not a file
  diff — that's expected.

## Versioning & releases

`package.json`'s `"version"` is the UPM package version shown by Unity's Package Manager and by
the README badges. Bump it (semver) when cutting a release-worthy change — commits doing only
this tend to use the `[^]` prefix (see below), e.g. "Version up".

## Verifying changes

There's no `.sln`/`.csproj`, no CI config, and no build outside the Unity Editor at the repo
root either. Same caveat as the nested agents.md: review changes manually; if Unity is
available, prefer opening the consuming project and letting it recompile.

## Commits

History uses bracketed prefixes: `[+]` new feature, `[*]` change/improvement to existing code,
`[!]` bug fix, `[-]` removed feature or old code, `[^]` version/dependency bump (e.g. submodule
version, `package.json` version). Follow this format for commit messages in this repository.
