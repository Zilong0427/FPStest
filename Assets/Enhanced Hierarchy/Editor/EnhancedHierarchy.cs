/* Enhanced Hierarchy for Unity
 * Version 2.1.2, last change 13/04/2017
 * Samuel Schultze
 * samuelschultze@gmail.com
*/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace EnhancedHierarchy {
    /// <summary>
    /// Main class, draws hierarchy items.
    /// </summary>
    [InitializeOnLoad]
    internal static class EnhancedHierarchy {

        private const string UNTAGGED = "Untagged";
        private const int UNLAYERED = 0;

        private static int warningsIconCount;
        private static bool isFirstVisible;
        private static bool isRepaintEvent;
        private static bool isGameObject;
        private static bool hasTag;
        private static bool hasLayer;
        private static string goLogs;
        private static string goWarnings;
        private static string goErrors;
        private static Rect rawRect;
        private static Rect lastRect;
        private static Rect selectionRect;
        private static Color currentColor;
        private static Vector2 selectionStart;
        private static GameObject currentGameObject;
        private static List<GameObject> dragSelection;
        private static GUIContent tempTooltipContent = new GUIContent();

        static EnhancedHierarchy() {
            Utility.ForceUpdateHierarchyEveryFrame();
            EditorApplication.hierarchyWindowItemOnGUI += SetItemInformation;
            EditorApplication.hierarchyWindowItemOnGUI += OnItemGUI;
            EditorApplication.RepaintHierarchyWindow();
        }

        private static void SetItemInformation(int id, Rect rect) {
            if(!Preferences.Enabled)
                return;

            using(new ProfilerSample("Enhanced Hierarchy"))
            using(new ProfilerSample("Getting items information"))
                try {
                    isRepaintEvent = Event.current.type == EventType.Repaint;

                    rawRect = rect;
                    currentGameObject = EditorUtility.InstanceIDToObject(id) as GameObject;

                    isGameObject = currentGameObject;

                    if(isGameObject) {
                        hasTag = currentGameObject.tag != UNTAGGED;
                        hasLayer = currentGameObject.layer != UNLAYERED;
                        currentColor = Utility.GetHierarchyColor(currentGameObject);
                    }

                    isFirstVisible = rawRect.y <= lastRect.y;

                    //if(isFirstVisible)
                    //finalRect = lastRect;

                    //isLastVisible = finalRect == rawRect;

                    //if(isFirstVisible)
                    //    EditorGUI.DrawRect(rawRect, Color.red);
                    //if(isLastVisible)
                    //    EditorGUI.DrawRect(rawRect, Color.blue);

                    if(isRepaintEvent && isFirstVisible)
                        Utility.ShowFPS();
                }
                catch(Exception e) {
                    Utility.LogException(e);
                }
        }

        private static void OnItemGUI(int id, Rect rect) {
            if(!Preferences.Enabled)
                return;

            using(new ProfilerSample("Enhanced Hierarchy"))
                try {
                    if(Preferences.Selection)
                        DoSelection(rawRect);

                    if(isFirstVisible && !Preferences.AllowSelectingLocked)
                        IgnoreLockedSelection();

                    if(isFirstVisible && isRepaintEvent) {
                        if(Preferences.ColorSeparator)
                            ColorSort(rawRect);
                        if(Preferences.LineSeparator)
                            DrawHorizontalSeparator(rawRect);
                    }

                    if(isGameObject && isRepaintEvent) {
                        if(Preferences.Tree)
                            DrawTree(rawRect);
                        warningsIconCount = 0;
                        if(Preferences.Warnings)
                            GetGameObjectWarnings();
                        if(Preferences.Trailing)
                            DoTrailing(rawRect);
                        if(Preferences.Warnings)
                            DrawWarnings(rawRect);
                    }

                    if(isGameObject) {
                        //Undo.RecordObject(currentGameObject, "Hierarchy Changed");
                        //Undo.RegisterFullObjectHierarchyUndo(currentGameObject, "Hierarchy Changed");

                        rect.xMin = rect.xMax - rect.height;
                        rect.x += rect.height - Preferences.Offset;
                        rect.y++;

                        if(Preferences.ReplaceToggle && currentGameObject.transform.childCount > 0)
                            ChildToggle();

                        for(var i = 0; i < Preferences.DrawOrder.Count; i++) {
                            rect.x -= rect.height;
                            GUI.backgroundColor = Styles.backgroundColorEnabled;
                            DrawButtonOnRect(Preferences.DrawOrder[i], rect);
                        }

                        var leftSideRect = rawRect;

                        if(Preferences.LeftmostButton) {
                            leftSideRect.xMin = 0f;
                            leftSideRect.xMax = 18f;
                        }
                        else {
                            leftSideRect.xMax = leftSideRect.xMin;
                            leftSideRect.xMin -= currentGameObject.transform.childCount > 0 || Preferences.Tree ? 30f : 18f;
                            leftSideRect.x += 2f;
                        }

                        DrawButtonOnRect(Preferences.LeftSideButton, leftSideRect);
                        DrawMiniLabel(ref rect);
                        GUI.backgroundColor = Color.white;
                    }

                    if(Preferences.Tooltips && isGameObject && isRepaintEvent)
                        DrawTooltip(rect);
                }
                catch(Exception e) {
                    Utility.LogException(e);
                }
                finally {
                    if(isRepaintEvent)
                        lastRect = rawRect;
                }
        }

        private static void DrawButtonOnRect(DrawType button, Rect rect) {
            switch(button) {
                case DrawType.Enable:
                    DrawActiveButton(rect);
                    break;

                case DrawType.Static:
                    DrawStaticButton(rect);
                    break;

                case DrawType.Lock:
                    DrawLockButton(rect);
                    break;

                case DrawType.Icon:
                    DrawIcon(rect);
                    break;

                case DrawType.ApplyPrefab:
                    DrawPrefabApply(rect);
                    break;

                case DrawType.Tag:
                    DrawTag(rect);
                    break;

                case DrawType.Layer:
                    DrawLayer(rect);
                    break;
            }
        }

        private static void IgnoreLockedSelection() {
            var selection = Selection.objects;
            var changed = false;

            for(var i = 0; i < selection.Length; i++)
                if(selection[i] is GameObject && !EditorUtility.IsPersistent(selection[i]) && (selection[i].hideFlags & HideFlags.NotEditable) != 0) {
                    selection[i] = null;
                    changed = true;
                }

            if(changed) {
                Selection.objects = selection;
                EditorApplication.RepaintHierarchyWindow();
            }
        }

        private static void ChildToggle() {
            switch(Event.current.type) {
                case EventType.Repaint:
                    var rect = rawRect;

                    rect.xMax = rect.xMin;
                    rect.xMin -= 18f;
                    rect.yMin += 2f;
                    rect.x -= 1f;

                    EditorGUI.DrawRect(rect, Styles.normalColor * Utility.PlaymodeTint);
                    if(Preferences.ColorSeparator && rect.y / 16f % 2 <= 1f)
                        EditorGUI.DrawRect(rect, Styles.sortColor * Utility.PlaymodeTint);
                    if(Selection.gameObjects.Contains(currentGameObject))
                        EditorGUI.DrawRect(rect, Utility.HierarchyFocused ? Styles.selectedFocusedColor : Styles.selectedUnfocusedColor);

                    GUI.contentColor = currentColor;
                    Styles.newToggleStyle.Draw(rect, new GUIContent(currentGameObject.transform.childCount.ToString("00")), -1);
                    GUI.contentColor = Color.white;
                    break;
            }
        }

        private static void DrawStaticButton(Rect rect) {
            using(new ProfilerSample("Static toggle")) {
                GUI.changed = false;
                GUI.backgroundColor = currentGameObject.isStatic ? Styles.backgroundColorDisabled : Styles.backgroundColorEnabled;
                GUI.Toggle(rect, currentGameObject.isStatic, Styles.staticContent, Styles.staticToggleStyle);

                if(!GUI.changed)
                    return;

                var isStatic = !currentGameObject.isStatic;
                var selectedObjects = GetSelectedObjectsAndCurrent();
                var changeMode = Utility.AskChangeModeIfNecessary(selectedObjects, Preferences.StaticAskMode.Value, "Change Static Flags",
                    "Do you want to " + (!isStatic ? "enable" : "disable") + " the static flags for all child objects as well?");

                foreach(var obj in selectedObjects)
                    Undo.RegisterFullObjectHierarchyUndo(obj, "Static Flags Changed");

                switch(changeMode) {
                    case ChildrenChangeMode.ObjectOnly:
                        foreach(var obj in selectedObjects)
                            obj.isStatic = isStatic;
                        break;

                    case ChildrenChangeMode.ObjectAndChildren:
                        foreach(var obj in selectedObjects) {
                            var transforms = obj.GetComponentsInChildren<Transform>(true);
                            foreach(var transform in transforms)
                                transform.gameObject.isStatic = isStatic;
                        }
                        break;
                }
            }
        }

        private static void DrawLockButton(Rect rect) {
            using(new ProfilerSample("Lock toggle")) {
                var locked = (currentGameObject.hideFlags & HideFlags.NotEditable) != 0;

                GUI.changed = false;
                GUI.backgroundColor = locked ? Styles.backgroundColorEnabled : Styles.backgroundColorDisabled;
                GUI.Toggle(rect, locked, Styles.lockContent, Styles.lockToggleStyle);

                if(!GUI.changed)
                    return;

                var selectedObjects = GetSelectedObjectsAndCurrent();
                var changeMode = Utility.AskChangeModeIfNecessary(selectedObjects, Preferences.LockAskMode.Value, "Lock Object",
                    "Do you want to " + (!locked ? "lock" : "unlock") + " the children objects as well?");

                foreach(var obj in selectedObjects)
                    Undo.RegisterFullObjectHierarchyUndo(obj, locked ? "Unlock Object" : "Lock object");

                switch(changeMode) {
                    case ChildrenChangeMode.ObjectOnly:
                        foreach(var obj in selectedObjects)
                            if(locked)
                                obj.hideFlags &= ~HideFlags.NotEditable;
                            else
                                obj.hideFlags |= HideFlags.NotEditable;
                        break;

                    case ChildrenChangeMode.ObjectAndChildren:
                        foreach(var obj in selectedObjects)
                            foreach(var transform in obj.GetComponentsInChildren<Transform>(true))
                                if(locked)
                                    transform.gameObject.hideFlags &= ~HideFlags.NotEditable;
                                else
                                    transform.gameObject.hideFlags |= HideFlags.NotEditable;
                        break;
                }

                InternalEditorUtility.RepaintAllViews();
            }
        }

        private static void DrawActiveButton(Rect rect) {
            using(new ProfilerSample("Active toggle")) {
                GUI.changed = false;
                GUI.backgroundColor = currentGameObject.activeSelf ? Styles.backgroundColorEnabled : Styles.backgroundColorDisabled;
                GUI.Toggle(rect, currentGameObject.activeSelf, Styles.activeContent, Styles.activeToggleStyle);

                if(GUI.changed) {
                    var objs = GetSelectedObjectsAndCurrent();
                    var active = !currentGameObject.activeSelf;

                    Undo.RecordObjects(objs.ToArray(), currentGameObject.activeSelf ? "Disabled GameObject" : "Enabled Gameobject");

                    foreach(var obj in objs)
                        obj.SetActive(active);
                }
            }
        }

        private static void DrawIcon(Rect rect) {
            using(new ProfilerSample("Icon")) {
                var content = EditorGUIUtility.ObjectContent(currentGameObject, typeof(GameObject));

                if(!content.image)
                    return;

                content.tooltip = Preferences.Tooltips ? "Change Icon" : string.Empty;
                content.text = string.Empty;

                rect.yMin++;
                rect.xMin++;

                GUI.changed = false;
                GUI.Button(rect, content, EditorStyles.label);

                if(!GUI.changed)
                    return;

                Undo.RegisterFullObjectHierarchyUndo(currentGameObject, "Icon Changed");
                Utility.ShowIconSelector(currentGameObject, rect, true);
            }
        }

        private static void DrawPrefabApply(Rect rect) {
            using(new ProfilerSample("Prefab apply button")) {
                var isPrefab = PrefabUtility.GetPrefabType(currentGameObject) == PrefabType.PrefabInstance;

                GUI.contentColor = isPrefab ? Styles.backgroundColorEnabled : Styles.backgroundColorDisabled;

                if(GUI.Button(rect, Styles.prefabApplyContent, Styles.applyPrefabStyle))
                    if(isPrefab) {
                        var selected = Selection.instanceIDs;
                        Selection.activeGameObject = currentGameObject;
                        EditorApplication.ExecuteMenuItem("GameObject/Apply Changes To Prefab");
                        Selection.instanceIDs = selected;
                    }
                    else {
                        var path = EditorUtility.SaveFilePanelInProject("Save prefab", "New Prefab", "prefab", "Save the selected prefab");
                        if(path.Length > 0)
                            PrefabUtility.CreatePrefab(path, currentGameObject, ReplacePrefabOptions.ConnectToPrefab);
                    }

                GUI.contentColor = Color.white;
            }
        }

        private static void DrawLayer(Rect rect) {
            using(new ProfilerSample("Layer")) {
                GUI.changed = false;

                EditorGUI.LabelField(rect, Styles.layerContent);
                var layer = EditorGUI.LayerField(rect, currentGameObject.layer, Styles.layerStyle);

                if(GUI.changed)
                    Utility.ChangeLayerAndAskForChildren(GetSelectedObjectsAndCurrent(), layer);
            }
        }

        private static void DrawTag(Rect rect) {
            using(new ProfilerSample("Tag")) {
                GUI.changed = false;

                EditorGUI.LabelField(rect, Styles.tagContent);
                var tag = EditorGUI.TagField(rect, Styles.tagContent, currentGameObject.tag, Styles.tagStyle);

                if(GUI.changed && tag != currentGameObject.tag)
                    Utility.ChangeTagAndAskForChildren(GetSelectedObjectsAndCurrent(), tag);
            }
        }

        private static void DrawHorizontalSeparator(Rect rect) {
            using(new ProfilerSample("Horizontal separator")) {
                rect.xMin = 0f;
                rect.xMax = rect.xMax + 50f;
                rect.yMax = rect.yMin + 1f;

                var count = Mathf.Max(100, (lastRect.y - rect.y) / lastRect.height);

                for(var i = 0; i < count; i++) {
                    rect.y += lastRect.height;
                    EditorGUI.DrawRect(rect, Styles.lineColor);
                }
            }
        }

        private static void ColorSort(Rect rect) {
            using(new ProfilerSample("Colored sort")) {
                rect.xMin = 0f;
                rect.xMax = rect.xMax + 50f;

                var count = Mathf.Max(100, (lastRect.y - rect.y) / lastRect.height);

                for(var i = 0; i < count; i++) {
                    if(rect.y / 16f % 2 < 1f)
                        EditorGUI.DrawRect(rect, Styles.sortColor);
                    rect.y += rect.height;
                }
            }
        }

        private static void DrawTree(Rect rect) {
            using(new ProfilerSample("Hierarchy tree")) {
                rect.xMin -= 14f;
                rect.xMax = rect.xMin + 14f;

                GUI.color = currentColor;

                if(currentGameObject.transform.childCount == 0 && currentGameObject.transform.parent) {
                    if(Utility.LastInHierarchy(currentGameObject.transform))
                        GUI.DrawTexture(rect, Styles.treeEndTexture);
                    else
                        GUI.DrawTexture(rect, Styles.treeMiddleTexture);
                }

                var parent = currentGameObject.transform.parent;

                for(rect.x -= 14f; rect.xMin > 0f && parent && parent.parent; rect.x -= 14f) {
                    GUI.color = Utility.GetHierarchyColor(parent.parent);
                    if(!Utility.LastInHierarchy(parent))
                        GUI.DrawTexture(rect, Styles.treeLineTexture);
                    parent = parent.parent;
                }

                GUI.color = Color.white;
            }
        }

        private static void GetGameObjectWarnings() {
            using(new ProfilerSample("Retrieve warnings")) {
                List<LogEntry> contextEntries;

                goLogs = string.Empty;
                goWarnings = string.Empty;
                goErrors = string.Empty;

                var components = currentGameObject.GetComponents<MonoBehaviour>();

                for(var i = 0; i < components.Length; i++)
                    if(!components[i]) {
                        goWarnings += "Missing mono behaviour\n";
                        break;
                    }

                if(LogEntry.ReferencedObjects.TryGetValue(currentGameObject, out contextEntries)) {
                    var count = contextEntries.Count;

                    for(var i = 0; i < count; i++)
                        if(goLogs.Length < 150 && contextEntries[i].HasMode(EntryMode.ScriptingLog))
                            goLogs += contextEntries[i] + "\n";
                        else if(goWarnings.Length < 150 && contextEntries[i].HasMode(EntryMode.ScriptingWarning))
                            goWarnings += contextEntries[i] + "\n";
                        else if(goErrors.Length < 150 && contextEntries[i].HasMode(EntryMode.ScriptingError))
                            goErrors += contextEntries[i] + "\n";
                }

                if(goLogs.Length > 0)
                    warningsIconCount++;
                if(goWarnings.Length > 0)
                    warningsIconCount++;
                if(goErrors.Length > 0)
                    warningsIconCount++;
            }
        }

        private static void DrawWarnings(Rect rect) {
            var labelSize = EditorStyles.label.CalcSize(new GUIContent(currentGameObject.name)).x;

            rect.xMin += labelSize;
            rect.xMin = Math.Min(rect.xMax - (Preferences.DrawOrder.Count + warningsIconCount) * rect.height - CalcMiniLabelSize().x - 5f - Preferences.Offset, rect.xMin);
            rect.height = 17f;
            rect.xMax = rect.xMin + rect.height;

            if(goLogs.Length > 0) {
                tempTooltipContent.tooltip = goLogs;
                GUI.DrawTexture(rect, Styles.infoIcon, ScaleMode.ScaleToFit);
                EditorGUI.LabelField(rect, tempTooltipContent);
                rect.x += rect.width;
            }
            if(goWarnings.Length > 0) {
                tempTooltipContent.tooltip = goWarnings;
                GUI.DrawTexture(rect, Styles.warningIcon, ScaleMode.ScaleToFit);
                EditorGUI.LabelField(rect, tempTooltipContent);
                rect.x += rect.width;
            }
            if(goErrors.Length > 0) {
                tempTooltipContent.tooltip = goErrors;
                GUI.DrawTexture(rect, Styles.errorIcon, ScaleMode.ScaleToFit);
                EditorGUI.LabelField(rect, tempTooltipContent);
                rect.x += rect.width;
            }
        }

        private static void DoTrailing(Rect rect) {
            using(new ProfilerSample("Trailing")) {
                var size = Styles.labelNormal.CalcSize(new GUIContent(currentGameObject.name));

                rect.xMax -= (Preferences.DrawOrder.Count + warningsIconCount) * rect.height + CalcMiniLabelSize().x + Preferences.Offset;

                if(size.x < rect.width)
                    return;

                rect.yMin += 2f;
                rect.xMin = rect.xMax - 18f;
                rect.xMax = rawRect.xMax;

                EditorGUI.DrawRect(rect, Styles.normalColor * Utility.PlaymodeTint);
                if(Preferences.ColorSeparator && rect.y / 16f % 2 <= 1f)
                    EditorGUI.DrawRect(rect, Styles.sortColor * Utility.PlaymodeTint);
                if(Selection.gameObjects.Contains(currentGameObject))
                    EditorGUI.DrawRect(rect, Utility.HierarchyFocused ? Styles.selectedFocusedColor : Styles.selectedUnfocusedColor);

                GUI.contentColor = currentColor;
                EditorGUI.LabelField(rect, "...");
                GUI.contentColor = Color.white;

                return;
            }
        }

        private static void TagMiniLabel(ref Rect rect) {
            Styles.miniLabelStyle.fontSize = Preferences.SmallerMiniLabel ? 8 : 9;
            rect.xMin -= Styles.miniLabelStyle.CalcSize(new GUIContent(currentGameObject.tag)).x;
            var tag = EditorGUI.TagField(rect, currentGameObject.tag, Styles.miniLabelStyle);

            if(GUI.changed)
                Utility.ChangeTagAndAskForChildren(GetSelectedObjectsAndCurrent(), tag);
        }

        private static void LayerMiniLabel(ref Rect rect) {
            Styles.miniLabelStyle.fontSize = Preferences.SmallerMiniLabel ? 8 : 9;
            rect.xMin -= Styles.miniLabelStyle.CalcSize(new GUIContent(LayerMask.LayerToName(currentGameObject.layer))).x;
            var layer = EditorGUI.LayerField(rect, currentGameObject.layer, Styles.miniLabelStyle);

            if(GUI.changed)
                Utility.ChangeLayerAndAskForChildren(GetSelectedObjectsAndCurrent(), layer);
        }

        private static void DrawMiniLabel(ref Rect rect) {
            using(new ProfilerSample("Mini label")) {
                rect.x -= rect.height + 4f;
                rect.xMin += 15f;

                GUI.contentColor = currentColor;
                GUI.changed = false;

                switch(Preferences.LabelType.Value) {
                    case MiniLabelType.Tag:
                        if(hasTag)
                            TagMiniLabel(ref rect);
                        break;

                    case MiniLabelType.Layer:
                        if(hasLayer)
                            LayerMiniLabel(ref rect);
                        break;

                    case MiniLabelType.LayerOrTag:
                        if(hasLayer)
                            LayerMiniLabel(ref rect);
                        else if(hasTag)
                            TagMiniLabel(ref rect);
                        break;

                    case MiniLabelType.TagOrLayer:
                        if(hasTag)
                            TagMiniLabel(ref rect);
                        else if(hasLayer)
                            LayerMiniLabel(ref rect);
                        break;
                }

                GUI.contentColor = Color.white;
            }
        }

        private static Vector2 CalcMiniLabelSize() {
            using(new ProfilerSample("Calculating mini label size"))
                switch(Preferences.LabelType.Value) {
                    case MiniLabelType.Tag:
                        if(hasTag)
                            return Styles.miniLabelStyle.CalcSize(new GUIContent(currentGameObject.tag));
                        break;

                    case MiniLabelType.Layer:
                        if(hasLayer)
                            return Styles.miniLabelStyle.CalcSize(new GUIContent(LayerMask.LayerToName(currentGameObject.layer)));
                        break;

                    case MiniLabelType.LayerOrTag:
                        if(hasLayer)
                            return Styles.miniLabelStyle.CalcSize(new GUIContent(LayerMask.LayerToName(currentGameObject.layer)));
                        else if(hasTag)
                            return Styles.miniLabelStyle.CalcSize(new GUIContent(currentGameObject.tag));
                        break;

                    case MiniLabelType.TagOrLayer:
                        if(hasTag)
                            return Styles.miniLabelStyle.CalcSize(new GUIContent(currentGameObject.tag));
                        else if(hasLayer)
                            return Styles.miniLabelStyle.CalcSize(new GUIContent(LayerMask.LayerToName(currentGameObject.layer)));
                        break;
                }

            return Vector2.zero;
        }

        private static void DrawTooltip(Rect rect) {
            using(new ProfilerSample("Tooltips")) {
                if(dragSelection != null)
                    return;

                rect.xMax = rect.xMin;
                rect.xMin = 0f;

                tempTooltipContent.tooltip = string.Format("{0}\nTag: {1}\nLayer: {2}", currentGameObject.name, currentGameObject.tag, LayerMask.LayerToName(currentGameObject.layer));
                EditorGUI.LabelField(rect, tempTooltipContent);
            }
        }

        private static void DoSelection(Rect rect) {
            using(new ProfilerSample("Enhanced selection")) {
                rect.xMin = 0f;

                if(isFirstVisible && Event.current.button == 1)
                    switch(Event.current.type) {
                        case EventType.MouseDrag:
                            if(dragSelection == null) {
                                dragSelection = new List<GameObject>();
                                selectionStart = Event.current.mousePosition;
                                selectionRect = new Rect();
                            }

                            selectionRect = new Rect() {
                                xMin = Mathf.Min(Event.current.mousePosition.x, selectionStart.x),
                                yMin = Mathf.Min(Event.current.mousePosition.y, selectionStart.y),
                                xMax = Mathf.Max(Event.current.mousePosition.x, selectionStart.x),
                                yMax = Mathf.Max(Event.current.mousePosition.y, selectionStart.y)
                            };

                            if(Event.current.control)
                                dragSelection.AddRange(Selection.gameObjects);

                            Selection.objects = dragSelection.ToArray();
                            Event.current.Use();
                            break;

                        case EventType.MouseUp:
                            if(dragSelection != null)
                                Event.current.Use();
                            dragSelection = null;
                            break;
                    }

                if(dragSelection != null && isGameObject)
                    if(dragSelection.Contains(currentGameObject) && !selectionRect.Overlaps(rect))
                        dragSelection.Remove(currentGameObject);
                    else if(!dragSelection.Contains(currentGameObject) && selectionRect.Overlaps(rect))
                        dragSelection.Add(currentGameObject);
            }
        }

        private static List<GameObject> GetSelectedObjectsAndCurrent() {
            if(!Preferences.ChangeAllSelected || Selection.gameObjects.Length <= 1)
                return new List<GameObject> { currentGameObject };

            var selection = new List<GameObject>(Selection.gameObjects);

            for(var i = 0; i < selection.Count; i++)
                if(EditorUtility.IsPersistent(selection[i]))
                    selection.RemoveAt(i);

            if(!selection.Contains(currentGameObject))
                selection.Add(currentGameObject);

            selection.Remove(null);
            return selection;
        }
    }
}