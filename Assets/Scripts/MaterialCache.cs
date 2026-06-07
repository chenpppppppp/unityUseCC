using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 材质缓存 — 按颜色缓存 Material，避免高频 new Material + Shader.Find
/// </summary>
public static class MaterialCache
{
    private static Shader _standardShader;
    private static Shader _spriteShader;
    private static PhysicMaterial _bouncyPhysMat;

    // 不透明材质缓存（按颜色哈希）
    private static Dictionary<int, Material> _opaque = new Dictionary<int, Material>();
    // 自发光材质缓存
    private static Dictionary<int, Material> _emissive = new Dictionary<int, Material>();
    // 拖尾材质（单例）
    private static Material _trailMat;

    // ===== 初始化 =====

    static MaterialCache()
    {
        _standardShader = Shader.Find("Standard");
        _spriteShader = Shader.Find("Sprites/Default");
        _bouncyPhysMat = new PhysicMaterial("PoolBouncy");
        _bouncyPhysMat.bounciness = 1f;
        _bouncyPhysMat.bounceCombine = PhysicMaterialCombine.Maximum;
        _bouncyPhysMat.frictionCombine = PhysicMaterialCombine.Minimum;
    }

    // ===== 公开 API =====

    public static Material GetOpaque(Color color)
    {
        int hash = ColorHash(color);
        if (!_opaque.TryGetValue(hash, out Material mat))
        {
            mat = new Material(_standardShader);
            mat.color = color;
            _opaque[hash] = mat;
        }
        return mat;
    }

    public static Material GetEmissive(Color color, float emissionMult = 0.5f)
    {
        int hash = ColorHash(color);
        if (!_emissive.TryGetValue(hash, out Material mat))
        {
            mat = new Material(_standardShader);
            mat.color = color;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * emissionMult);
            _emissive[hash] = mat;
        }
        return mat;
    }

    public static Material GetTrailMaterial()
    {
        if (_trailMat == null)
        {
            _trailMat = new Material(_spriteShader);
            _trailMat.hideFlags = HideFlags.HideAndDontSave;
        }
        return _trailMat;
    }

    public static PhysicMaterial GetBouncyPhysicMat()
    {
        return _bouncyPhysMat;
    }

    public static Shader StandardShader => _standardShader;
    public static Shader SpriteShader => _spriteShader;

    // ===== 工具 =====

    static int ColorHash(Color c)
    {
        return ((int)(c.r * 255) << 16) | ((int)(c.g * 255) << 8) | (int)(c.b * 255);
    }
}
