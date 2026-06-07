using UnityEngine;

/// <summary>
/// 粒子特效管理器 — 程序化生成所有视觉特效
/// </summary>
public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    ParticleSystem CreatePS(GameObject go, float lifetime)
    {
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        Destroy(go, lifetime);
        return ps;
    }

    // ========== 弹珠碰撞火花 ==========
    public void PlayMarbleBounce(Vector3 position)
    {
        GameObject go = new GameObject("FX_Bounce");
        go.transform.position = position;
        ParticleSystem ps = CreatePS(go, 1.5f);

        var main = ps.main;
        main.duration = 0.3f;
        main.startLifetime = 0.4f;
        main.startSpeed = 3f;
        main.startSize = 0.08f;
        main.startColor = new Color(1f, 0.7f, 0.2f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 8) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;

        ps.GetComponent<ParticleSystemRenderer>().material =
            new Material(Shader.Find("Sprites/Default"));

        ps.Play();
    }

    // ========== 敌人死亡爆炸 ==========
    public void PlayEnemyDeath(Vector3 position, Color enemyColor)
    {
        GameObject go = new GameObject("FX_Death");
        go.transform.position = position;
        ParticleSystem ps = CreatePS(go, 2f);

        var main = ps.main;
        main.duration = 0.5f;
        main.startLifetime = 0.6f;
        main.startSpeed = 5f;
        main.startSize = 0.12f;
        main.startColor = enemyColor;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 15) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.05f;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 1f), new Keyframe(0.5f, 0.5f), new Keyframe(1f, 0f)));

        ps.GetComponent<ParticleSystemRenderer>().material =
            new Material(Shader.Find("Sprites/Default"));

        ps.Play();
    }

    // ========== 升级闪光 ==========
    public void PlayLevelUp(Vector3 position)
    {
        GameObject go = new GameObject("FX_LevelUp");
        go.transform.position = position;
        ParticleSystem ps = CreatePS(go, 3f);

        var main = ps.main;
        main.duration = 1f;
        main.startLifetime = 1.5f;
        main.startSpeed = 2f;
        main.startSize = 0.2f;
        main.startColor = Color.yellow;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 30) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;

        ps.GetComponent<ParticleSystemRenderer>().material =
            new Material(Shader.Find("Sprites/Default"));

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.yellow, 0f),
                new GradientColorKey(Color.white, 0.5f),
                new GradientColorKey(new Color(1f, 0.8f, 0f, 0f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLifetime.color = grad;

        ps.Play();
    }

    // ========== Boss 大爆炸 ==========
    public void PlayBossDeath(Vector3 position)
    {
        for (int ring = 0; ring < 3; ring++)
        {
            float delay = ring * 0.2f;
            int count = 20 + ring * 10;
            float size = 0.15f + ring * 0.1f;
            float speed = 6f + ring * 3f;
            Color c = ring == 0 ? Color.red : ring == 1 ? Color.yellow : Color.white;

            GameObject go = new GameObject($"FX_BossDeath_{ring}");
            go.transform.position = position;
            ParticleSystem ps = CreatePS(go, 3f);

            var main = ps.main;
            main.duration = 0.3f;
            main.startLifetime = 0.8f - ring * 0.15f;
            main.startSpeed = speed;
            main.startSize = size;
            main.startColor = c;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(delay, (short)count) });

            ps.GetComponent<ParticleSystemRenderer>().material =
                new Material(Shader.Find("Sprites/Default"));

            ps.Play();
        }
    }

    // ========== 拾取小粒子 ==========
    public void PlayPickup(Vector3 position, Color color)
    {
        GameObject go = new GameObject("FX_Pickup");
        go.transform.position = position;
        ParticleSystem ps = CreatePS(go, 1f);

        var main = ps.main;
        main.duration = 0.2f;
        main.startLifetime = 0.3f;
        main.startSpeed = 1f;
        main.startSize = 0.05f;
        main.startColor = color;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 5) });

        ps.GetComponent<ParticleSystemRenderer>().material =
            new Material(Shader.Find("Sprites/Default"));

        ps.Play();
    }
}
