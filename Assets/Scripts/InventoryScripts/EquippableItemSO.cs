using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventoryModel {

    [CreateAssetMenu] // Allow for easy customization on our end without needing major changes to code over time
    public class EquippableItem: ItemSO, IDestroyableItem, IItemAction {
        public string ActionName => "Equip";

        public AudioClip actionSFX { get; private set; }

        public bool PerformAction(GameObject character, List<ItemParameter> itemState = null)  {

            return false;
        }   
    }
}
