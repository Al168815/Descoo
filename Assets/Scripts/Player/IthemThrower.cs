using UnityEngine;

public class ItemThrower : MonoBehaviour
{
    public GameObject premioPrefab;
    public GameObject pescadoPrefab;
    public float throwForce = 12f;
    public Camera playerCamera; // usa la cámara FPS

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) // Lanza premio
            ThrowItem(premioPrefab);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) // Lanza pescado
            ThrowItem(pescadoPrefab);
    }

    void ThrowItem(GameObject itemPrefab)
    {
        if (itemPrefab == null || playerCamera == null) return;

        Vector3 spawnPos = playerCamera.transform.position + playerCamera.transform.forward * 0.6f;
        Quaternion rot = Quaternion.LookRotation(playerCamera.transform.forward);

        GameObject item = Instantiate(itemPrefab, spawnPos, rot);
        if (item.TryGetComponent<Rigidbody>(out var rb))
            rb.AddForce(playerCamera.transform.forward * throwForce, ForceMode.Impulse);
    }
}
