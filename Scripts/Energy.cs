// for health, mana, etc.
using UnityEngine;
using UnityEngine.Events;

public abstract class Energy : MonoBehaviour
{
    // current value
    // set & get: keep between min and max
   int _current = 0;
    public int current
    {
        get { return Mathf.Min(_current, max); }
        set
        {
            bool emptyBefore = _current == 0;
            _current = Mathf.Clamp(value, 0, max);
            if (_current == 0 && !emptyBefore) onEmpty.Invoke();
        }
    }

    // maximum value (may depend on buffs, items, etc.)
    public abstract int max { get; }

    // recovery rate (may depend on buffs, items etc.)
    public abstract int recoveryRate { get; }

    // don't recover while dead. all energy scripts need to check Health.
    public Health health;

    // spawn with full energy? important for monsters, etc.
    public bool spawnFull = true;

    [Header("Events")]
    public UnityEvent onEmpty;

    public  void Start()
    {
        // set full energy on start if needed
        if (spawnFull) current = max;

        // recovery every second
        InvokeRepeating(nameof(Recover), 1, 1);
    }

    // get percentage
    public float Percent() =>
        (current != 0 && max != 0) ? (float)current / (float)max : 0;

    // recover once a second
   
    public void Recover()
    {
        if (enabled && health.current > 0)
            current += recoveryRate;
    }
}
