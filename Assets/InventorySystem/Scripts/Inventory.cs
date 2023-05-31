using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Inventory : MonoBehaviour
{
    // remember item grid in inventory start from left down conrenr 
    // UI shuld chage it from down to up

    #region Events
    public UnityEvent<Inventory, InventoyUpdateMode> InventoryIsUpdateEvent;
    public UnityEvent<Inventory, Vector2Int, uint, ItemUpdateMode> InventoryIsUpdateInCellEvent;

    // ADD Update mode is use after create new item
    // Rmove Update mode is use before removeItem

    #endregion

    public enum InventoyUpdateMode { DESTROY }
    public enum ItemUpdateMode { ADD, REMOVE, CHAGE_COUNT }

    [SerializeField] InventoryStats _inventoryStats;
    bool isInit = false;
    ItemInventory[,] inventoryGrid;

    uint lastItemInventoryID = 0; // never use
    uint inventoryID = 0;
    static uint inventoryIDCout = 0;

    List<ItemInventory> _inventoryItems = new List<ItemInventory>();


  

    #region Static Methos

    public static bool TransferItemFromTo(ItemWorld itemObject, Inventory inventory, int count) // transfer from object to inventory
    {
        if (!AllGood(inventory) || !itemObject.AllGood())
            return false;

        int curCount = inventory.AddItem(itemObject.ItemStats, itemObject.Count);
        itemObject.SetItemCount(curCount);

        if (curCount <= 0)
            Destroy(itemObject.gameObject);


        return curCount == count;
    } 

    public static bool TransferItemFromTo(Inventory inventory, Vector2Int itemFormCell, int count, Vector3 positionInWorld, Quaternion rotationInWorld) // transfer item from invntory to object
    {
        if (!(AllGood(inventory) && inventory.CellInGrid(itemFormCell) && inventory.InventoryGrid[itemFormCell.x, itemFormCell.y] != null))
            return false;



        //   ItemInventory curItem = inventory.InventoryGrid[itemFormCell.x, itemFormCell.y];

        int getItemIndex = inventory.FindIndexFormItemInventoryID(inventory.InventoryGrid[itemFormCell.x, itemFormCell.y].ItemInventoryID);

        if (getItemIndex < 0)
            return false;

        if (inventory.InventoryItems[getItemIndex].AllGood())
        {
            ItemStats stats = inventory.InventoryItems[getItemIndex].ItemStats;

            int howMuchtransfer = Mathf.Clamp(count, 0, stats.MaxObjectCount);

            int howMuchLeft = inventory.TakeCountFormItem(getItemIndex, howMuchtransfer);

            if (howMuchLeft < 0)
                howMuchtransfer += howMuchLeft;


            ItemWorld itemWorld = Instantiate(stats.Prefab, positionInWorld, rotationInWorld).GetComponent<ItemWorld>();
            if (itemWorld)
            {
                itemWorld.SetItemCount(howMuchtransfer);
            }
        }


        return true;
    }

    public static bool TransferItemFromTo(Inventory inventoryFrom, Vector2Int itemFormCell, int count, Inventory inventoryTo, Vector2Int itemToCell, bool isTurned) // transfer item form inventory to other inventory
    {
        if (!(AllGood(inventoryFrom) && inventoryFrom.CellInGrid(itemFormCell) && inventoryFrom.InventoryGrid[itemFormCell.x, itemFormCell.y] != null))
            return false;

        if (!(AllGood(inventoryTo) && inventoryTo.CellInGrid(itemFormCell)))
            return false;

        // inventoryTo.AddItem()
        int fromItemIndex = inventoryFrom.FindIndexFormItemInventoryID(inventoryFrom.InventoryGrid[itemFormCell.x, itemFormCell.y].ItemInventoryID);

        if (fromItemIndex < 0)
            return false;

        if (!inventoryFrom.InventoryItems[fromItemIndex].AllGood())
            return false;

        ItemStats curItemStats = inventoryFrom.InventoryItems[fromItemIndex].ItemStats;


        if (inventoryTo.IsEmpty(itemToCell, inventoryTo.CalculateEndCell(itemToCell, curItemStats.Size, isTurned))) // tranfer item to empty space
        {
            int howMuchCanTransfer = Mathf.Clamp(inventoryFrom.InventoryItems[fromItemIndex].Count, 0, Mathf.Min(count, curItemStats.MaxInventoryCount));

            int itemLeft = inventoryFrom.TakeCountFormItem(fromItemIndex, howMuchCanTransfer);

            if (itemLeft < 0)
                howMuchCanTransfer -= itemLeft;


            inventoryTo.AddItemToInventory(curItemStats, itemToCell, isTurned, howMuchCanTransfer);
            return true;
        }
        else // transfer item to exist stack
        {
            ItemInventory toItem = inventoryTo.FindItemInInventoryGrid(curItemStats.ItemID, itemToCell, itemToCell);
            
            if (toItem != null && toItem.Count < curItemStats.MaxInventoryCount)
            {
                 int toItemIndex = inventoryTo.FindIndexFormItemInventoryID(toItem.ItemInventoryID);
                 int howMuchCanGet = Mathf.Clamp(count, 0, inventoryFrom.InventoryItems[fromItemIndex].Count);
            //    inventoryTo.AddCountToItem(toItemIndex, inventoryFrom.TakeCountFormItem(fromItemIndex, howMuchCanGet));
                
                inventoryFrom.TakeCountFormItem(fromItemIndex, howMuchCanGet - inventoryTo.AddCountToItem(toItemIndex, howMuchCanGet));

            }
        }
        return false;
    }

    public static bool AllGood(Inventory inventry)
    {
        if (inventry && inventry.AllGood())
            return true;

        return false;
    }

    #endregion

    #region Start / Init
    void Awake()
    {
        inventoryID = inventoryIDCout;
        ++inventoryIDCout;
    }
    void Start()
    {
        if (!isInit)
            InitInventory();
    }



    public void InitInventory()
    {
        if (isInit)
        {
            Debug.Log("Inventory is already init");
            return;
        }

        if (!_inventoryStats)
        {
            Debug.LogError("_inventoryStats is not connected");
            return;
        }

        if (_inventoryStats.Size.x <= 0 || _inventoryStats.Size.y <= 0)
        {
            Debug.Log("Inventory Size is to small x or/and y <= 0");
            // return;
        }




        inventoryGrid = new ItemInventory[_inventoryStats.Size.x, _inventoryStats.Size.y];
        isInit = true;
    }



    #endregion


    #region inventory interaction
    public int AddItem(ItemStats itemStats, int Count) // just add item. decrease item if Count is bellow 0 // return how much item left 
    {
        if (!AllGood())
            return Count; // inventory is not correct

        if (!itemStats && itemStats.AllGood())
            return Count; // item is not correct



        int currentCount = AddItemToExistsStack(itemStats, Count);
        // try add item to exists stack 


        // try place item as a new stack
        bool newStack = true;
        while (currentCount > 0 && newStack)
        {
            int countToAdd = Mathf.Clamp(currentCount, 0, itemStats.MaxInventoryCount);
            newStack = AddCreateNewStack(itemStats, countToAdd);

            if(newStack)
            currentCount -= countToAdd;
        }

        return currentCount;
    }
    #endregion

    #region Item operation

    int AddItemToExistsStack(ItemStats itemStats, int Count) // return int, how much item count left after add
    {
        int currentCount = Count;
        int nextIndex = 0;

        while (currentCount != 0 && nextIndex < _inventoryItems.Count)
        {
            int itemIndex = FindItemInInventoryIndex(itemStats.ItemID, nextIndex);

            if (itemIndex < 0)
                break;

            int addCount = 0;
            if (Count > 0)  // add item count To slot
            {
                addCount = Mathf.Clamp(itemStats.MaxInventoryCount - _inventoryItems[itemIndex].Count, 0, currentCount);
                _inventoryItems[itemIndex].AddCount(addCount);
                InventoryIsUpdateInCellEvent?.Invoke(this, _inventoryItems[itemIndex].CellInInventory, _inventoryItems[itemIndex].ItemInventoryID, ItemUpdateMode.CHAGE_COUNT);
            }
            else if (Count < 0) // decrease item cout in slot
            {
                addCount = Mathf.Clamp(_inventoryItems.Count, currentCount, 0);
                _inventoryItems[itemIndex].AddCount(addCount);

                if (_inventoryItems.Count <= 0) // remove item from inventory if count is below/eqal 0
                {
                    RemoveFromInventory(itemIndex);
                }

                InventoryIsUpdateInCellEvent?.Invoke(this, _inventoryItems[itemIndex].CellInInventory, _inventoryItems[itemIndex].ItemInventoryID, ItemUpdateMode.CHAGE_COUNT);
            }
            

            currentCount -= addCount;
            nextIndex = itemIndex + 1;
        }

        return currentCount;
    }

    bool AddCreateNewStack(ItemStats itemStats, int count) // place all count in one stack, function DON'T check stats maxInventoryStack, all count put i to one slot. returnt true if add new stack 
    {
        if (count <= 0)
            return false;

        bool isTurned = false;


        Vector2Int cell = FindEmptySpaceInGrid(itemStats.Size);
        if (cell.x < 0 || cell.y < 0)
        {
            cell = FindEmptySpaceInGrid(new Vector2Int(itemStats.Size.y, itemStats.Size.x));
            if (cell.x < 0 || cell.y < 0)
                return false;

            isTurned = true;
        }

        AddItemToInventory(itemStats, cell, isTurned, count);
        return true;
    }
   
    void AddItemToInventory(ItemStats itemStats,Vector2Int cellInInventory,bool isTurned, int count)
    {
        ItemInventory newitem = new ItemInventory(itemStats,lastItemInventoryID, cellInInventory, isTurned, count);
        _inventoryItems.Add(newitem);
        ++lastItemInventoryID;

        Vector2Int EndCell = CalculateEndCell(cellInInventory, itemStats.Size, isTurned);
        EditGrid(newitem, cellInInventory, EndCell);

        InventoryIsUpdateInCellEvent?.Invoke(this, cellInInventory, newitem.ItemInventoryID, ItemUpdateMode.ADD);
    }




    void RemoveFromInventory(int itemIndex) 
    {
        Vector2Int endCell = CalculateEndCell(_inventoryItems[itemIndex].CellInInventory, _inventoryItems[itemIndex].ItemStats.Size, _inventoryItems[itemIndex].IsTurned);

        EditGrid(null, _inventoryItems[itemIndex].CellInInventory, endCell);

        uint itemInventoryID = _inventoryItems[itemIndex].ItemInventoryID;
        Vector2Int cellInInventory = _inventoryItems[itemIndex].CellInInventory;

        _inventoryItems.Remove(_inventoryItems[itemIndex]);

        InventoryIsUpdateInCellEvent?.Invoke(this, cellInInventory, itemInventoryID, ItemUpdateMode.REMOVE);
    }


    int TakeCountFormItem(int itemIndex, int itemCount) //return how much item left in slot after operation, !! item be remove if return value <= 0 !! can return < 0 if item count in slot is < itemCount
    {
        int itemLeft = _inventoryItems[itemIndex].Count - itemCount;

        if (itemLeft > 0)
        {
            _inventoryItems[itemIndex].SetCount(itemLeft);
            InventoryIsUpdateInCellEvent?.Invoke(this, _inventoryItems[itemIndex].CellInInventory, _inventoryItems[itemIndex].ItemInventoryID, ItemUpdateMode.CHAGE_COUNT);
        }
        else
            RemoveFromInventory(itemIndex);

        return itemLeft;
    }

    int AddCountToItem(int itemIndex, int itemCount) // return how much left from itemCount
    {
        if (_inventoryItems[itemIndex].Count >= _inventoryItems[itemIndex].ItemStats.MaxInventoryCount)
            return itemCount;

        int howMuchCanAdd = Mathf.Clamp(_inventoryItems[itemIndex].ItemStats.MaxInventoryCount - _inventoryItems[itemIndex].Count, 0, itemCount);

        _inventoryItems[itemIndex].AddCount(howMuchCanAdd);

        InventoryIsUpdateInCellEvent?.Invoke(this, _inventoryItems[itemIndex].CellInInventory, _inventoryItems[itemIndex].ItemInventoryID, ItemUpdateMode.CHAGE_COUNT);

        return itemCount - howMuchCanAdd;
    }


    #endregion

    #region Cell operation

    void EditGrid(ItemInventory itemInventory, Vector2Int startCell, Vector2Int endCell) 
    {
        if (!CellInGrid(startCell) || !CellInGrid(endCell))
            return;

        int startX = Mathf.Min(startCell.x, endCell.x);
        int endX = Mathf.Max(startCell.x, endCell.x);

        int startY = Mathf.Min(startCell.y, endCell.y);
        int endY = Mathf.Max(startCell.y, endCell.y);

        for (int y = startY; y <= endY; ++y)
            for (int x = startX; x <= endX; ++x)
            {
                inventoryGrid[x, y] = itemInventory;
            }
    }// done

    #endregion

    #region Find operation

    int FindItemInInventoryIndex(int itemID, int startIndex)
    {
        if (startIndex >= _inventoryItems.Count)
            return -1; //return -1 if start index is to hight

        for (int i = startIndex; i < _inventoryItems.Count; ++i)
        {
            if (_inventoryItems[i].ItemStats.ItemID == itemID)
                return i;
        }


        return -1; // return -1 if item not exist
    } // done 


   int FindIndexFormItemInventoryID(uint itemInventoryID)  // return item index from InventoryItems list, return -1 if can't find 
    {
        for(int i =0; i < InventoryItems.Count; ++i) 
        {
            if(InventoryItems[i].ItemInventoryID == itemInventoryID)
            return i;
        }

        return -1;
   } // done

    ItemInventory FindItemInInventoryGrid(int itemID, Vector2Int startCell, Vector2Int endCell) // not ready yet
    {
        if (!CellInGrid(startCell) || !CellInGrid(endCell))
            return null;

        int startX = Mathf.Min(startCell.x, endCell.x);
        int endX = Mathf.Max(startCell.x, endCell.x);

        int startY = Mathf.Min(startCell.y, endCell.y);
        int endY = Mathf.Max(startCell.y, endCell.y);

        for (int y = startY; y <= endY; ++y)
            for (int x = startX; x <= endX; ++x)
            {
                if (inventoryGrid[x, y] != null && inventoryGrid[x,y].ItemStats.ItemID == itemID)
                {
                    return inventoryGrid[x, y];
                }
            }

        return null;
    } //done

    Vector2Int FindItemInInventoryGrid(int itemID, Vector2Int startCell) // not ready yet
    {
        return -Vector2Int.one;
    }



    Vector2Int FindEmptySpaceInGrid(Vector2Int itemSize) // return -Vector if can't find enought space for item
    {
        for(int y = 0; y < _inventoryStats.Size.y; ++y)
            for (int x= 0; x < _inventoryStats.Size.x; ++x) 
            {
                if(IsEmpty(new Vector2Int(x,y), new Vector2Int(x, y) + itemSize - Vector2Int.one))
                {
                    return new Vector2Int(x, y);
                }

            }

        return -Vector2Int.one;
    }// done




    bool IsEmpty(Vector2Int startCell, Vector2Int endCell) 
    {
        if (!CellInGrid(startCell) || !CellInGrid(endCell))
            return false;

        int startX = Mathf.Min(startCell.x, endCell.x);
        int endX = Mathf.Max(startCell.x, endCell.x);

        int startY = Mathf.Min(startCell.y, endCell.y);
        int endY = Mathf.Max(startCell.y, endCell.y);

        for (int y = startY; y <= endY; ++y)
            for (int x = startX; x <= endX; ++x)
            {
                if (inventoryGrid[x, y] != null)
                {
                    return false;
                }
            }

       return true;
    }// done

    bool IsEmpty(Vector2Int startCell, Vector2Int endCell, uint IgnoreItemInventoryID) 
    {

        if (!CellInGrid(startCell) || !CellInGrid(endCell))
            return false;

        int startX = Mathf.Min(startCell.x, endCell.x);
        int endX = Mathf.Max(startCell.x, endCell.x);

        int startY = Mathf.Min(startCell.y, endCell.y);
        int endY = Mathf.Max(startCell.y, endCell.y);

        for (int y = startY; y <= endY; ++y)
            for (int x = startX; x <= endX; ++x)
            {
                if (inventoryGrid[x, y] != null && inventoryGrid[x,y].ItemInventoryID != IgnoreItemInventoryID)
                {
                    return false;
                }
            }

        return true;
    }


    #endregion

    #region Other
    public bool AllGood()
    {
        if (!isInit)
        {
            Debug.Log("Inventory is not init");
            return false;
        }

        return true;
    }

    bool CellInGrid(Vector2Int Cell)
    {
        if (Cell.x < 0 || _inventoryStats.Size.x <= Cell.x || Cell.y < 0 || _inventoryStats.Size.y <= Cell.y)
            return false;

        return true;
    }// done

    public Vector2Int CalculateEndCell(Vector2Int startCell, Vector2Int ItemSIze, bool isTurned)
    {
        Vector2Int endCell;
        if (!isTurned)
            endCell = startCell + ItemSIze - Vector2Int.one;
        else
            endCell = startCell + new Vector2Int(ItemSIze.y, ItemSIze.x) - Vector2Int.one;

        return endCell;
    }//done

    #endregion

    #region Get


    public InventoryStats InventoryStats => _inventoryStats;

    public List<ItemInventory> InventoryItems => _inventoryItems;

    public ItemInventory[,] InventoryGrid => inventoryGrid;

    public uint LastItemInventoryID => lastItemInventoryID;

    public uint InventoryID => inventoryID;

    public bool IsInit => isInit;
    #endregion

    void OnDestroy()
    {
        InventoryIsUpdateEvent?.Invoke(this, InventoyUpdateMode.DESTROY);
    }
}
