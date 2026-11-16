using UnityEngine;

public class EnemyMaterialDrop : MonoBehaviour
{
   [Header("References")]
   [SerializeField] private EnemyCombat enemyCombat;

   [Header("Material Drop")]
   [SerializeField] private GameObject whetstonePrefab;

   [SerializeField, Min(1)]
   private int dropAmount = 1;

   private bool hasDropped = false;

   private void Awake()
    {
        if(enemyCombat == null)
        {
          enemyCombat = GetComponent<EnemyCombat>();  
        }
        if(enemyCombat == null)
        {
          Debug.LogWarning("EnemyCombat reference is missing on " + gameObject.name);
        }
    }

    private void OnEnable()
  {
    if(enemyCombat != null)
    {
        enemyCombat.OnEnemyDied += DropWhetstone;
    }
  }

  private void OnDisable()
  {
    if(enemyCombat != null)
    {
        enemyCombat.OnEnemyDied -= DropWhetstone;
    }
  }

  private void DropWhetstone()
  {
    if (hasDropped)
    {
      return;
    }

    if(whetstonePrefab == null)
    {
      Debug.LogWarning("Whetstone prefab is missing");
      return;
    }

    hasDropped = true;

    for(int i = 0; i < dropAmount; i++)
    {
      Instantiate(whetstonePrefab, transform.position + Vector3.up, Quaternion.identity);
    }

    Debug.Log("Dropped " + dropAmount + " whetstone(s)");
  }


}
