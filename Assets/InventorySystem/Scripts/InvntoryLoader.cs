using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvntoryLoader : MonoBehaviour
{
    [SerializeField] Inventory inventoryToLoad;
    public List<ItemInventory> items = new List<ItemInventory>();


    void Start()
    {

        if (inventoryToLoad)
        {
            if (!inventoryToLoad.IsInit)
                inventoryToLoad.InitInventory();

            LoadInventory();
        }
    }

    void LoadInventory() 
    {
        for(int i =0; i < items.Count; ++i) 
        {
            if (items[i].AllGood())
            {
                inventoryToLoad.AddItem(items[i].ItemStats, items[i].Count);
            }

        }
        Destroy(gameObject);
    }
}
