using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemStats", menuName = "InventorySystem/ItemStats")]
public class ItemStats : ScriptableObject
{
    [Header("Base Stats")]
    [SerializeField] string nameID;
    [SerializeField] int itemID;
    [Header("World Stats")]
    [SerializeField] GameObject prefab;
    [SerializeField] int maxObjectCount = 10;
   // [SerializeField] bool autoDestroy = false;
    [Header("Inventory Stats")]
    [SerializeField] Vector2Int size = Vector2Int.one;
    [SerializeField] float weight = 1f;
    [SerializeField] int maxInventoryCount = 10;
    [Header("Inventory View")]
    [SerializeField] Sprite itemIcon;
    [SerializeField] Color colorIcon = Color.white;
    //[SerializeField] bool canTurned = false;


    #region Get Base Stats
    public string NameID => nameID;
    public int ItemID => itemID;
    #endregion

    #region Get Object Stats
    public GameObject Prefab => prefab;
    public int MaxObjectCount => maxObjectCount;
    #endregion

    #region Get Inventory Stats
    public Vector2Int Size => size;
    public float Weight => weight;
    public int MaxInventoryCount => maxInventoryCount;
    #endregion

    #region Get Inventory View
    public Sprite ItemIcon => itemIcon;
    public Color ColorIcon => colorIcon;

    #endregion

    public bool AllGood()
    {
        if (itemID < 0)
        {
            Debug.Log("itemID cannot be below 0" + " itemID: " + itemID + "NameID: " + nameID + "item stats: " + this);
            return false;

        }
        if (size.x <= 0 || size.y <= 0)
        {
            Debug.Log("item size is to small" + " itemID: " + itemID + "NameID: " + nameID + "item stats: " + this);
            return false;
        }

        if (maxInventoryCount <= 0)
        {
            Debug.Log("maxInventoryCount cannot be below or equal 0" + " itemID: " + itemID + "NameID: " + nameID + "item stats: " + this);
            return false;
        }



        return true;
    }


}
