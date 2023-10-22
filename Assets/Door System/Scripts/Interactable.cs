using UnityEngine;

namespace DoorSystem
{
    public abstract class Interactable : MonoBehaviour
    {
        public abstract void OnInteract();
        public abstract void OnAlternateInteract();
        public abstract void OnFocus();
        public abstract void OnLoseFocus();
    }
}