#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using BuffSystem.Core;
using BuffSystem.Data;
using BuffSystem.Events;
using BuffSystem.Runtime;
using UnityEditor;
using UnityEngine;

namespace BuffSystem.Editor
{
    /// <summary>
    /// Buff系统调试窗口 - 可视化调试工具
    /// </summary>
    public class BuffSystemDebugger : EditorWindow
    {
        // 滚动视图位置
        private Vector2 ownerListScroll;
        private Vector2 buffListScroll;
        private Vector2 eventLogScroll;
        private Vector2 buffDetailsScroll;

        // 选中项
        private BuffOwner selectedOwner;
        private IBuff selectedBuff;
        private IBuffData selectedBuffData;

        // 自动刷新设置
        private bool autoRefresh = true;
        private float refreshInterval = 0.5f;
        private double lastRefreshTime;

        // 事件日志
        private List<EventLogEntry> eventLogs = new();
        private bool showEventLog = true;
        private int maxEventLogs = 100;

        // 搜索过滤
        private string ownerSearchFilter = "";
        private string buffSearchFilter = "";

        // 折叠状态
        private bool showOwnerDetails = true;
        private bool showBuffStats = true;
        private bool showSystemInfo = true;

        // 添加Buff对话框
        private bool showAddBuffDialog;
        private int selectedBuffId;
        private string selectedBuffName = "";
        private Vector2 buffDatabaseScroll;

        // 窗口实例
        private static BuffSystemDebugger instance;

        /// <summary>
        /// 事件日志条目
        /// </summary>
        private struct EventLogEntry
        {
            public double Timestamp;
            public string Message;
            public LogType LogType;
            public Color Color;

            public EventLogEntry(string message, LogType logType = LogType.Log)
            {
                Timestamp = EditorApplication.timeSinceStartup;
                Message = message;
                LogType = logType;
                Color = logType switch
                {
                    LogType.Error => Color.red,
                    LogType.Warning => Color.yellow,
                    LogType.Log => Color.white,
                    _ => Color.white
                };
            }
        }

        [MenuItem("Window/BuffSystem/Buff Debugger")]
        public static void ShowWindow()
        {
            instance = GetWindow<BuffSystemDebugger>("Buff Debugger");
            instance.Show();
        }

        private void OnEnable()
        {
            instance = this;
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
            if (instance == this)
                instance = null;
        }

        /// <summary>
        /// 订阅Buff系统事件
        /// </summary>
        private void SubscribeToEvents()
        {
            BuffEventSystem.OnBuffAdded += OnBuffAdded;
            BuffEventSystem.OnBuffRemoved += OnBuffRemoved;
            BuffEventSystem.OnStackChanged += OnStackChanged;
            BuffEventSystem.OnBuffRefreshed += OnBuffRefreshed;
            BuffEventSystem.OnBuffExpired += OnBuffExpired;
        }

        /// <summary>
        /// 取消订阅事件
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            BuffEventSystem.OnBuffAdded -= OnBuffAdded;
            BuffEventSystem.OnBuffRemoved -= OnBuffRemoved;
            BuffEventSystem.OnStackChanged -= OnStackChanged;
            BuffEventSystem.OnBuffRefreshed -= OnBuffRefreshed;
            BuffEventSystem.OnBuffExpired -= OnBuffExpired;
        }

        #region Event Handlers

        private void OnBuffAdded(object sender, BuffAddedEventArgs e)
        {
            AddEventLog($"[+] {e.Buff.Name} 添加到 {e.Buff.Owner?.OwnerName}", LogType.Log);
        }

        private void OnBuffRemoved(object sender, BuffRemovedEventArgs e)
        {
            AddEventLog($"[-] {e.Buff.Name} 从 {e.Buff.Owner?.OwnerName} 移除", LogType.Log);
        }

        private void OnStackChanged(object sender, BuffStackChangedEventArgs e)
        {
            AddEventLog($"[↻] {e.Buff.Name} 层数变化: {e.OldStack} → {e.NewStack}", LogType.Log);
        }

        private void OnBuffRefreshed(object sender, BuffRefreshedEventArgs e)
        {
            AddEventLog($"[↺] {e.Buff.Name} 刷新", LogType.Log);
        }

        private void OnBuffExpired(object sender, BuffExpiredEventArgs e)
        {
            AddEventLog($"[×] {e.Buff.Name} 过期", LogType.Warning);
        }

        private void AddEventLog(string message, LogType logType)
        {
            eventLogs.Add(new EventLogEntry(message, logType));
            if (eventLogs.Count > maxEventLogs)
            {
                eventLogs.RemoveAt(0);
            }
        }

        #endregion

