using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using Sirenix.OdinInspector;

namespace Objects
{
    [RequireComponent(typeof(Animator))] // Requires animator with parameter "flySpeed" for idle/flap states
    [RequireComponent(typeof(Rigidbody))] // Requires Rigidbody for movement
    public class PhoenixFlyController : UdonSharpBehaviour
    {
        [TitleGroup("Movement Settings")]
        [Tooltip("Base speed when in idle state")]
        [SerializeField] private float idleSpeed = 1.0f;
        
        [Tooltip("How quickly the phoenix can rotate")]
        [SerializeField] private float turnSpeed = 120.0f;
        
        [Tooltip("How long it takes to transition between states (seconds)")]
        [SerializeField] private float switchSeconds = 2.0f;
        
        [Tooltip("Probability of entering idle state (0-1)")]
        [SerializeField] private float idleRatio = 0.3f;

        [TitleGroup("Animation & Speed")]
        [Tooltip("Min and max animation speeds")]
        [SerializeField] private Vector2 animSpeedMinMax = new Vector2(0.5f, 2.0f);
        
        [Tooltip("Movement speeds corresponding to min/max animation")]
        [SerializeField] private Vector2 moveSpeedMinMax = new Vector2(2.0f, 8.0f);
        
        [Tooltip("How often to change animation (seconds)")]
        [SerializeField] private Vector2 changeAnimEveryFromTo = new Vector2(3.0f, 8.0f);
        
        [Tooltip("How often to change direction (seconds)")]
        [SerializeField] private Vector2 changeTargetEveryFromTo = new Vector2(2.0f, 6.0f);

        [TitleGroup("Targeting")]
        [Tooltip("Base position to return to when returnToBase is true")]
        [SerializeField] private Transform homeTarget;
        
        [Tooltip("Center point to fly around")]
        [SerializeField] private Transform flyingTarget;
        
        [Tooltip("Min and max distance from flying center")]
        [SerializeField] private Vector2 radiusMinMax = new Vector2(5.0f, 20.0f);
        
        [Tooltip("Min and max flying height")]
        [SerializeField] private Vector2 yMinMax = new Vector2(1.0f, 30.0f);
        
        [Tooltip("Whether the phoenix should return to home position")]
        [SerializeField] public bool returnToBase = false;
        
        [Tooltip("Random offset applied to home position")]
        [SerializeField] public float randomBaseOffset = 5.0f;
        
        [Tooltip("Delay before starting movement")]
        [SerializeField] public float delayStart = 0.0f;

        [TitleGroup("Collision Avoidance", "Settings to prevent getting stuck")]
        [Tooltip("Layers to avoid colliding with")]
        [SerializeField] private LayerMask collisionLayers = Physics.DefaultRaycastLayers;
        
        [Tooltip("How far to check for obstacles")]
        [SerializeField] private float obstacleDetectionDistance = 5.0f;
        
        [Tooltip("Upward boost force when stuck")]
        [SerializeField] private float unstuckForce = 10.0f;
        
        [Tooltip("How long to wait before applying unstuck forces")]
        [SerializeField] private float stuckDetectionTime = 0.5f;
        
        [Tooltip("Extra height buffer from ground")]
        [SerializeField] private float groundAvoidanceHeight = 2.0f;

        // Non-serialized runtime variables
        [NonSerialized] public float changeTarget = 0f, changeAnim = 0f;
        [NonSerialized] public float timeSinceTarget = 0f, timeSinceAnim = 0f;
        [NonSerialized] public float prevAnim = 0f, currentAnim = 0f;
        [NonSerialized] public float prevSpeed = 0f, speed = 0f;
        [NonSerialized] public float zturn = 0f, prevz = 0f;
        [NonSerialized] public float distanceFromBase = 0f, distanceFromTarget = 0f;
        
        // Private state variables
        private Animator animator;
        private Rigidbody body;
        private float turnSpeedBackup;
        private Vector3 rotateTarget;
        private Vector3 direction;
        private Vector3 randomizedBase;
        private Quaternion lookRotation;
        private Vector3 lastPosition;
        private float stuckTime = 0f;
        private bool wasStuck = false;

