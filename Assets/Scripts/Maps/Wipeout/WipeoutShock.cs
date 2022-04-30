using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WipeoutShock : MonoBehaviour
{
    public WipeoutSpinner wipeoutSpinner;

    public float shockForce = 5;
    public int shockTime = 1;

    private float shockCooldown = 0.25f;
    private bool canShock = true;

    private void OnCollisionEnter(Collision collision)
    {
        if (!canShock) return;

        if (collision.gameObject.GetComponent<Rigidbody>())
        {
            ContactPoint contact = collision.contacts[0];

            PlayerMovement playerMovement = GetPlayerMovement(collision.gameObject.transform);

            if (playerMovement)
            {
                float calculatedShockForce = shockForce * (wipeoutSpinner.spinnerTopSpeed * 0.005f);
                Vector3 shockDir = playerMovement.transform.position - contact.point;
                StartCoroutine(playerMovement.GetShocked(shockDir, calculatedShockForce, shockTime)); // shockForce + wipeoutSpinner.spinnerTopSpeed
                StartCoroutine(ShockCooldown());
                Instantiate(wipeoutSpinner.shockParticlePrefab, contact.point, Quaternion.identity, playerMovement.transform);

                AudioSource audio = Audio.instance.MakeSound(playerMovement.transform.position, transform);
                audio.clip = Audio.instance.clips_zap[Random.Range(0, Audio.instance.clips_zap.Count)];
                audio.volume = 0.2f;
                audio.spatialBlend = 1f;
                audio.Play();
                audio.GetComponent<Sound>().StartDelete(audio.clip.length);

                AudioSource audio2 = Audio.instance.MakeSound(playerMovement.transform.position, transform);
                audio2.clip = Audio.instance.clips_vocal_zapped[Random.Range(0, Audio.instance.clips_vocal_zapped.Count)];
                audio2.volume = 0.4f;
                audio2.spatialBlend = 1f;
                audio2.Play();
                audio2.GetComponent<Sound>().StartDelete(audio2.clip.length);
            }
        }
    }

    PlayerMovement GetPlayerMovement(Transform startObject)
    {
        int checkLimit = 50;
        Transform checkingTransform = startObject;

        while (!checkingTransform.GetComponent<PlayerMovement>())
        {
            checkingTransform = checkingTransform.parent;

            if (checkingTransform.GetComponent<PlayerMovement>())
            {
                return checkingTransform.GetComponent<PlayerMovement>();
            }

            checkLimit--;
            if (checkLimit <= 0)
                break;
        }

        return null;
    }

    IEnumerator ShockCooldown()
    {
        canShock = false;
        yield return new WaitForSeconds(shockCooldown);
        canShock = true;
    }
}
