using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
    [SerializeField] private float destroyTime = 0.1f; // Durasi efek muncul

    private void Start()
    {
        Destroy(gameObject, destroyTime);
    }
}