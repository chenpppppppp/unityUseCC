using UnityEditor;
using UnityEngine;

/// <summary>
/// 一键搭建《弹珠守卫者》完整场景
/// 使用：Unity 菜单栏 → Tools → 搭建弹珠守卫者场景
/// </summary>
public class SceneBuilder : EditorWindow
{
    [MenuItem("Tools/搭建弹珠守卫者场景")]
    static void BuildScene()
    {
        // === 清理旧场景物体 ===
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.scene.name != null && !obj.CompareTag("MainCamera"))
            {
                // 保留 Camera 和 Light
                if (obj.GetComponent<Camera>() != null) continue;
                if (obj.GetComponent<Light>() != null) continue;
                DestroyImmediate(obj);
            }
        }

        // === 物理材质 ===
        PhysicMaterial bouncyMat = new PhysicMaterial("BouncyMat");
        bouncyMat.bounciness = 1f;
        bouncyMat.bounceCombine = PhysicMaterialCombine.Maximum;
        bouncyMat.frictionCombine = PhysicMaterialCombine.Minimum;
        bouncyMat.dynamicFriction = 0f;
        bouncyMat.staticFriction = 0f;

        // === 创建材质 ===
        Material matGround = CreateMaterial("Mat_Ground", new Color(0.18f, 0.16f, 0.13f));
        Material matWall = CreateMaterial("Mat_Wall", new Color(0.45f, 0.32f, 0.18f));
        Material matPlayer = CreateMaterial("Mat_Player", new Color(0.2f, 0.75f, 0.25f));

        // === 1. 地面 ===
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(2f, 1f, 2f);
        ground.GetComponent<MeshRenderer>().material = matGround;

        // === 2. 玩家 ===
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        player.name = "Player";
        player.transform.position = new Vector3(0f, 0.1f, 0f);
        player.transform.localScale = new Vector3(0.6f, 0.15f, 0.6f);
        player.GetComponent<MeshRenderer>().material = matPlayer;
        player.GetComponent<SphereCollider>().material = bouncyMat;
        player.AddComponent<PlayerAim>();
        player.AddComponent<PlayerHealth>();

        // === 3. 围墙 ===
        CreateWall("Wall_North", new Vector3(0f, 0.5f, -10f), new Vector3(20f, 1f, 0.5f), matWall, bouncyMat);
        CreateWall("Wall_South", new Vector3(0f, 0.5f, 10f), new Vector3(20f, 1f, 0.5f), matWall, bouncyMat);
        CreateWall("Wall_East", new Vector3(10f, 0.5f, 0f), new Vector3(0.5f, 1f, 20f), matWall, bouncyMat);
        CreateWall("Wall_West", new Vector3(-10f, 0.5f, 0f), new Vector3(0.5f, 1f, 20f), matWall, bouncyMat);

        // === 4. 内部障碍墙 ===
        CreateWall("Wall_Obs_01", new Vector3(-4f, 0.5f, 0f), new Vector3(0.3f, 1f, 5f), matWall, bouncyMat);
        CreateWall("Wall_Obs_02", new Vector3(3f, 0.5f, -3f), new Vector3(5f, 1f, 0.3f), matWall, bouncyMat);
        CreateWall("Wall_Obs_03", new Vector3(5f, 0.5f, 3f), new Vector3(0.3f, 1f, 4f), matWall, bouncyMat);
        CreateWall("Wall_Obs_04", new Vector3(-3f, 0.5f, 5f), new Vector3(6f, 1f, 0.3f), matWall, bouncyMat);

        // === 5. 场景管理器 ===
        GameObject manager = new GameObject("GameManager");
        manager.AddComponent<GameManager>();
        manager.AddComponent<WaveManager>();
        manager.AddComponent<GameUI>();

        // === 5b. 主菜单 ===
        GameObject menu = new GameObject("MainMenu");
        menu.AddComponent<MainMenuUI>();

        // === 5c. 音效 + 粒子 ===
        GameObject fx = new GameObject("SoundManager");
        fx.AddComponent<SoundManager>();
        GameObject px = new GameObject("ParticleManager");
        px.AddComponent<ParticleManager>();

        // === 5d. 基础设施 ===
        GameObject eb = new GameObject("EventBus");
        eb.AddComponent<EventBus>();
        GameObject pool = new GameObject("PoolManager");
        pool.AddComponent<PoolManager>();
        GameObject xlua = new GameObject("XLuaManager");
        xlua.AddComponent<XLuaManager>();

        // === 5e. UGUI Canvas ===
        GameObject canvas = new GameObject("CanvasManager");
        canvas.AddComponent<CanvasManager>();

        // === 6. 摄像机 ===
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = new Vector3(0f, 15f, 0f);
            cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            cam.orthographic = true;
            cam.orthographicSize = 12f;
            cam.backgroundColor = new Color(0.08f, 0.08f, 0.1f);
        }

        // === 7. 灯光 ===
        Light[] lights = FindObjectsOfType<Light>();
        foreach (Light light in lights)
        {
            if (light.type == LightType.Directional)
            {
                light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                light.intensity = 1.2f;
            }
        }

        // === 8. 保存 ===
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("=== 弹珠守卫者场景搭建完成！===");
        Debug.Log($"地面: 20x20 | 墙壁: 8面 | 玩家: 中心 | 摄像机: 俯视角");
    }

    static void CreateWall(string name, Vector3 pos, Vector3 scale, Material mat, PhysicMaterial physMat)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.position = pos;
        wall.transform.localScale = scale;
        wall.GetComponent<MeshRenderer>().material = mat;
        wall.GetComponent<BoxCollider>().material = physMat;
        wall.AddComponent<WallMarker>();
    }

    static Material CreateMaterial(string name, Color color)
    {
        // 确认 Materials 文件夹存在
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");

        Material mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        AssetDatabase.CreateAsset(mat, $"Assets/Materials/{name}.mat");
        return mat;
    }
}
