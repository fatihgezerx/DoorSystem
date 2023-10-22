using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoorSystem
{
    public class Player : MonoBehaviour
    {
        #region Variables
        [Header("Movement Parameters")]
        [SerializeField] private float walkSpeed = 3.0f;
        [SerializeField] private float gravity = 30.0f;
        private Vector3 moveDirection;
        private Vector2 currentInput;

        [Header("Look Parameters")]
        [SerializeField, Range(1, 10)] private float lookSpeedX = 2.0f;
        [SerializeField, Range(1, 10)] private float lookSpeedY = 2.0f;
        [SerializeField, Range(1, 180)] private float upperLookLimit = 80.0f;
        [SerializeField, Range(1, 180)] private float lowerLookLimit = 80.0f;
        private float rotationX = 0;

        [Header("Interaction")]
        [SerializeField] private Vector3 interactionRayPoint = default;
        [SerializeField] private float interactionDistance = default;
        [SerializeField] private LayerMask interactionLayer = default;
        [SerializeField] private KeyCode interactionKey = KeyCode.E;
        [SerializeField] private KeyCode alternateKey = KeyCode.Q;
        private Interactable currentInteractable;
        public RaycastHit raycastHit;

        private Camera playerCamera;
        private CharacterController characterController;
        #endregion

        #region BasicMethods
        private void Start()
        {
            characterController = GetComponent<CharacterController>();
            playerCamera = GetComponentInChildren<Camera>();

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            HandleMovementInput();
            HandleMouseLook();
            ApplyFinalMovements();

            HandleInteractionCheck();
            HandleInteractionInput();
            HandleAlternateInput();
        }
        #endregion

        #region BasicMovements
        private void HandleMovementInput()
        {
            currentInput = new Vector2(walkSpeed * Input.GetAxis("Vertical"), walkSpeed * Input.GetAxis("Horizontal"));
            float moveDirectionY = moveDirection.y;
            moveDirection = (transform.TransformDirection(Vector3.forward) * currentInput.x) + (transform.TransformDirection(Vector3.right) * currentInput.y);
            moveDirection = moveDirection.normalized * Mathf.Clamp(moveDirection.magnitude, 0, walkSpeed);
            moveDirection.y = moveDirectionY;
        }

        private void HandleMouseLook()
        {
            rotationX -= Input.GetAxis("Mouse Y") * lookSpeedY;
            rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeedX, 0);
        }

        private void ApplyFinalMovements()
        {
            if (!characterController.isGrounded)
                moveDirection.y -= gravity * Time.deltaTime;
            characterController.Move(moveDirection * Time.deltaTime);
        }
        #endregion

        #region Interaction
        private void HandleInteractionCheck()
        {
            if (Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out RaycastHit hit, interactionDistance) && hit.collider.gameObject.layer == 6)
            {
                if (hit.collider.gameObject.layer == 6 && (currentInteractable == null || hit.collider.gameObject.GetInstanceID() != currentInteractable.gameObject.GetInstanceID()))
                {
                    hit.collider.TryGetComponent(out currentInteractable);

                    if (currentInteractable)
                        currentInteractable.OnFocus();
                }
            }

            else if (currentInteractable)
            {
                currentInteractable.OnLoseFocus();
                currentInteractable = null;
            }
        }

        private void HandleInteractionInput()
        {
            if (Input.GetKeyDown(interactionKey) && currentInteractable != null && Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out raycastHit, interactionDistance, interactionLayer))
            {
                currentInteractable.OnInteract();
            }
        }

        private void HandleAlternateInput()
        {
            if (Input.GetKeyDown(alternateKey) && currentInteractable != null && Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out raycastHit, interactionDistance, interactionLayer))
            {
                currentInteractable.OnAlternateInteract();
            }
        }
        #endregion
    }
}