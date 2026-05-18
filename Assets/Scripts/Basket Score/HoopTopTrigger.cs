using BasketRobbins;
using UnityEngine;

public class HoopTopTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(GameConstants.Tag.Ball)) return;

        if (!TryGetBallAndRb(other, out BallController ball, out Rigidbody rb)) return;

        if (rb.velocity.y < 0f)
        {
            // Ball moving downward — only valid if it came from above legitimately
            if (ball.approachedFromAbove && !ball.enteredFromBelow)
            {
                ball.passedTopDownward = true;
                ball.scoringInProgress = true;
            }
            else
            {
                // Thrown from below, fell back down — reject
                CancelScoring(ball);
            }
        }
        else
        {
            // Ball moving upward through top = coming from below
            CancelScoring(ball);
            ball.enteredFromBelow = true;
            ball.approachedFromAbove = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(GameConstants.Tag.Ball)) return;

        if (!TryGetBallAndRb(other, out BallController ball, out Rigidbody rb)) return;

        // Ball bounced back up after entering — cancel
        if (rb.velocity.y > 0f)
            CancelScoring(ball);
    }

    private void CancelScoring(BallController ball)
    {
        ball.passedTopDownward = false;
        ball.scoringInProgress = false;
    }

    private bool TryGetBallAndRb(Collider col, out BallController ball, out Rigidbody rb)
    {
        ball = col.GetComponent<BallController>();
        rb = col.GetComponent<Rigidbody>();
        return ball != null && rb != null;
    }
}