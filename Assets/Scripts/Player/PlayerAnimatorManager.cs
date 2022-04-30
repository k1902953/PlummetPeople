using UnityEngine;
using System.Collections;
using Photon.Pun;

public class PlayerAnimatorManager : MonoBehaviourPun
{
    private Animator animator;
    public GameObject playerRoot;
    private CharacterController controller;
    private PlayerMovement movement;

    #region MonoBehaviour Callbacks

    // Use this for initialization
    void Start()
    {
        animator = GetComponent<Animator>();
        if (!animator) Debug.LogError("PlayerAnimatorManager is Missing Animator Component", this);

        controller = playerRoot.GetComponent<CharacterController>();
        if (!controller) Debug.LogError("PlayerAnimatorManager Parent is Missing Character Controller", this);

        movement = playerRoot.GetComponent<PlayerMovement>();
        if (!movement) Debug.LogError("PlayerAnimatorManager Parent is Missing PlayerMovement", this);
    }

    private void FixedUpdate()
    {
        UpdateAnimator();
    }

    #endregion

    void UpdateAnimator()
    {
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true) return;
        if (!animator) return;

        Vector3 controllerVelocityXZ = new Vector3(controller.velocity.x, 0, controller.velocity.z);
        float v = controllerVelocityXZ.magnitude;
        if (animator) animator.SetFloat("speed", v);
    }

    public IEnumerator SetAnimTrigger(string _name)
    {
        animator.SetTrigger(_name);
        yield return new WaitForSeconds(0.1f);
        animator.ResetTrigger(_name);
    }

    public void PlayAnim(string _name)
    {
        if (animator) animator.Play(_name);
    }

    public void SetAnimBool(string _name, bool _set)
    {
        if (animator) animator.SetBool(_name, _set);
    }
}
