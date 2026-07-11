// this code has reference to that script file code
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSmoothTime = 0.1f;
    public Animator animator;
    public bool canMove = true;
    public bool isAutoMoving = false;
    public Transform autoMoveTarget;

    private NPCController currentNPC;
    private float currentVelocity;

    private void Update()
    {
        if (isAutoMoving && autoMoveTarget != null)
        {
            ExecuteAutoMovement();
            return;
        }

        if (!canMove)
        {
            animator.SetBool("isWalking", false);
            return;
        }

        ExecuteManualMovement();
        CheckInteractionInput();
    }

    private void ExecuteManualMovement()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        Vector3 inputDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        if (inputDirection.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;
            float smoothedAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref currentVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, smoothedAngle, 0f);
            transform.Translate(inputDirection * moveSpeed * Time.deltaTime, Space.World);
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }
    }

    private void CheckInteractionInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && currentNPC != null)
        {
            currentNPC.InitiateDialog();
        }
    }

    public void TriggerAutoMovement(Transform targetTransform)
    {
        canMove = false;
        isAutoMoving = true;
        autoMoveTarget = targetTransform;
    }

    private void ExecuteAutoMovement()
    {
        Vector3 directionToTarget = (autoMoveTarget.position - transform.position).normalized;
        directionToTarget.y = 0;
        float distanceToTarget = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(autoMoveTarget.position.x, 0, autoMoveTarget.position.z));

        if (distanceToTarget > 0.1f)
        {
            float targetAngle = Mathf.Atan2(directionToTarget.x, directionToTarget.z) * Mathf.Rad2Deg;
            float smoothedAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref currentVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, smoothedAngle, 0f);
            transform.Translate(directionToTarget * moveSpeed * Time.deltaTime, Space.World);
            animator.SetBool("isWalking", true);
        }
        else
        {
            isAutoMoving = false;
            animator.SetBool("isWalking", false);
            canMove = true;
        }
    }

    private void OnTriggerEnter(Collider otherCollider)
    {
        if (otherCollider.CompareTag("NPC"))
        {
            currentNPC = otherCollider.GetComponent<NPCController>();
            if (currentNPC != null)
            {
                currentNPC.SetPromptVisibility(true);
            }
        }
    }

    private void OnTriggerExit(Collider otherCollider)
    {
        if (otherCollider.CompareTag("NPC"))
        {
            if (currentNPC != null)
            {
                currentNPC.SetPromptVisibility(false);
                currentNPC = null;
            }
        }
    }

    public void ExecuteHappyAnimation()
    {
        animator.SetTrigger("happyTrigger");
    }
}