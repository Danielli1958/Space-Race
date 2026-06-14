using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Players")]
    public Transform player1;
    public Transform player2;

    [Header("Feel")]
    public float lookAheadY = 2f;

    private float highestYSeen = float.MinValue;

    void LateUpdate()
    {
        if (GameManager.Instance == null) return;
        if (player1 == null) return;

        float currentHighest = GetHighestY();

        // Ratchet: only ever increase the highest Y seen
        // This prevents the camera from reacting to per-frame interpolation
        // differences between the two rockets
        if (currentHighest > highestYSeen)
            highestYSeen = currentHighest;

        float targetY = highestYSeen + lookAheadY;

        transform.position = new Vector3(
            transform.position.x,
            targetY,
            transform.position.z
        );
    }

    float GetHighestY()
    {
        bool p1alive = player1 != null && player1.gameObject.activeSelf;
        bool p2alive = player2 != null && player2.gameObject.activeSelf;

        if (p1alive && p2alive)
            return Mathf.Max(player1.position.y, player2.position.y);
        if (p1alive)  return player1.position.y;
        if (p2alive)  return player2.position.y;
        return transform.position.y;
    }
}