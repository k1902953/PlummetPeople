using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetTrigger : MonoBehaviour
{
    [SerializeField]
    private Vector3 resetPosition;

    [SerializeField]
    private AudioClip clip_waterSplash;

    private void OnTriggerEnter(Collider other)
    {
        CharacterController cc = other.GetComponent<CharacterController>();
        if (cc)
        {
            AudioSource audio = Audio.instance.MakeSound(transform.position);
            audio.clip = clip_waterSplash;
            audio.volume = 0.1f;
            audio.spatialBlend = 1f;
            audio.Play();
            audio.GetComponent<Sound>().StartDelete(audio.clip.length);

            cc.enabled = false;
            Vector3 randomSpawnOffset = Vector3.right * Random.Range(-3, 3) + Vector3.forward * Random.Range(-3, 3);
            other.transform.position = resetPosition + randomSpawnOffset;
            cc.enabled = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(resetPosition, 0.5f);
    }
}
