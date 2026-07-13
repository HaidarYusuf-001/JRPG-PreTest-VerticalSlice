using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float manualMoveSpeed = 6f;
    public float autoMoveSpeed = 2.5f;
    public float rotationSmoothTime = 0.1f;
    public Animator animator;

    public float encounterStepThreshold = 3f;
    public float encounterChance = 0.3f;

    private bool canMove = true;
    private bool isAutoMoving = false;
    private Transform autoMoveTarget;
    private NPCController currentNPC;
    private float currentVelocity;

    private float currentStepDistance = 0f;
    private EncounterArea currentEncounterArea;

    private void Update()
    {
        if (isAutoMoving && autoMoveTarget != null)
        {
            ExecuteAutoMovement();
            return;
        }

        if (!canMove)
        {
            if (animator != null)
            {
                animator.SetBool("isRunning", false);
                animator.SetBool("isWalking", false);
            }
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

            float distanceMoved = manualMoveSpeed * Time.deltaTime;
            transform.Translate(inputDirection * distanceMoved, Space.World);

            if (animator != null) animator.SetBool("isRunning", true);

            if (currentEncounterArea != null)
            {
                CalculateEncounterSteps(distanceMoved);
            }
        }
        else
        {
            if (animator != null) animator.SetBool("isRunning", false);
        }
    }

    private void CalculateEncounterSteps(float distanceMoved)
    {
        currentStepDistance += distanceMoved;
        if (currentStepDistance >= encounterStepThreshold)
        {
            currentStepDistance = 0f;
            if (UnityEngine.Random.value <= encounterChance)
            {
                UnitData randomEnemy = currentEncounterArea.GetRandomEnemy();
                if (randomEnemy != null)
                {
                    canMove = false;
                    if (animator != null) animator.SetBool("isRunning", false);
                    GameFlowManager.Instance.TriggerRandomEncounter(randomEnemy);
                }
            }
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
            transform.Translate(directionToTarget * autoMoveSpeed * Time.deltaTime, Space.World);

            if (animator != null) animator.SetBool("isWalking", true);
        }
        else
        {
            isAutoMoving = false;
            canMove = true;
            if (animator != null) animator.SetBool("isWalking", false);
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

        EncounterArea area = otherCollider.GetComponent<EncounterArea>();
        if (area != null)
        {
            currentEncounterArea = area;
            currentStepDistance = 0f;
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

        if (otherCollider.GetComponent<EncounterArea>() != null)
        {
            currentEncounterArea = null;
            currentStepDistance = 0f;
        }
    }

    public void ExecuteHappyAnimation()
    {
        if (animator != null) animator.SetTrigger("happyTrigger");
    }

    public void SetMovementState(bool state)
    {
        canMove = state;
    }

    public NPCController GetCurrentNPC()
    {
        return currentNPC;
    }
}