using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>
/// シーン内の全TMPテキストのフォントサイズがSampling Point Sizeの整数倍か検証するエディタウィンドウ。
/// </summary>
public sealed class PixelPerfectTextValidator : EditorWindow
{
    /// <summary>検証結果を格納する構造体。</summary>
    struct Entry
    {
        public TextMeshProUGUI Text;
        public float FontSize;
        public int SamplingPointSize;
        public bool IsValid;
    }

    readonly List<Entry> _entries = new();
    Vector2 _scroll;

    [MenuItem("Tools/Pixel Perfect Text Validator")]
    static void Open()
    {
        GetWindow<PixelPerfectTextValidator>("PP Text Validator");
    }

    /// <summary>Transformのヒエラルキーパスを取得する。</summary>
    static string GetPath(Transform t)
    {
        var path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }

        return path;
    }

    /// <summary>シーン内の全TMPテキストを検証する。</summary>
    void Scan()
    {
        _entries.Clear();
        var texts = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var t in texts)
        {
            int sps = t.font != null ? (int)t.font.faceInfo.pointSize : 0;
            bool valid = sps > 0 && t.fontSize > 0
                         && Mathf.Approximately(t.fontSize % sps, 0);

            _entries.Add(new Entry
            {
                Text = t,
                FontSize = t.fontSize,
                SamplingPointSize = sps,
                IsValid = valid,
            });
        }

        Repaint();
    }

    void OnGUI()
    {
        if (GUILayout.Button("シーン内の全TMPテキストを検証", GUILayout.Height(30)))
        {
            Scan();
        }

        if (_entries.Count == 0)
        {
            EditorGUILayout.HelpBox("「検証」ボタンを押してください。", MessageType.Info);
            return;
        }

        // サマリー
        int errorCount = 0;
        foreach (var e in _entries)
        {
            if (!e.IsValid) errorCount++;
        }

        if (errorCount == 0)
        {
            EditorGUILayout.HelpBox(
                $"全{_entries.Count}件 OK — 全てのフォントサイズがSampling Point Sizeの整数倍です。",
                MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox(
                $"{errorCount}/{_entries.Count}件 NG — フォントサイズがSampling Point Sizeの整数倍ではありません。",
                MessageType.Error);
        }

        // テーブルヘッダー
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("状態", GUILayout.Width(30));
        GUILayout.Label("GameObject", GUILayout.Width(200));
        GUILayout.Label("フォントサイズ", GUILayout.Width(90));
        GUILayout.Label("SPS", GUILayout.Width(40));
        GUILayout.Label("フォント", GUILayout.MinWidth(100));
        EditorGUILayout.EndHorizontal();

        // テーブル本体
        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        foreach (var entry in _entries)
        {
            if (entry.Text == null) continue;

            // 行の領域を確保してNG行の背景を赤く塗る
            var rowRect = EditorGUILayout.BeginHorizontal(GUILayout.Height(EditorGUIUtility.singleLineHeight));
            if (!entry.IsValid)
            {
                EditorGUI.DrawRect(rowRect, new Color(0.8f, 0.2f, 0.2f, 0.3f));
            }

            GUILayout.Label(entry.IsValid ? "OK" : "NG", GUILayout.Width(30));

            // クリックでオブジェクトを選択
            if (GUILayout.Button(GetPath(entry.Text.transform), EditorStyles.linkLabel, GUILayout.Width(200)))
            {
                Selection.activeGameObject = entry.Text.gameObject;
                EditorGUIUtility.PingObject(entry.Text.gameObject);
            }

            GUILayout.Label(entry.FontSize.ToString("F1"), GUILayout.Width(90));
            GUILayout.Label(entry.SamplingPointSize.ToString(), GUILayout.Width(40));
            GUILayout.Label(
                entry.Text.font != null ? entry.Text.font.name : "(なし)",
                GUILayout.MinWidth(100));

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }
}
