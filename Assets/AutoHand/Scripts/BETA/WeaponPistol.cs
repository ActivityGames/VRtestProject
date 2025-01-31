using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

namespace Autohand
{
    public class WeaponPistol : MonoBehaviour
    {
        [AutoHeader("Weapon Pistol")]
        public bool ignoreMeAutoHeader1;
        public GrabbableHeldJoint pistolGate;
        public PlacePoint magazinePoint;
        public Transform bulletExitPoint;
        public Rigidbody pistolBody;

        [AutoSmallHeader("Pistol Settings")]
        public bool ignoreMeAutoHeader2;
        public AudioClip shootSound;
        public float shootVolume = 1f;
        public float forceHit = 1f;
        public float recoilForce = 1f;
        public float range = 50f;
        public float maxHitDistance = 1000f;
        public LayerMask layer;        
        

        [AutoSmallHeader("Events")]
        public UnityEvent<WeaponPistol> OnShoot;
        public UnityEvent<WeaponPistol> OnEmptyShoot;
        public UnityEvent<WeaponPistol, SlideLoadType> OnSlideEvent;
        public UnityEvent<WeaponPistol, AutoAmmo> OnAmmoPlaceEvent;
        public UnityEvent<WeaponPistol, AutoAmmo> OnAmmoRemoveEvent;

        protected AutoAmmo loadedAmmo;
        internal Grabbable grabbable;
        protected bool slideLoaded = false;
        protected bool hasMagazine = false;
        bool squeezingTrigger;
        private InputDevice rightController;

        private void Start()
        {
            grabbable = GetComponent<Grabbable>();
            if (magazinePoint != null)
                magazinePoint.dontAllows.Add(grabbable);

            rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            slideLoaded = false;
            hasMagazine = false;
            squeezingTrigger = false;
        }

        private void Update()
        {
            if (rightController.isValid)
            {
                bool buttonPressed;
                if (rightController.TryGetFeatureValue(CommonUsages.primaryButton, out buttonPressed) && buttonPressed)
                {
                    EjectMagazine();
                }
            }
            else
            {
                rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            }

            if (pistolGate != null)
            {
                LoadSlide();
            }
        }

        private void OnEnable()
        {
            if (magazinePoint != null)
            {
                magazinePoint.OnPlaceEvent += OnMagPlace;
                magazinePoint.OnRemoveEvent += OnMagRemove;
            }
        }

        private void OnDisable()
        {
            if (magazinePoint != null)
            {
                magazinePoint.OnPlaceEvent -= OnMagPlace;
                magazinePoint.OnRemoveEvent -= OnMagRemove;
            }
        }

        public void PressTrigger()
        {
            if (!squeezingTrigger && slideLoaded && hasMagazine && loadedAmmo != null && loadedAmmo.currentAmmo > 0)
            {
                squeezingTrigger = true;
            }
            else
            {
                squeezingTrigger = false;
                OnEmptyShoot?.Invoke(this);
            }
        }

        public void ReleaseTrigger()
        {
            squeezingTrigger = false;
        }

        public void Shoot()
        {
            /*if (!slideLoaded || loadedAmmo == null || loadedAmmo.currentAmmo <= 0)
            {
                OnEmptyShoot?.Invoke(this);
                return;
            }*/

            if (shootSound)
                AudioSource.PlayClipAtPoint(shootSound, transform.position, shootVolume);

            RaycastHit hit;
            if (Physics.Raycast(bulletExitPoint.position, bulletExitPoint.forward, out hit, range, layer))
            {
                var hitBody = hit.transform.GetComponent<Rigidbody>();
                if (hitBody != null)
                {
                    Debug.DrawRay(bulletExitPoint.position, (hit.point - bulletExitPoint.position), Color.green, 5);                    
                    hitBody.AddForceAtPosition((hit.point - bulletExitPoint.position).normalized * forceHit * 10, hit.point, ForceMode.Impulse);
                    hitBody.GetComponent<Autohand.Demo.Smash>()?.DoSmash();
                }
            }
            pistolBody.AddForce(bulletExitPoint.transform.up * recoilForce * 5, ForceMode.Impulse);
            Debug.Log(hit.collider.gameObject.name);
        }

        public void EjectMagazine()
        {
            if (magazinePoint != null && magazinePoint.placedObject != null)
            {
                magazinePoint.Remove();
                hasMagazine = false;
                slideLoaded = false;
            }
        }

        public void LoadSlide()
        {
            if (loadedAmmo == null || loadedAmmo.currentAmmo <= 0)
                return;

            OnSlideEvent?.Invoke(this, SlideLoadType.HandLoaded);
            slideLoaded = true;
            loadedAmmo.RemoveAmmo();
        }

        public void FireLoadSlide()
        {
            if (loadedAmmo == null || loadedAmmo.currentAmmo <= 0)
                return;

            OnSlideEvent?.Invoke(this, SlideLoadType.ShotLoaded);
            slideLoaded = true;
            loadedAmmo.RemoveAmmo();
        }

        public void UnloadSlide()
        {
            slideLoaded = false;
        }

        public bool IsSlideLoaded()
        {
            return slideLoaded;
        }

        public int GetAmmo()
        {
            int ammo = slideLoaded ? 1 : 0;
            if (loadedAmmo != null)
                ammo += loadedAmmo.currentAmmo;
            return magazinePoint == null ? 1 : ammo;
        }

        void OnMagPlace(PlacePoint point, Grabbable mag)
        {
            if (mag.TryGetComponent<AutoAmmo>(out var ammo))
            {
                this.loadedAmmo = ammo;
                hasMagazine = true;
                OnAmmoPlaceEvent?.Invoke(this, loadedAmmo);
            }
            slideLoaded = false;
        }

        void OnMagRemove(PlacePoint point, Grabbable mag)
        {
            OnAmmoRemoveEvent?.Invoke(this, loadedAmmo);
            loadedAmmo = null;
            hasMagazine = false;
            slideLoaded = false;
        }
    }
}
