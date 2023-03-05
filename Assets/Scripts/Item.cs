using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item")]
public class Item : ScriptableObject
{
    public string objectName;

    public static int quatity = 0;

    public bool stackable;

    public enum ItemType
    {
        COIN,
    }

    public ItemType item;
}
