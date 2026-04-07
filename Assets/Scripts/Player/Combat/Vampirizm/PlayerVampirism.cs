using UnityEngine;

public class PlayerVampirism : MonoBehaviour
{
    [SerializeField] private InputReader _input;
    [SerializeField] private VampirismAbility _ability;

    private void Awake()
    {
        if (_input == null || _ability == null)
        {
            Debug.LogError($"[{nameof(PlayerVampirism)}] Не назначены обязательные ссылки. Скрипт отключён.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        if (_input.ConsumeVampirismPressed() == false)
            return;

        _ability.TryActivate();
    }
}