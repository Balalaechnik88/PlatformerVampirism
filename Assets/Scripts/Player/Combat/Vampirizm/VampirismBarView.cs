using UnityEngine;
using UnityEngine.UI;

public class VampirismBarView : MonoBehaviour
{
    [SerializeField] private VampirismAbility _ability;
    [SerializeField] private Slider _slider;

    private void Awake()
    {
        if (_ability == null || _slider == null)
        {
            Debug.LogError($"[{nameof(VampirismBarView)}] Не назначены обязательные ссылки. Скрипт отключён.", this);
            enabled = false;
            return;
        }

        _slider.minValue = 0f;
        _slider.maxValue = 1f;
        _slider.value = _ability.ProgressNormalized;
    }

    private void OnEnable()
    {
        _ability.ProgressChanged += OnProgressChanged;
        OnProgressChanged(_ability.ProgressNormalized);
    }

    private void OnDisable()
    {
        if (_ability != null)
            _ability.ProgressChanged -= OnProgressChanged;
    }

    private void OnProgressChanged(float progressNormalized)
    {
        _slider.value = progressNormalized;
    }
}