using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GPUInstancer.CrowdAnimations;
using GPUInstancer;
using Unity.VisualScripting;

public class AnimationManager : MonoBehaviour
{
    [Header("References")]
    public GPUICrowdManager gpuiCrowdManager;

    [Header("Settings")]
    public int _rowCount = 30;
    public int _collumnCount = 30;
    public float spaceMin = 1.5f;
    public float spaceMax = 2.0f;
    public Vector3 center = new Vector3(0, 0, 0);
    public float normalSpeed = 1f;
    public float waveSpeed = 0.2f;
    public float waveMoveSpeed = 2f;
    public float waveFrequency = 0.8f;

    [SerializeField] private int globalIndex = 0;

    private float _space = 1.5f;
    private int _selectedPrototypeIndex = 0;
    private List<GPUInstancerPrototype> _prototypeList;
    private List<GPUInstancerPrefab> _instanceList;

    private void Start()
    {
        if (gpuiCrowdManager == null)
            return;

        // Disabling the Crowd Manager here to change prototype settings. 
        // Enabling it after this will make it re-initialize with the new settings for the prototypes
        gpuiCrowdManager.enabled = false;

        _instanceList = new List<GPUInstancerPrefab>();

        foreach (GPUICrowdPrototype prototype in gpuiCrowdManager.prototypeList)
        {
            prototype.animationData.useCrowdAnimator = true;

            prototype.enableRuntimeModifications = true;
            prototype.addRemoveInstancesAtRuntime = true;
            prototype.extraBufferSize = 10000;
        }
        // // Setup the prototype in the manager
        // GPUICrowdPrototype crowdPrototype = (GPUICrowdPrototype)gpuiCrowdManager.prototypeList[_selectedPrototypeIndex];
        // crowdPrototype.animationData.useCrowdAnimator = true; // indicate the Crowd Animator Workflow will be used initially

        // // Edit runtime properties for this prototype:
        // crowdPrototype.enableRuntimeModifications = true;   // Enable runtime modifications to be able to...
        // crowdPrototype.addRemoveInstancesAtRuntime = true;  // add and remove instances at runtime ...
        // crowdPrototype.extraBufferSize = 10000;             // with this amount of extra instances that can be added after the initial ones.

        // Instantiate instance GOs:
        float width = (_rowCount - 1) * _space; 
        float height = (_collumnCount - 1) * _space;

        for (int r = 0; r < _rowCount; r++)
        {
            for (int c = 0; c < _collumnCount; c++)
            {
                Vector3 pos = center;

                _space = Random.Range(spaceMin, spaceMax);

                pos.x += r * _space - width * 0.5f;
                pos.z += c * _space - height * 0.5f;

                int randomIndex = Random.Range(0, gpuiCrowdManager.prototypeList.Count);

                GPUICrowdPrototype prototype =
                    (GPUICrowdPrototype)gpuiCrowdManager.prototypeList[randomIndex];

                GameObject prefabObject = prototype.prefabObject;

                GameObject instanceGO = Instantiate(
                    prefabObject,
                    pos,
                    prefabObject.transform.rotation);

                var gp = instanceGO.GetComponent<PrefabIndex>();
                gp.index = globalIndex++;

                _instanceList.Add(instanceGO.GetComponent<GPUICrowdPrefab>());
            }
        }
        
        // Register the instantiated GOs to the Crowd Manager
        GPUInstancerAPI.RegisterPrefabInstanceList(gpuiCrowdManager, _instanceList);

        // Enabling the Crowd Manager back; this will re-initialize it with the new settings for the prototypes
        gpuiCrowdManager.enabled = true;

        _prototypeList = gpuiCrowdManager.prototypeList; // cache the prototype list on the Manager to access later
    }

    private void OnDestroy()
    {
        if (_prototypeList == null)
            return;

        // We reset the protoypes back to using the Crowd Animator workflow, since changes would persist in the prototype data.
        foreach (GPUICrowdPrototype prototype in _prototypeList)
        {
            prototype.animationData.useCrowdAnimator = true;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) StartCoroutine(SetCrowdAnimators(0));
        else if (Input.GetKeyDown(KeyCode.Alpha2)) StartCoroutine(SetWaveCrowdAnimators(1));
        else if (Input.GetKeyDown(KeyCode.Alpha3)) StartCoroutine(SetCrowdAnimators(2));
        else if (Input.GetKeyDown(KeyCode.Alpha4)) StartCoroutine(SetCrowdAnimators(3));
    }

    public IEnumerator SetCrowdAnimators(int index)
    {
        if (gpuiCrowdManager != null)
        {
            while (!gpuiCrowdManager.isInitialized)
                yield return null;
            SetAnimations(index);
        }
    }

    public IEnumerator SetWaveCrowdAnimators(int index)
    {
        if (gpuiCrowdManager != null)
        {
            while (!gpuiCrowdManager.isInitialized)
                yield return null;
            SetWaveAnimations(index);
        }
    }

    public void SetAnimations(int index)
    {
        if (gpuiCrowdManager != null)
        {
            Dictionary<GPUInstancerPrototype, List<GPUInstancerPrefab>> registeredPrefabInstances = gpuiCrowdManager.GetRegisteredPrefabsRuntimeData();
            GPUIAnimationClipData clipData;
            float startTime;
            if (registeredPrefabInstances != null)
            {
                foreach (GPUICrowdPrototype crowdPrototype in registeredPrefabInstances.Keys)
                {
                    if (crowdPrototype.animationData != null && crowdPrototype.animationData.useCrowdAnimator)
                    {
                        foreach (GPUICrowdPrefab crowdInstance in registeredPrefabInstances[crowdPrototype])
                        {
                            clipData = crowdPrototype.animationData.clipDataList[index];
                            startTime = UnityEngine.Random.Range(0, clipData.length);

                            GPUICrowdAPI.StartAnimation(crowdInstance, clipData, startTime);
                            GPUICrowdAPI.SetAnimationSpeed(crowdInstance, normalSpeed);
                        }
                    }
                }
            }
        }
    }

    public void SetWaveAnimations(int index)
    {
        if (gpuiCrowdManager == null) return;

        var registered = gpuiCrowdManager.GetRegisteredPrefabsRuntimeData();

        if (registered == null) return;

        foreach (GPUICrowdPrototype prototype in registered.Keys)
        {
            if (prototype.animationData == null ||
                !prototype.animationData.useCrowdAnimator)
                continue;


            GPUIAnimationClipData clipData =
                prototype.animationData.clipDataList[index];

            List<GPUInstancerPrefab> instances = registered[prototype];

            int columnCount = _collumnCount;

            for (int i = 0; i < instances.Count; i++)
            {
                GPUICrowdPrefab instance = (GPUICrowdPrefab)instances[i];

                int row = instance.gameObject.GetComponent<PrefabIndex>().index / _collumnCount;
                float wave = Mathf.Sin(row * waveFrequency + Time.time * waveMoveSpeed);

                float startTime = Mathf.Lerp(0, clipData.length * 0.5f,  (wave + 1f) * 0.5f);

                GPUICrowdAPI.StartAnimation(instance, clipData, startTime);
                GPUICrowdAPI.SetAnimationSpeed(instance, waveSpeed);
            }
        }
    }
}
