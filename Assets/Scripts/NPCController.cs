using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Animator))] // 強制要求該物件必須有 Animator
public class NpcController : MonoBehaviour
{
    [Header("時間設定")]
    public float timeVariation = 30f;

    private Animator _animator;

    private NPCSpawner spawner;

    public bool is_trolling = false;

    [Header("隨機速度設定")]
    public float minIdleSpeed = 0.8f;
    public float maxIdleSpeed = 1.2f;

    public bool isSpinning = false;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        spawner = Object.FindAnyObjectByType<NPCSpawner>();
    }

    private void Start()
    {
        // StartCoroutine(PlayAnimation("FanDance"));
        RandomizeAnimatorSpeed();
    }

    public void RandomizeAnimatorSpeed()
    {
        if (_animator != null)
        {
            _animator.speed = Random.Range(minIdleSpeed, maxIdleSpeed);
        }
    }

    void Update()
    {
        Vector3 targetPosition = Camera.main.transform.position;
        
        targetPosition.y = transform.position.y;

        if (!isSpinning)
        {
            transform.LookAt(targetPosition);
        }
    }

    public IEnumerator PlayAnimation(string animationName)
    {
        if (_animator == null || _animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning($"{gameObject.name} 缺少 Animator Controller！");
            yield break;
        }

        _animator.speed = 1.0f;
        _animator.SetBool(animationName, true);
        is_trolling = true;

        yield return new WaitForSeconds(timeVariation);

        if (is_trolling)
        {
            _animator.SetBool(animationName, false);
            is_trolling = false;
            _animator.speed = Random.Range(minIdleSpeed, maxIdleSpeed);
        }

        // Debug.Log($"{gameObject.name} 執行了動畫: {animationName}");
    }

    public IEnumerator PlayRandomAnimation()
    {
        if (_animator == null || _animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning($"{gameObject.name} 缺少 Animator Controller！");
            yield break;
        }

        List<string> boolParams = new List<string>();
        foreach (AnimatorControllerParameter param in _animator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Bool)
            {
                boolParams.Add(param.name);
            }
        }

        if (boolParams.Count == 0)
        {
            Debug.LogWarning($"{gameObject.name} 的 Animator 中沒有任何 Bool 參數！");
            yield break;
        }

        string selectedName = boolParams[Random.Range(0, boolParams.Count)];

        _animator.speed = 1.0f;
        _animator.SetBool(selectedName, true);
        is_trolling = true;

        yield return new WaitForSeconds(timeVariation);

        if (is_trolling)
        {
            _animator.SetBool(selectedName, false);
            is_trolling = false;
            _animator.speed = Random.Range(minIdleSpeed, maxIdleSpeed);
        }

        // Debug.Log($"{gameObject.name} 隨機執行了動畫: {selectedName}");
    }

    public void StopAnimation(string animationName)
    {
        if (_animator == null || _animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning($"{gameObject.name} 缺少 Animator Controller！");
            return;
        }
        
        StopAllCoroutines();
        _animator.SetBool(animationName, false);
        is_trolling = false;
        
        Debug.Log($"{gameObject.name} 停止了動畫: {animationName}");
    }

    public void ReturnToIdle()
    {
        if (_animator == null || _animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning($"{gameObject.name} 缺少 Animator Controller！");
            return;
        }
        
        StopAllCoroutines();
        _animator.speed = Random.Range(minIdleSpeed, maxIdleSpeed);
        foreach (AnimatorControllerParameter param in _animator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Bool)
            {
                _animator.SetBool(param.name, false);
            }
        }
        spawner.StartCoroutine(spawner.AddScore());
        is_trolling = false;
    }

    public void GoToSpin(){
        if (_animator == null || _animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning($"{gameObject.name} 缺少 Animator Controller！");
            return;
        }
        if (_animator.IsInTransition(0)) return;
        isSpinning = true;
        foreach (AnimatorControllerParameter param in _animator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Bool)
            {
                _animator.SetBool(param.name, false);
            }
        }
        StopAllCoroutines();
        _animator.SetTrigger("Spin");
        is_trolling = false;
        
        // Debug.Log($"{gameObject.name} 停止了動畫: {animationName}");
    }
}