        void Start()
        {
            // Initialize components
            animator = GetComponent<Animator>();
            body = GetComponent<Rigidbody>();
            
            // Configure rigidbody for better flight
            body.useGravity = false;
            body.drag = 0.5f;
            body.angularDrag = 2.0f;
            
            // Set initial values
            turnSpeedBackup = turnSpeed;
            direction = transform.forward;
            lastPosition = transform.position;
            
            // Set initial timers for animation and direction changes
            changeAnim = UnityEngine.Random.Range(changeAnimEveryFromTo.x, changeAnimEveryFromTo.y);
            changeTarget = UnityEngine.Random.Range(changeTargetEveryFromTo.x, changeTargetEveryFromTo.y);
            
            // Apply initial velocity if not delayed
            if (delayStart <= 0f) 
                body.velocity = idleSpeed * direction;
        }

        void FixedUpdate()
        {
            // Wait if start should be delayed
            if (delayStart > 0f)
            {
                delayStart -= Time.fixedDeltaTime;
                return;
            }

            // Calculate distances
            UpdateDistances();
            
            // Handle stuck detection and recovery
            DetectAndHandleStuckState();

            // Update timers and behavior states
            UpdateTimers();

            // Handle rotation
            UpdateRotation();

            // Apply movement
            UpdateMovement();
            
            // Enforce height limits
            EnforceHeightLimits();
            
            // Store position for next frame
            lastPosition = transform.position;
        }

        private void UpdateDistances()
        {
            if (homeTarget)
            {
                randomizedBase = homeTarget.position;
                randomizedBase.y += UnityEngine.Random.Range(-randomBaseOffset, randomBaseOffset);
                distanceFromBase = Vector3.Distance(randomizedBase, body.position);
                
                // Allow drastic turns close to base to ensure target can be reached
                if (returnToBase && distanceFromBase < 10f)
                {
                    if (!Mathf.Approximately(turnSpeed, 300f) && body.velocity.magnitude != 0f)
                    {
                        turnSpeedBackup = turnSpeed;
                        turnSpeed = 300f;
                    }
                    else if (distanceFromBase <= 2f)
                    {
                        body.velocity = Vector3.zero;
                        turnSpeed = turnSpeedBackup;
                        return;
                    }
                }
            }
            
            if (flyingTarget)
            {
                distanceFromTarget = Vector3.Distance(flyingTarget.position, body.position);
            }
        }

        private void DetectAndHandleStuckState()
        {
            // Check if we're barely moving
            float movementMagnitude = Vector3.Distance(transform.position, lastPosition);
            
            if (movementMagnitude < 0.05f && currentAnim > 0.1f)
            {
                stuckTime += Time.fixedDeltaTime;
                
                // If stuck for enough time, try to escape
                if (stuckTime > stuckDetectionTime)
                {
                    wasStuck = true;
                    AttemptToEscape();
                }
            }
            else
            {
                stuckTime = 0f;
                
                // If we were stuck but now moving, reset stuck state
                if (wasStuck && movementMagnitude > 0.2f)
                {
                    wasStuck = false;
                }
            }
            
            // Ground avoidance - check if too close to ground
            if (Physics.Raycast(transform.position, Vector3.down, groundAvoidanceHeight, collisionLayers))
            {
                // Apply small upward force and bias direction upward
                body.AddForce(Vector3.up * (unstuckForce * 0.5f), ForceMode.Impulse);
                rotateTarget = Vector3.Lerp(rotateTarget, Vector3.up, 0.5f).normalized;
                changeTarget = 0.5f; // Force a direction update soon
            }
        }
        
