using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUInstancer
{
    /// <summary>
    /// Add this component to your Camera GameObject and include the GPUI Managers in the Managers list to automatically modify the occlusion offset based on camera movement.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [DefaultExecutionOrder(-10)]
    public class GPUIDynamicOcclusionOffset : MonoBehaviour
    {
        public GPUInstancerManager[] managers;
        public float offsetIntensity = 1.0f;

        private Transform _cachedTransform;
        private Vector3 _position;
        private Quaternion _rotation;

        public void Reset()
        {
            if (managers == null || managers.Length == 0)
                managers = GPUInstancerUtility.FindObjectsOfType<GPUInstancerManager>();
        }

        public void Start()
        {
            _cachedTransform = transform;
            _position = _cachedTransform.position;
            _rotation = _cachedTransform.rotation;
        }

        public void LateUpdate()
        {
            if (offsetIntensity <= 0 || managers == null || managers.Length == 0)
                return;
            Vector3 cameraPos = _cachedTransform.position;
            Quaternion cameraRot = _cachedTransform.rotation;
            float offset = Mathf.Max(Vector3.Distance(cameraPos, _position) * 0.01f, (1f - Mathf.Abs(Quaternion.Dot(cameraRot, _rotation))) * 100f) * offsetIntensity;
            foreach (GPUInstancerManager manager in managers)
            {
                if (manager == null || !manager.isInitialized)
                    continue;
                foreach (GPUInstancerPrototype prototype in manager.prototypeList)
                {
                    prototype.occlusionOffset = offset;
                }
            }

            _position = cameraPos;
            _rotation = cameraRot;
        }
    }
}
