using UnityEngine;

public class Asteroid : MonoBehaviour
{
    // Each asteroid just scrolls downward relative to the world.
    // The camera scrolls up at the same speed, so asteroids appear
    // to flow toward the player from above.

    private float destroyBelowY;

    void Start()
    {
        // Destroy when 5 units below the camera's bottom edge
        Camera cam = Camera.main;
        destroyBelowY = cam.transform.position.y
                        - cam.orthographicSize - 5f;
    }

    void Update()
    {
        // Asteroids are stationary in world space — the camera moves up,
        // making them appear to scroll downward toward the player.
        // We only need to destroy them once they're far off screen below.
        Camera cam = Camera.main;
        float bottomEdge = cam.transform.position.y - cam.orthographicSize - 5f;

        if (transform.position.y < bottomEdge)
            Destroy(gameObject);
    }
}