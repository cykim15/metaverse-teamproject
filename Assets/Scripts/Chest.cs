using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ChestState { Closed, Opening, Opened, }
public enum ChestPotion { None, HP, Stamina, }

public class Chest : MonoBehaviour
{
    private ChestState state = ChestState.Closed;
    private ChestPotion potion = ChestPotion.None;

    public Coin collidingCoin = null;

    [SerializeField]
    private Transform lid;

    [SerializeField]
    private Light pointLight;
    [SerializeField]
    private Color defaultLightColor;
    [SerializeField]
    private Color hpLightColor;
    [SerializeField]
    private Color staminaLightColor;
    [SerializeField]
    private GameObject hpPotionPrefab;
    [SerializeField]
    private GameObject staminaPotionPrefab;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (state == ChestState.Closed)
        {
            if (collidingCoin != null)
            {
                if (Vector3.Dot(collidingCoin.transform.up, Camera.main.transform.TransformVector(Vector3.right)) > 0)
                {
                    potion = ChestPotion.HP;
                }
                else
                {
                    potion = ChestPotion.Stamina;
                }
            }
            else
            {
                potion = ChestPotion.None;
            }
            ChangeLightColor();
        }
    }

    private void ChangeLightColor()
    {
        if (potion == ChestPotion.None)
        {
            pointLight.color = defaultLightColor;
        }
        else if (potion == ChestPotion.HP)
        {
            pointLight.color = hpLightColor;
        }
        else if (potion == ChestPotion.Stamina)
        {
            pointLight.color = staminaLightColor;
        }
    }

    public void Open()
    {
        SpawnPotion();
        state = ChestState.Opening;
        StartCoroutine(RotateLid());
    }

    private void SpawnPotion()
    {
        GameObject potionPrefab = null;
        if (potion == ChestPotion.HP)
        {
            potionPrefab = hpPotionPrefab;
        }
        else if (potion == ChestPotion.Stamina)
        {
            potionPrefab = staminaPotionPrefab;
        }
        Instantiate(potionPrefab, transform.position, transform.rotation);
    }

    private IEnumerator RotateLid()
    {
        BGMManager.Instance.audioSource.Pause();

        audioSource.Play();
        yield return new WaitForSeconds(2f);
        BGMManager.Instance.audioSource.Play();

        pointLight.color = defaultLightColor;

        float targetRotationX = -90f;
        float rotationSpeed = 1f;

        Vector3 currentRotation = lid.rotation.eulerAngles;
        Vector3 targetRotation = new Vector3(targetRotationX, currentRotation.y, currentRotation.z);

        while (Quaternion.Angle(lid.rotation, Quaternion.Euler(targetRotation)) > 0.1f)
        {
            lid.rotation = Quaternion.Slerp(lid.rotation, Quaternion.Euler(targetRotation), rotationSpeed * Time.deltaTime);
            yield return null;
        }

        state = ChestState.Opened;
        pointLight.enabled = false;
        

    }


}
