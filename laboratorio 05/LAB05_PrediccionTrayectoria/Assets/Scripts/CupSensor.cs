using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CupSensor : MonoBehaviour
{
    public Cup cup;
    void OnTriggerEnter2D(Collider2D other)
    {
        var ball = other.GetComponent<Ball>();
        if (ball != null) cup.Hit(ball);
    }
}
