using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class BillBoard : MonoBehaviour
{
   public Transform cam;
   
   void LateUpdate()
    {
        transform.LookAt(transform.position + cam.forward);
    }
}
