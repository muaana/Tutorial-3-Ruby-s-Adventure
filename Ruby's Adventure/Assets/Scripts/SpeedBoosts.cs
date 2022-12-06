using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBoosts : MonoBehaviour
{
    public AudioClip speedBoostSound;
    public ParticleSystem healthEffectObject;

    void OnTriggerEnter2D(Collider2D other)
    {
        RubyController controller = other.GetComponent<RubyController>();

        if (controller != null)
        {
            controller.SpeedBoost(1);
            healthEffectObject = Instantiate(healthEffectObject, transform.position, Quaternion.identity);
            Destroy(gameObject);

            controller.PlaySound(speedBoostSound);
            
        }
    }
}
