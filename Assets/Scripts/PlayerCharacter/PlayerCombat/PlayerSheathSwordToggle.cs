using UnityEngine;

public class PlayerSheathSwordToggle : MonoBehaviour
{
    [SerializeField] private GameObject activeSword;
    [SerializeField] private GameObject inactiveSword;

    private void Start()
    {
        if(activeSword != null)
        {
            activeSword.SetActive(false);
        }

        if (inactiveSword != null)
        {
            inactiveSword.SetActive(true);
        }
    }

    public void EquipSword()
    {
        //Debug.Log("EquipSword called from animator event!");
        if(inactiveSword != null && activeSword != null)
        {
            activeSword.SetActive(true);
            inactiveSword.SetActive(false);
        }
    }
    public void UnequipSword()
    {
        //Debug.Log("UnequipSword called from animator event!");

        if (inactiveSword != null && activeSword != null)
        {
            activeSword.SetActive(false);
            inactiveSword.SetActive(true);
        }
    }
}
