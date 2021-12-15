/*
Copyright (c) 2020 Omar Duarte
Unauthorized copying of this file, via any medium is strictly prohibited.
Writen by Omar Duarte, 2020.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace PluginMaster
{
    public class PlayModeSave : EditorWindow
    {
        #region CONTEXT
        private const string TOOL_NAME = "Play Mode Save";
        [MenuItem("CONTEXT/Component/Save Now", true, 1201)]
        private static bool ValidateSaveNowMenu(MenuCommand command)
            => !PrefabUtility.IsPartOfPrefabAsset(command.context) && Application.IsPlaying(command.context);
        [MenuItem("CONTEXT/Component/Save Now", false, 1201)]
        private static void SaveNowMenu(MenuCommand command)
            => Add(command.context as Component, SaveCommand.SAVE_NOW, false, true);

        [MenuItem("CONTEXT/Component/Save When Exiting Play Mode", true, 1202)]
        private static bool ValidateSaveOnExtiMenu(MenuCommand command) => ValidateSaveNowMenu(command);
        [MenuItem("CONTEXT/Component/Save When Exiting Play Mode", false, 1202)]
        private static void SaveOnExitMenu(MenuCommand command)
            => Add(command.context as Component, SaveCommand.SAVE_ON_EXITING_PLAY_MODE, false, true);

        [MenuItem("CONTEXT/Component/Always Save When Exiting Play Mode", true, 1203)]
        private static bool ValidateAlwaysSaveOnExitMenu(MenuCommand command)
           => (EditorSceneManager.IsPreviewScene((command.context as Component).gameObject.scene)
                || PrefabUtility.IsPartOfPrefabAsset(command.context)) ? false
            : !PMSData.Contains(new ComponentSaveDataKey(command.context as Component), out ComponentSaveDataKey foundKey);

        [MenuItem("CONTEXT/Component/Always Save When Exiting Play Mode", false, 1203)]
        private static void AlwaysSaveOnExitMenu(MenuCommand command)
            => Add(command.context as Component, SaveCommand.SAVE_ON_EXITING_PLAY_MODE, true, true);

        [MenuItem("CONTEXT/Component/Remove From Save List", true, 1204)]
        private static bool ValidateRemoveFromSaveList(MenuCommand command)
            => PrefabUtility.IsPartOfPrefabAsset(command.context) ? false
            : PMSData.Contains(new ComponentSaveDataKey(command.context as Component), out ComponentSaveDataKey foundKey);
        [MenuItem("CONTEXT/Component/Remove From Save List", false, 1204)]
        private static void RemoveFromSaveList(MenuCommand command)
        {
            var component = command.context as Component;
            var key = new ComponentSaveDataKey(component);
            PMSData.Remove(key);
            CompDataRemoveKey(key);
        }

        [MenuItem("CONTEXT/Component/Apply Play Mode Changes", true, 1210)]
        private static bool ValidateApplyMenu(MenuCommand command)
            => !Application.isPlaying && CompDataContainsKey(GetKey(command.context), out ComponentSaveDataKey foundKey);
        [MenuItem("CONTEXT/Component/Apply Play Mode Changes", false, 1210)]
        private static void ApplyMenu(MenuCommand command) => Apply(GetKey(command.context));

        [MenuItem("CONTEXT/Component/", false, 1300)]
        private static void Separator(MenuCommand command) { }

        [MenuItem("CONTEXT/ScriptableObject/Save Now", false, 1211)]
        private static void SaveScriptableObject(MenuCommand command)
        {
            AssetDatabase.Refresh();
            EditorUtility.SetDirty(command.context);
            AssetDatabase.SaveAssets();
        }
        #endregion

        #region WINDOW
        [MenuItem("Tools/Plugin Master/" + TOOL_NAME, false, int.MaxValue)]
        private static void ShowWindow() => GetWindow<PlayModeSave>(TOOL_NAME);

        private const string SAVE_ENTIRE_HIERARCHY_PREF = "PLAY_MODE_SAVE_saveEntireHierarchy";
        private const string AUTO_APPLY_PREF = "PLAY_MODE_SAVE_autoApply";
        private const string INCLUDE_CHILDREN_PREF = "PLAY_MODE_SAVE_includeChildren";
        private static bool _showAplyButtons = true;

        private static void LoadPrefs()
        {
            _saveEntireHierarchy = EditorPrefs.GetBool(SAVE_ENTIRE_HIERARCHY_PREF, false);
            _autoApply = EditorPrefs.GetBool(AUTO_APPLY_PREF, true);
            _includeChildren = EditorPrefs.GetBool(INCLUDE_CHILDREN_PREF, true);
        }

        private void OnEnable() => LoadPrefs();

        private void OnDisable()
        {
            EditorPrefs.SetBool(SAVE_ENTIRE_HIERARCHY_PREF, _saveEntireHierarchy);
            EditorPrefs.SetBool(AUTO_APPLY_PREF, _autoApply);
            EditorPrefs.SetBool(INCLUDE_CHILDREN_PREF, _includeChildren);
        }

        private void OnSelectionChange() => Repaint();

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        var label = "Auto-Apply All Changes When Exiting Play Mode";
                        _autoApply = EditorGUILayout.ToggleLeft(label, _autoApply);
                        if (check.changed) EditorPrefs.SetBool(AUTO_APPLY_PREF, _autoApply);
                    }
                    if (!_autoApply)
                    {
                        if (_compData.Count == 0 && !_showAplyButtons) EditorGUILayout.LabelField("Nothing to apply");
                        else if (_compData.Count > 0 && _showAplyButtons)
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                if (GUILayout.Button("Apply All Changes"))
                                {
                                    ApplyAll();
                                    _showAplyButtons = false;
                                }
                                GUILayout.FlexibleSpace();
                            }
                        }
                    }
                }
            }
            var selection = Selection.GetFiltered<GameObject>(SelectionMode.Editable
                | SelectionMode.ExcludePrefab | SelectionMode.TopLevel);
            if (selection.Length > 0)
            {
                void SaveSelection(SaveCommand cmd, bool always)
                {
                    foreach (var obj in selection)
                    {
                        var components = _includeChildren ? obj.GetComponentsInChildren<Component>()
                            : obj.GetComponents<Component>();
                        foreach (var comp in components) Add(comp, cmd, always, true);
                        if (cmd == SaveCommand.SAVE_ON_EXITING_PLAY_MODE)
                        {
                            var objKey = new ObjectDataKey(obj);
                            AddFullObjectData(objKey, always);
                            if (always) PMSData.AlwaysSaveFull(objKey);
                        }
                    }
                }

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Execute for all selected objects: ");
                        GUILayout.FlexibleSpace();
                        _includeChildren = EditorGUILayout.ToggleLeft("Include children",
                            _includeChildren, GUILayout.Width(109));
                    }
                    if (Application.isPlaying)
                    {
                        if (GUILayout.Button("Save all components now"))
                            SaveSelection(SaveCommand.SAVE_NOW, false);
                        if (GUILayout.Button("Save all components when exiting play mode"))
                            SaveSelection(SaveCommand.SAVE_ON_EXITING_PLAY_MODE, false);
                        else maxSize = minSize = new Vector2(376, 180);
                    }
                    else maxSize = minSize = new Vector2(376, _autoApply ? 162 : 182);
                    if (GUILayout.Button("Always save all components when exiting play mode"))
                        SaveSelection(SaveCommand.SAVE_ON_EXITING_PLAY_MODE, true);
                    if (GUILayout.Button("Remove all components from save list"))
                    {
                        foreach (var obj in selection)
                        {
                            var components = _includeChildren ? obj.GetComponentsInChildren<Component>()
                           : obj.GetComponents<Component>();
                            foreach (var comp in components)
                            {
                                var key = new ComponentSaveDataKey(comp);
                                PMSData.Remove(key);
                                CompDataRemoveKey(key);
                            }
                            var objKey = new ObjectDataKey(obj);
                            RemoveFullObjectData(objKey);
                            PMSData.RemoveFull(objKey);
                            if (_includeChildren)
                            {
                                for (int i = 0; i < obj.transform.childCount; ++i)
                                {
                                    var child = obj.transform.GetChild(i);
                                    objKey = new ObjectDataKey(child.gameObject);
                                    RemoveFullObjectData(objKey);
                                    PMSData.RemoveFull(objKey);
                                }
                            }
                        }
                    }
                }
            }
            else maxSize = minSize = new Vector2(376, _autoApply ? 100 : 120);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    _saveEntireHierarchy = EditorGUILayout.ToggleLeft("Save The Entire Hierarchy When Exiting Play Mode",
                        _saveEntireHierarchy);
                    if (check.changed) EditorPrefs.SetBool(SAVE_ENTIRE_HIERARCHY_PREF, _saveEntireHierarchy);
                    if (_saveEntireHierarchy)
                    {
                        EditorGUILayout.HelpBox("NOT RECOMMENDED. Enabling this option in large scenes " +
                            "can cause a long delay when saving and applying changes.", MessageType.Warning);
                    }
                }
            }
        }
        #endregion

        #region SAVE
        private static bool _autoApply = true;
        private static bool _saveEntireHierarchy = false;
        private static bool _includeChildren = true;
        private static List<string> _scenesToOpen = new List<string>();
        private static List<ComponentSaveDataKey> _componentsToBeDeleted = new List<ComponentSaveDataKey>();
        private static Dictionary<ObjectDataKey, SaveObject> _objData
            = new Dictionary<ObjectDataKey, SaveObject>();

        private static Dictionary<ComponentSaveDataKey, SaveDataValue> _compData
            = new Dictionary<ComponentSaveDataKey, SaveDataValue>();
        private static ComponentSaveDataKey GetKey(UnityEngine.Object comp)
            => new ComponentSaveDataKey(comp as Component);
        private static List<FullObjectData> _fullObjData = new List<FullObjectData>();
        private static List<ObjectDataKey> _objectsToBeDeleted = new List<ObjectDataKey>();

        private static bool _openingScenes = false;
        private enum SaveCommand { SAVE_NOW, SAVE_ON_EXITING_PLAY_MODE }

        [Serializable]
        private class PMSData
        {
            private const string FILE_NAME = "PMSData";
            private const string RELATIVE_PATH = "/PluginMaster/PlayModeSave/Editor/Resources/";
            [SerializeField] private string _rootDirectory = null;
            [SerializeField] private List<ComponentSaveDataKey> _alwaysSave = new List<ComponentSaveDataKey>();
            [SerializeField] private List<ObjectDataKey> _alwaysSaveFull = new List<ObjectDataKey>();

            public static ComponentSaveDataKey[] alwaysSaveArray => instance._alwaysSave.ToArray();
            public static ObjectDataKey[] alwaysSaveFullArray => instance._alwaysSaveFull.ToArray();
            public static void AlwaysSave(ComponentSaveDataKey key)
            {
                if (instance._alwaysSave.Contains(key)) return;
                instance._alwaysSave.Add(key);
                Save();
            }
            public static void AlwaysSaveFull(ObjectDataKey data)
            {
                if (instance._alwaysSaveFull.Contains(data)) return;
                instance._alwaysSaveFull.Add(data);
                Save();
            }
            public static bool saveAfterLoading { get; set; }
            public static void Save()
            {
                if (string.IsNullOrEmpty(instance._rootDirectory)) instance._rootDirectory = Application.dataPath;
                var fullDirectoryPath = instance._rootDirectory + RELATIVE_PATH;
                var fullFilePath = fullDirectoryPath + FILE_NAME + ".txt";
                if (!File.Exists(fullFilePath))
                {
                    var directories = Directory.GetDirectories(Application.dataPath,
                        "PluginMaster", SearchOption.AllDirectories);
                    if (directories.Length == 0) Directory.CreateDirectory(fullDirectoryPath);
                    else
                    {
                        instance._rootDirectory = Directory.GetParent(directories[0]).FullName;
                        fullDirectoryPath = instance._rootDirectory + RELATIVE_PATH;
                        fullFilePath = fullDirectoryPath + FILE_NAME + ".txt";
                    }
                    if (!Directory.Exists(fullDirectoryPath)) Directory.CreateDirectory(fullDirectoryPath);
                }
                var jsonString = JsonUtility.ToJson(instance);
                File.WriteAllText(fullFilePath, jsonString);
                AssetDatabase.Refresh();
            }
            public static void Remove(ComponentSaveDataKey item)
            {
                if (Contains(item, out ComponentSaveDataKey foundKey))
                {
                    instance._alwaysSave.Remove(foundKey);
                    Save();
                }
            }

            public static void RemoveFull(ObjectDataKey item)
            {
                if (ContainsFull(item, out ObjectDataKey foundKey))
                {
                    instance._alwaysSaveFull.Remove(foundKey);
                    Save();
                }
            }
            public static bool Load()
            {
                var jsonTextAsset = Resources.Load<TextAsset>(FILE_NAME);
                if (jsonTextAsset == null) return false;
                _instance = JsonUtility.FromJson<PMSData>(jsonTextAsset.text);
                _loaded = true;
                if (saveAfterLoading) Save();
                return true;
            }

            public static bool Contains(ComponentSaveDataKey key, out ComponentSaveDataKey foundKey)
            {
                foundKey = null;
                if (!_loaded) Load();
                foreach (var compKey in instance._alwaysSave)
                {
                    if ((compKey.objId == key.objId || compKey.globalObjId == key.globalObjId)
                        && (compKey.compId == key.compId || compKey.globalCompId == key.globalCompId))
                    {
                        foundKey = compKey;
                        return true;
                    }
                }
                return false;
            }

            public static bool ContainsFull(ObjectDataKey key, out ObjectDataKey foundKey)
            {
                foundKey = null;
                if (!_loaded) Load();
                foreach (var objKey in instance._alwaysSaveFull)
                {
                    if (objKey.objId == key.objId || objKey.globalObjId == key.globalObjId)
                    {
                        foundKey = objKey;
                        return true;
                    }
                }
                return false;

            }
            private static PMSData _instance = null;
            private static bool _loaded = false;
            private PMSData() { }
            private static PMSData instance
            {
                get
                {
                    if (_instance == null) _instance = new PMSData();
                    return _instance;
                }
            }
        }

        [Serializable]
        private class ComponentSaveDataKey : ISerializationCallbackReceiver, IEquatable<ComponentSaveDataKey>
        {
            [SerializeField] private int _objId;
            [SerializeField] private int _compId;
            [SerializeField] private string _globalObjId;
            [SerializeField] private string _globalCompId;
            [SerializeField] private string _scenePath;

            public int objId { get => _objId; set => _objId = value; }
            public int compId { get => _compId; set => _compId = value; }
            public string globalObjId { get => _globalObjId; set => _globalObjId = value; }
            public string globalCompId { get => _globalCompId; set => _globalCompId = value; }
            public string scenePath => _scenePath;

            public ComponentSaveDataKey(Component component)
            {
                _objId = component.gameObject.GetInstanceID();
                _compId = component.GetInstanceID();
                _globalObjId = GlobalObjectId.GetGlobalObjectIdSlow(component.gameObject).ToString();
                _globalCompId = GlobalObjectId.GetGlobalObjectIdSlow(component).ToString();
                _scenePath = component.gameObject.scene.path;
            }

            public void OnBeforeSerialize() { }

            public void OnAfterDeserialize()
            {
                if (GlobalObjectId.TryParse(_globalObjId, out GlobalObjectId id))
                {
                    var obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) as GameObject;
                    if (obj == null) return;
                    _objId = obj.GetInstanceID();
                }
                if (GlobalObjectId.TryParse(_globalCompId, out GlobalObjectId compId))
                {
                    var comp = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(compId) as Component;
                    if (comp == null) return;
                    _compId = comp.GetInstanceID();
                }
                PMSData.saveAfterLoading = true;
            }

            public ObjectDataKey objectKey => new ObjectDataKey(_objId, _globalObjId, _scenePath);

            public override int GetHashCode() => (_globalObjId, _globalCompId).GetHashCode();
            public bool Equals(ComponentSaveDataKey other) => (_globalObjId == other._globalObjId || _objId == other._objId)
                && (_globalCompId == other._globalCompId || _compId == other._compId);
            public override bool Equals(object obj) => obj is ComponentSaveDataKey other && this.Equals(other);
            public static bool operator ==(ComponentSaveDataKey lhs, ComponentSaveDataKey rhs) => lhs.Equals(rhs);
            public static bool operator !=(ComponentSaveDataKey lhs, ComponentSaveDataKey rhs) => !lhs.Equals(rhs);
        }

        private class SaveDataValue
        {
            public SerializedObject serializedObj;
            public SaveCommand saveCmd;
            public Type componentType;
            public int compId;
            public string globalCompId;

            public SaveDataValue(SerializedObject serializedObj, SaveCommand saveCmd, Component component)
            {
                this.serializedObj = serializedObj;
                this.saveCmd = saveCmd;
                this.componentType = component.GetType();
                compId = component.GetInstanceID();
                globalCompId = GlobalObjectId.GetGlobalObjectIdSlow(component).ToString();
            }
            public virtual void Update(int componentId)
            {
                if (serializedObj.targetObject == null) return;
                serializedObj.Update();
            }
        }

        private class SpriteRendererSaveDataValue : SaveDataValue
        {
            public int sortingOrder;
            public int sortingLayerID;
            public SpriteRendererSaveDataValue(SerializedObject serializedObj, SaveCommand saveCmd, Component component,
                int sortingOrder, int sortingLayerID) : base(serializedObj, saveCmd, component)
                => (this.sortingOrder, this.sortingLayerID) = (sortingOrder, sortingLayerID);

            public override void Update(int componentId)
            {
                base.Update(componentId);
                var spriteRenderer = EditorUtility.InstanceIDToObject(componentId) as SpriteRenderer;
                sortingOrder = spriteRenderer.sortingOrder;
                sortingLayerID = spriteRenderer.sortingLayerID;
            }
        }

        private class ClothSaveDataValue : SaveDataValue
        {
            public ClothSkinningCoefficient[] coefficients;
            public ClothSaveDataValue(SerializedObject serializedObj, SaveCommand saveCmd, Component component,
                ClothSkinningCoefficient[] coefficients) : base(serializedObj, saveCmd, component)
                => (this.coefficients) = (coefficients);

            public override void Update(int componentId)
            {
                base.Update(componentId);
                var cloth = EditorUtility.InstanceIDToObject(componentId) as Cloth;
                coefficients = cloth.coefficients.ToArray();
            }
        }

        [Serializable]
        private class ObjectDataKey : IEquatable<ObjectDataKey>
        {
            [SerializeField] private int _objId;
            [SerializeField] private string _globalObjId;
            [SerializeField] private string _scenePath;
            public int objId { get => _objId; set => value = _objId; }
            public string globalObjId { get => _globalObjId; set => _globalObjId = value; }
            public string scenePath => _scenePath;
            public ObjectDataKey(GameObject gameObject)
            {
                if (gameObject == null)
                {
                    _objId = -1;
                    _globalObjId = null;
                    _scenePath = null;
                    return;
                }
                _objId = gameObject.GetInstanceID();
                _globalObjId = GlobalObjectId.GetGlobalObjectIdSlow(gameObject).ToString();
                _scenePath = gameObject.scene.path;
            }

            public ObjectDataKey(int objId, string globalObjId, string scenePath)
            {
                _objId = objId;
                _globalObjId = globalObjId;
                _scenePath = scenePath;
            }

            public override int GetHashCode() => _globalObjId.GetHashCode();
            public bool Equals(ObjectDataKey other) => _globalObjId == other._globalObjId || _objId == other._objId;
            public override bool Equals(object obj) => obj is ObjectDataKey other && this.Equals(other);
            public static bool operator ==(ObjectDataKey lhs, ObjectDataKey rhs) => lhs.Equals(rhs);
            public static bool operator !=(ObjectDataKey lhs, ObjectDataKey rhs) => !lhs.Equals(rhs);

            public void Copy(ObjectDataKey other)
            {
                _objId = other._objId;
                _globalObjId = other._globalObjId;
                _scenePath = other._scenePath;
            }
        }

        private class SaveObject
        {
            public SaveCommand saveCmd;
            public string name = null;
            public string tag = null;
            public int layer = 0;
            public bool isStatic = false;
            public bool always = false;
            public List<SaveObject> saveChildren = null;
            public Dictionary<Type, List<SaveDataValue>> compDataDictionary = new Dictionary<Type, List<SaveDataValue>>();
            public bool isRoot = true;
            public ObjectDataKey parentKey;
            public string prefabPath = null;
            public int siblingIndex = -1;
            public SaveObject(Transform transform, SaveCommand cmd, bool always)
            {
                this.always = always;
                var obj = transform.gameObject;
                var prefabRoot = PrefabUtility.GetNearestPrefabInstanceRoot(obj);
                if (prefabRoot == obj)
                    prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj);
                name = obj.name;
                tag = obj.tag;
                layer = obj.layer;
                isStatic = obj.isStatic;
                isRoot = transform.parent == null;
                var parent = isRoot ? null : transform.parent.gameObject;
                parentKey = new ObjectDataKey(parent);
                siblingIndex = obj.transform.GetSiblingIndex();
                saveCmd = cmd;
                var components = obj.GetComponents<Component>();
                foreach (var comp in components)
                {
                    var data = new SerializedObject(comp);
                    var type = comp.GetType();
                    if (!compDataDictionary.ContainsKey(type)) compDataDictionary.Add(type, new List<SaveDataValue>());
                    if (comp is SpriteRenderer)
                    {
                        var renderer = comp as SpriteRenderer;
                        compDataDictionary[type].Add(
                            new SpriteRendererSaveDataValue(data, saveCmd, renderer, renderer.sortingOrder, renderer.sortingLayerID));
                    }
                    if (comp is Cloth)
                        compDataDictionary[type].Add(new ClothSaveDataValue(data, saveCmd, comp, (comp as Cloth).coefficients));
                    else compDataDictionary[type].Add(new SaveDataValue(data, saveCmd, comp));
                    var prop = new SerializedObject(comp).GetIterator();
                    while (prop.NextVisible(true)) data.CopyFromSerializedProperty(prop);
                }
                var childCount = obj.transform.childCount;
                if (childCount == 0) return;
                var children = new List<Transform>();
                for (int i = 0; i < childCount; ++i) children.Add(obj.transform.GetChild(i));
                saveChildren = new List<SaveObject>();
                foreach (var child in children)
                {
                    if (child == transform) continue;
                    var saveChild = new SaveObject(child, cmd, always);
                    saveChildren.Add(saveChild);
                }
            }

            public Type[] types => compDataDictionary.Keys.ToArray();

            public void Update()
            {
                void UpdateComponents(SaveObject saveObj)
                {
                    foreach (var dataList in saveObj.compDataDictionary.Values)
                        foreach (var data in dataList)
                        {
                            if (data.serializedObj.targetObject == null) continue;
                            data.serializedObj.Update();
                        }
                    if (saveObj.saveChildren == null) return;
                    foreach (var child in saveObj.saveChildren) UpdateComponents(child);
                }
                UpdateComponents(this);
            }
        }

        private class FullObjectData
        {
            public ObjectDataKey key = null;
            public bool always = false;
            public FullObjectData(ObjectDataKey key, bool always) => (this.key, this.always) = (key, always);
        }

        private static bool CompDataContainsKey(ComponentSaveDataKey key, out ComponentSaveDataKey foundKey)
        {
            foundKey = null;
            foreach (var compKey in _compData.Keys)
            {
                if ((compKey.objId == key.objId || compKey.globalObjId == key.globalObjId)
                    && (compKey.compId == key.compId || compKey.globalCompId == key.globalCompId))
                {
                    foundKey = compKey;
                    return true;
                }
            }
            return false;
        }

        private static void CompDataRemoveKey(ComponentSaveDataKey key)
        {
            if (CompDataContainsKey(key, out ComponentSaveDataKey foundKey))
            {
                var dataClone = _compData.ToDictionary(entry => entry.Key, entry => entry.Value);
                _compData.Clear();
                foreach (var compKey in dataClone.Keys)
                {
                    if (compKey == key) continue;
                    _compData.Add(compKey, dataClone[compKey]);
                }
            }
        }

        private static void UpdateCompKey(ComponentSaveDataKey oldKey, ComponentSaveDataKey newKey)
        {
            oldKey.objId = newKey.objId;
            oldKey.globalObjId = newKey.globalObjId;
            oldKey.compId = newKey.compId;
            oldKey.globalCompId = newKey.globalCompId;
        }

        private static bool ObjectDataContainsKey(ObjectDataKey key, out ObjectDataKey foundKey)
        {
            foundKey = null;
            foreach (var objKey in _objData.Keys)
            {
                if (objKey.objId == key.objId || objKey.globalObjId == key.globalObjId)
                {
                    foundKey = objKey;
                    return true;
                }
            }
            return false;
        }

        private static void UpdateObjectDataKey(ObjectDataKey oldKey, ObjectDataKey newKey)
        {
            oldKey.objId = newKey.objId;
            oldKey.globalObjId = newKey.globalObjId;
        }

        private static bool FullObjectDataContains(ObjectDataKey objKey, out FullObjectData foundItem)
        {
            foundItem = null;
            foreach (var item in _fullObjData)
            {
                if (objKey == item.key)
                {
                    foundItem = item;
                    return true;
                }
            }
            return false;
        }

        private static void AddFullObjectData(ObjectDataKey objKey, bool always)
        {
            if (FullObjectDataContains(objKey, out FullObjectData foundItem))
            {
                foundItem.key.Copy(objKey);
                foundItem.always = always;
                return;
            }
            _fullObjData.Add(new FullObjectData(objKey, always));
        }

        private static void RemoveFullObjectData(ObjectDataKey objKey)
        {
            if (FullObjectDataContains(objKey, out FullObjectData foundItem)) _fullObjData.Remove(foundItem);
        }

        private static bool WillBeDeleted(ObjectDataKey objKey, out ObjectDataKey foundKey)
        {
            foundKey = null;
            foreach (var toBeDeleted in _objectsToBeDeleted)
            {
                if (toBeDeleted == objKey)
                {
                    foundKey = toBeDeleted;
                    return true;
                }
            }
            return false;
        }

        private static void ToBeDeleted(ObjectDataKey objKey)
        {
            if (WillBeDeleted(objKey, out ObjectDataKey foundKey))
            {
                foundKey.Copy(objKey);
            }
            else _objectsToBeDeleted.Add(objKey);
        }
        private static void Add(Component component, SaveCommand cmd, bool always, bool serialize)
        {
            if (IsCombinedOrInstance(component)) return;
            var scenePath = component.gameObject.scene.path;
            if (!_scenesToOpen.Contains(scenePath)) _scenesToOpen.Add(scenePath);
            var key = new ComponentSaveDataKey(component);
            if (always) PMSData.AlwaysSave(key);
            var data = new SerializedObject(component);
            SaveDataValue GetValue()
            {
                if (!serialize) return new SaveDataValue(null, cmd, component);
                if (component is SpriteRenderer)
                {
                    var spriteRenderer = component as SpriteRenderer;
                    return new SpriteRendererSaveDataValue(data, cmd, component, spriteRenderer.sortingOrder,
                        spriteRenderer.sortingLayerID);
                }
                else if (component is Cloth)
                {
                    var cloth = component as Cloth;
                    return new ClothSaveDataValue(data, cmd, component, cloth.coefficients.ToArray());
                }
                else return new SaveDataValue(data, cmd, component);
            }
            if (CompDataContainsKey(key, out ComponentSaveDataKey foundKey))
            {
                _compData[foundKey] = GetValue();
                UpdateCompKey(foundKey, key);
            }
            else _compData.Add(key, GetValue());
            var prop = new SerializedObject(component).GetIterator();
            while (prop.NextVisible(true)) data.CopyFromSerializedProperty(prop);

            var objKey = new ObjectDataKey(component.gameObject);
            var saveObj = new SaveObject(component.transform, cmd, always);
            if (ObjectDataContainsKey(objKey, out ObjectDataKey foundObjKey))
            {
                UpdateObjectDataKey(foundObjKey, objKey);
                _objData[objKey] = saveObj;
            }
            else _objData.Add(objKey, saveObj);
            EditorApplication.RepaintHierarchyWindow();
        }

        private static bool IsCombinedOrInstance(Component comp)
        {
            if (comp is MeshFilter)
            {
                var meshFilter = comp as MeshFilter;
                if (meshFilter.sharedMesh != null)
                    if (meshFilter.sharedMesh.name.Contains("Combined Mesh (root: scene)")
                        || meshFilter.sharedMesh.name.ToLower().Contains("instance")) return true;
            }
            else if (comp is Renderer)
            {
                var renderer = comp as Renderer;
                var materials = renderer.sharedMaterials;
                if (materials != null)
                    foreach (var mat in materials)
                        if (mat.name.ToLower().Contains("instance")) return true;
            }
            return false;
        }

        private static void AddAll()
        {
            var components = GameObject.FindObjectsOfType<Component>();
            foreach (var comp in components) Add(comp, SaveCommand.SAVE_ON_EXITING_PLAY_MODE, false, true);
        }

        private static void Apply(ComponentSaveDataKey key)
        {
            if (CompDataContainsKey(key, out ComponentSaveDataKey foundCompKey)) key = foundCompKey;
            {
                GameObject obj;
                var comp = FindComponent(key, out obj);
                if (WillBeDeleted(key.objectKey, out ObjectDataKey delFoundKey)) return;
                if (comp == null && obj != null && !_componentsToBeDeleted.Contains(key))
                {
                    comp = obj.AddComponent(_compData[key].componentType);
                    var objKey = key.objectKey;
                    if (PMSData.ContainsFull(objKey, out ObjectDataKey objFoundKey)) PMSData.AlwaysSave(key);
                }
                if (comp == null) return;
                var data = _compData[key].serializedObj;
                if (data == null) return;
                var serializedObj = new SerializedObject(comp);

                var prop = data.GetIterator();
                while (prop.NextVisible(true)) serializedObj.CopyFromSerializedProperty(prop);
                serializedObj.ApplyModifiedProperties();
                if (_compData[key] is SpriteRendererSaveDataValue)
                {
                    var spriteRendererData = _compData[key] as SpriteRendererSaveDataValue;
                    var spriteRenderer = EditorUtility.InstanceIDToObject(key.compId) as SpriteRenderer;
                    spriteRenderer.sortingOrder = spriteRendererData.sortingOrder;
                    spriteRenderer.sortingLayerID = spriteRendererData.sortingLayerID;
                }
                else if (_compData[key] is ClothSaveDataValue)
                {
                    var clothData = _compData[key] as ClothSaveDataValue;
                    var cloth = EditorUtility.InstanceIDToObject(key.compId) as Cloth;
                    cloth.coefficients = clothData.coefficients;
                }
            }
            if (!PMSData.Contains(key, out ComponentSaveDataKey foundKey)) CompDataRemoveKey(key);
        }

        private static void ApplyAll()
        {
            var comIds = _compData.Keys.ToArray();
            Delete();
            Create();
            foreach (var id in comIds) Apply(id);
            _objectsToBeDeleted.Clear();
            _componentsToBeDeleted.Clear();
            LoadData();
        }

        private static GameObject FindObject(ObjectDataKey key)
        {
            var obj = EditorUtility.InstanceIDToObject(key.objId) as GameObject;
            if (obj == null)
            {
                if (GlobalObjectId.TryParse(key.globalObjId, out GlobalObjectId id))
                {
                    obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) as GameObject;
                    if (obj != null) key.objId = obj.GetInstanceID();
                }
            }
            return obj;
        }

        private static Component FindComponent(ComponentSaveDataKey key, out GameObject obj)
        {
            obj = EditorUtility.InstanceIDToObject(key.objId) as GameObject;
            if (obj == null)
            {
                if (GlobalObjectId.TryParse(key.globalObjId, out GlobalObjectId id))
                {
                    obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) as GameObject;
                    if (obj != null) key.objId = obj.GetInstanceID();
                }
            }
            if (obj == null) return null;
            var comp = EditorUtility.InstanceIDToObject(key.compId) as Component;
            if (comp == null)
            {
                if (GlobalObjectId.TryParse(key.globalCompId, out GlobalObjectId id))
                {
                    comp = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) as Component;
                    if (comp != null) key.compId = comp.GetInstanceID();
                }
            }
            return comp;
        }

        private static void Create()
        {
            GameObject CreateObj(SaveObject saveObj, Transform parent, string scenePath)
            {
                var scene = EditorSceneManager.GetSceneByPath(scenePath);
                if (scene.IsValid()) EditorSceneManager.SetActiveScene(scene);
                else return null;
                GameObject obj = null;
                if (parent != null && saveObj.siblingIndex >= 0 && parent.childCount > saveObj.siblingIndex)
                {
                    var t = parent.GetChild(saveObj.siblingIndex);
                    if (t != null && t.name == saveObj.name) obj = t.gameObject;
                }
                if (obj == null) obj = saveObj.prefabPath != null
                        ? (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadMainAssetAtPath(saveObj.prefabPath))
                        : new GameObject(saveObj.name);

                Undo.RegisterCreatedObjectUndo(obj, "Save Object Created In Play Mode");
                obj.transform.parent = parent;
                obj.isStatic = saveObj.isStatic;
                obj.tag = saveObj.tag;
                obj.layer = saveObj.layer;
                foreach (var type in saveObj.compDataDictionary.Keys)
                {
                    var compDataList = saveObj.compDataDictionary[type];
                    var components = obj.GetComponents(type);
                    for (int i = 0; i < compDataList.Count; ++i)
                    {
                        var compData = compDataList[i];
                        var comp = obj.GetComponent(type);
                        if (comp == null || components.Length < i + 1) comp = obj.AddComponent(type);
                        if (components.Length > i) comp = components[i];
                        var serializedObj = new SerializedObject(comp);
                        var prop = compData.serializedObj.GetIterator();
                        while (prop.NextVisible(true)) serializedObj.CopyFromSerializedProperty(prop);
                        serializedObj.ApplyModifiedProperties();
                        if (compData is ClothSaveDataValue)
                        {
                            var clothData = compData as ClothSaveDataValue;
                            var cloth = comp as Cloth;
                            cloth.coefficients = clothData.coefficients.ToArray();
                        }
                    }
                }
                if (saveObj.saveChildren == null) return obj;
                foreach (var child in saveObj.saveChildren) CreateObj(child, obj.transform, scenePath);
                return obj;
            }
            var objDataClone = _objData.ToDictionary(entry => entry.Key, entry => entry.Value);
            foreach (var key in objDataClone.Keys)
            {
                if (WillBeDeleted(key, out ObjectDataKey TBDFoundKey)) continue;
                var root = FindObject(key);
                var data = objDataClone[key];
                if (root != null) continue;
                Transform rootParent = null;
                if (!data.isRoot)
                {
                    var rootParentObj = FindObject(data.parentKey);
                    if (rootParentObj == null) continue;
                    rootParent = rootParentObj.transform;
                }
                var obj = CreateObj(data, rootParent, key.scenePath);
                if (data.always)
                {
                    var components = _includeChildren ? obj.GetComponentsInChildren<Component>()
                        : obj.GetComponents<Component>();
                    foreach (var comp in components) Add(comp, SaveCommand.SAVE_ON_EXITING_PLAY_MODE, true, true);
                    var objKey = new ObjectDataKey(obj);
                    PMSData.AlwaysSaveFull(objKey);
                }
            }
            _objData.Clear();
        }

        private static void Delete()
        {
            foreach (var objItem in _objectsToBeDeleted)
            {
                var obj = FindObject(objItem);
                if (obj == null) return;
                Undo.DestroyObjectImmediate(obj);
            }
            foreach (var item in _componentsToBeDeleted)
            {
                GameObject obj = null;
                var comp = FindComponent(item, out obj);
                if (comp == null) continue;
                Undo.DestroyObjectImmediate(comp);
            }

        }

        private static void UpdateFullObjects()
        {
            PMSData.Load();
            var alwaysSaveFull = PMSData.alwaysSaveFullArray;
            foreach (var item in alwaysSaveFull)
            {
                var obj = FindObject(item);
                if (obj == null)
                {
                    PMSData.RemoveFull(item);
                    continue;
                }
                var objKey = new ObjectDataKey(obj);
                var components = _includeChildren ? obj.GetComponentsInChildren<Component>() : obj.GetComponents<Component>();
                foreach (var comp in components) Add(comp, SaveCommand.SAVE_ON_EXITING_PLAY_MODE, true, true);
                AddFullObjectData(objKey, true);
            }
        }

        private static void LoadData()
        {
            if (!PMSData.Load()) return;
            foreach (var key in PMSData.alwaysSaveArray)
            {
                GameObject obj = null;
                var comp = FindComponent(key, out obj);
                if (obj == null || comp == null)
                {
                    if (string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(key.scenePath)))
                        PMSData.Remove(key);
                    else
                    {
                        var activeScene = EditorSceneManager.GetActiveScene();
                        if (activeScene.path == key.scenePath) PMSData.Remove(key);
                    }
                    continue;
                }
                Add(comp, SaveCommand.SAVE_ON_EXITING_PLAY_MODE, true, Application.isPlaying);
            }
            EditorApplication.RepaintHierarchyWindow();
        }
        
        [InitializeOnLoad]
        private static class ApplicationEventHandler
        {
            private static GameObject autoApplyFlag = null;
            private const string AUTO_APPLY_OBJECT_NAME = "PlayModeSave_AutoApply";
            private static Texture2D _icon = Resources.Load<Texture2D>("Save");
            private static string _currentScenePath = null;
            private static bool _loadData = true;
            static ApplicationEventHandler()
            {
                EditorApplication.playModeStateChanged += OnStateChanged;
                EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemCallback;
                EditorApplication.hierarchyChanged += OnHierarchyChanged;
                LoadPrefs();
                EditorSceneManager.sceneOpened += OnSceneOpened;
            }

            static void OnHierarchyChanged()
            {
                var activeScene = EditorSceneManager.GetActiveScene();
                string activeScenePath = null;
                if (activeScene != null) activeScenePath = activeScene.path;
                if (!_loadData || _currentScenePath == activeScenePath) return;
                if (_currentScenePath != activeScenePath) _currentScenePath = activeScenePath;
                LoadData();
            }

            static private void OnSceneOpened(Scene scene, OpenSceneMode mode)
            {
                if (!_openingScenes)
                {
                    _scenesToOpen.Clear();
                    return;
                }
                _scenesToOpen.Remove(scene.path);
                if (_scenesToOpen.Count > 0) return;
                _openingScenes = false;
                if (_autoApply) PlayModeSave.ApplyAll();
            }



            private static void OnStateChanged(PlayModeStateChange state)
            {
                if (state == PlayModeStateChange.ExitingEditMode && _autoApply)
                {
                    autoApplyFlag = new GameObject(AUTO_APPLY_OBJECT_NAME);
                    autoApplyFlag.hideFlags = HideFlags.HideAndDontSave;
                    return;
                }
                if (state == PlayModeStateChange.ExitingPlayMode)
                {
                    if (_saveEntireHierarchy) AddAll();
                    foreach (var data in _compData)
                    {
                        if (data.Value.saveCmd == SaveCommand.SAVE_NOW) continue;
                        if (data.Value.serializedObj.targetObject == null) _componentsToBeDeleted.Add(data.Key);
                        else data.Value.Update(data.Key.compId);
                    }

                    var objDataClone = _objData.ToDictionary(entry => entry.Key, entry => entry.Value);

                    foreach (var key in objDataClone.Keys)
                    {
                        var data = objDataClone[key];
                        if (data.saveCmd == SaveCommand.SAVE_NOW) continue;

                        if (FullObjectDataContains(key, out FullObjectData foundItem))
                        {
                            var obj = FindObject(key);
                            if (obj == null)
                            {
                                var objKey = foundItem;
                                ToBeDeleted(key);
                                RemoveFullObjectData(key);
                                continue;
                            }
                            var components = _includeChildren ? obj.GetComponentsInChildren<Component>()
                             : obj.GetComponents<Component>();

                            foreach (var comp in components)
                                Add(comp, SaveCommand.SAVE_ON_EXITING_PLAY_MODE, data.always, true);
                            if (!data.always) RemoveFullObjectData(key);
                        }
                        data.Update();
                    }
                    return;
                }
                if (state == PlayModeStateChange.EnteredPlayMode)
                {
                    _scenesToOpen.Clear();
                    _componentsToBeDeleted.Clear();
                    UpdateFullObjects();
                    return;
                }
                if (state == PlayModeStateChange.EnteredEditMode)
                {
                    _showAplyButtons = true;
                    var scenesToOpen = _scenesToOpen.ToArray();
                    var openedSceneCount = EditorSceneManager.sceneCount;
                    var openedScenes = new List<string>();
                    for (int i = 0; i < openedSceneCount; ++i) openedScenes.Add(SceneManager.GetSceneAt(i).path);
                    bool applyAll = true;
                    _openingScenes = false;
                    foreach (var scenePath in scenesToOpen)
                    {
                        if (openedScenes.Contains(scenePath))
                        {
                            _scenesToOpen.Remove(scenePath);
                            continue;
                        }
                        applyAll = false;
                        _openingScenes = true;
                        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                    }
                    autoApplyFlag = GameObject.Find(AUTO_APPLY_OBJECT_NAME);
                    _autoApply = autoApplyFlag != null;
                    if (_autoApply)
                    {
                        DestroyImmediate(autoApplyFlag);
                        if (applyAll) PlayModeSave.ApplyAll();
                    }
                    _loadData = false;
                }
            }

            private static void HierarchyItemCallback(int instanceID, Rect selectionRect)
            {
                var data = _compData;
                var keys = _compData.Keys.Where(k => k.objId == instanceID).ToArray();
                if (keys.Length == 0) return;
                if (_icon == null) _icon = Resources.Load<Texture2D>("Save");
                var rect = new Rect(selectionRect.xMax - 10, selectionRect.y + 2, 11, 11);
                GUI.Box(rect, _icon, GUIStyle.none);
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0
                    && rect.Contains(Event.current.mousePosition))
                {
                    var obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
                    var compNames = obj.name + " components to save: ";
                    foreach (var key in keys)
                    {
                        if (key.objId != instanceID) continue;
                        var comp = EditorUtility.InstanceIDToObject(key.compId) as Component;
                        if (comp == null) continue;
                        compNames += comp.GetType().Name + ", ";
                    }
                    compNames = compNames.Remove(compNames.Length - 2);
                    Debug.Log(compNames);
                }
                EditorApplication.RepaintHierarchyWindow();
            }
        }
        #endregion
    }
}