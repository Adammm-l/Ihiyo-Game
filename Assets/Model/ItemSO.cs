using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace InventoryModel {

    public abstract class ItemSO : ScriptableObject
    {
    
        public int MyProperty { get; set; }

        [field: SerializeField]

        public bool IsStack { get; set; }

        public int ID => GetInstanceID();

        [field: SerializeField]

        public int MaxStackSize { get; set; } = 1;

        [field: SerializeField]
    
        public string Name { get; set; }

        [field: SerializeField]
        [field: TextArea]

        public string Description { get; set; }
    
        [field: SerializeField]

        public Sprite ItemIMG { get; set;}

        [field: SerializeField]
        public List<ItemParameter> DefaultParamList { get; set; }

    }

        [Serializable]

        public struct ItemParameter : IEquatable<ItemParameter> {

            public ItemParameterSO itemParam;
            public float val;

            public bool Equals(ItemParameter other) {

                return other.itemParam == itemParam;

            }
        }

}