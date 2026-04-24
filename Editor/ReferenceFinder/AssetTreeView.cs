using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;

//带数据的TreeViewItem
public class AssetViewItem : TreeViewItem
{
    public ReferenceFinderData.AssetDescription data;
}

//资源引用树
public class AssetTreeView : TreeView
{
    //图标宽度
    const float kIconWidth = 18f;
    //列表高度
    const float kRowHeights = 20f;
    public AssetViewItem assetRoot;

    private GUIStyle stateGUIStyle = new GUIStyle { richText = true, alignment = TextAnchor.MiddleCenter };
    private readonly Dictionary<string, Texture2D> iconCache = new Dictionary<string, Texture2D>();

    //列信息
    enum MyColumns
    {
        Name,
        Path,
        State,
    }

    public AssetTreeView(TreeViewState state,MultiColumnHeader multicolumnHeader):base(state,multicolumnHeader)
    {
        rowHeight = kRowHeights;
        columnIndexForTreeFoldouts = 0;
        showAlternatingRowBackgrounds = true;
        showBorder = false;
        customFoldoutYOffset = (kRowHeights - EditorGUIUtility.singleLineHeight) * 0.5f; // center foldout in the row since we also center content. See RowGUI
        extraSpaceBeforeIconAndLabel = kIconWidth;
    }

    //响应右击事件
    protected override void ContextClickedItem(int id)
    {
        var item = (AssetViewItem)FindItem(id, rootItem);
        if (item == null || item.data == null)
            return;

        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Ping"), false, () => PingAsset(item));
        menu.AddItem(new GUIContent("Select"), false, () => SelectAsset(item));
        menu.AddItem(new GUIContent("Open"), false, () => OpenAsset(item));
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Copy Path"), false, () => EditorGUIUtility.systemCopyBuffer = item.data.path);
        menu.AddItem(new GUIContent("Copy GUID"), false, () => EditorGUIUtility.systemCopyBuffer = AssetDatabase.AssetPathToGUID(item.data.path));
        menu.AddSeparator("");
        menu.AddItem(new GUIContent(IsExpanded(id) ? "Collapse" : "Expand"), false, () => SetExpanded(id, !IsExpanded(id)));
        menu.AddItem(new GUIContent("Reveal in Explorer"), false, () => EditorUtility.RevealInFinder(item.data.path));
        menu.ShowAsContext();
    }

    //响应双击事件
    protected override void DoubleClickedItem(int id)
    {
        var item = (AssetViewItem)FindItem(id, rootItem);
        //在ProjectWindow中高亮双击资源
        if (item != null && item.data != null)
        {
            PingAsset(item);
        }
    }
    
    //生成ColumnHeader
    public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState(float treeViewWidth)
    {
        var columns = new[]
        {
            //图标+名称
            new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent("Name"),
                headerTextAlignment = TextAlignment.Center,
                sortedAscending = false,
                width = 200,
                minWidth = 60,
                autoResize = false,
                allowToggleVisibility = false,
                canSort = false        
            },
            //路径
            new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent("Path"),
                headerTextAlignment = TextAlignment.Center,
                sortedAscending = false,
                width = 360,
                minWidth = 60,
                autoResize = false,
                allowToggleVisibility = false,
                canSort = false
    },
            //状态
            new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent("State"),
                headerTextAlignment = TextAlignment.Center,
                sortedAscending = false,
                width = 60,
                minWidth = 60,
                autoResize = false,
                allowToggleVisibility = true,
                canSort = false          
            },
        };
        var state = new MultiColumnHeaderState(columns);
        return state;
    }

    protected override TreeViewItem BuildRoot()
    {
        return assetRoot;
    }

    protected override void RowGUI(RowGUIArgs args)
    {
        var item = (AssetViewItem)args.item;
        for(int i = 0; i < args.GetNumVisibleColumns(); ++i)
        {
            CellGUI(args.GetCellRect(i), item, (MyColumns)args.GetColumn(i), ref args);
        }
    }

    //绘制列表中的每项内容
    void CellGUI(Rect cellRect,AssetViewItem item,MyColumns column, ref RowGUIArgs args)
    {
        CenterRectUsingSingleLineHeight(ref cellRect);
        switch (column)
        {
            case MyColumns.Name:
                {
                    var iconRect = cellRect;
                    iconRect.x += GetContentIndent(item);
                    iconRect.width = kIconWidth;
                    if (iconRect.x < cellRect.xMax)
                    {
                        var icon = GetIcon(item.data.path);
                        if(icon != null)
                            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
                    }                        
                    args.rowRect = cellRect;
                    base.RowGUI(args);
                }
                break;
            case MyColumns.Path:
                {
                    GUI.Label(cellRect, item.data.path);
                }
                break;
            case MyColumns.State:
                {
                    GUI.Label(cellRect, ReferenceFinderData.GetInfoByState(item.data.state),stateGUIStyle);
                }
                break;
        }
    }

    //根据资源信息获取资源图标
    private Texture2D GetIcon(string path)
    {
        if (string.IsNullOrEmpty(path))
            return null;

        if (iconCache.ContainsKey(path))
            return iconCache[path];

        Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
        if (obj != null)
        {
            Texture2D icon = AssetPreview.GetMiniThumbnail(obj);
            if (icon == null)
                icon = AssetPreview.GetMiniTypeThumbnail(obj.GetType());
            iconCache[path] = icon;
            return icon;
        }
        iconCache[path] = null;
        return null;
    }

    private static Object LoadAsset(AssetViewItem item)
    {
        if (item == null || item.data == null || string.IsNullOrEmpty(item.data.path))
            return null;

        return AssetDatabase.LoadAssetAtPath(item.data.path, typeof(Object));
    }

    private static void PingAsset(AssetViewItem item)
    {
        Object assetObject = LoadAsset(item);
        if (assetObject == null)
            return;

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = assetObject;
        EditorGUIUtility.PingObject(assetObject);
    }

    private static void SelectAsset(AssetViewItem item)
    {
        Object assetObject = LoadAsset(item);
        if (assetObject == null)
            return;

        Selection.activeObject = assetObject;
    }

    private static void OpenAsset(AssetViewItem item)
    {
        Object assetObject = LoadAsset(item);
        if (assetObject != null)
            AssetDatabase.OpenAsset(assetObject);
    }
}
