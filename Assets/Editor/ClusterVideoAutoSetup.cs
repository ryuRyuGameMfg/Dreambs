using UnityEngine;
using UnityEngine.Video;
using UnityEditor;
using System.Collections.Generic;

public class ClusterVideoAutoSetup : EditorWindow
{
    private VideoClip[] videoClips = new VideoClip[5];
    private float videoDuration = 30f;
    
    [MenuItem("Cluster/Auto Setup Video Loop")]
    static void Init()
    {
        ClusterVideoAutoSetup window = (ClusterVideoAutoSetup)EditorWindow.GetWindow(typeof(ClusterVideoAutoSetup));
        window.titleContent = new GUIContent("Video Loop Setup");
        window.Show();
    }
    
    void OnGUI()
    {
        GUILayout.Label("Cluster Video Loop Auto Setup", EditorStyles.boldLabel);
        
        videoDuration = EditorGUILayout.FloatField("Each Video Duration (seconds):", videoDuration);
        
        GUILayout.Space(10);
        GUILayout.Label("Video Clips (5 files required):");
        
        for (int i = 0; i < 5; i++)
        {
            videoClips[i] = (VideoClip)EditorGUILayout.ObjectField(
                $"Video Part {i + 1}:", 
                videoClips[i], 
                typeof(VideoClip), 
                false
            );
        }
        
        GUILayout.Space(20);
        
        if (GUILayout.Button("Create Video Loop System with Cluster Gimmicks", GUILayout.Height(30)))
        {
            CreateVideoSystemWithGimmicks();
        }
    }
    
    void CreateVideoSystemWithGimmicks()
    {
        // 親オブジェクトを作成
        GameObject videoSystemRoot = new GameObject("ClusterVideoLoopSystem");
        videoSystemRoot.transform.position = Vector3.zero;
        
        // 5つのVideoPlayerを作成
        List<GameObject> videoPlayers = new List<GameObject>();
        for (int i = 0; i < 5; i++)
        {
            videoPlayers.Add(CreateVideoPlayer(i, videoSystemRoot.transform));
        }
        
        // EditorUtilityで更新を通知
        EditorUtility.SetDirty(videoSystemRoot);
        
        // 各VideoPlayerにコンポーネントを追加
        for (int i = 0; i < videoPlayers.Count; i++)
        {
            AddClusterComponents(videoPlayers[i], i);
        }
        
        // 遅延実行で値を設定
        EditorApplication.delayCall += () =>
        {
            for (int i = 0; i < videoPlayers.Count; i++)
            {
                if (videoPlayers[i] != null)
                {
                    ConfigureComponentValues(videoPlayers[i], i);
                }
            }
            
            Debug.Log("<color=green><b>✓ Video Loop System configured successfully!</b></color>");
            EditorUtility.DisplayDialog("Complete!", 
                "Video Loop System has been created and configured!\n\n" +
                "All components and values have been set automatically.\n" +
                "Ready to test in Play Mode!", 
                "Great!");
        };
        
        Debug.Log("Creating Video Loop System...");
        Selection.activeGameObject = videoSystemRoot;
        EditorGUIUtility.PingObject(videoSystemRoot);
    }
    
    GameObject CreateVideoPlayer(int index, Transform parent)
    {
        // VideoPlayer GameObjectを作成
        GameObject videoPlayerObj = new GameObject($"VideoPlayer{index + 1}");
        videoPlayerObj.transform.SetParent(parent);
        videoPlayerObj.transform.localPosition = Vector3.zero;
        
        // スクリーンとなるQuadを作成
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.transform.SetParent(videoPlayerObj.transform);
        quad.transform.localPosition = Vector3.zero;
        quad.transform.localScale = new Vector3(16, 9, 1);
        quad.name = "Screen";
        
        // Video Playerコンポーネントを追加
        VideoPlayer videoPlayer = videoPlayerObj.AddComponent<VideoPlayer>();
        if (videoClips[index] != null)
        {
            videoPlayer.clip = videoClips[index];
        }
        videoPlayer.targetMaterialRenderer = quad.GetComponent<Renderer>();
        videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
        videoPlayer.isLooping = false;
        videoPlayer.playOnAwake = (index == 0);
        
        // 初期状態の設定（最初のビデオ以外は非表示）
        if (index != 0)
        {
            videoPlayerObj.SetActive(false);
        }
        
        return videoPlayerObj;
    }
    