        private void OnGUI()
        {
            DrawToolbar();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            {
                // 左侧面板：Owner列表
                EditorGUILayout.BeginVertical(GUILayout.Width(280));
                DrawOwnerList();
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                {
                    // 中间面板：Buff详情
                    DrawBuffDetails();

                    EditorGUILayout.Space();

                    // 底部面板：事件日志
                    if (showEventLog)
                    {
                        DrawEventLog();
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();

            // 自动刷新
            if (autoRefresh && EditorApplication.timeSinceStartup - lastRefreshTime > refreshInterval)
            {
                Repaint();
                lastRefreshTime = EditorApplication.timeSinceStartup;
            }
        }

        #region Toolbar

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                autoRefresh = GUILayout.Toggle(autoRefresh, "自动刷新", EditorStyles.toolbarButton, GUILayout.Width(70));

                EditorGUI.BeginChangeCheck();
                refreshInterval = EditorGUILayout.Slider(refreshInterval, 0.1f, 2f, GUILayout.Width(120));
                if (EditorGUI.EndChangeCheck())
                {
                    refreshInterval = Mathf.Round(refreshInterval * 10f) / 10f;
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("刷新", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    Repaint();
                }

                showEventLog = GUILayout.Toggle(showEventLog, "显示日志", EditorStyles.toolbarButton, GUILayout.Width(70));

                if (GUILayout.Button("清空日志", EditorStyles.toolbarButton, GUILayout.Width(70)))
                {
                    eventLogs.Clear();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region Owner List

        private void DrawOwnerList()
        {
            EditorGUILayout.LabelField("Buff Owners", EditorStyles.boldLabel);

            // 搜索框
            EditorGUILayout.BeginHorizontal();
            {
                ownerSearchFilter = EditorGUILayout.TextField(ownerSearchFilter, EditorStyles.toolbarSearchField);
                if (GUILayout.Button("×", GUILayout.Width(20)))
                {
                    ownerSearchFilter = "";
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(2);

            // 系统信息折叠
            showSystemInfo = EditorGUILayout.Foldout(showSystemInfo, "系统信息", true);
            if (showSystemInfo)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.LabelField($"活跃持有者: {BuffOwner.ActiveOwnerCount}", EditorStyles.miniLabel);
                    EditorGUILayout.LabelField($"事件日志数: {eventLogs.Count}/{maxEventLogs}", EditorStyles.miniLabel);
                }
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space(2);

            // Owner列表
            ownerListScroll = EditorGUILayout.BeginScrollView(ownerListScroll, GUI.skin.box);
            {
                var owners = BuffOwner.AllOwners;
                bool anyOwnerDisplayed = false;

                foreach (var owner in owners)
                {
                    if (owner == null) continue;

                    // 搜索过滤
                    if (!string.IsNullOrEmpty(ownerSearchFilter))
                    {
                        if (!owner.OwnerName.ToLower().Contains(ownerSearchFilter.ToLower()))
                            continue;
                    }

                    anyOwnerDisplayed = true;
                    DrawOwnerItem(owner);
                }

                if (!anyOwnerDisplayed)
                {
                    EditorGUILayout.HelpBox(
                        string.IsNullOrEmpty(ownerSearchFilter)
                            ? "场景中没有BuffOwner"
                            : "没有匹配的BuffOwner",
                        MessageType.Info);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawOwnerItem(BuffOwner owner)
        {
            bool isSelected = owner == selectedOwner;
            var style = isSelected ? new GUIStyle(EditorStyles.selectionRect) : new GUIStyle(GUIStyle.none);
            if (isSelected)
            {
                style.normal.background = EditorGUIUtility.isProSkin
                    ? MakeTex(2, 2, new Color(0.2f, 0.4f, 0.6f))
                    : MakeTex(2, 2, new Color(0.3f, 0.5f, 0.9f));
            }

            EditorGUILayout.BeginHorizontal(style, GUILayout.Height(24));
            {
                // 选中点击区域
                Rect clickRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.Height(20));

                // Owner名称和Buff数量
                EditorGUILayout.LabelField(owner.OwnerName, EditorStyles.boldLabel, GUILayout.Width(140));
                EditorGUILayout.LabelField($"({owner.BuffCount})", EditorStyles.miniLabel, GUILayout.Width(40));

                // Ping按钮
                if (GUILayout.Button("Ping", EditorStyles.miniButton, GUILayout.Width(40)))
                {
                    EditorGUIUtility.PingObject(owner);
                    Selection.activeGameObject = owner.gameObject;
                }

                // 处理点击
                if (Event.current.type == EventType.MouseDown && clickRect.Contains(Event.current.mousePosition))
                {
                    selectedOwner = owner;
                    selectedBuff = null;
                    Event.current.Use();
                    Repaint();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region Buff Details

        private void DrawBuffDetails()
        {
            if (selectedOwner == null)
            {
                EditorGUILayout.HelpBox("选择一个BuffOwner查看详情", MessageType.Info);
                return;
            }

            // Owner详情头部
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField($"Owner: {selectedOwner.OwnerName}", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Ping", EditorStyles.miniButton, GUILayout.Width(50)))
                {
                    EditorGUIUtility.PingObject(selectedOwner);
                    Selection.activeGameObject = selectedOwner.gameObject;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(2);

            // 搜索过滤
            EditorGUILayout.BeginHorizontal();
            {
                buffSearchFilter = EditorGUILayout.TextField(buffSearchFilter, EditorStyles.toolbarSearchField);
                if (GUILayout.Button("×", GUILayout.Width(20)))
                {
                    buffSearchFilter = "";
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(2);

            // Buff列表
            buffListScroll = EditorGUILayout.BeginScrollView(buffListScroll, GUI.skin.box, GUILayout.MinHeight(200));
            {
                var buffs = selectedOwner.BuffContainer?.AllBuffs;
                if (buffs == null || buffs.Count == 0)
                {
                    EditorGUILayout.HelpBox("没有Buff", MessageType.Info);
                }
                else
                {
                    bool anyBuffDisplayed = false;
                    foreach (var buff in buffs)
                    {
                        if (buff == null) continue;

                        // 搜索过滤
                        if (!string.IsNullOrEmpty(buffSearchFilter))
                        {
                            if (!buff.Name.ToLower().Contains(buffSearchFilter.ToLower()))
                                continue;
                        }

                        anyBuffDisplayed = true;
                        DrawBuffItem(buff);
                    }

                    if (!anyBuffDisplayed)
                    {
                        EditorGUILayout.HelpBox("没有匹配的Buff", MessageType.Info);
                    }
                }
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(2);

            // 手动操作按钮
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("添加Buff", GUILayout.Height(25)))
                {
                    showAddBuffDialog = true;
                }

                if (GUILayout.Button("清空所有Buff", GUILayout.Height(25)))
                {
                    if (EditorUtility.DisplayDialog("确认", $"确定要清空 {selectedOwner.OwnerName} 的所有Buff吗？", "确定", "取消"))
                    {
                        selectedOwner.ClearBuffs();
                        AddEventLog($"[!] 清空 {selectedOwner.OwnerName} 的所有Buff", LogType.Warning);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            // 添加Buff对话框
            if (showAddBuffDialog)
            {
                DrawAddBuffDialog();
            }

            // 选中Buff详情
            if (selectedBuff != null)
            {
                EditorGUILayout.Space(5);
                DrawSelectedBuffDetails();
            }
        }

        private void DrawBuffItem(IBuff buff)
        {
            bool isSelected = buff == selectedBuff;
            var style = isSelected ? new GUIStyle(EditorStyles.selectionRect) : new GUIStyle(GUIStyle.none);
            if (isSelected)
            {
                style.normal.background = EditorGUIUtility.isProSkin
                    ? MakeTex(2, 2, new Color(0.2f, 0.4f, 0.6f))
                    : MakeTex(2, 2, new Color(0.3f, 0.5f, 0.9f));
            }

            EditorGUILayout.BeginHorizontal(style, GUILayout.Height(28));
            {
                // 选中点击区域
                Rect clickRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.Height(24));

                // Buff信息
                EditorGUILayout.LabelField(buff.Name, EditorStyles.boldLabel, GUILayout.Width(120));
                EditorGUILayout.LabelField($"层数: {buff.CurrentStack}/{buff.MaxStack}", GUILayout.Width(80));

                if (buff.IsPermanent)
                {
                    EditorGUILayout.LabelField("永久", GUILayout.Width(60));
                }
                else
                {
                    EditorGUILayout.LabelField($"{buff.RemainingTime:F1}s", GUILayout.Width(60));
                }

                GUILayout.FlexibleSpace();

                // 操作按钮
                if (GUILayout.Button("+", GUILayout.Width(25)))
                {
                    buff.AddStack(1);
                }

                if (GUILayout.Button("-", GUILayout.Width(25)))
                {
                    buff.RemoveStack(1);
                }

                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("×", GUILayout.Width(25)))
                {
                    buff.MarkForRemoval();
                }
                GUI.backgroundColor = Color.white;

                // 处理点击
                if (Event.current.type == EventType.MouseDown && clickRect.Contains(Event.current.mousePosition))
                {
                    selectedBuff = buff;
                    Event.current.Use();
                    Repaint();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawAddBuffDialog()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("添加Buff", EditorStyles.boldLabel);

                // Buff列表
                buffDatabaseScroll = EditorGUILayout.BeginScrollView(buffDatabaseScroll, GUILayout.Height(150));
                {
                    var allBuffs = BuffDatabase.Instance.GetAllBuffData();
                    foreach (var buffData in allBuffs)
                    {
                        if (buffData == null) continue;

                        EditorGUILayout.BeginHorizontal();
                        {
                            bool isSelected = selectedBuffData == buffData;
                            if (GUILayout.Toggle(isSelected, $"{buffData.Name} (ID: {buffData.Id})", EditorStyles.radioButton))
                            {
                                selectedBuffData = buffData;
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndScrollView();

                EditorGUILayout.Space(5);

                // 按钮
                EditorGUILayout.BeginHorizontal();
                {
                    GUI.enabled = selectedBuffData != null;
                    if (GUILayout.Button("添加", GUILayout.Height(25)))
                    {
                        if (selectedBuffData != null && selectedOwner != null)
                        {
                            selectedOwner.AddBuff(selectedBuffData.Id);
                            showAddBuffDialog = false;
                        }
                    }
                    GUI.enabled = true;

                    if (GUILayout.Button("取消", GUILayout.Height(25)))
                    {
                        showAddBuffDialog = false;
                        selectedBuffData = null;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawSelectedBuffDetails()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Buff详情", EditorStyles.boldLabel);

                buffDetailsScroll = EditorGUILayout.BeginScrollView(buffDetailsScroll, GUILayout.MaxHeight(200));
                {
                    EditorGUILayout.LabelField("基本信息", EditorStyles.miniBoldLabel);
                    EditorGUILayout.LabelField($"  名称: {selectedBuff.Name}");
                    EditorGUILayout.LabelField($"  实例ID: {selectedBuff.InstanceId}");
                    EditorGUILayout.LabelField($"  数据ID: {selectedBuff.DataId}");

                    EditorGUILayout.Space(5);

                    EditorGUILayout.LabelField("层数信息", EditorStyles.miniBoldLabel);
                    EditorGUILayout.LabelField($"  当前层数: {selectedBuff.CurrentStack}");
                    EditorGUILayout.LabelField($"  最大层数: {selectedBuff.MaxStack}");

                    EditorGUILayout.Space(5);

                    EditorGUILayout.LabelField("时间信息", EditorStyles.miniBoldLabel);
                    if (selectedBuff.IsPermanent)
                    {
                        EditorGUILayout.LabelField("  类型: 永久Buff");
                    }
                    else
                    {
                        EditorGUILayout.LabelField($"  总持续时间: {selectedBuff.TotalDuration:F2}s");
                        EditorGUILayout.LabelField($"  当前持续时间: {selectedBuff.Duration:F2}s");
                        EditorGUILayout.LabelField($"  剩余时间: {selectedBuff.RemainingTime:F2}s");
                    }

                    EditorGUILayout.Space(5);

                    EditorGUILayout.LabelField("状态信息", EditorStyles.miniBoldLabel);
                    EditorGUILayout.LabelField($"  是否激活: {selectedBuff.IsActive}");
                    EditorGUILayout.LabelField($"  标记移除: {selectedBuff.IsMarkedForRemoval}");

                    if (selectedBuff.Source != null)
                    {
                        EditorGUILayout.Space(5);
                        EditorGUILayout.LabelField("来源信息", EditorStyles.miniBoldLabel);
                        EditorGUILayout.LabelField($"  来源: {selectedBuff.Source}");
                        EditorGUILayout.LabelField($"  来源ID: {selectedBuff.SourceId}");
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }

        #endregion

        #region Event Log

        private void DrawEventLog()
        {
            EditorGUILayout.LabelField("事件日志", EditorStyles.boldLabel);

            eventLogScroll = EditorGUILayout.BeginScrollView(eventLogScroll, GUI.skin.box, GUILayout.Height(150));
            {
                if (eventLogs.Count == 0)
                {
                    EditorGUILayout.LabelField("暂无事件", EditorStyles.centeredGreyMiniLabel);
                }
                else
                {
                    for (int i = eventLogs.Count - 1; i >= 0; i--)
                    {
                        var log = eventLogs[i];
                        double relativeTime = EditorApplication.timeSinceStartup - log.Timestamp;
                        string timeStr = $"[{relativeTime:F1}s ago]";

                        GUI.color = log.Color;
                        EditorGUILayout.LabelField($"{timeStr} {log.Message}", EditorStyles.miniLabel);
                        GUI.color = Color.white;
                    }
                }
            }
            EditorGUILayout.EndScrollView();
        }

        #endregion

        #region Utility

        /// <summary>
        /// 创建纯色纹理
        /// </summary>
        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        #endregion
    }
}
#endif
