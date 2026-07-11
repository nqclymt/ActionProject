# ActionEditor Development Roadmap / ActionEditor 开发路线图

This checklist records the work required to turn the current timeline framework into a production-ready combat skill editor. / 本清单记录将当前时间轴框架完善为可用于正式生产的战斗技能编辑器所需的工作。

## P0 - Runtime Foundation / 运行时基础

- [ ] Split editor-only and runtime assemblies so skill data and playback code are available in player builds. / 拆分编辑器程序集与运行时程序集，使技能数据和播放代码能够进入正式 Player 构建。
- [ ] Define a versioned `CombatSkillAsset` schema with cast phases, duration, tags, cooldown, costs, targeting rules, and cancellation windows. / 定义带版本号的 `CombatSkillAsset` 数据结构，包含施法阶段、持续时间、标签、冷却、消耗、目标规则和取消窗口。
- [ ] Implement a runtime `SkillPlayer` with play, pause, stop, interrupt, seek, loop, and completion events. / 实现运行时 `SkillPlayer`，支持播放、暂停、停止、打断、跳转、循环和完成事件。
- [ ] Introduce a runtime execution context for caster, target, world position, direction, and shared combat services. / 建立运行时执行上下文，提供施法者、目标、世界坐标、方向和共享战斗服务。
- [ ] Guarantee deterministic ordering for clips that start or end on the same frame. / 保证同一帧开始或结束的片段具有确定且稳定的执行顺序。
- [ ] Add frame-based evaluation that produces the same result in editor preview and runtime playback. / 增加基于帧的求值机制，确保编辑器预览与运行时播放结果一致。

## P0 - Core Combat Tracks / 核心战斗轨道

- [ ] Add an animation track with state, speed, transition, layer, avatar mask, and root-motion controls. / 增加动画轨道，支持状态、速度、过渡、层级、Avatar Mask 和 Root Motion 控制。
- [ ] Add an audio track with volume, pitch, spatial settings, mixer routing, looping, and stop behavior. / 增加音频轨道，支持音量、音调、空间化、混音器路由、循环和停止行为。
- [ ] Add a VFX track with prefab binding, attachment point, transform offsets, lifetime, pooling, and scrub-safe preview. / 增加特效轨道，支持预制体绑定、挂点、变换偏移、生命周期、对象池和可安全拖动时间轴的预览。
- [ ] Add a movement track for displacement, rotation, dash curves, and root-motion overrides. / 增加位移轨道，支持位置移动、旋转、冲刺曲线和 Root Motion 覆盖。
- [ ] Add hitbox and hurtbox clips with shape, bone attachment, target filters, multi-hit interval, and Gizmo rendering. / 增加攻击盒与受击盒片段，支持形状、骨骼挂点、目标过滤、多段命中间隔和 Gizmo 绘制。
- [ ] Add damage and gameplay-event clips for damage, stagger, knockback, buffs, debuffs, invulnerability, and custom events. / 增加伤害与玩法事件片段，覆盖伤害、硬直、击退、增益、减益、无敌和自定义事件。
- [ ] Add camera clips for shake, impulse, FOV, and target framing. / 增加相机片段，支持震动、冲击、视野角和目标构图。

## P1 - Preview Environment / 预览环境

- [ ] Implement caster and target binding instead of relying on placeholder actor references. / 实现施法者和目标绑定，替换当前占位性质的 Actor 引用。
- [ ] Provide an isolated preview scene with configurable characters, ground, camera, and spawn positions. / 提供独立预览场景，可配置角色、地面、相机和出生位置。
- [ ] Make animation, audio, VFX, movement, and hitboxes seekable and reversible while scrubbing. / 使动画、音频、特效、位移和碰撞盒在拖动时间轴时可跳转并可逆向恢复。
- [ ] Reset all preview objects reliably when switching assets, recompiling scripts, closing the window, or stopping playback. / 在切换资源、重新编译脚本、关闭窗口或停止播放时可靠重置所有预览对象。
- [ ] Add track mute, solo, enable, and preview-only controls. / 增加轨道静音、独奏、启用和仅预览控制。
- [ ] Display current frame, active clips, fired events, and hit results during preview. / 预览期间显示当前帧、活动片段、已触发事件和命中结果。

## P1 - Authoring Reliability / 编辑可靠性

- [ ] Add complete Unity Undo/Redo support for create, delete, move, resize, paste, reorder, and inspector edits. / 为创建、删除、移动、缩放、粘贴、重排和 Inspector 修改提供完整的 Unity Undo/Redo 支持。
- [ ] Complete copy, cut, paste, duplicate, and multi-selection workflows for groups, tracks, and clips. / 完善组、轨道和片段的复制、剪切、粘贴、创建副本和多选工作流。
- [ ] Add keyboard shortcuts for playback, frame stepping, delete, duplicate, save, focus, and zoom. / 增加播放、逐帧、删除、复制、保存、聚焦和缩放快捷键。
- [ ] Add reusable skill, track, and clip templates. / 增加可复用的技能、轨道和片段模板。
- [ ] Add drag-and-drop creation from animation, audio, and prefab assets. / 支持从动画、音频和预制体资源拖放创建片段。
- [ ] Add clip snapping to frames, markers, neighboring clips, animation events, and range boundaries. / 支持片段吸附到帧、标记、相邻片段、动画事件和播放范围边界。
- [ ] Add batch editing for shared clip properties. / 支持批量修改多个片段的公共属性。

