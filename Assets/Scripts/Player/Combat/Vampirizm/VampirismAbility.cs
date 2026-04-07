using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class VampirismAbility : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _origin;

    [Header("Targets")]
    [SerializeField] private LayerMask _targetLayerMask;

    [Header("Ability")]
    [SerializeField] private float _radius = 3f;
    [SerializeField] private float _durationSeconds = 6f;
    [SerializeField] private float _cooldownSeconds = 4f;
    [SerializeField] private float _damagePerSecond = 5f;
    [SerializeField] private float _healMultiplier = 1f;

    private Health _ownerHealth;
    private Coroutine _abilityRoutine;
    private bool _isActive;
    private bool _isOnCooldown;
    private float _progressNormalized;
    private float _damageBuffer;
    private float _healBuffer;

    public event Action<bool> ActivityChanged;
    public event Action<float> ProgressChanged;

    public bool IsActive => _isActive;
    public bool IsOnCooldown => _isOnCooldown;
    public float ProgressNormalized => _progressNormalized;
    public float Radius => _radius;

    private void Awake()
    {
        _ownerHealth = GetComponent<Health>();

        if (_origin == null)
            _origin = transform;

        if (_durationSeconds <= 0f || _cooldownSeconds <= 0f || _radius <= 0f || _damagePerSecond < 0f)
        {
            Debug.LogError($"[{nameof(VampirismAbility)}] Некорректные параметры способности. Скрипт отключён.", this);
            enabled = false;
            return;
        }

        _progressNormalized = 1f;
    }

    private void OnEnable()
    {
        ProgressChanged?.Invoke(_progressNormalized);
        ActivityChanged?.Invoke(_isActive);
    }

    private void OnDisable()
    {
        if (_abilityRoutine != null)
            StopCoroutine(_abilityRoutine);

        _abilityRoutine = null;
        _isActive = false;
        _isOnCooldown = false;
        _damageBuffer = 0f;
        _healBuffer = 0f;
        _progressNormalized = 1f;

        ProgressChanged?.Invoke(_progressNormalized);
        ActivityChanged?.Invoke(_isActive);
    }

    public bool TryActivate()
    {
        if (_isActive || _isOnCooldown)
            return false;

        _abilityRoutine = StartCoroutine(RunAbilityRoutine());
        return true;
    }

    private IEnumerator RunAbilityRoutine()
    {
        _isActive = true;
        _isOnCooldown = false;
        _damageBuffer = 0f;
        _healBuffer = 0f;

        ActivityChanged?.Invoke(true);

        float activeElapsed = 0f;

        while (activeElapsed < _durationSeconds)
        {
            float deltaTime = Time.deltaTime;

            activeElapsed += deltaTime;
            ApplyDrain(deltaTime);

            _progressNormalized = 1f - Mathf.Clamp01(activeElapsed / _durationSeconds);
            ProgressChanged?.Invoke(_progressNormalized);

            yield return null;
        }

        _isActive = false;
        _isOnCooldown = true;
        _progressNormalized = 0f;

        ActivityChanged?.Invoke(false);
        ProgressChanged?.Invoke(_progressNormalized);

        float cooldownElapsed = 0f;

        while (cooldownElapsed < _cooldownSeconds)
        {
            cooldownElapsed += Time.deltaTime;

            _progressNormalized = Mathf.Clamp01(cooldownElapsed / _cooldownSeconds);
            ProgressChanged?.Invoke(_progressNormalized);

            yield return null;
        }

        _isOnCooldown = false;
        _progressNormalized = 1f;
        _abilityRoutine = null;

        ProgressChanged?.Invoke(_progressNormalized);
    }

    private void ApplyDrain(float deltaTime)
    {
        DamageReceiver nearestReceiver = FindNearestDamageReceiver();

        if (nearestReceiver == null)
            return;

        Health targetHealth = nearestReceiver.Health;

        if (targetHealth == null || targetHealth.CurrentHealth <= 0)
            return;

        _damageBuffer += _damagePerSecond * deltaTime;

        int damageAmount = Mathf.FloorToInt(_damageBuffer);

        if (damageAmount <= 0)
            return;

        _damageBuffer -= damageAmount;

        int targetHealthBeforeDamage = targetHealth.CurrentHealth;
        targetHealth.TakeDamage(damageAmount);

        int dealtDamage = targetHealthBeforeDamage - targetHealth.CurrentHealth;

        if (dealtDamage <= 0)
            return;

        _healBuffer += dealtDamage * _healMultiplier;

        int healAmount = Mathf.FloorToInt(_healBuffer);

        if (healAmount <= 0)
            return;

        _healBuffer -= healAmount;
        _ownerHealth.Heal(healAmount);
    }

    private DamageReceiver FindNearestDamageReceiver()
    {
        Vector2 originPosition = _origin.position;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(originPosition, _radius, _targetLayerMask);

        if (hitColliders == null || hitColliders.Length == 0)
            return null;

        DamageReceiver nearestReceiver = null;
        float nearestDistanceSquared = float.MaxValue;

        for (int i = 0; i < hitColliders.Length; i++)
        {
            Collider2D hitCollider = hitColliders[i];

            if (hitCollider == null)
                continue;

            if (hitCollider.TryGetComponent(out DamageReceiver damageReceiver) == false)
                continue;

            Health targetHealth = damageReceiver.Health;

            if (targetHealth == null || targetHealth.CurrentHealth <= 0)
                continue;

            Vector2 deltaToTarget = (Vector2)hitCollider.transform.position - originPosition;
            float distanceSquared = deltaToTarget.sqrMagnitude;

            if (distanceSquared < nearestDistanceSquared)
            {
                nearestDistanceSquared = distanceSquared;
                nearestReceiver = damageReceiver;
            }
        }

        return nearestReceiver;
    }

    private void OnDrawGizmosSelected()
    {
        Transform drawOrigin = _origin != null ? _origin : transform;

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(drawOrigin.position, _radius);
    }
}