# 同帧确定性排序验证案例

该目录验证同一时间点的轨道和片段事件在任意输入顺序下都得到相同执行顺序。

1. 在任意 GameObject 上添加 `DeterministicOrderingCase`。
2. 右键组件标题并执行“验证同帧确定性排序”。
3. 确认 Console 输出“同帧确定性排序验证通过。”

删除该目录不会影响 ActionEditor Runtime 或 Editor 程序集。