## P1 - Data Safety And Validation / 数据安全与校验

- [ ] Replace fragile asset paths with GUID-based resource references where applicable. / 在适用位置使用基于 GUID 的资源引用替换脆弱的资源路径。
- [ ] Add schema versioning and migrations for existing JSON skill files. / 为现有 JSON 技能文件增加数据版本和迁移机制。
- [ ] Save atomically and keep recoverable autosave snapshots. / 实现原子保存并保留可恢复的自动保存快照。
- [ ] Add a validation panel with navigation to missing references, invalid ranges, overlaps, unsupported audio import settings, and duplicate identifiers. / 增加校验面板，并可定位缺失引用、非法范围、片段重叠、不支持的音频导入设置和重复标识符。
- [ ] Validate that hit, movement, audio, and VFX timing stays inside the intended skill and animation ranges. / 校验命中、位移、音频和特效时序是否处于预期技能与动画范围内。
- [ ] Add stable IDs for groups, tracks, and clips so references survive reordering. / 为组、轨道和片段增加稳定 ID，使引用在重排后仍然有效。
- [ ] Define merge-friendly serialization rules for team development. / 定义便于团队协作和版本合并的序列化规则。

## P2 - Combat Workflow / 战斗工作流

- [ ] Add skill markers for cast start, release, hit confirm, cancel start, cancel end, and recovery end. / 增加施法开始、释放、命中确认、取消开始、取消结束和后摇结束等技能标记。
- [ ] Add branching and conditional execution for hit, miss, critical hit, charge level, and target state. / 支持根据命中、未命中、暴击、蓄力等级和目标状态进行分支与条件执行。
- [ ] Add combo links and transition rules between skills. / 增加技能之间的连招连接和转换规则。
- [ ] Add target-selection previews for point, direction, cone, circle, box, chain, and locked-target skills. / 为点选、方向、扇形、圆形、盒形、链式和锁定目标技能增加选区预览。
- [ ] Add resource-cost and cooldown previews. / 增加资源消耗和冷却预览。
- [ ] Add localization-ready names, descriptions, and designer notes. / 增加支持本地化的名称、描述和设计备注。
- [ ] Add search, filtering, favorites, recent assets, and skill-library browsing. / 增加搜索、筛选、收藏、最近资源和技能库浏览。

## P2 - Debugging And Performance / 调试与性能

- [ ] Add an execution trace showing clip enter, update, exit, reverse, and emitted gameplay events. / 增加执行轨迹，显示片段进入、更新、退出、逆向和发出的玩法事件。
- [ ] Add damage and crowd-control result inspection using configurable test stats. / 使用可配置测试属性检查伤害和控制效果结果。
- [ ] Visualize movement paths, hitboxes, attachment points, and target areas in the Scene view. / 在 Scene 视图中可视化移动路径、碰撞盒、挂点和目标区域。
- [ ] Add warnings and budgets for VFX count, particle count, audio voices, spawned objects, and long-running clips. / 为特效数量、粒子数量、音频声道、生成对象和长时间片段增加警告与预算限制。
- [ ] Integrate pooling checks for runtime VFX and audio emitters. / 为运行时特效和音频发射器集成对象池检查。
- [ ] Add exportable diagnostic reports for invalid skills. / 为非法技能增加可导出的诊断报告。

## P2 - Tests And Documentation / 测试与文档

- [ ] Add unit tests for time conversion, sibling ordering, range playback, blending, serialization, and migrations. / 为时间转换、同级排序、范围播放、混合、序列化和迁移增加单元测试。
- [ ] Add editor tests for create, delete, drag, resize, copy/paste, Undo/Redo, save, and reload workflows. / 为创建、删除、拖动、缩放、复制粘贴、Undo/Redo、保存和重新加载工作流增加编辑器测试。
- [ ] Add runtime tests that compare preview events with player-build execution events. / 增加运行时测试，对比编辑器预览事件和 Player 构建中的执行事件。
- [ ] Add sample combat skills covering melee, projectile, area damage, channeling, movement, and combo cases. / 增加覆盖近战、投射物、范围伤害、引导、位移和连招场景的示例技能。
- [ ] Document extension points for custom assets, groups, tracks, clips, inspectors, and previews. / 记录自定义资源、组、轨道、片段、Inspector 和预览的扩展点。
- [ ] Document the JSON format, version policy, runtime API, and troubleshooting workflow. / 记录 JSON 格式、版本策略、运行时 API 和问题排查流程。

## Definition Of Done / 完成标准

- [ ] A designer can create, preview, validate, save, reload, and execute a complete combat skill without writing code. / 策划无需编写代码即可创建、预览、校验、保存、重新加载并执行完整战斗技能。
- [ ] The same skill produces equivalent ordered events in editor preview and a player build. / 同一技能在编辑器预览和 Player 构建中产生等价且顺序一致的事件。
- [ ] Invalid data cannot be saved without clear, actionable diagnostics. / 非法数据不能在缺少明确且可操作诊断信息的情况下保存。
- [ ] All destructive editor operations support Undo/Redo and recovery. / 所有破坏性编辑操作都支持 Undo/Redo 和恢复。
- [ ] Core authoring and runtime paths are covered by automated tests. / 核心编辑流程和运行时路径均由自动化测试覆盖。
