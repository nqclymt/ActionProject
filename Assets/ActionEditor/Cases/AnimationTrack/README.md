# 动画轨道验收案例

该案例验证动画状态、播放速度、过渡时间、Animator 层级、Avatar Mask 和 Root Motion 配置，以及编辑器预览与运行时播放。

1. 执行菜单 `Tools/ActionEditor Cases/创建动画轨道验收案例`。
2. 在 ActionEditor 播放或拖动时间轴，确认立方体先放大、再切换状态缩小，并在两个片段之间过渡。
3. 选中动画片段，确认 Inspector 中可编辑状态名称、播放速度、过渡时间和动画起始时间。
4. 选中动画轨道，确认 Inspector 中可编辑 Animator 层级、Avatar Mask 和 Root Motion。
5. 进入 Play Mode，在案例物体组件菜单执行“播放动画轨道运行时案例”，确认立方体播放同一动画且 Console 输出完成消息。
6. 执行菜单 `Tools/ActionEditor Cases/清理动画轨道验收案例` 清理生成资源。

删除整个 `Assets/ActionEditor/Cases` 目录不会影响 ActionEditor Runtime 或 Editor 程序集。
