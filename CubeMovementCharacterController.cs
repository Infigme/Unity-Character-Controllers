using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class CubeController : MonoBehaviour
{
    private Rigidbody rb;
    private bool m_isRolling;

    [Header("Settings")]
    [SerializeField]private float rollDuration = 0.25f;
    [SerializeField]private float cubeSize = 1f;

    private void Awake(){
        rb = GetComponentInParent<Rigidbody>() ?? GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
    }//awake

    private void Update(){
        if (m_isRolling) return;

        // Grounding safety check using dynamic bounds
        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, cubeSize * 0.6f);
        if (rb != null) rb.isKinematic = isGrounded;

        if (!isGrounded) return;

        // Explicitly check your singleton input handler instance
        if (InputHandler.instance == null) return;
        
        Vector2 input = InputHandler.instance.GetMovementInput();
        if (input == Vector2.zero) return;

        // Map the 2D input space directly to 3D world space coordinates
        Vector3 direction = Vector3.zero;
        if (input.y > 0) direction = Vector3.forward;
        else if (input.y < 0) direction = Vector3.back;
        else if (input.x > 0) direction = Vector3.right;
        else if (input.x < 0) direction = Vector3.left;

        // Obstacle checking before execution
        if (direction != Vector3.zero && !Physics.Raycast(transform.position, direction, cubeSize * 0.6f))StartCoroutine(RollCube(direction));
    }//update

    private IEnumerator RollCube(Vector3 direction){
        m_isRolling = true;

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        // Define exact physical pivot edge and rotation variables 
        Vector3 pivot = startPosition + (Vector3.down * (cubeSize * 0.5f)) + (direction * (cubeSize * 0.5f));
        Vector3 axis = Vector3.Cross(Vector3.up, direction);

        Vector3 targetPosition = startPosition + (direction * cubeSize);
        Quaternion targetRotation = Quaternion.AngleAxis(90f, axis) * startRotation;

        float elapsed = 0f;
        while (elapsed < rollDuration){
            elapsed += Time.deltaTime;
            float percent = Mathf.Clamp01(elapsed / rollDuration);
            
            // Clean mathematical rotation step around pivot using smooth stepping evaluation
            float smoothPercent = Mathf.SmoothStep(0f, 1f, percent);
            
            // Explicitly calculate the absolute frame rotation instead of compounding deltas
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, smoothPercent);
            
            // Derive the position based on the rotation arc relative to the pivot point
            transform.position = pivot + (Quaternion.Slerp(Quaternion.identity, Quaternion.AngleAxis(90f, axis), smoothPercent) * (startPosition - pivot));

            yield return null;
        }

        // Hard lock variables to pristine values at execution termination
        transform.SetPositionAndRotation(targetPosition, targetRotation);
        m_isRolling = false;
    }//rollcube
}//class
