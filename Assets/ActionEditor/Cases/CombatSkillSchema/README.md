# Combat Skill Schema Case

This removable case verifies the versioned `CombatSkillAsset` schema and its JSON
round trip.

1. Add `CombatSkillSchemaCase` to any GameObject.
2. Open the component context menu.
3. Run `Validate Combat Skill Schema`.
4. Confirm the Console contains `CombatSkillAsset schema validation passed.`

Delete `Assets/ActionEditor/Cases/CombatSkillSchema` to remove this case. The
ActionEditor runtime and editor assemblies do not reference the case assembly.

The batch-mode editor authoring entry point is
`PKC.ActionEditor.Cases.Editor.CombatSkillSchemaEditorCase.Run`.
