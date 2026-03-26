using UnityEngine;

public class ThirdPersonRigidbodyController : MonoBehaviour
{
    [SerializeField]private float moveSpeed = 0, walkSpeed = 5f, runSpeed = 10f;
    [SerializeField]private float rotationSpeed = 10f;
    [SerializeField]private bool isMoving = false;

    private Rigidbody rb;
    private Vector3 moveDirection;

    private void Start(){
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        moveSpeed = walkSpeed; 
    }//start

    private void Update(){
        float horizontal = InputHandler.instance.move.ReadValue<Vector2>().x;
        float vertical = InputHandler.instance.move.ReadValue<Vector2>().y;
        moveDirection = new Vector3(horizontal, 0f, vertical).normalized;

        Run();
    }//update

    private void FixedUpdate(){
        Move();
    }//fixed update

   private void Move(){
        if (moveDirection.magnitude >= 0.1f){
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

            Vector3 move = moveDirection * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(transform.position + move);
            isMoving = true;
        }else isMoving = false;
    }//move

    private void Run(){
        if(InputHandler.instance.run.ReadValue<float>() > 0.1f && isMoving)moveSpeed = runSpeed;
        else moveSpeed = walkSpeed;
    }//run
}//class