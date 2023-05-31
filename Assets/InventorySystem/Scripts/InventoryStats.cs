using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InventorySystem", menuName = "InventorySystem/InventoryStats")]
public class InventoryStats : ScriptableObject
{
    
    [SerializeField] Vector2Int inventorySize = new Vector2Int(5,5);
    [SerializeField] UI_Inventory uI_prefab;
    [SerializeField] string invntoryName;


    #region Get
    public Vector2Int Size => inventorySize;

    public UI_Inventory UI_Prefab => uI_prefab;


    public string InvntoryName => invntoryName;
    #endregion

}
