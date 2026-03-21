using UnityEngine;

public class TwoDimensionalCharacterController : MonoBehaviour
{
    private PlayerInputManager _playerInputManager;

    [Header("Movement Controllers")]
    public bool Movement = false;
    public bool Sprinting = false;
    
    [Header("Gravity")]
    public float gravity = 9.8f;
    public bool groundedPlayer;
    [SerializeField]private Transform _groundCheck;
    private float _checkRadius = 0.1f;
    public LayerMask layerMask;
    private Vector3 _verticalVelocity;
    private bool _canMove = true;

    [Header("Movement")]
    public float walkSpeed = 1f;
    public bool isMoving = false;
    private float _moveSpeed;
    private CharacterController _controller;
    private Vector3 _direction;
    private Vector2 _moveInput;

    [Header("Rotation")]
    [SerializeField]private float _turnSmoothTime = 0.15f;
    private float _turnSmoothVel = 0f;
    private Camera _mainCamera;

    [Header("Sprinting")]
    public float sprintSpeed = 4f;

    [Header("Dodging")]
    public bool isDodging = false;
    [SerializeField]private float _dodgeSpeed = 1f;
    [SerializeField]private float _dodgeDuration = 0.75f;

    private PlayerAnimation _playerAnimation;
    private PlayerAttack _playerAttack;
    private AudioManager _audioManager;

    private void Awake()
    {
        _playerInputManager = GetComponent<PlayerInputManager>();

        _playerAnimation = gameObject.AddComponent<PlayerAnimation>();

        _playerAttack = GetComponent<PlayerAttack>();

        _audioManager = FindObjectOfType<AudioManager>();
    }//awake

    private void Start()
    {
        _controller = GetComponent<CharacterController>();

        _mainCamera = Camera.main;

        _moveSpeed = walkSpeed;

    }//start

    private void Update()
    {   
        PlayerGravity();
        
        if(Movement == true)
        {
            MovePlayer();
        }

        if(Sprinting == true)
        {
            PlayerSprint();
        }

        Dodging();

        if(isDodging)
        {
            _moveSpeed += _dodgeSpeed;
        }

    }//update

    private void MovePlayer()
    {
        _moveInput = _playerInputManager.moveAction.ReadValue<Vector2>();
        _direction = new Vector3(_moveInput.x, 0, _moveInput.y).normalized;

        if(_direction.magnitude >= 0.1f && _canMove)
        {
            float _targetAngle = Mathf.Atan2(_direction.x, _direction.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
            float _angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetAngle, ref _turnSmoothVel, _turnSmoothTime);
            
            if(!_playerAttack.isAiming)
            {
                transform.rotation = Quaternion.Euler(0, _angle, 0);
            }

            Vector3 _moveDirection = Quaternion.Euler(0f, _targetAngle, 0f) * Vector3.forward;
            _controller.Move(_moveDirection.normalized * _moveSpeed * Time.deltaTime);

            if(!isDodging)
            {
                _audioManager.SetFootStepsVolume(0.05f, 0.06f);
                _playerAnimation.Move(1f); 
                
            }else{
                _audioManager.SetFootStepsVolume(0.08f, 0.12f);
            }

            isMoving = true;
        }
        else{
            _playerAnimation.Move(0f);
            isMoving = false;
        }


    }//move player

    private void PlayerSprint()
    {
        if(_playerInputManager.sprintAction.ReadValue<float>() > 0 && isMoving 
        && !isDodging && !_playerAttack.isAiming)
        {
            _audioManager.SetFootStepsVolume(0.1f, 0.11f);
            _moveSpeed = sprintSpeed;
            _playerAnimation.Sprint(true);

        }else{
            _playerAnimation.Sprint(false);
            _moveSpeed = walkSpeed;

        }
    }//PlayerSprint

    private void PlayerGravity()
    {
        groundedPlayer = Physics.CheckSphere(_groundCheck.position, _checkRadius, layerMask);
        if(groundedPlayer && _verticalVelocity.y < 0)
        {
            _verticalVelocity.y = 0f;
        }

        _verticalVelocity.y += -gravity * Time.deltaTime;
        _controller.Move(_verticalVelocity * Time.deltaTime);

        if(!groundedPlayer)
        {
            _playerAnimation.Fall(true);
        }else{
            _playerAnimation.Fall(false);
        }
    }//player gravity

    public void DisableMove()
    {
        if(_canMove)
        {
            _canMove = false;
        }
    }//disable move as you land

    public void EnableMove()
    {
        if(!_canMove){
            _canMove = true;
        }
    }//enable move after landing

    private void Dodging(){
        
        if(_playerInputManager.dodgeAction.triggered && isMoving && !isDodging && 
        groundedPlayer && !_playerAttack.isAiming && !_playerAttack.isReloading)
        {
            isDodging = true;
            if(_playerAttack.isEquipped)
            {
                _playerAnimation.Equip(1, 0);
                _playerAnimation.Equip(2, 0);
            }
            _playerAnimation.Dodge();
            
            Invoke("DodgeDisable", _dodgeDuration);
        }else return;

    }//dodging

    private void DodgeDisable()
    {
        _moveSpeed = walkSpeed;
        isDodging = false;
        if(_playerAttack.isEquipped)
        {
            if(_playerAttack.weaponState == WeaponState.Armed){
                _playerAnimation.Equip(1, 1);
            }else if(_playerAttack.weaponState == WeaponState.Unarmed)
            {
                _playerAnimation.Equip(1, 0);
            }
        }
        
    }//dodge disable

    public void PlayFootSteps()
    {
        if(isMoving)
        {
            _audioManager.playerAudio.clip = _audioManager.footStepClips[Random.Range(0, _audioManager.footStepClips.Length)];
            _audioManager.playerAudio.Play();
        }

    }//play footstep Audio

    public void PlayDodgeRoll()
    {
        _audioManager.playerAudio.clip = _audioManager.dodgeRollClip;
        _audioManager.playerAudio.Play();
    }//play dodge roll

}//class
