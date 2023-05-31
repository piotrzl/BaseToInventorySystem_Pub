using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemInventory
{
    [SerializeField] ItemStats _itemStats;
    [SerializeField] uint itemInventoryID;  // unique item id in inventory
    [SerializeField] Vector2Int cellInInventory;
    [SerializeField] bool isTurned;
    [SerializeField] int count;

    public ItemInventory(ItemStats itemStats,uint itemInventoryID,Vector2Int positionInInventory,bool isTurned, int count)
    {
        _itemStats = itemStats;
        this.itemInventoryID = itemInventoryID;
        this.cellInInventory = positionInInventory;
        this.isTurned = isTurned;
        this.count = count;
    }

    public ItemInventory(ItemStats itemStats)
    {
        _itemStats = itemStats;
        count = 0;
    }
    

    public int HowMuchNeed() 
    {
        return ItemStats.MaxInventoryCount - count;
    }

    public void AddCount(int addCount) 
    {
        count += addCount;
    }

    public void SetCount(int newCount) 
    {
        count = newCount;
    }

   public bool AllGood() 
   {
        if (_itemStats == null || count <= 0)
            return false;

        return true;
   }

    #region Get
    public ItemStats ItemStats => _itemStats;
    public uint ItemInventoryID => itemInventoryID;
    public Vector2Int CellInInventory => cellInInventory;
    public bool IsTurned => isTurned;
    public int Count => count;

   
    #endregion

}
