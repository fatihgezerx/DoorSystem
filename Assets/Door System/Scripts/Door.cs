using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoorSystem
{
    public class Door : Interactable
    {
        #region Variables
        [Header("Basic Configs")]
        [SerializeField] private DoorType doorType;
        [SerializeField] private bool locked = true; //in the beginning, door should be lock or unlock
        [SerializeField] private bool removeKey = false; //if you set it to true, then when you lock/unlock, it will remove the key
        [SerializeField] private float speed = 1.0f; //door open/close speed
        public int doorIndex = -1;

        private enum DoorType
        {
            Rotating,
            Sliding
        }

        [Header("Rotation Configs")]
        [SerializeField] private float rotationAmount = 90.0f;
        [SerializeField] private float forwardDirection = 0.0f;

        [Header("Sliding Configs")]
        [SerializeField] private Vector3 slideDirection = Vector3.back;
        [SerializeField] private float slideAmount = 1.0f;

        [Header("Sound Configs")]
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip closeSound;
        [SerializeField] private AudioClip pushingSound;
        [SerializeField] private AudioClip unlockSound;
        [SerializeField] private AudioClip lockSound;
        [SerializeField] private AudioClip[] creakSound;

        private bool isOpen = false;
        private bool hasPlayedCreakSound = false;
        private bool hasPlayedCloseSound = false;
        private bool canInteract = true;

        private float volume = 1.0f;

        private Vector3 startRotation;
        private Vector3 startPosation;
        private Vector3 forward;

        private Coroutine animationCoroutine;
        private Transform player;
        private Image crosshair;
        private TextMeshProUGUI interactText;

        //private const string PushingDoor = "PushingDoor";
        //private Animator animator;
        #endregion

        #region StartMethod
        private void Awake()
        {
            player = GameObject.Find("Player").transform;
            crosshair = GameObject.Find("Crosshair").GetComponent<Image>();
            interactText = GameObject.Find("InteractText").GetComponent<TextMeshProUGUI>();

            //animator = GetComponent<Animator>();
        }

        private void Start()
        {
            startRotation = transform.rotation.eulerAngles;
            startPosation = transform.position;
            forward = transform.right;
        }
        #endregion

        #region Implementation
        public override void OnFocus()
        {
            Hide(crosshair.gameObject);
            Show(interactText.gameObject);
            interactText.text = "Open the door [E]";
        }

        public override void OnInteract()
        {
            if (locked)
            {
                PushTheDoor();
            }

            else
            {
                OpenOrClose();
            }
        }

        public override void OnAlternateInteract()
        {
            if (locked)
            {
                UnLock();
            }
            else
            {
                Lock();
            }
        }

        public override void OnLoseFocus()
        {
            Hide(interactText.gameObject);
            Show(crosshair.gameObject);
        }
        #endregion

        #region Methods
        private void OpenOrClose()
        {
            if (canInteract)
            {
                //animator.enabled = false;
                canInteract = false;

                if (isOpen)
                {
                    Close();
                    StartCoroutine(EnableInteractAfterDelay(1.0f));
                }

                else
                {
                    Open(player.transform.position);
                    StartCoroutine(EnableInteractAfterDelay(1.0f));
                }
            }
        }

        private void Open(Vector3 userPosition)
        {
            if (!isOpen)
            {
                if (animationCoroutine != null)
                {
                    StopCoroutine(animationCoroutine);
                }

                if (doorType == DoorType.Rotating)
                {
                    float dot = Vector3.Dot(forward, (userPosition - transform.position).normalized);
                    animationCoroutine = StartCoroutine(DoRotationOpen(dot));
                }

                else if (doorType == DoorType.Sliding)
                {
                    animationCoroutine = StartCoroutine(DoSlidingOpen());
                }
            }
        }

        private void Close()
        {
            if (isOpen)
            {
                if (animationCoroutine != null)
                {
                    StopCoroutine(animationCoroutine);
                }

                if (doorType == DoorType.Rotating)
                {
                    animationCoroutine = StartCoroutine(DoRotationClose());
                }

                else if (doorType == DoorType.Sliding)
                {
                    animationCoroutine = StartCoroutine(DoSlidingClose());
                }
            }
        }

        private void PushTheDoor()
        {
            if (canInteract)
            {
                canInteract = false;
                //animator.SetTrigger(PushingDoor);
                PlaySound(pushingSound, transform.position);
                StartCoroutine(EnableInteractAfterDelay(1.0f));
            }
        }

        public void Lock()
        {
            if (!isOpen && Inventory.keys[doorIndex] == true)
            {
                locked = true;
                //animator.enabled = true;

                PlaySound(lockSound, transform.position);

                if (removeKey)
                {
                    Inventory.keys[doorIndex] = false;
                }
            }
        }

        public void UnLock()
        {
            if (Inventory.keys[doorIndex] == true)
            {
                locked = false;
                //animator.enabled = false;

                PlaySound(unlockSound, transform.position);

                if (removeKey)
                {
                    Inventory.keys[doorIndex] = false;
                }
            }
        }
        #endregion

        #region Enumerators
        private IEnumerator DoRotationOpen(float forwardAmount)
        {
            Quaternion startRotation = transform.rotation;
            Quaternion endRotation;

            hasPlayedCloseSound = false;
            PlaySound(openSound, transform.position);

            if (forwardAmount >= forwardDirection)
            {
                endRotation = Quaternion.Euler(new Vector3(0, startRotation.y - rotationAmount, 0));
            }

            else
            {
                endRotation = Quaternion.Euler(new Vector3(0, startRotation.y + rotationAmount, 0));
            }

            isOpen = true;

            float time = 0;

            while (time < 1)
            {
                transform.rotation = Quaternion.Slerp(startRotation, endRotation, time);
                yield return null;
                time += Time.deltaTime * speed;

                if (!hasPlayedCreakSound && time >= 0.5f)
                {
                    PlaySound(creakSound, transform.position);
                    hasPlayedCreakSound = true;
                }
            }
        }

        private IEnumerator DoRotationClose()
        {
            Quaternion startRotation = transform.rotation;
            Quaternion endRotaion = Quaternion.Euler(this.startRotation);

            float time = 0;

            while (time < 1)
            {
                transform.rotation = Quaternion.Slerp(startRotation, endRotaion, time);
                yield return null;
                time += Time.deltaTime * speed;

                if (time >= 0.2f && hasPlayedCreakSound)
                {
                    PlaySound(creakSound, transform.position);
                    hasPlayedCreakSound = false;
                }

                if (time >= 0.8f && !hasPlayedCloseSound)
                {
                    PlaySound(closeSound, transform.position);
                    hasPlayedCloseSound = true;
                    isOpen = false;
                }
            }
        }

        private IEnumerator DoSlidingOpen()
        {
            Vector3 endPosition = startPosation + slideAmount * slideDirection;
            Vector3 startPosition = transform.position;

            PlaySound(openSound, transform.position);

            float time = 0;

            isOpen = true;

            while (time < 1)
            {
                transform.position = Vector3.Lerp(startPosition, endPosition, time);
                yield return null;
                time += Time.deltaTime * speed;

                if (!hasPlayedCreakSound && time >= 0.5f)
                {
                    PlaySound(creakSound, transform.position);
                    hasPlayedCreakSound = true;
                }
            }
        }

        private IEnumerator DoSlidingClose()
        {
            Vector3 endPosition = startPosation;
            Vector3 startPosition = transform.position;

            float time = 0;

            isOpen = false;

            while (time < 1)
            {
                transform.position = Vector3.Lerp(startPosition, endPosition, time);
                yield return null;
                time += Time.deltaTime * speed;

                if (time >= 0.2f && hasPlayedCreakSound)
                {
                    PlaySound(creakSound, transform.position);
                    hasPlayedCreakSound = false;
                }

                if (time >= 0.8f && !hasPlayedCloseSound)
                {
                    PlaySound(closeSound, transform.position);
                    hasPlayedCloseSound = true;
                }
            }

            PlaySound(closeSound, transform.position);
        }

        private IEnumerator EnableInteractAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            canInteract = true;
        }
        #endregion

        #region OtherMethods
        private void PlaySound(AudioClip audioClip, Vector3 position, float volumeMultiplier = 1f)
        {
            AudioSource.PlayClipAtPoint(audioClip, position, volumeMultiplier * volume);
        }

        private void PlaySound(AudioClip[] audioClipArray, Vector3 position, float volume = 1f)
        {
            PlaySound(audioClipArray[Random.Range(0, audioClipArray.Length)], position, volume);
        }

        private void Show(GameObject gameObject)
        {
            gameObject.SetActive(true);
        }

        private void Hide(GameObject gameObject)
        {
            gameObject.SetActive(false);
        }
        #endregion
    }
}