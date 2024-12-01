using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIInventoryPage;

public class MouseFollower : MonoBehaviour
{

    [SerializeField] private Canvas canvas;


    [SerializeField] private UIInventoryItem item;

    public void Awake() {

        canvas = transform.root.GetComponent<Canvas>(); // Get the Canvas from the Game Object
        item = GetComponentInChildren<UIInventoryItem>(); // Get our Item through Mouse
    }

    public void SetData(Sprite sprite, int num) { // Set the data of the item 

        item.SetData(sprite, num);
        
    }

    void Update() {

        Vector2 pos;

        // Pass in RectTrans of canvas, the mouse position, the camera and then output the position

        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)canvas.transform, Input.mousePosition, canvas.worldCamera, out pos); // Find the position of our mouse

        transform.position = canvas.transform.TransformPoint(pos); // Set position and transform it into the TransformRect of the given Canvas

    }

    public void Toggle(bool value) {

        Debug.Log($"Item Toggled {value}");
        gameObject.SetActive(value);
    }
}
