using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace InventoryModel {

    [CreateAssetMenu]
    public class FoodItemSO : ItemSO, IDestroyableItem, IItemAction
    {
        [SerializeField] private List<ModifierData> modifiers = new List<ModifierData>(); // Making a list of modifiers that we want to apply via eating
        public string ActionName => "Consume";
        public AudioClip actionSFX { get; private set; }
        public bool PerformAction(GameObject character) {

            foreach (ModifierData data in modifiers) {

                data.statModify.AffectChara(character, data.value); // Modify the character stats 
            }
            return true;
        }
    
    }

    public interface IDestroyableItem {

        
    }

    public interface IItemAction {

        public string ActionName { get; }
        public AudioClip actionSFX { get; }
        bool PerformAction(GameObject character);

    }

    [Serializable]

    public class ModifierData { // Now we can define different types of modifiers

        public CharacterStatModsSO statModify;
        public float value;

    }

}