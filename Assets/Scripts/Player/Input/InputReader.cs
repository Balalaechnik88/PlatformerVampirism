using UnityEngine;

public class InputReader : MonoBehaviour
{
    [SerializeField] private string _horizontalAxisName = "Horizontal";
    [SerializeField] private KeyCode _jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode _attackKey = KeyCode.F;
    [SerializeField] private KeyCode _vampirismKey = KeyCode.Q;

    private bool _jumpPressed;
    private bool _attackPressed;
    private bool _vampirismPressed;

    public float Horizontal { get; private set; }

    private void Update()
    {
        Horizontal = Input.GetAxisRaw(_horizontalAxisName);

        if (Input.GetKeyDown(_jumpKey))
            _jumpPressed = true;

        if (Input.GetKeyDown(_attackKey))
            _attackPressed = true;

        if (Input.GetKeyDown(_vampirismKey))
            _vampirismPressed = true;
    }

    public bool ConsumeAttackPressed()
    {
        return ConsumeAsTrigger(ref _attackPressed);
    }

    public bool ConsumeJumpPressed()
    {
        return ConsumeAsTrigger(ref _jumpPressed);
    }

    public bool ConsumeVampirismPressed()
    {
        return ConsumeAsTrigger(ref _vampirismPressed);
    }

    private static bool ConsumeAsTrigger(ref bool triggerValue)
    {
        bool wasPressed = triggerValue;
        triggerValue = false;
        return wasPressed;
    }
}