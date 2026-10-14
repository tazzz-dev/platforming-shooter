#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class SetupSceneEnemies
{
    private static bool hasExecuted = false;

    static SetupSceneEnemies()
    {
        EditorApplication.update += OnEditorUpdate;
    }

    private static void OnEditorUpdate()
    {
        if (hasExecuted) return;
        hasExecuted = true;
        EditorApplication.update -= OnEditorUpdate;
        ExecuteSetup();
    }

    [MenuItem("Tools/Setup All Enemies In Scene")]
    public static void ExecuteSetup()
    {
        string scenePath = "Assets/Scenes/MainScene.unity";
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            Debug.LogError("[SetupSceneEnemies] Scene tidak valid: " + scenePath);
            return;
        }

        Debug.Log("[SetupSceneEnemies] Memulai konfigurasi musuh di scene: " + scene.name);

        // 1. Load Animation Sprites
        Sprite[] ninjaIdle = LoadSprites("Assets/Tilemap/Free/Main Characters/Ninja Frog/Idle (32x32).png");
        Sprite[] ninjaRun = LoadSprites("Assets/Tilemap/Free/Main Characters/Ninja Frog/Run (32x32).png");
        Sprite[] ninjaHit = LoadSprites("Assets/Tilemap/Free/Main Characters/Ninja Frog/Hit (32x32).png");

        Sprite[] pinkIdle = LoadSprites("Assets/Tilemap/Free/Main Characters/Pink Man/Idle (32x32).png");
        Sprite[] pinkRun = LoadSprites("Assets/Tilemap/Free/Main Characters/Pink Man/Run (32x32).png");
        Sprite[] pinkHit = LoadSprites("Assets/Tilemap/Free/Main Characters/Pink Man/Hit (32x32).png");

        Sprite[] virtualIdle = LoadSprites("Assets/Tilemap/Free/Main Characters/Virtual Guy/Idle (32x32).png");
        Sprite[] virtualRun = LoadSprites("Assets/Tilemap/Free/Main Characters/Virtual Guy/Run (32x32).png");
        Sprite[] virtualHit = LoadSprites("Assets/Tilemap/Free/Main Characters/Virtual Guy/Hit (32x32).png");

        // 2. Load Weapon Assets
        GameObject bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Asset/Character/Enemy/EnemyBullet.prefab");
        GameObject muzzleFlashPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Asset/Prefab/GunPrefab/MuzzleFlash.prefab");
        Sprite pistolSprite = LoadFirstSprite("Assets/Asset/Guns/pistol/pistol.png");
        Sprite uziSprite = LoadFirstSprite("Assets/Asset/Guns/uzi/uzi.png");
        PhysicsMaterial2D noFriction = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/Asset/Prefab/Prefab Character/NoFriction.physicsMaterial2D");

        // 3. Scan all SpriteRenderers in scene
        SpriteRenderer[] allRenderers = Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);

        // Hapus musuh duplikat yang bertumpuk di koordinat yang sama
        List<GameObject> enemiesToDelete = new List<GameObject>();
        List<Vector2> recordedPositions = new List<Vector2>();

        foreach (SpriteRenderer r in allRenderers)
        {
            if (r == null || r.sprite == null) continue;
            string p = AssetDatabase.GetAssetPath(r.sprite);
            if (string.IsNullOrEmpty(p)) continue;

            GameObject g = r.gameObject;
            if (g.CompareTag("Player") || g.name.Contains("Player") || g.name == "EnemyWeapon") continue;

            if (!p.Contains("Ninja Frog") && !p.Contains("Pink Man") && !p.Contains("Virtual Guy")) continue;

            Vector2 pos = g.transform.position;
            bool isDup = false;
            foreach (var rec in recordedPositions)
            {
                if (Vector2.Distance(rec, pos) < 0.15f)
                {
                    isDup = true;
                    break;
                }
            }

            if (isDup)
            {
                enemiesToDelete.Add(g);
            }
            else
            {
                recordedPositions.Add(pos);
            }
        }

        foreach (var dup in enemiesToDelete)
        {
            Debug.Log($"[SetupSceneEnemies] Menghapus musuh duplikat bertumpuk: {dup.name} di {dup.transform.position}");
            Undo.DestroyObjectImmediate(dup);
        }

        int count = 0;

        foreach (SpriteRenderer sr in allRenderers)
        {
            if (sr == null || sr.sprite == null) continue;

            string path = AssetDatabase.GetAssetPath(sr.sprite);
            if (string.IsNullOrEmpty(path)) continue;

            GameObject go = sr.gameObject;
            if (go.CompareTag("Player") || go.name.Contains("Player") || go.name == "EnemyWeapon") continue;

            int level = 0;
            if (path.Contains("Ninja Frog")) level = 1;
            else if (path.Contains("Pink Man")) level = 2;
            else if (path.Contains("Virtual Guy")) level = 3;

            if (level == 0) continue;

            count++;
            Undo.RecordObject(go, "Configure Enemy");

            // Tag, Layer, dan SortingOrder (15 agar SELALU di depan Tilemap)
            go.tag = "Enemy";
            go.layer = 0;
            sr.sortingOrder = 15;

            // Normalisasi scale jika sebelumnya diperbesar saat era PPU 100
            if (go.transform.localScale.y > 3.0f)
            {
                go.transform.localScale = new Vector3(go.transform.localScale.x * 0.32f, go.transform.localScale.y * 0.32f, 1f);
            }

            // Hapus BoxCollider2D lama jika ada
            BoxCollider2D oldBox = go.GetComponent<BoxCollider2D>();
            if (oldBox != null)
            {
                Undo.DestroyObjectImmediate(oldBox);
            }

            // CapsuleCollider2D (anti-snag pada sambungan tile)
            CapsuleCollider2D col = go.GetComponent<CapsuleCollider2D>();
            if (col == null) col = Undo.AddComponent<CapsuleCollider2D>(go);
            col.size = new Vector2(0.72f, 0.85f);
            col.offset = Vector2.zero;
            col.direction = CapsuleDirection2D.Vertical;
            if (noFriction != null) col.sharedMaterial = noFriction;

            // Rigidbody2D
            Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
            if (rb == null) rb = Undo.AddComponent<Rigidbody2D>(go);
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.gravityScale = 1f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            if (noFriction != null) rb.sharedMaterial = noFriction;

            // EnemyHealth
            EnemyHealth health = go.GetComponent<EnemyHealth>();
            if (health == null) health = go.AddComponent<EnemyHealth>();

            // EnemyAI
            EnemyAI ai = go.GetComponent<EnemyAI>();
            if (ai == null) ai = go.AddComponent<EnemyAI>();

            // EnemySpriteAnimator
            EnemySpriteAnimator anim = go.GetComponent<EnemySpriteAnimator>();
            if (anim == null) anim = go.AddComponent<EnemySpriteAnimator>();

            // Ground layer mask (Layer 6: Ground -> mask = 64)
            LayerMask groundMask = LayerMask.GetMask("Ground");
            if (groundMask.value == 0) groundMask = 1 << 6;

            SerializedObject aiSo = new SerializedObject(ai);
            aiSo.FindProperty("groundLayer").intValue = groundMask.value;
            aiSo.FindProperty("bulletPrefab").objectReferenceValue = bulletPrefab;
            aiSo.FindProperty("muzzleFlashPrefab").objectReferenceValue = muzzleFlashPrefab;
            aiSo.FindProperty("spriteRenderer").objectReferenceValue = sr;
            aiSo.ApplyModifiedProperties();

            if (level == 1)
            {
                go.name = "Enemy_Level1_NinjaFrog";
                health.SetMaxHealth(3);
                ai.Configure(EnemyAI.EnemyLevel.Level1_NinjaFrog, 3, 2.2f, 3.8f, 0f, 4f, 1);
                anim.SetupSprites(ninjaIdle, ninjaRun, ninjaHit);
            }
            else if (level == 2)
            {
                go.name = "Enemy_Level2_PinkMan";
                health.SetMaxHealth(5);
                ai.Configure(EnemyAI.EnemyLevel.Level2_PinkMan, 5, 1.6f, 3.8f, 8f, 5f, 1);
                anim.SetupSprites(pinkIdle, pinkRun, pinkHit);

                // Setup Pistol Weapon
                SetupWeapon(go, pistolSprite, bulletPrefab, muzzleFlashPrefab, 1.8f, 8f, false, 1, 0f);
            }
            else if (level == 3)
            {
                go.name = "Enemy_Level3_VirtualGuy";
                health.SetMaxHealth(8);
                ai.Configure(EnemyAI.EnemyLevel.Level3_VirtualGuy, 8, 2.2f, 4.0f, 10f, 4f, 1);
                anim.SetupSprites(virtualIdle, virtualRun, virtualHit);

                // Setup Burst Weapon (Uzi or Pistol)
                SetupWeapon(go, uziSprite != null ? uziSprite : pistolSprite, bulletPrefab, muzzleFlashPrefab, 1.4f, 10f, true, 3, 0.14f);
            }

            EditorUtility.SetDirty(go);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[SetupSceneEnemies] BERHASIL! Berhasil mengonfigurasi {count} musuh di scene {scene.name}!");
    }

    private static void SetupWeapon(GameObject enemyObj, Sprite weaponSprite, GameObject bulletPrefab, GameObject muzzlePrefab, float fireRate, float attackRange, bool burst, int burstCount, float burstInterval)
    {
        Transform weaponTransform = enemyObj.transform.Find("EnemyWeapon");
        GameObject weaponObj;
        if (weaponTransform == null)
        {
            weaponObj = new GameObject("EnemyWeapon");
            weaponObj.transform.SetParent(enemyObj.transform);
            weaponObj.transform.localPosition = new Vector3(0.5f, 0f, 0f);
            weaponObj.transform.localScale = new Vector3(0.58f, 0.58f, 0.58f);
        }
        else
        {
            weaponObj = weaponTransform.gameObject;
        }

        SpriteRenderer weaponRenderer = weaponObj.GetComponent<SpriteRenderer>();
        if (weaponRenderer == null) weaponRenderer = weaponObj.AddComponent<SpriteRenderer>();
        weaponRenderer.sprite = weaponSprite;
        weaponRenderer.sortingOrder = 16;

        Transform firePointTransform = weaponObj.transform.Find("Firepoint");
        GameObject firePointObj;
        if (firePointTransform == null)
        {
            firePointObj = new GameObject("Firepoint");
            firePointObj.transform.SetParent(weaponObj.transform);
            firePointObj.transform.localPosition = new Vector3(0.5f, 0f, 0f);
            firePointObj.transform.localRotation = Quaternion.identity;
        }
        else
        {
            firePointObj = firePointTransform.gameObject;
        }

        EnemyPistol pistol = weaponObj.GetComponent<EnemyPistol>();
        if (pistol == null) pistol = weaponObj.AddComponent<EnemyPistol>();

        pistol.ConfigureWeapon(enemyObj.transform, bulletPrefab, muzzlePrefab, fireRate, attackRange, burst, burstCount, burstInterval);

        SerializedObject so = new SerializedObject(pistol);
        so.FindProperty("firePoint").objectReferenceValue = firePointObj.transform;
        so.FindProperty("enemyBodyTransform").objectReferenceValue = enemyObj.transform;
        so.FindProperty("enemyBulletPrefab").objectReferenceValue = bulletPrefab;
        so.FindProperty("muzzleFlashPrefab").objectReferenceValue = muzzlePrefab;
        so.ApplyModifiedProperties();
    }

    private static Sprite[] LoadSprites(string path)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        List<Sprite> list = new List<Sprite>();
        foreach (Object a in assets)
        {
            if (a is Sprite s) list.Add(s);
        }
        return list.ToArray();
    }

    private static Sprite LoadFirstSprite(string path)
    {
        Sprite[] s = LoadSprites(path);
        return s.Length > 0 ? s[0] : null;
    }
}
#endif