        private void AttemptToEscape()
        {
            // Apply stronger upward force
            body.AddForce(Vector3.up * unstuckForce, ForceMode.Impulse);
            
            // Cast rays in multiple directions to find clear path
            Vector3[] directions = new Vector3[]
            {
                Vector3.up,
                Vector3.up + transform.forward,
                Vector3.up - transform.forward,
                Vector3.up + transform.right,
                Vector3.up - transform.right,
                transform.forward + transform.right,
                transform.forward - transform.right
            };
            
            foreach (Vector3 dir in directions)
            {
                // Check if direction is clear
                if (!Physics.Raycast(transform.position, dir.normalized, obstacleDetectionDistance, collisionLayers))
                {
                    // Found a clear direction, use it
                    rotateTarget = dir.normalized;
                    changeTarget = 0.1f; // Force a new direction soon
                    return;
                }
            }
            
            // No clear path found, default to up
            rotateTarget = Vector3.up;
            changeTarget = 0.1f;
        }

        private void UpdateTimers()
        {
            // Time for a new animation speed
            if (changeAnim < 0f)
            {
                prevAnim = currentAnim;
                currentAnim = ChangeAnim(currentAnim);
                changeAnim = UnityEngine.Random.Range(changeAnimEveryFromTo.x, changeAnimEveryFromTo.y);
                timeSinceAnim = 0f;
                prevSpeed = speed;
                
                // Set speed based on animation
                if (currentAnim == 0) 
                    speed = idleSpeed;
                else 
                    speed = Mathf.Lerp(moveSpeedMinMax.x, moveSpeedMinMax.y, 
                        (currentAnim - animSpeedMinMax.x) / (animSpeedMinMax.y - animSpeedMinMax.x));
            }
            
            // Time for a new target direction
            if (changeTarget < 0f || wasStuck)
            {
                rotateTarget = ChangeDirection(body.transform.position);
                
                if (returnToBase) 
                    changeTarget = 0.2f; // More frequent updates when returning to base
                else 
                    changeTarget = UnityEngine.Random.Range(changeTargetEveryFromTo.x, changeTargetEveryFromTo.y);
                
                timeSinceTarget = 0f;
            }
            
            // Update times
            changeAnim -= Time.fixedDeltaTime;
            changeTarget -= Time.fixedDeltaTime;
            timeSinceTarget += Time.fixedDeltaTime;
            timeSinceAnim += Time.fixedDeltaTime;
        }

        private void UpdateRotation()
        {
            // Check if approaching obstacles
            CheckForObstaclesAhead();
            
            // Calculate tilt based on turn direction
            zturn = Mathf.Clamp(Vector3.SignedAngle(rotateTarget, direction, Vector3.up), -45f, 45f);
            
            // Rotate towards target
            if (rotateTarget != Vector3.zero) 
                lookRotation = Quaternion.LookRotation(rotateTarget, Vector3.up);
            
            Vector3 rotation = Quaternion.RotateTowards(body.transform.rotation, 
                lookRotation, turnSpeed * Time.fixedDeltaTime).eulerAngles;
            body.transform.eulerAngles = rotation;
            
            // Apply z-axis tilt for banking effect
            float tempZ = prevz;
            if (prevz < zturn) 
                prevz += Mathf.Min(turnSpeed * Time.fixedDeltaTime, zturn - prevz);
            else if (prevz >= zturn) 
                prevz -= Mathf.Min(turnSpeed * Time.fixedDeltaTime, prevz - zturn);
            
            // Clamp tilt
            prevz = Mathf.Clamp(prevz, -45f, 45f);
            body.transform.Rotate(0f, 0f, prevz - tempZ, Space.Self);
            
            // Update direction
            direction = body.transform.forward;
        }
        
