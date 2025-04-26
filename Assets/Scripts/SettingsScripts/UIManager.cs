using UnityEngine;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance { get { return instance; } }

    [SerializeField] private GameObject[] uiPanels;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetActivePanel(GameObject panel)
    {
        if (NPCInteraction.IsInteracting)
            return;

        foreach (GameObject p in uiPanels)
        {
            if (p != panel)
                p.SetActive(false);
        }
    }
}