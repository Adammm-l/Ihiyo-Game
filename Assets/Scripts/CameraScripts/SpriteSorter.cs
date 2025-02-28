using UnityEngine;
//Adam
//This automatically makes characters lower on the screen appear in front of characters higher on the screen.
public class SpriteSorter : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    [SerializeField] private float yPositionOffset = 0f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        spriteRenderer.sortingOrder = Mathf.RoundToInt(-(transform.position.y + yPositionOffset) * 100);
    }
}