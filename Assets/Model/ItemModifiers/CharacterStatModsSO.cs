using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterStatModsSO : ScriptableObject // Abstraction to affect character stats 
{
    public abstract void AffectChara(GameObject character, float val); // Affect object by how much
}
