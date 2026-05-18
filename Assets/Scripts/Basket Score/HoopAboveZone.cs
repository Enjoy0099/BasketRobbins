using BasketRobbins;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoopAboveZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(GameConstants.Tag.Ball)) return;

        if (!TryGetBallAndRb(other, out BallController ball, out Rigidbody rb)) return;

        if (rb.velocity.y < 0f)
        {
            // Legitimate downward approach
            ball.approachedFromAbove = true;
            ball.enteredFromBelow = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(GameConstants.Tag.Ball)) return;

        if (!TryGetBallAndRb(other, out BallController ball, out Rigidbody rb)) return;

        if (rb.velocity.y > 0f)
        {
            // Ball went upward out of above zone = thrown from below
            ball.approachedFromAbove = false;
            ball.enteredFromBelow = true;
        }
    }

    private bool TryGetBallAndRb(Collider col, out BallController ball, out Rigidbody rb)
    {
        ball = col.GetComponent<BallController>();
        rb = col.GetComponent<Rigidbody>();
        return ball != null && rb != null;
    }
}
