using UnityEngine;
using System.Collections;

public class CubeController : MonoBehaviour
{
    private InputHandler input;
    private Rigidbody rb;

    private float m_vertical, m_horizontal;
    public float rollDuration = 0.5f; 

    private bool m_isRolling = false;
    private bool m_grounded;
    
    public bool canTumble = true;

    private void Awake(){
        input = FindObjectOfType<InputHandler>();
        rb = GetComponentInParent<Rigidbody>();
        rb.isKinematic = true;
    }//awake

    private void Update(){
        if(m_isRolling)return;

        m_grounded = Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1f);

        m_vertical = input.move.ReadValue<Vector2>().y;
        m_horizontal = input.move.ReadValue<Vector2>().x;

        if(m_grounded){
            if(canTumble){
                if(m_vertical > 0)StartCoroutine(Roll(Vector3.forward));
                else if(m_vertical < 0)StartCoroutine(Roll(Vector3.back));
                else if(m_horizontal > 0)StartCoroutine(Roll(Vector3.right));
                else if(m_horizontal < 0)StartCoroutine(Roll(Vector3.left));
            }
            rb.isKinematic = true;
        }else rb.isKinematic = false;
    }//update

    private IEnumerator Roll(Vector3 _direction){
        if(Physics.Raycast(transform.position, _direction, out RaycastHit hit, 1f)){
            if(hit.collider.CompareTag("Obstacle"))yield break;
        }
        m_isRolling = true;
        float _elapsed = 0;
        
        //Define the anchor and axis
        //Anchor is the edge of the cube in the direction of movement
        Vector3 _anchor = transform.position + (Vector3.down * 0.5f) + (_direction * 0.5f);
        Vector3 _axis = Vector3.Cross(Vector3.up, _direction);

        //Perform the rotation
        Quaternion _startRotation = transform.rotation;
        Vector3 _startPosition = transform.position;

        while (_elapsed < rollDuration){
            _elapsed += Time.deltaTime;
            float m_percent = Mathf.Min(_elapsed / rollDuration, 1f);
            
            //Rotate 90 degrees total. 
            transform.RotateAround(_anchor, _axis, (90f / rollDuration) * Time.deltaTime);
            yield return null;
        }

        //After the loop, the cube might be at 89.9 or 90.1 degrees.
        //Force it to the exact target position and 90-degree rotation.
        SnapToFinalPosition(_direction);

        m_isRolling = false;
    }//roll

    private void SnapToFinalPosition(Vector3 _direction){
        //Calculate where the cube SHOULD be after a 90-degree roll
        //It moves 1 unit in the _direction and stays at the same height (if level)
        Vector3 _targetPos = new Vector3(
            Mathf.Round(transform.position.x),
            0.5f,
            Mathf.Round(transform.position.z)
        );
        
        transform.position = _targetPos;

        //Snap rotation to the nearest 90 degrees on all axes
        Vector3 _angles = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(
            Mathf.Round(_angles.x / 90) * 90,
            Mathf.Round(_angles.y / 90) * 90,
            Mathf.Round(_angles.z / 90) * 90
        );
    }//snap to final position
}//class
