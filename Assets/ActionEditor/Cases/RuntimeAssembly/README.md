# Runtime Assembly Smoke Case

This removable case proves that a normal player-compatible assembly can reference
the ActionEditor runtime assembly.

1. Add `RuntimeAssemblySmokeCase` to any GameObject.
2. Enable `Log On Start`, then enter Play Mode or make a Player build.
3. Confirm the Console or Player log contains:
   `ActionEditor runtime assembly: PKC.ActionEditor`

To verify a Player build, use
`Tools > ActionEditor Cases > Build Runtime Assembly Smoke Player`. The generated
Windows development build is written to
`Temp/ActionEditorCases/RuntimeAssemblySmoke.exe` by default. This generated case
logs the runtime assembly name and then exits automatically when launched.

For batch mode, execute
`PKC.ActionEditor.Cases.Editor.RuntimeAssemblyPlayerBuildCase.BuildFromCommandLine`.
Set `ACTION_EDITOR_CASE_BUILD_PATH` to override the output path.

Delete `Assets/ActionEditor/Cases` to remove this case. The case assembly is not
referenced by the ActionEditor runtime or editor assemblies.
