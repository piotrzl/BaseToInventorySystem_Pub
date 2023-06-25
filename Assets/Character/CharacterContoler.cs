using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterContoler : MonoBehaviour
{
    [SerializeField] float speed = 10;
    [SerializeField] float mouseSens;
    [SerializeField] LayerMask interactLayer;
    [SerializeField] float interactRange = 3f;
    [SerializeField] Inventory handsInventory;
    [SerializeField] List<Inventory> inventoryList;
    [SerializeField] List<Vector2> invnetoryPositions;
    [SerializeField] Vector2 otherInventoryPosition;
    Inventory otherInventory;
    Coroutine otherInvnetoryCoroutine;

    Rigidbody _rigidbody;
    Camera _camera;

    Vector3 moveDirection = Vector3.zero;
    float xRotation  = 0;
    float yRotation = 0;

    bool inventoryIsOpen = false;


    void Start()
    {
        if(TryGetComponent(out Rigidbody rb)) 
        {
            _rigidbody = rb;
        }

        Cursor.lockState = CursorLockMode.Locked;
        _camera = Camera.main;

        if (handsInventory)
        {
            if (!handsInventory.IsInit)
                handsInventory.InitInventory();

            UI_Menager_Inventory.Instance.AddHandsInvntory(handsInventory, new Vector2(4000, 4000));
        }
    }

    
    void Update()
    {
        if (!inventoryIsOpen)
        {
            xRotation = Input.GetAxis("Mouse X") * mouseSens;

            yRotation -= Input.GetAxis("Mouse Y") * mouseSens;
            yRotation = Mathf.Clamp(yRotation, -90, 90);

            Rotate();
        }

      

        if (Input.GetKeyDown(KeyCode.F))
            Interact();

        if (Input.GetKeyDown(KeyCode.I))
            if (inventoryIsOpen)
                HideInventory();
            else
                OpenInventory();
    }

    void FixedUpdate()
    {
        moveDirection.x = Input.GetAxis("Horizontal");
        moveDirection.z = Input.GetAxis("Vertical");

        Move();
    }



    #region Inventory
    void OpenInventory()
    {
        for (int i = 0; i < inventoryList.Count; ++i)
        {
            UI_Menager_Inventory.Instance.OpenInventory(inventoryList[i], invnetoryPositions[i]);
        }

        Cursor.lockState = CursorLockMode.None;
        inventoryIsOpen = true;
    }

    void HideInventory()
    {
        UI_Menager_Inventory.Instance.HideInventoryUI();
        Cursor.lockState = CursorLockMode.Locked;
        inventoryIsOpen = false;

        if (otherInventory)
            CloseInventory(otherInventory);

        otherInventory = null;
    }

    void CloseInventory(Inventory inventory) 
    {
        UI_Menager_Inventory.Instance.CloseInvntory(inventory);
        if (otherInvnetoryCoroutine != null)
        {
            StopCoroutine(otherInvnetoryCoroutine);
            otherInvnetoryCoroutine = null;
        }

        
    }

    IEnumerator OpenOtherInventory() 
    {
        if (!inventoryIsOpen)
            OpenInventory();

        UI_Menager_Inventory.Instance.OpenInventory(otherInventory, otherInventoryPosition);

        while (otherInventory && Vector3.Distance(transform.position, otherInventory.transform.position) < interactRange * 1.5f) 
        {

            yield return new WaitForSeconds(1f);
        }



        if (otherInventory) 
        {
            CloseInventory(otherInventory);
            otherInventory = null;
        }
    }

    #endregion

    void Interact()
    {
        Ray ray = new Ray(_camera.transform.position, _camera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactLayer))
        {
            if (hit.transform.TryGetComponent(out ItemWorld itemWorld))
            {
                for (int i = 0; i < inventoryList.Count; ++i)
                {
                    if (itemWorld.AllGood())
                        Inventory.TransferItemFromTo(itemWorld, inventoryList[i], itemWorld.Count);
                }
            }
            else if(hit.transform.TryGetComponent(out Inventory inventory))
            {

                if (otherInvnetoryCoroutine != null)// hide current inventory 
                {
                    if(otherInventory == inventory) // hide current inventory if you try again open same inventory
                    HideInventory();
                    else // hide current inventory and open new one 
                    {
                        //   HideInventory();
                        if(otherInventory)
                        CloseInventory(otherInventory);

                        otherInventory = inventory;
                        otherInvnetoryCoroutine = StartCoroutine(OpenOtherInventory());
                    }
                }
                else // open new inventory
                {
                    otherInventory = inventory;
                    otherInvnetoryCoroutine = StartCoroutine(OpenOtherInventory());
                }
            }
            
        }
        else // hide current invntory if raycast hit nothing
        {
            if (otherInventory)
                CloseInventory(otherInventory);
        }
    }

    #region Character Contorl
    void Move() 
    {
        
        Vector3 finalMove  = transform.right * moveDirection.x;
        finalMove += transform.forward * moveDirection.z;
        finalMove = speed * finalMove.normalized;

        _rigidbody.velocity = new Vector3(finalMove.x, _rigidbody.velocity.y, finalMove.z);
        

        /*
        float velMagnitude = _rigidbody.velocity.magnitude;
        if (velMagnitude < speed)
        {
            float currentSpeed = speed - velMagnitude;
            currentSpeed = Mathf.Clamp(currentSpeed, 0, speed);

            Vector3 finalMove = transform.right * moveDirection.x;
            finalMove += transform.forward * moveDirection.z;
            finalMove = currentSpeed * finalMove.normalized;


            _rigidbody.AddForce(finalMove, ForceMode.Impulse);
        }
        */

    }

    void Rotate() 
    {
        transform.Rotate(0, xRotation, 0);

        _camera.transform.localRotation = Quaternion.Euler(yRotation, 0, 0);
    }
    #endregion





   
}
