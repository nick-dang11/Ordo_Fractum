using UnityEngine;

public class PlayerBridge : MonoBehaviour
// PlayerBridge connects the visuals (where animator, and this script! is attached)
// to the parent with the rest of the player scripts in order to access important info - N 6/27
{
    public GameObject parentPlayer; // should just be named Player

    public void OnFootstep(AnimationEvent _animationEvent)
    {
        if(parentPlayer != null)
        {
            parentPlayer.SendMessage("OnFootstep", _animationEvent, SendMessageOptions.DontRequireReceiver);
            // communicates with parent, asking it if it has function "OnFootstep" that accepts an AnimationEvent
            // 3rd param: carry on without throwing error
        }
    }

    public void OnLand(AnimationEvent _animationEvent)
    {
        if (parentPlayer != null)
        {
            parentPlayer.SendMessage("OnLand", _animationEvent, SendMessageOptions.DontRequireReceiver);
        }
    }
}
