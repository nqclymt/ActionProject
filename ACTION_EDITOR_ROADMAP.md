# ActionEditor Development Roadmap

This checklist records the work required to turn the current timeline framework into a production-ready combat skill editor.

## P0 - Runtime Foundation

- [ ] Split editor-only and runtime assemblies so skill data and playback code are available in player builds.
- [ ] Define a versioned `CombatSkillAsset` schema with cast phases, duration, tags, cooldown, costs, targeting rules, and cancellation windows.
- [ ] Implement a runtime `SkillPlayer` with play, pause, stop, interrupt, seek, loop, and completion events.
- [ ] Introduce a runtime execution context for caster, target, world position, direction, and shared combat services.
- [ ] Guarantee deterministic ordering for clips that start or end on the same frame.
- [ ] Add frame-based evaluation that produces the same result in editor preview and runtime playback.

## P0 - Core Combat Tracks

- [ ] Add an animation track with state, speed, transition, layer, avatar mask, and root-motion controls.
- [ ] Add an audio track with volume, pitch, spatial settings, mixer routing, looping, and stop behavior.
- [ ] Add a VFX track with prefab binding, attachment point, transform offsets, lifetime, pooling, and scrub-safe preview.
- [ ] Add a movement track for displacement, rotation, dash curves, and root-motion overrides.
- [ ] Add hitbox and hurtbox clips with shape, bone attachment, target filters, multi-hit interval, and Gizmo rendering.
- [ ] Add damage and gameplay-event clips for damage, stagger, knockback, buffs, debuffs, invulnerability, and custom events.
- [ ] Add camera clips for shake, impulse, FOV, and target framing.

## P1 - Preview Environment

- [ ] Implement caster and target binding instead of relying on placeholder actor references.
- [ ] Provide an isolated preview scene with configurable characters, ground, camera, and spawn positions.
- [ ] Make animation, audio, VFX, movement, and hitboxes seekable and reversible while scrubbing.
- [ ] Reset all preview objects reliably when switching assets, recompiling scripts, closing the window, or stopping playback.
- [ ] Add track mute, solo, enable, and preview-only controls.
- [ ] Display current frame, active clips, fired events, and hit results during preview.

## P1 - Authoring Reliability

- [ ] Add complete Unity Undo/Redo support for create, delete, move, resize, paste, reorder, and inspector edits.
- [ ] Complete copy, cut, paste, duplicate, and multi-selection workflows for groups, tracks, and clips.
- [ ] Add keyboard shortcuts for playback, frame stepping, delete, duplicate, save, focus, and zoom.
- [ ] Add reusable skill, track, and clip templates.
- [ ] Add drag-and-drop creation from animation, audio, and prefab assets.
- [ ] Add clip snapping to frames, markers, neighboring clips, animation events, and range boundaries.
- [ ] Add batch editing for shared clip properties.

## P1 - Data Safety And Validation

- [ ] Replace fragile asset paths with GUID-based resource references where applicable.
- [ ] Add schema versioning and migrations for existing JSON skill files.
- [ ] Save atomically and keep recoverable autosave snapshots.
- [ ] Add a validation panel with navigation to missing references, invalid ranges, overlaps, unsupported audio import settings, and duplicate identifiers.
- [ ] Validate that hit, movement, audio, and VFX timing stays inside the intended skill and animation ranges.
- [ ] Add stable IDs for groups, tracks, and clips so references survive reordering.
- [ ] Define merge-friendly serialization rules for team development.

## P2 - Combat Workflow

- [ ] Add skill markers for cast start, release, hit confirm, cancel start, cancel end, and recovery end.
- [ ] Add branching and conditional execution for hit, miss, critical hit, charge level, and target state.
- [ ] Add combo links and transition rules between skills.
- [ ] Add target-selection previews for point, direction, cone, circle, box, chain, and locked-target skills.
- [ ] Add resource-cost and cooldown previews.
- [ ] Add localization-ready names, descriptions, and designer notes.
- [ ] Add search, filtering, favorites, recent assets, and skill-library browsing.

## P2 - Debugging And Performance

- [ ] Add an execution trace showing clip enter, update, exit, reverse, and emitted gameplay events.
- [ ] Add damage and crowd-control result inspection using configurable test stats.
- [ ] Visualize movement paths, hitboxes, attachment points, and target areas in the Scene view.
- [ ] Add warnings and budgets for VFX count, particle count, audio voices, spawned objects, and long-running clips.
- [ ] Integrate pooling checks for runtime VFX and audio emitters.
- [ ] Add exportable diagnostic reports for invalid skills.

## P2 - Tests And Documentation

- [ ] Add unit tests for time conversion, sibling ordering, range playback, blending, serialization, and migrations.
- [ ] Add editor tests for create, delete, drag, resize, copy/paste, Undo/Redo, save, and reload workflows.
- [ ] Add runtime tests that compare preview events with player-build execution events.
- [ ] Add sample combat skills covering melee, projectile, area damage, channeling, movement, and combo cases.
- [ ] Document extension points for custom assets, groups, tracks, clips, inspectors, and previews.
- [ ] Document the JSON format, version policy, runtime API, and troubleshooting workflow.

## Definition Of Done

- [ ] A designer can create, preview, validate, save, reload, and execute a complete combat skill without writing code.
- [ ] The same skill produces equivalent ordered events in editor preview and a player build.
- [ ] Invalid data cannot be saved without clear, actionable diagnostics.
- [ ] All destructive editor operations support Undo/Redo and recovery.
- [ ] Core authoring and runtime paths are covered by automated tests.
