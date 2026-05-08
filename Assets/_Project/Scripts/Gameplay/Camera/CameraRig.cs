using UnityEngine;

namespace LibraryGame.Gameplay
{
    /// <summary>
    /// Holds two cameras (first-person attached to player head, third-person
    /// orbit chase) and toggles which one is active. No Cinemachine — keeps
    /// us version-independent on Unity 6.
    /// </summary>
    public sealed class CameraRig : MonoBehaviour
    {
        public Transform target;            // player root
        public Transform headMount;         // PlayerController.CameraMount
        public float thirdPersonDistance = 3.5f;
        public float thirdPersonHeight = 1.6f;

        private Camera _firstPerson;
        private Camera _thirdPerson;
        private bool _isThirdPerson;

        public bool IsThirdPerson => _isThirdPerson;

        private void Start()
        {
            _firstPerson = Make("Camera_FirstPerson", isMain: true);
            _thirdPerson = Make("Camera_ThirdPerson", isMain: false);

            if (headMount != null) _firstPerson.transform.SetParent(headMount, false);
            _thirdPerson.transform.SetParent(transform, false);

            SetThirdPerson(false);
        }

        private Camera Make(string name, bool isMain)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var cam = go.AddComponent<Camera>();
            cam.fieldOfView = 65f;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 60f;
            go.AddComponent<AudioListener>().enabled = isMain;
            cam.tag = isMain ? "MainCamera" : "Untagged";
            return cam;
        }

        private void LateUpdate()
        {
            if (!_isThirdPerson || target == null || _thirdPerson == null) return;

            var lookAt = target.position + Vector3.up * thirdPersonHeight;
            var back = -target.forward;
            var camPos = lookAt + back * thirdPersonDistance + Vector3.up * 0.5f;
            _thirdPerson.transform.position = camPos;
            _thirdPerson.transform.LookAt(lookAt);
        }

        public void Toggle() => SetThirdPerson(!_isThirdPerson);

        public void SetThirdPerson(bool on)
        {
            _isThirdPerson = on;
            if (_firstPerson != null) _firstPerson.gameObject.SetActive(!on);
            if (_thirdPerson != null) _thirdPerson.gameObject.SetActive(on);

            var fpAl = _firstPerson != null ? _firstPerson.GetComponent<AudioListener>() : null;
            var tpAl = _thirdPerson != null ? _thirdPerson.GetComponent<AudioListener>() : null;
            if (fpAl) fpAl.enabled = !on;
            if (tpAl) tpAl.enabled = on;
        }
    }
}
