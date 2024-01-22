
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AnimatorSleeper : UdonSharpBehaviour
{
    int isActiveHash;

    int delayer;

    [SerializeField]Animator animator;

    void Start()
    {
        isActiveHash = Animator.StringToHash("isActive");
    }
    public void Update()
    {
        //Debug.Log("debug animator enabled" + animator.enabled);
        if (animator.GetFloat(isActiveHash) == 0 && animator.enabled)
        {
            //Debug.Log("debug_disabling");
            if (delayer > 10)
            {
                animator.enabled = false;
                delayer = 0;
            }
            delayer++;
        }
        else
        {
            delayer = 0;
        }
        this.enabled = false;
    }
}