        private void CheckForObstaclesAhead()
        {
            // Cast a ray forward to detect obstacles
            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, 
                obstacleDetectionDistance, collisionLayers))
            {
                // Calculate reflection direction to avoid obstacle
                Vector3 avoidDirection = Vector3.Reflect(direction, hit.normal).normalized;
                
                // Add some upward bias
                avoidDirection += Vector3.up * 0.5f;
                avoidDirection.Normalize();
                
                // Update target
                rotateTarget = avoidDirection;
                changeTarget = 0.5f;
            }
        }

        private void UpdateMovement()
        {
            // Apply velocity based on current state
            if (returnToBase && distanceFromBase < idleSpeed)
            {
                // Slow down when approaching home
                body.velocity = Mathf.Min(idleSpeed, distanceFromBase) * direction;
            }
            else
            {
                // Smooth transition between animation states
                float targetSpeed = Mathf.Lerp(prevSpeed, speed, 
                    Mathf.Clamp01(timeSinceAnim / switchSeconds));
                
                body.velocity = targetSpeed * direction;
            }
        }
        
        private void EnforceHeightLimits()
        {
            // Get current height
            float currentHeight = transform.position.y;
            
            // Adjust direction when approaching height limits
            if (currentHeight < yMinMax.x + 5f)
            {
                rotateTarget = Vector3.Lerp(rotateTarget, Vector3.up, 0.5f);
                rotateTarget.Normalize();
            }
            else if (currentHeight > yMinMax.y - 5f)
            {
                rotateTarget = Vector3.Lerp(rotateTarget, Vector3.down, 0.5f);
                rotateTarget.Normalize();
            }
            
            // Hard limit if outside bounds
            if (currentHeight < yMinMax.x || currentHeight > yMinMax.y)
            {
                Vector3 position = transform.position;
                position.y = Mathf.Clamp(position.y, yMinMax.x, yMinMax.y);
                transform.position = position;
                
                // Apply additional force
                if (currentHeight < yMinMax.x)
                {
                    body.AddForce(Vector3.up * (unstuckForce * 0.5f), ForceMode.Impulse);
                }
            }
        }

        // Select a new animation speed randomly
        private float ChangeAnim(float curAnim)
        {
            float newState;
            
            // Chance to go idle based on idleRatio
            if (UnityEngine.Random.Range(0f, 1f) < idleRatio) 
                newState = 0f;
            else
                newState = UnityEngine.Random.Range(animSpeedMinMax.x, animSpeedMinMax.y);
            
            // Only update if changed
            if (!Mathf.Approximately(newState, curAnim))
            {
                animator.SetFloat("AnimationPar", newState);
                if (newState == 0) 
                    animator.speed = 1f;
                else 
                    animator.speed = newState;
            }
            
            return newState;
        }

        // Select a new direction to fly in
        private Vector3 ChangeDirection(Vector3 currentPosition)
        {
            Vector3 newDir;
            
            // Return to base
            if (returnToBase && homeTarget)
            {
                randomizedBase = homeTarget.position;
                randomizedBase.y += UnityEngine.Random.Range(-randomBaseOffset, randomBaseOffset);
                newDir = (randomizedBase - currentPosition).normalized;
            }
            // Too far from target, move back toward it
            else if (flyingTarget && distanceFromTarget > radiusMinMax.y)
            {
                newDir = (flyingTarget.position - currentPosition).normalized;
            }
            // Too close to target, move away
            else if (flyingTarget && distanceFromTarget < radiusMinMax.x)
            {
                newDir = (currentPosition - flyingTarget.position).normalized;
            }
            // Free roaming within bounds
            else
            {
                // Random horizontal direction
                float angleXZ = UnityEngine.Random.Range(-Mathf.PI, Mathf.PI);
                // Limited vertical angle
                float angleY = UnityEngine.Random.Range(-Mathf.PI / 36f, Mathf.PI / 36f);
                
                // Calculate direction
                newDir = Mathf.Sin(angleXZ) * Vector3.forward + 
                         Mathf.Cos(angleXZ) * Vector3.right + 
                         Mathf.Sin(angleY) * Vector3.up;
                
                // If we were stuck recently, add more upward tendency
                if (wasStuck)
                {
                    newDir += Vector3.up;
                    newDir.Normalize();
                }
            }
            
            return newDir.normalized;
        }

        // Public method to command return to base
        public void ReturnHome()
        {
            returnToBase = true;
            changeTarget = 0f; // Force immediate redirection
        }
        
        // Public method to set flying free
        public void FlyFree()
        {
            returnToBase = false;
            changeTarget = 0f; // Force immediate redirection
        }
    }
}