using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class EnemyResourceBillboard : MonoBehaviour
{
   public Transform mainCamera;
   
   void LateUpdate()
    {
        transform.LookAt(transform.position + mainCamera.forward);
    }
}
