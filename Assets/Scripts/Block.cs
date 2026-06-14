using UnityEngine;

public class Block : MonoBehaviour
{
    private float destroyBelowY;

    void Start()
    {
        // Calculate the Y position just below the bottom of the screen
        Camera cam = Camera.main;
        destroyBelowY = cam.ScreenToWorldPoint(new Vector3(0, 0, 0)).y - 1f;
    }

    void Update()
    {
        // Move downward at the global scroll speed
        transform.Translate(0, -ScrollManager.Instance.CurrentSpeed * Time.deltaTime, 0);

        // Destroy when off the bottom of the screen
        if (transform.position.y < destroyBelowY)
        {
            Destroy(gameObject);
        }
    }
}
