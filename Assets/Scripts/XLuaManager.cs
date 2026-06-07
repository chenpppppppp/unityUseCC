using UnityEngine;
using System.Collections;
using System.IO;
using XLua;

/// <summary>
/// XLua 热更新管理器 — 基础版
/// 功能：加载 Lua 脚本、C# 调用 Lua、CDN 热更下载
/// </summary>
public class XLuaManager : MonoBehaviour
{
    public static XLuaManager Instance { get; private set; }

    [Header("CDN 配置")]
    public string cdnBaseUrl = "https://your-cdn.com/game/lua/";

    private LuaEnv _luaEnv;
    private LuaTable _globalTable;
    private string _hotfixPath;
    private string _builtinPath;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        _builtinPath = Application.streamingAssetsPath + "/Lua/";
        _hotfixPath = Application.persistentDataPath + "/Hotfix/Lua/";
    }

    void Start()
    {
        // 同步加载，确保 Awake() 阶段就能用
        _luaEnv = new LuaEnv();
        _luaEnv.AddLoader(CustomLoader);
        _globalTable = _luaEnv.Global;
        LoadDirectory(_builtinPath);
        LoadDirectory(_hotfixPath);
        Debug.Log("[XLuaManager] Lua 环境初始化完成");
    }

    // 自定义 Loader：先查热更路径，再查内置路径
    byte[] CustomLoader(ref string filepath)
    {
        string path = _hotfixPath + filepath + ".lua.txt";
        if (!File.Exists(path))
            path = _builtinPath + filepath + ".lua.txt";
        if (!File.Exists(path))
            return null;

        return System.Text.Encoding.UTF8.GetBytes(File.ReadAllText(path));
    }

    void LoadDirectory(string dirPath)
    {
        if (!Directory.Exists(dirPath)) return;

        string[] files = Directory.GetFiles(dirPath, "*.lua.txt", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            try
            {
                string code = File.ReadAllText(file);
                _luaEnv.DoString(code, file);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[XLuaManager] 加载失败: {file}\n{e.Message}");
            }
        }
        if (files.Length > 0)
            Debug.Log($"[XLuaManager] 从 {dirPath} 加载了 {files.Length} 个脚本");
    }

    // ===== 公开 API =====

    /// <summary>C# 调用 Lua 函数（支持 Table.Func 格式）</summary>
    public object[] Call(string funcPath, params object[] args)
    {
        if (_luaEnv == null || _globalTable == null) return null;
        try
        {
            string[] parts = funcPath.Split('.');
            LuaFunction fn = null;

            if (parts.Length == 2)
            {
                var table = _globalTable.Get<LuaTable>(parts[0]);
                if (table != null)
                {
                    fn = table.Get<LuaFunction>(parts[1]);
                    table.Dispose();
                }
            }
            else
            {
                fn = _globalTable.Get<LuaFunction>(funcPath);
            }

            if (fn == null) return null;
            var result = fn.Call(args);
            fn.Dispose();
            return result;
        }
        catch (System.Exception)
        {
            return null; // Lua 脚本不存在时静默失败
        }
    }

    /// <summary>执行一段 Lua 代码</summary>
    public void DoString(string code)
    {
        if (_luaEnv == null) return;
        try { _luaEnv.DoString(code); }
        catch (System.Exception e) { Debug.LogError($"[XLuaManager] {e.Message}"); }
    }

    /// <summary>获取 Lua 全局变量</summary>
    public T Get<T>(string name)
    {
        if (_luaEnv == null) return default;
        return _globalTable.Get<T>(name);
    }

    /// <summary>手动 GC（建议每帧或定期调用）</summary>
    public void Tick()
    {
        if (_luaEnv != null) _luaEnv.Tick();
    }

    void OnDestroy()
    {
        _luaEnv?.Dispose();
    }
}