    void AddClusterComponents(GameObject videoObj, int index)
    {
        // Cluster Itemコンポーネントを追加
        System.Type itemType = GetClusterType("ClusterVR.CreatorKit.Item.Implements.Item");
        if (itemType != null)
        {
            videoObj.AddComponent(itemType);
            Debug.Log($"Added Item to VideoPlayer{index + 1}");
        }
        
        // Item Timerを追加
        System.Type itemTimerType = GetClusterType("ClusterVR.CreatorKit.Operation.Implements.ItemTimer");
        if (itemTimerType != null)
        {
            videoObj.AddComponent(itemTimerType);
            Debug.Log($"Added ItemTimer to VideoPlayer{index + 1}");
        }
        
        // Set Game Object Active Gimmick（Hideギミックのみ追加、Showは後で追加）
        System.Type gimmickType = GetClusterType("ClusterVR.CreatorKit.Gimmick.Implements.SetGameObjectActiveGimmick");
        if (gimmickType != null)
        {
            videoObj.AddComponent(gimmickType);
            Debug.Log($"Added SetGameObjectActiveGimmick (Hide) to VideoPlayer{index + 1}");
        }
    }
    
    void ConfigureComponentValues(GameObject videoObj, int index)
    {
        int nextIndex = (index + 1) % 5;
        
        // ItemTimerの設定
        var itemTimerType = GetClusterType("ClusterVR.CreatorKit.Operation.Implements.ItemTimer");
        if (itemTimerType != null)
        {
            var itemTimer = videoObj.GetComponent(itemTimerType);
            if (itemTimer != null)
            {
                ConfigureItemTimer(itemTimer, index, nextIndex);
            }
        }
        
        // SetGameObjectActiveGimmickの設定
        var gimmickType = GetClusterType("ClusterVR.CreatorKit.Gimmick.Implements.SetGameObjectActiveGimmick");
        if (gimmickType != null)
        {
            // Hideギミック: 同一GameObject上の既存コンポーネントを使用
            var hideGimmick = videoObj.GetComponent(gimmickType);
            if (hideGimmick != null)
            {
                // target = This(0), active = false
                ConfigureGimmick(hideGimmick, $"HideVideo{index + 1}", false, 0);
            }

            // Showギミック: 子オブジェクトに追加してParentを対象に有効化
            GameObject showChild = null;
            var existingChild = videoObj.transform.Find("ShowGimmick");
            if (existingChild != null)
            {
                showChild = existingChild.gameObject;
            }
            else
            {
                showChild = new GameObject("ShowGimmick");
                showChild.transform.SetParent(videoObj.transform);
                showChild.transform.localPosition = Vector3.zero;
            }

            var showGimmick = showChild.GetComponent(gimmickType) ?? showChild.AddComponent(gimmickType);
            if (showGimmick != null)
            {
                // target = Parent(1), active = true
                ConfigureGimmick(showGimmick, $"ShowVideo{index + 1}", true, 1);
            }
        }
        
        Debug.Log($"Configured VideoPlayer{index + 1}");
    }
    
