using System;
using UnityEngine;

namespace TW08.Race
{
    [DisallowMultipleComponent]
    public sealed class ForkliftDamage : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float maximumIntegrity = 100f;
        [SerializeField, Min(0)] private int shieldCharges;

        public float CurrentIntegrity { get; private set; }
        public float MaximumIntegrity => maximumIntegrity;
        public int ShieldCharges => shieldCharges;
        public bool IsDisabled => CurrentIntegrity <= 0f;

        public event Action<float, float> IntegrityChanged;
        public event Action Disabled;

        private void Awake()
        {
            CurrentIntegrity = maximumIntegrity;
        }

        public void ApplyDamage(float amount)
        {
            if (amount <= 0f || IsDisabled)
            {
                return;
            }

            if (shieldCharges > 0)
            {
                shieldCharges--;
                return;
            }

            CurrentIntegrity = Mathf.Max(0f, CurrentIntegrity - amount);
            IntegrityChanged?.Invoke(CurrentIntegrity, maximumIntegrity);

            if (IsDisabled)
            {
                Disabled?.Invoke();
            }
        }

        public void Repair(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            CurrentIntegrity = Mathf.Min(maximumIntegrity, CurrentIntegrity + amount);
            IntegrityChanged?.Invoke(CurrentIntegrity, maximumIntegrity);
        }

        public void GrantShield(int charges = 1)
        {
            shieldCharges += Mathf.Max(1, charges);
        }
    }
}
