using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class UI_Menager_Inventory : MonoBehaviour
{
    static UI_Menager_Inventory _uI_Menager_Inventory;
    public static UI_Menager_Inventory Instance => _uI_Menager_Inventory;

    List<UI_Inventory> openUI_Inventory = new List<UI_Inventory>();

   [SerializeField] Canvas parrentCanvas;

    UI_Item currPickedItem;
    UI_Inventory handsInventory;// use only to hold mouse item


    void Awake()
    {
        if (_uI_Menager_Inventory)
            Destroy(gameObject);
        else
            _uI_Menager_Inventory = this;

        if (!parrentCanvas)
            Debug.Log("add canvas to UI_Menager");

        gameObject.SetActive(false);

    }


    #region Open / Hide / Close Inventory
    public void AddHandsInvntory(Inventory inventory, Vector2 position) 
    {
        if (!Inventory.AllGood(inventory))
            return;

        if (handsInventory && handsInventory.Inventory != inventory)
            Destroy(handsInventory);

        if(!handsInventory)
        {
            handsInventory = Instantiate(inventory.InventoryStats.UI_Prefab, transform);
            handsInventory.RectTransform.anchoredPosition = position;
        }



        if (!handsInventory.IsBuild)
            handsInventory.BuildUIInventory(inventory);
        else
            handsInventory.UpdateUIInventory(false);

    }

    public void OpenInventory(Inventory inventory, Vector2 position) 
    {
        gameObject.SetActive(true);

        if (!Inventory.AllGood(inventory))
            return;


        int inventoryIndex = -1;

        UI_Inventory newUIInventory;

        for (int i = 0; i < openUI_Inventory.Count; ++i) 
        {
            if (openUI_Inventory[i].Inventory == inventory)
            {
                inventoryIndex = i;
                break;
            }
        }


        if (inventoryIndex < 0)
        {
            newUIInventory = Instantiate(inventory.InventoryStats.UI_Prefab, transform);
            newUIInventory.RectTransform.anchoredPosition = position;
            inventoryIndex = openUI_Inventory.Count;
            openUI_Inventory.Add(newUIInventory);
        }
        else
            newUIInventory = openUI_Inventory[inventoryIndex];




        if (!newUIInventory.IsBuild)
            newUIInventory.BuildUIInventory(inventory);
        else
            newUIInventory.UpdateUIInventory(false);

        // to make sure item be always above inventory
        if (currPickedItem)
        currPickedItem.transform.SetAsLastSibling();

    }
    public void HideInventoryUI()
    {
        OnDisableUI();
        gameObject.SetActive(false);
    }

    public void HideThisInventory(Inventory inventory)
    {
        if (!Inventory.AllGood(inventory))
            return;

        for (int i = 0; i < openUI_Inventory.Count; ++i)
        {
            if (openUI_Inventory[i].Inventory == inventory)
            {
                openUI_Inventory[i].gameObject.SetActive(false);
                return;
            }
        }
    }

    public void CloseInvntory(Inventory inventory)
    {
        if (!Inventory.AllGood(inventory))
            return;

        for (int i = 0; i < openUI_Inventory.Count; ++i)
        {
            if (openUI_Inventory[i].Inventory == inventory)
            {
                GameObject objectUIToDestroy= openUI_Inventory[i].gameObject;
                openUI_Inventory.Remove(openUI_Inventory[i]);
                Destroy(objectUIToDestroy);
                return;
            }
        }
    }

    void OnDisableUI() 
    {

        if (handsInventory && currPickedItem)
        {
            while (handsInventory.Items.Count > 0) // drop all item from handsInvntory to world
            {
                Inventory.TransferItemFromTo(handsInventory.Inventory, currPickedItem.InventoryCell, int.MaxValue, handsInventory.Inventory.transform.position + handsInventory.Inventory.transform.forward *2f, Quaternion.identity);
            }
        }

    }





    #endregion

    #region Item Menagment
    public void PickItem(UI_Item uI_Item) 
    {

       if(handsInventory && !currPickedItem)
       {
            UI_Inventory curPickedItemInventoryParrent = uI_Item.GetComponentInParent<UI_Inventory>();
            Inventory.TransferItemFromTo(curPickedItemInventoryParrent.Inventory, uI_Item.InventoryCell, int.MaxValue, handsInventory.Inventory, Vector2Int.zero, uI_Item.Image.rectTransform.eulerAngles.z > 1f, Inventory.TransferMode.NON);

            currPickedItem = handsInventory.Items[0];
        //    lastPositionPickedItem = uI_Item.RectTransform.position;

            currPickedItem.transform.SetParent(transform);
            currPickedItem.transform.SetAsLastSibling();

            StartCoroutine(PickedItemControl());
       }
      

    }


    IEnumerator PickedItemControl() 
    {
        //  Camera _camera = Camera.main;
        Vector2 placeMod = currPickedItem.RectTransform.sizeDelta / 2;
        placeMod = new Vector2(-placeMod.x, placeMod.y);

        bool isTunred = false;
        if (currPickedItem && currPickedItem.Image.rectTransform.eulerAngles.z > 1f)
            isTunred = true;


        while (currPickedItem && handsInventory) 
        {

            if (Input.GetMouseButtonDown(0)) // one item operation
            {
                UI_Inventory newInventory = MouseOnInventory();

                if (newInventory) // put one item to slot
                {
                    Vector2 point = GetPositionOnUIGrid(newInventory);

                    if (!Inventory.TransferItemFromTo(handsInventory.Inventory, currPickedItem.InventoryCell, int.MaxValue, newInventory.Inventory, GetCellFormUIGrid(point, newInventory.GridScale), isTunred, Inventory.TransferMode.ONLY_EXIST))
                    {
                        point += (new Vector2(newInventory.GridScale, newInventory.GridScale) - currPickedItem.RectTransform.sizeDelta) / 2f;

                        Inventory.TransferItemFromTo(handsInventory.Inventory, currPickedItem.InventoryCell, int.MaxValue, newInventory.Inventory, GetCellFormUIGrid(point, newInventory.GridScale), isTunred, Inventory.TransferMode.ONLY_EMPTY);
                    }
                }
                else //drop one item
                {
                    Inventory.TransferItemFromTo(handsInventory.Inventory, currPickedItem.InventoryCell, int.MaxValue, handsInventory.Inventory.transform.position + handsInventory.Inventory.transform.forward * 2f, Quaternion.identity);
                }
            }
            else if(Input.GetMouseButtonDown(1)) // all item operaition
            {
                UI_Inventory newInventory = MouseOnInventory();

               

                if (newInventory) // put all item to slot
                {

                    Vector2 point = GetPositionOnUIGrid(newInventory);

                    // UI_Item newitem = MouseOnItem(currPickedItem);
                    if (!Inventory.TransferItemFromTo(handsInventory.Inventory, currPickedItem.InventoryCell, 1, newInventory.Inventory, GetCellFormUIGrid(point, newInventory.GridScale), isTunred, Inventory.TransferMode.ONLY_EXIST))
                    {
                        point += (new Vector2(newInventory.GridScale, newInventory.GridScale) - currPickedItem.RectTransform.sizeDelta) / 2f;

                        Inventory.TransferItemFromTo(handsInventory.Inventory, currPickedItem.InventoryCell, 1, newInventory.Inventory, GetCellFormUIGrid(point, newInventory.GridScale), isTunred, Inventory.TransferMode.ONLY_EMPTY);
                    }
                }
                else //drop all item
                {
                    Inventory.TransferItemFromTo(handsInventory.Inventory, currPickedItem.InventoryCell, 1, handsInventory.Inventory.transform.position + handsInventory.Inventory.transform.forward * 2f, Quaternion.identity);
                }
            }
            else // move item
            {
                if (Input.GetKeyDown(KeyCode.R)) // rotate item in UI
                {
                    isTunred = !isTunred;

                    placeMod = currPickedItem.RectTransform.sizeDelta / 2;
                    placeMod = new Vector2(-placeMod.y, placeMod.x);


                    currPickedItem.RectTransform.sizeDelta = new Vector2(currPickedItem.RectTransform.sizeDelta.y, currPickedItem.RectTransform.sizeDelta.x);
                    if (isTunred)
                    {
                        currPickedItem.SeImageTransform(new Vector2(currPickedItem.RectTransform.sizeDelta.y, currPickedItem.RectTransform.sizeDelta.x), 90);
                    }
                    else
                    {
                        currPickedItem.SeImageTransform(new Vector2(currPickedItem.RectTransform.sizeDelta.x, currPickedItem.RectTransform.sizeDelta.y), 0);
                    }
                }
                    


                Vector2 mousePosition = Input.mousePosition;

                //  Debug.Log("IEnumerator PickedItem() work");
                currPickedItem.RectTransform.position = mousePosition + placeMod;
            }
            



            yield return null;
        }

        OnDisableUI();
    }

   
    UI_Inventory MouseOnInventory() 
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;

        List<RaycastResult> raycastResults = new List<RaycastResult>();

        EventSystem.current.RaycastAll(pointerEventData, raycastResults);
        for(int i =0; i < raycastResults.Count; ++i )
        {
            if(raycastResults[i].gameObject.TryGetComponent(out UI_Inventory uI_Inventory)) 
                return uI_Inventory;
            
        }

        return null;
    }

    UI_Item MouseOnItem(UI_Item ignoreItem) 
    {

        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;

        List<RaycastResult> raycastResults = new List<RaycastResult>();

        EventSystem.current.RaycastAll(pointerEventData, raycastResults);
        for (int i = 0; i < raycastResults.Count; ++i)
        {
            if (raycastResults[i].gameObject.TryGetComponent(out UI_Item uI_item) && uI_item != ignoreItem)
                return uI_item;

        }

        return null;
    }

    #endregion

    #region other

    Vector2 GetPositionOnUIGrid(UI_Inventory uI_Inventory) 
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(uI_Inventory.GridImage.rectTransform, Input.mousePosition, parrentCanvas.worldCamera, out Vector2 localPoint))
        {
            //Vector2 point;// = uI_Inventory.GridImage.rectTransform.sizeDelta/ 2f;
                          //   Debug.Log(uI_Inventory.GridImage.rectTransform.;
            Vector2 point = new Vector2(uI_Inventory.GridScale * uI_Inventory.Inventory.InventoryStats.Size.x, -uI_Inventory.GridScale * uI_Inventory.Inventory.InventoryStats.Size.y)/2f;
            point += localPoint;
            point = new Vector2(point.x, Mathf.Abs(point.y));

           return point;
        }

        return new Vector2(0, 0);
    }

    Vector2Int GetCellFormUIGrid(Vector2 positionInUIGrid, float scale) 
    {

        Vector2Int cell =new Vector2Int((int)(positionInUIGrid.x / scale),(int) (positionInUIGrid.y / scale));

        return cell;
    }

    #endregion

}
