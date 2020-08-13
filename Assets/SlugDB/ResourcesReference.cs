using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable, InlineProperty]
public class ResourcesReference
{
    [ShowInInspector, OnValueChanged("BuildResourcesPath"), ShowIf("ShowObjectDrawer"), HideLabel]
    Object assetReference;

    [SerializeField, ReadOnly]
    string guid;

    [SerializeField, ReadOnly]
    string friendlyName = "";

    public AsyncOperationHandle<T> Load<T> () where T : UnityEngine.Object
    {
        AssetReference assetReference = new AssetReference(guid);
        return assetReference.LoadAssetAsync<T>();
    }


    public void Unload(Object @object)
    {
        Resources.UnloadAsset(@object);
    }

#if UNITY_EDITOR

    public ResourcesReference (Object asset)
    {
        assetReference = asset;
        BuildResourcesPath();
    }

    private bool Validate (string path)
    {
        return Resources.Load(path) != null;
    }

    [Button]
    void Ping ()
    {
        UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(guid));

        if (asset == null)
        {
            Clear();
        }
        else
        {
            EditorGUIUtility.PingObject(asset);
        }
    }

    [Button]
    void Clear ()
    {
        guid = "";
        friendlyName = "";
    }

    void BuildResourcesPath ()
    {
        string projectPath = AssetDatabase.GetAssetPath(assetReference);
        var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(projectPath);
        if (asset != null)
        {
            friendlyName = asset.name;
            guid = AssetDatabase.AssetPathToGUID(projectPath);
        }
        
        assetReference = null;
    }

    bool ShowObjectDrawer ()
    {
        return guid == "" || guid == null;
    }
#endif
}
