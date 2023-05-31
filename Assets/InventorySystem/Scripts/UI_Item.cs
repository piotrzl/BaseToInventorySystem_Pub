using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
public class UI_Item : MonoBehaviour
{
    public UnityEvent<UI_Item> ItemIsPick; 

    RectTransform _rectTransform;
    Vector2Int inventoryCell;
    uint itemInventoryID;
    [SerializeField] TMP_Text textCount;
    [SerializeField] TMP_Text textName;
    [SerializeField] Image image;
    bool isTurned;

    public void InitItem(Vector2Int inventoryCell,uint itemInventoryID, Sprite sprtie, Color color ,int count, string name) 
    {
        _rectTransform = GetComponent<RectTransform>();
        
        this.inventoryCell = inventoryCell;
        this.itemInventoryID = itemInventoryID;

        image.sprite = sprtie;
        image.color = color;
        textCount.text = count.ToString();
        textName.text = name;
    }

    public void SeImageTransform(Vector2 size, float angle) 
    {
        image.rectTransform.eulerAngles = new Vector3(0, 0, angle);
        image.rectTransform.sizeDelta = size;
    }

    public void UpdateCount(int count) 
    {
        textCount.text = count.ToString();
    }



   public void PickItem() 
   {
        UI_Menager_Inventory.Instance.PickItem(this);
    }

    #region Get
    public Vector2Int InventoryCell => inventoryCell;

    public uint ItemInventoryID => itemInventoryID;

    public RectTransform RectTransform => _rectTransform;

    public Image Image => image;
    #endregion
}
