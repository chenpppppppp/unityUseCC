using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 玩家瞄准发射系统 + 升级属性
/// </summary>
public class PlayerAim : MonoBehaviour
{
    [Header("Launch Settings")]
    public float launchForce = 15f;
    public float cooldown = 1.5f;

    // ===== 升级属性 =====
    [HideInInspector] public int bonusBounces;
    [HideInInspector] public float marbleSizeMult = 1f;
    [HideInInspector] public float marbleSpeedMult = 1f;
    [HideInInspector] public float pickupRangeMult = 1f;
    [HideInInspector] public float explosionChance;
    [HideInInspector] public int bounceHealThreshold; // 0=未激活
    [HideInInspector] public bool doubleShot;
    [HideInInspector] public bool hasSlowMotion;
    [HideInInspector] public bool canRecall;
    [HideInInspector] public bool lightningMarble;     // 闪电弹珠
    [HideInInspector] public bool fireMarble;          // 火焰弹珠

    private LineRenderer aimLine;
    private bool isDragging;
    private Vector3 aimDirection;
    private float power;
    private float cooldownTimer;
    private GameObject debugMarker;
    private float slowMotionTimer;

    void Start()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = new Vector3(0f, 15f, 0f);
            cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            cam.orthographic = true;
            cam.orthographicSize = 12f;
        }

        aimLine = GetComponent<LineRenderer>();
        if (aimLine == null) aimLine = gameObject.AddComponent<LineRenderer>();
        aimLine.positionCount = 2;
        aimLine.startWidth = 0.06f;
        aimLine.endWidth = 0.03f;
        aimLine.material = new Material(Shader.Find("Sprites/Default"));
        aimLine.enabled = false;

        SetupBouncyWalls();
    }

    void SetupBouncyWalls()
    {
        PhysicMaterial bouncy = new PhysicMaterial("Bouncy");
        bouncy.bounciness = 1f;
        bouncy.bounceCombine = PhysicMaterialCombine.Maximum;
        bouncy.frictionCombine = PhysicMaterialCombine.Minimum;
        bouncy.dynamicFriction = 0f;
        bouncy.staticFriction = 0f;
        foreach (WallMarker w in FindObjectsOfType<WallMarker>())
        {
            Collider c = w.GetComponent<Collider>();
            if (c) c.material = bouncy;
        }
        Collider pc = GetComponent<Collider>();
        if (pc) pc.material = bouncy;
    }

    void Update()
    {
        cooldownTimer -= Time.deltaTime;

        // 时间缓滞
        if (hasSlowMotion)
        {
            slowMotionTimer -= Time.deltaTime;
            if (slowMotionTimer <= 0f)
            {
                StartCoroutine(SlowMotionRoutine());
                slowMotionTimer = 30f;
            }
        }

        if (cooldownTimer > 0f) return;

        // 弹珠召回
        if (canRecall && Input.GetKeyDown(KeyCode.R))
            RecallAllMarbles();

        HandleInput();
    }

    System.Collections.IEnumerator SlowMotionRoutine()
    {
        Time.timeScale = 0.3f;
        yield return new WaitForSecondsRealtime(3f);
        Time.timeScale = 1f;
    }

    void RecallAllMarbles()
    {
        if (PoolManager.Instance)
            PoolManager.Instance.DespawnAllMarbles();
        cooldownTimer = cooldown * 0.3f;
    }

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            aimLine.enabled = true;
        }

        if (Input.GetMouseButton(0) && isDragging)
            UpdateAimLine();

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            aimLine.enabled = false;
            if (power > 0.05f)
                LaunchMarble();
        }
    }

    void UpdateAimLine()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 100f)) return;

        Vector3 wc = hit.point;

        if (debugMarker == null)
        {
            debugMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            debugMarker.name = "DebugCursor";
            debugMarker.transform.localScale = Vector3.one * 0.3f;
            debugMarker.GetComponent<MeshRenderer>().material.color = Color.magenta;
            Destroy(debugMarker.GetComponent<SphereCollider>());
        }
        debugMarker.transform.position = wc;

        float dx = wc.x - transform.position.x;
        float dz = wc.z - transform.position.z;
        float dist = Mathf.Sqrt(dx * dx + dz * dz);
        if (dist < 0.1f) return;

        power = Mathf.Clamp01(dist / 8f);
        aimDirection = new Vector3(dx, 0f, dz).normalized;

        float len = 2f + power * 4f;
        aimLine.SetPosition(0, transform.position);
        aimLine.SetPosition(1, transform.position + aimDirection * len);

        Color c = Color.Lerp(Color.green, Color.red, power);
        aimLine.startColor = c;
        aimLine.endColor = c;
    }

    void LaunchMarble()
    {
        float speed = launchForce * power * marbleSpeedMult;

        if (doubleShot)
        {
            Vector3 right = Vector3.Cross(Vector3.up, aimDirection);
            CreateMarble(Quaternion.AngleAxis(-5f, Vector3.up) * aimDirection, speed);
            CreateMarble(Quaternion.AngleAxis(5f, Vector3.up) * aimDirection, speed);
        }
        else
        {
            CreateMarble(aimDirection, speed);
        }

        // 音效
        if (SoundManager.Instance) SoundManager.Instance.Play("marble_launch");

        cooldownTimer = cooldown;
    }

    void CreateMarble(Vector3 dir, float force)
    {
        if (PoolManager.Instance == null) return;

        Vector3 spawnPos = transform.position + dir * 0.5f;
        float size = 0.25f * marbleSizeMult;
        int bounces = 8 + bonusBounces;

        PoolManager.Instance.SpawnMarble(spawnPos, dir, force, bounces, bounceHealThreshold, size);
    }
}
