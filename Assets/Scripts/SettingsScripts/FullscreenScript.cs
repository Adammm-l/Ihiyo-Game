using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullscreenScript : MonoBehaviour
{

    public void setFullScreen(bool isFull) {
        
        // Connect with Unity to enable fullscreen support

        Screen.fullScreen = isFull;

    }
}
