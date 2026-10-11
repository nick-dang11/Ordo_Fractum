using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class EnemyHealthUI : MonoBehaviour
{
   public Transform cam;
   
   void LateUpdate()
    {
        transform.LookAt(transform.position + cam.forward);
    }
}
