using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyRuntimeSetup : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void InitializeSceneEnemies()
    {
        Debug.Log("[EnemyRuntimeSetup] Memeriksa musuh di dalam scene...");

        // 1. Kumpulkan seluruh Sprite yang termuat di memori game
        Sprite[] allSprites = Resources.FindObjectsOfTypeAll<Sprite>();

        // Cache sprite animasi per karakter (diurutkan berdasarkan nama agar frame terurut)
        Sprite[] ninjaIdle = GetSortedSprites(allSprites, "Ninja Frog", "Idle");
        Sprite[] ninjaRun = GetSortedSprites(allSprites, "Ninja Frog", "Run");
        Sprite[] ninjaHit = GetSortedSprites(allSprites, "Ninja Frog", "Hit");

        Sprite[] pinkIdle = GetSortedSprites(allSprites, "Pink Man", "Idle");
        Sprite[] pinkRun = GetSortedSprites(allSprites, "Pink Man", "Run");
        Sprite[] pinkHit = GetSortedSprites(allSprites, "Pink Man", "Hit");

        Sprite[] virtualIdle = GetSortedSprites(allSprites, "Virtual Guy", "Idle");
        Sprite[] virtualRun = GetSortedSprites(allSprites, "Virtual Guy", "Run");
        Sprite[] virtualHit = GetSortedSprites(allSprites, "Virtual Guy", "Hit");

        // 2. Cari aset peluru & efek senjata musuh
        GameObject[] allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        GameObject bulletPrefab = allGameObjects.FirstOrDefault(g => g.name == "EnemyBullet" && !g.scene.IsValid());
        GameObject muzzleFlashPrefab = allGameObjects.FirstOrDefault(g => g.name == "MuzzleFlash" && !g.scene.IsValid());

        Sprite pistolSprite = allSprites.FirstOrDefault(s => s.name.StartsWith("pistol") || (s.texture != null && s.texture.name.ToLower().Contains("pistol")));
        Sprite uziSprite = allSprites.FirstOrDefault(s => s.name.StartsWith("uzi") || (s.texture != null && s.texture.name.ToLower().Contains("uzi")));

        // 3. Scan semua SpriteRenderer di scene
        SpriteRenderer[] sceneRenderers = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        int initializedCount = 0;

        List<Vector2> registeredPositions = new List<Vector2>();
        List<GameObject> duplicatesToDestroy = new List<GameObject>();

        foreach (SpriteRenderer sr in sceneRenderers)
        {
            if (sr == null || sr.sprite == null || sr.sprite.texture == null) continue;

            GameObject go = sr.gameObject;
            if (go.CompareTag("Player") || go.name.Contains("Player") || go.name == "EnemyWeapon") continue;

            string texName = sr.sprite.texture.name;
            int level = 0;

            if (texName.Contains("Ninja Frog") || sr.sprite.name.Contains("Ninja")) level = 1;
            else if (texName.Contains("Pink Man") || sr.sprite.name.Contains("Pink")) level = 2;
            else if (texName.Contains("Virtual Guy") || sr.sprite.name.Contains("Virtual")) level = 3;

            if (level == 0) continue;

            // Hapus musuh duplikat yang bertumpuk di koordinat yang sama persis
            Vector2 pos = go.transform.position;
            bool isDuplicate = false;
            foreach (var reg in registeredPositions)
            {
                if (Vector2.Distance(reg, pos) < 0.15f)
                {
                    isDuplicate = true;
                    break;
                }
            }
            if (isDuplicate)
            {
                duplicatesToDestroy.Add(go);
                continue;
            }
            registeredPositions.Add(pos);

            initializedCount++;

            // Tag, Layer, dan SortingOrder (15 agar SELALU di depan Tilemap yang bernilai 0-10)
            go.tag = "Enemy";
            go.layer = 0;
            sr.sortingOrder = 15;

            // Normalisasi scale jika sebelumnya diperbesar saat era PPU 100
            if (go.transform.localScale.y > 3.0f)
            {
                go.transform.localScale = new Vector3(go.transform.localScale.x * 0.32f, go.transform.localScale.y * 0.32f, 1f);
            }

            // Material physics tanpa gesekan agar tidak seret di dinding/lantai
            PhysicsMaterial2D noFriction = Resources.FindObjectsOfTypeAll<PhysicsMaterial2D>()
                .FirstOrDefault(m => m != null && m.name.ToLower().Contains("nofriction"));
            if (noFriction == null)
            {
                noFriction = new PhysicsMaterial2D("EnemyNoFriction") { friction = 0f, bounciness = 0f };
            }

            // Hapus BoxCollider2D lama jika ada (agar tidak nyangkut di celah tilemap)
            BoxCollider2D oldBox = go.GetComponent<BoxCollider2D>();
            if (oldBox != null)
            {
                if (Application.isPlaying) Destroy(oldBox);
                else DestroyImmediate(oldBox);
            }

            // CapsuleCollider2D (ujung bawah melengkung sehingga anti-snag di sambungan tile)
            CapsuleCollider2D col = go.GetComponent<CapsuleCollider2D>();
            if (col == null) col = go.AddComponent<CapsuleCollider2D>();
            col.size = new Vector2(0.72f, 0.85f);
            col.offset = Vector2.zero;
            col.direction = CapsuleDirection2D.Vertical;
            col.sharedMaterial = noFriction;

            // Rigidbody2D
            Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
            if (rb == null) rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.gravityScale = 1f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.sharedMaterial = noFriction;

            // EnemyHealth
            EnemyHealth health = go.GetComponent<EnemyHealth>();
            if (health == null) health = go.AddComponent<EnemyHealth>();

            // EnemyAI
            EnemyAI ai = go.GetComponent<EnemyAI>();
            if (ai == null) ai = go.AddComponent<EnemyAI>();

            // EnemySpriteAnimator
            EnemySpriteAnimator anim = go.GetComponent<EnemySpriteAnimator>();
            if (anim == null) anim = go.AddComponent<EnemySpriteAnimator>();

            // Konfigurasi spesifik tiap level
            if (level == 1)
            {
                go.name = "Enemy_Level1_NinjaFrog";
                health.SetMaxHealth(3);
                ai.Configure(EnemyAI.EnemyLevel.Level1_NinjaFrog, 3, 2.2f, 3.8f, 0f, 4f, 1);
                if (ninjaIdle != null && ninjaIdle.Length > 0) anim.SetupSprites(ninjaIdle, ninjaRun, ninjaHit);
            }
            else if (level == 2)
            {
                go.name = "Enemy_Level2_PinkMan";
                health.SetMaxHealth(5);
                ai.Configure(EnemyAI.EnemyLevel.Level2_PinkMan, 5, 1.6f, 3.8f, 8f, 5f, 1);
                if (pinkIdle != null && pinkIdle.Length > 0) anim.SetupSprites(pinkIdle, pinkRun, pinkHit);

                // Pasang Senjata Pistol
                SetupRuntimeWeapon(go, pistolSprite, bulletPrefab, muzzleFlashPrefab, 1.8f, 8f, false, 1, 0f);
            }
            else if (level == 3)
            {
                go.name = "Enemy_Level3_VirtualGuy";
                health.SetMaxHealth(8);
                ai.Configure(EnemyAI.EnemyLevel.Level3_VirtualGuy, 8, 2.2f, 4.0f, 10f, 4f, 1);
                if (virtualIdle != null && virtualIdle.Length > 0) anim.SetupSprites(virtualIdle, virtualRun, virtualHit);

                // Pasang Senjata Uzi / Burst
                SetupRuntimeWeapon(go, uziSprite != null ? uziSprite : pistolSprite, bulletPrefab, muzzleFlashPrefab, 1.4f, 10f, true, 3, 0.14f);
            }
        }

        foreach (var dup in duplicatesToDestroy)
        {
            if (dup != null)
            {
                if (Application.isPlaying) Destroy(dup);
                else DestroyImmediate(dup);
            }
        }

        Debug.Log($"[EnemyRuntimeSetup] Sukses! {initializedCount} musuh berhasil diaktifkan dengan AI, animasi, dan senjata!");
    }

    private static void SetupRuntimeWeapon(GameObject enemyObj, Sprite weaponSprite, GameObject bulletPrefab, GameObject muzzlePrefab, float fireRate, float attackRange, bool burst, int burstCount, float burstInterval)
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
        if (weaponSprite != null) weaponRenderer.sprite = weaponSprite;
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

        // Assign private field firePoint via reflection jika diperlukan
        var firePointField = typeof(EnemyPistol).GetField("firePoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (firePointField != null)
        {
            firePointField.SetValue(pistol, firePointObj.transform);
        }
    }

    private static Sprite[] GetSortedSprites(Sprite[] allSprites, string charKeyword, string stateKeyword)
    {
        var filtered = allSprites.Where(s => 
            s != null && 
            s.texture != null && 
            s.texture.name.Contains(charKeyword) && 
            s.name.StartsWith(stateKeyword)
        ).OrderBy(s => ExtractFrameNumber(s.name)).ToArray();

        return filtered;
    }

    private static int ExtractFrameNumber(string spriteName)
    {
        int underscore = spriteName.LastIndexOf('_');
        if (underscore >= 0 && int.TryParse(spriteName.Substring(underscore + 1), out int num))
        {
            return num;
        }
        return 0;
    }
}
