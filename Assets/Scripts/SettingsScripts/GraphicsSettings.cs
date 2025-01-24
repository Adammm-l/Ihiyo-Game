using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Eri (Edwin)
public class GraphicsSettings : MonoBehaviour
{
    public void SetQuality(int qIndex) {
        
        // Quickly adjust game quality level using Unity's Quality Settings

        QualitySettings.SetQualityLevel(qIndex);

    }

}
