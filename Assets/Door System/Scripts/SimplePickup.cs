using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoorSystem
{
    public class SimplePickup : Interactable
    {
        #region Variables
        [Header("Key Configs")]
        [SerializeField] private int keyIndex = -1;
        //[SerializeField] private AudioClip pickupSound;

        private float volume = 1.0f;

        private Image crosshair;
        private TextMeshProUGUI interactText;
        #endregion

        #region StartMethod
        private void Start()
        {
            crosshair = GameObject.Find("Crosshair").GetComponent<Image>();
            interactText = GameObject.Find("InteractText").GetComponent<TextMeshProUGUI>();
        }
        #endregion

        #region Implementation
        public override void OnFocus()
        {
            Hide(crosshair.gameObject);
            Show(interactText.gameObject);
            interactText.text = "Pickup the key [E]";
        }

        public override void OnInteract()
        {
            //PlaySound(pickupSound, Vector3.zero);
            Destroy(gameObject);
            Inventory.keys[keyIndex] = true;
        }

        public override void OnAlternateInteract()
        {
            //You can assign a method here, for instance, to examine the object before interacting with it
        }

        public override void OnLoseFocus()
        {
            Hide(interactText.gameObject);
            Show(crosshair.gameObject);
        }
        #endregion

        #region OtherMethods
        private void PlaySound(AudioClip audioClip, Vector3 position, float volumeMultiplier = 1f)
        {
            AudioSource.PlayClipAtPoint(audioClip, position, volumeMultiplier * volume);
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