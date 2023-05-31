using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class UI_Inventory : MonoBehaviour
{
    [SerializeField] RectTransform _rectTransform;
    [SerializeField] RectTransform itemContener;
    [SerializeField] Image gridImage;
    [SerializeField] UI_Item UI_ItemPrefab;
    [SerializeField] Inventory _inventory;
    [SerializeField] float gridScale = 25f;
    [SerializeField] TMP_Text invntoryNameText;

    List<UI_Item> items = new List<UI_Item>();
    bool isInit = false;
    bool isBuild = false;

    #region start / init / destroy

    void Start()
    {

    }

    void Init()
    {
        if (!_rectTransform)
            _rectTransform = GetComponent<RectTransform>();

        isInit = true;


    }

    void OnDestroy()
    {
        if (_inventory)
            _inventory.InventoryIsUpdateInCellEvent.RemoveListener(UpdateItem);
    }

    public void BuildUIInventory(Inventory inventory)
    {
        if (!isInit)
            Init();

        if (!Inventory.AllGood(inventory))
            return;

        _inventory = inventory;

        _inventory.InventoryIsUpdateInCellEvent.AddListener(UpdateItem);


        _rectTransform.sizeDelta = new Vector2(inventory.InventoryStats.Size.x * gridScale, inventory.InventoryStats.Size.y * gridScale);
        DisplayItems();

        invntoryNameText.SetText(_inventory.InventoryStats.InvntoryName);

        isBuild = true;
    }
    #endregion

    #region Inventory Updates

    public void UpdateUIInventory(bool forceUpdate)
    {
        if (!_inventory)
            return;

        if (!isBuild)
        {
            Debug.LogError("Build inventory before Update");
            return;
        }

        if (forceUpdate || items.Count != _inventory.InventoryItems.Count)
        {
          //  Debug.Log("inventory need updae");
            UpdateDispalyItem();
        }
    }


    void DisplayItems()
    {
        for (int i = 0; i < Inventory.InventoryItems.Count; ++i)
        {
            CreateItem(_inventory.InventoryItems[i]);
        }
    }

    void UpdateDispalyItem()
    {

        for (int i = 0; i < Inventory.InventoryItems.Count; ++i)
        {
            bool itemExist = false;
            for (int k = 0; k < items.Count; ++k)
            {
                if (Inventory.InventoryItems[i].ItemInventoryID == items[k].ItemInventoryID)
                {
                    itemExist = true;
                    break;
                }
            }

            if (!itemExist)
                CreateItem(_inventory.InventoryItems[i]);
        }
    }

    #endregion

    #region Update Item

    public void UpdateItem(Inventory inventory, Vector2Int cellItem, uint itemInventoryID, Inventory.ItemUpdateMode updateMode)
    {
        switch (updateMode)
        {
            case Inventory.ItemUpdateMode.ADD:
                CreateItem(Inventory.InventoryGrid[cellItem.x, cellItem.y]);
                break;
            case Inventory.ItemUpdateMode.REMOVE:
                RemoveItem(itemInventoryID);
                break;
            case Inventory.ItemUpdateMode.CHAGE_COUNT:
                UpdateItemCount(itemInventoryID, Inventory.InventoryGrid[cellItem.x, cellItem.y].Count);
                break;
        }
    }



    void RemoveItem(uint itemInventoryID)
    {
        for (int i = 0; i < items.Count; ++i)
        {
            if (items[i].ItemInventoryID == itemInventoryID)
            {
                GameObject gameObject = items[i].gameObject;
                items.Remove(items[i]);
                Destroy(gameObject);
                break;
            }
        }

    }

    void RemoveItem(int indexUI)
    {
        items.Remove(items[indexUI]);
    }

    void UpdateItemCount(uint itemInventoryID, int count)
    {
        for (int i = 0; i < items.Count; ++i)
        {
            if (items[i].ItemInventoryID == itemInventoryID)
            {
                items[i].UpdateCount(count);
                break;
            }
        }
    }


    void CreateItem(ItemInventory item) 
    {
        UI_Item newItem = Instantiate(UI_ItemPrefab, itemContener);

        items.Add(newItem);

        newItem.InitItem(item.CellInInventory,
            item.ItemInventoryID,
            item.ItemStats.ItemIcon,
            item.ItemStats.ColorIcon,
            item.Count,
            item.ItemStats.NameID
            );

        RectTransform newItemRectTransform = newItem.GetComponent<RectTransform>();

        if (!item.IsTurned)
        {
            newItemRectTransform.sizeDelta = new Vector2(item.ItemStats.Size.x * gridScale, item.ItemStats.Size.y * gridScale);
            newItem.SeImageTransform(new Vector2(newItemRectTransform.sizeDelta.x, newItemRectTransform.sizeDelta.y), 0f);

            newItemRectTransform.anchoredPosition = new Vector2(item.CellInInventory.x * gridScale, -item.CellInInventory.y * gridScale);


        }
        else
        {
            newItemRectTransform.sizeDelta = new Vector2(item.ItemStats.Size.y * gridScale, item.ItemStats.Size.x * gridScale);
            newItem.SeImageTransform(new Vector2(newItemRectTransform.sizeDelta.y, newItemRectTransform.sizeDelta.x), 90f);

            newItemRectTransform.anchoredPosition = new Vector2(item.CellInInventory.x * gridScale, -item.CellInInventory.y * gridScale);
        }
    }



    #endregion

    #region Get

    public List<UI_Item> Items => items;

    public Inventory Inventory => _inventory;

    public RectTransform RectTransform => _rectTransform;

    public bool IsBuild => isBuild;

    public Image GridImage => gridImage;

    public float GridScale => gridScale;
    #endregion
}
