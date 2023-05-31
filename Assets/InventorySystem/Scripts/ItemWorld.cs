using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemWorld : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] ItemStats _itemStats;
    [SerializeField] Rigidbody _rigidbody;
    [SerializeField] int count;

    public bool AllGood()
    {
        if (_itemStats == null || count <= 0)
            return false;
        return true;
    }

    public void SetItemCount(int newCount) 
    {
        count = newCount;
    }


    public int PutItem(int count) // return rest;
    {
        int putCount = Mathf.Clamp(this.count + count, count, _itemStats.MaxObjectCount);
        this.count += putCount;

        return count - putCount;
    }

    public int TakeItemForm(int count) // return how much get from item 
    {
        int rest = Mathf.Clamp(this.count - count, 0, this.count);
        this.count -= rest;

        return count - rest;
    }



    #region Get
    public ItemStats ItemStats => _itemStats;

    public int Count => count;

    #endregion
}
