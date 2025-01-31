using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Autohand
{
    [RequireComponent(typeof(Grabbable))]
    public class PistolAmmo : MonoBehaviour
    {
        public int currentAmmo = 13;
        private void Start()
        {
            SetAmmo(currentAmmo);
        }
        public bool RemoveAmmo()
        {
            if (currentAmmo > 0)
            {
                SetAmmo(--currentAmmo);
                return true;
            }
            return false;
        }
        public void SetAmmo(int amount)
        {
            currentAmmo = amount;
        }
    }
}
