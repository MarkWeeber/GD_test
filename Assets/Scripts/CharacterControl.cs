using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CharacterControl : MonoBehaviour
{
    [SerializeField] private float walkingSpeed = 5f;
    [SerializeField] private float runningSpeed = 15f;
    [Tooltip(tooltip:"Lap count, if 0 or less then it will be infinite")]
    [SerializeField] private int lapCount = 1;
    [SerializeField] Transform waypointsParent;
    [SerializeField] private float turnSpeed = 12f;
    [SerializeField] private LayerMask waypointLayerMask;

    private CharacterController _charController;
    private Animator _animator;
    private Waypoint[] _waypoints;
    private Waypoint _nextWaypoint;
    private int _currentWaypointIndex = 0;
    private int _waypointsSize;
    private int _remainingLapCount;
    private Vector3 _currentMovement = Vector3.zero;
    private float _currentSpeed;

    private void Start()
    {
        _charController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _waypoints = waypointsParent.GetComponentsInChildren<Waypoint>();
        _waypointsSize = _waypoints.Length;
        if (_waypointsSize > 0)
        {
            _nextWaypoint = _waypoints[_currentWaypointIndex];
            if (_waypoints[_waypointsSize - 1].Running)
            {
                _currentSpeed = runningSpeed;
            }
            else
            {
                _currentSpeed = walkingSpeed;
            }
        }
        _remainingLapCount = (lapCount>0)? lapCount : 1;
    }

    
    private void Update()
    {
        if (_waypointsSize > 0 && _remainingLapCount > 0)
        {
            MoveTowardsWaypoint();
            RotateTowardsWaypoint();
        }
        SetAnimatorParameters();
    }

    private void MoveTowardsWaypoint()
    {
        _currentMovement = (_nextWaypoint.transform.position - this.transform.position).normalized;
        _charController.Move(_currentMovement * _currentSpeed  * Time.deltaTime);
    }

    private void StopMovement()
    {
        _charController.Move(Vector3.zero);
    }

    private void SetAnimatorParameters()
    {
        _animator.SetFloat("Speed", _charController.velocity.magnitude);
    }

    private void RotateTowardsWaypoint()
    {
        Vector3 __direction = _currentMovement;
        __direction.y = 0f;
        Quaternion __lookRotation = Quaternion.LookRotation(__direction);
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, __lookRotation, turnSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((waypointLayerMask.value & (1 << other.transform.gameObject.layer)) > 0)
        {
            if (other.transform.GetComponent<Waypoint>() == _nextWaypoint)
            {
                if(_nextWaypoint.Running)
                {
                    _currentSpeed = runningSpeed;
                }
                else
                {
                    _currentSpeed = walkingSpeed;
                }
                if (_currentWaypointIndex == _waypointsSize - 1) // lap finished
                {
                    _currentWaypointIndex = 0;
                    _remainingLapCount = (lapCount > 0) ? _remainingLapCount - 1 : 1;
                    if (_remainingLapCount <= 0)
                    {
                        StopMovement();
                    }
                }
                else
                {
                    _currentWaypointIndex++;
                }
                _nextWaypoint = _waypoints[_currentWaypointIndex];
            }
        }
    }
}
