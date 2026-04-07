using UnityEngine;

public class VampirismRadiusView : MonoBehaviour
{
    [SerializeField] private VampirismAbility _ability;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        if (_ability == null || _spriteRenderer == null)
        {
            Debug.LogError($"[{nameof(VampirismRadiusView)}] Не назначены обязательные ссылки. Скрипт отключён.", this);
            enabled = false;
            return;
        }


        _spriteRenderer.enabled = false;
    }

    private void OnEnable()
    {
        _ability.ActivityChanged += OnActivityChanged;
        OnActivityChanged(_ability.IsActive);
    }

    private void OnDisable()
    {
        if (_ability != null)
            _ability.ActivityChanged -= OnActivityChanged;
    }

    private void OnActivityChanged(bool isActive)
    {
        _spriteRenderer.enabled = isActive;
    }
}