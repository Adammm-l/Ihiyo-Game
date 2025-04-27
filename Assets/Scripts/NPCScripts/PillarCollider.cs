using UnityEngine;

public class PillarCollider : MonoBehaviour
{
    private PillarInteraction parentInteraction;

    void Start()
    {
        parentInteraction = GetComponentInParent<PillarInteraction>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            parentInteraction.SetPlayerInRange(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            parentInteraction.SetPlayerInRange(false);
        }
    }
}