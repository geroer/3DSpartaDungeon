using System;
using System.Collections;
using UnityEngine;

public interface IDamageable
{
    void TakePhysicalDamage(int damage);
}

public class PlayerCondition : MonoBehaviour, IDamageable
{
    public UICondition uiCondition;

    Condition health { get { return uiCondition.health; } }
    Condition stamina { get { return uiCondition.stamina; } }

    private bool canRecoverStamina = false;
    private float lastStaminaUseTime = 0f;

    //public event Action onTakeDamage;

    void Update()
    {
        if (Time.time - lastStaminaUseTime >= 3f)
        {
            canRecoverStamina = true;
        }

        if (canRecoverStamina)
        {
            stamina.Add(stamina.passiveValue * Time.deltaTime);
        }

        if (health.curValue <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        health.Add(amount);
    }

    public void Die()
    {
        Debug.Log("Á×À½");
    }

    public void TakePhysicalDamage(int damage)
    {
        health.Subtract(damage);
        //onTakeDamage?.Invoke();
    }

    public bool UseStamina(float amount)
    {
        stamina.Subtract(amount);

        canRecoverStamina = false;
        lastStaminaUseTime = Time.time;

        return true;
    }
}
