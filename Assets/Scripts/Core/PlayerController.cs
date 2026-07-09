// this code has reference to that script file code
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Animator animator;
    public bool canMove = true;
    public bool isAutoMoving = false;
    public Transform autoMoveTarget;
    private NPCController currentNPC;

    private void Update()
    {
        if (isAutoMoving && autoMoveTarget != null)
        {
            MoveTowardsTarget();
            return;
        }

        if (!canMove)
        {
            animator.SetBool("isWalking", false);
            return;
        }

        HandleMovement();
        HandleInteraction();
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }
    }

    private void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.Space) && currentNPC != null)
        {
            currentNPC.StartInteraction();
        }
    }

    public void SetAutoMove(Transform target)
    {
        canMove = false;
        isAutoMoving = true;
        autoMoveTarget = target;
    }

    private void MoveTowardsTarget()
    {
        Vector3 direction = (autoMoveTarget.position - transform.position).normalized;
        direction.y = 0;

        if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(autoMoveTarget.position.x, 0, autoMoveTarget.position.z)) > 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
            animator.SetBool("isWalking", true);
        }
        else
        {
            isAutoMoving = false;
            animator.SetBool("isWalking", false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            currentNPC = other.GetComponent<NPCController>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            currentNPC = null;
        }
    }

    public void PlayHappyAnimation()
    {
        animator.SetTrigger("happyTrigger");
    }
}