    void ConfigureItemTimer(Component itemTimer, int currentIndex, int nextIndex)
    {
        if (itemTimer == null) return;
        
        SerializedObject so = new SerializedObject(itemTimer);
        
        // key設定 (GimmickKey型)
        var keyProp = so.FindProperty("key");
        if (keyProp != null)
        {
            var targetProp = keyProp.FindPropertyRelative("target");
            if (targetProp != null) targetProp.intValue = 0; // GimmickTarget.Item
            
            var keyKeyProp = keyProp.FindPropertyRelative("key");
            if (keyKeyProp != null) keyKeyProp.stringValue = "StartTimer";
        }
        
        // delayTimeSeconds設定
        var delayProp = so.FindProperty("delayTimeSeconds");
        if (delayProp != null)
        {
            delayProp.floatValue = videoDuration;
            Debug.Log($"Set delay time to {videoDuration} seconds for VideoPlayer{currentIndex + 1}");
        }
        
        // triggers配列を設定 (ConstantTriggerParam型)
        var triggersProp = so.FindProperty("triggers");
        if (triggersProp != null && triggersProp.isArray)
        {
            triggersProp.ClearArray();
            triggersProp.arraySize = 2;
            
            // Trigger 0: 自分を非表示
            var trigger0 = triggersProp.GetArrayElementAtIndex(0);
            SetConstantTriggerParam(trigger0, $"HideVideo{currentIndex + 1}");
            
            // Trigger 1: 次を表示
            var trigger1 = triggersProp.GetArrayElementAtIndex(1);
            SetConstantTriggerParam(trigger1, $"ShowVideo{nextIndex + 1}");
            
            Debug.Log($"Set triggers for VideoPlayer{currentIndex + 1}: Hide self, Show VideoPlayer{nextIndex + 1}");
        }
        
        so.ApplyModifiedProperties();
    }
    
    void SetConstantTriggerParam(SerializedProperty trigger, string keyValue)
    {
        if (trigger == null) return;
        
        // ConstantTriggerParamの構造に基づいて設定
        // target (TriggerTarget.Item = 1)
        var targetProp = trigger.FindPropertyRelative("target");
        if (targetProp != null)
        {
            targetProp.enumValueIndex = 1; // TriggerTarget.Item
        }
        
        // key
        var keyProp = trigger.FindPropertyRelative("key");
        if (keyProp != null)
        {
            keyProp.stringValue = keyValue;
        }
        
        // type (ParameterType.Signal = 0)
        var typeProp = trigger.FindPropertyRelative("type");
        if (typeProp != null)
        {
            typeProp.enumValueIndex = 0; // ParameterType.Signal
        }
        
        // rawValue (Signalの場合は空文字列)
        var rawValueProp = trigger.FindPropertyRelative("rawValue");
        if (rawValueProp != null)
        {
            rawValueProp.stringValue = "";
        }
    }
    
    void ConfigureGimmick(Component gimmick, string keyValue, bool active, int targetEnumIndex)
    {
        if (gimmick == null) return;
        
        SerializedObject so = new SerializedObject(gimmick);
        
        // key (GimmickKey型)
        var keyProp = so.FindProperty("key");
        if (keyProp != null)
        {
            // key.target (GimmickTarget.Item = 0)
            var targetProp = keyProp.FindPropertyRelative("target");
            if (targetProp != null)
            {
                targetProp.enumValueIndex = 0; // GimmickTarget.Item
            }
            
            // key.key
            var keyKeyProp = keyProp.FindPropertyRelative("key");
            if (keyKeyProp != null)
            {
                keyKeyProp.stringValue = keyValue;
                Debug.Log($"Set gimmick key to: {keyValue}");
            }
        }
        
        // target (SetGameObjectActiveTarget)
        var targetGimmickProp = so.FindProperty("target");
        if (targetGimmickProp != null)
        {
            targetGimmickProp.enumValueIndex = targetEnumIndex;
        }
        
        // active
        var activeProp = so.FindProperty("active");
        if (activeProp != null)
        {
            activeProp.boolValue = active;
            Debug.Log($"Set active to: {active}");
        }
        
        so.ApplyModifiedProperties();
    }
    
    System.Type GetClusterType(string fullTypeName)
    {
        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = assembly.GetType(fullTypeName);
            if (type != null) return type;
        }
        return null;
    }
}