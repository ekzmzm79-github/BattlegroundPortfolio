using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어의 생명력을 담당
/// 피격시 피격 이펙트를 표시하며 UI를 업데이트
/// 죽을 경우 모든 동작 스크립트를 멈추게 만든다.
/// </summary>
public class PlayerHealth : HealthBase
{
    #region Variable
    public float health = 100f;
    public float criticalHealth = 30f;
    public Transform healthHUD;
    public SoundList deathSound;
    public SoundList hitSound;
    public GameObject hurtPrefab;
    public float decayFactor = 0.8f; // 감쇄?

    private float totalHealth;
    private RectTransform healthBar, placeHolderBar;
    private Text healthLabel;
    private float originalBarScale;
    private bool critical;

    private BlinkHUD criticalHUD;
    private HurtHUD hurtHUD;

    #endregion Variable

    #region Method

    private void Awake()
    {
        myAnimator = GetComponent<Animator>();
        totalHealth = health;

        healthBar = healthHUD.Find("HealthBar/Bar").GetComponent<RectTransform>();
        placeHolderBar = healthHUD.Find("HealthBar/Placeholder").GetComponent<RectTransform>();
        healthLabel = healthHUD.Find("HealthBar/Label").GetComponent<Text>();
        originalBarScale = healthBar.sizeDelta.x;
        healthLabel.text = "" + (int)health;

        criticalHUD = healthHUD.Find("Bloodframe").GetComponent<BlinkHUD>();
        hurtHUD = this.gameObject.AddComponent<HurtHUD>();
        hurtHUD.Setup(healthHUD, hurtPrefab, decayFactor, transform);
    }

    private void Update()
    {
        //hpBar가 서서히 줄어들도록 Lerp 
        if (placeHolderBar.sizeDelta.x > healthBar.sizeDelta.x)
        {
            placeHolderBar.sizeDelta = Vector2.Lerp(placeHolderBar.sizeDelta, healthBar.sizeDelta, 2f * Time.deltaTime);
        }
    }

    public bool IsFullLife()
    {
        return Mathf.Abs(health - totalHealth) < float.Epsilon;
    }

    private void UpdateHealthBar()
    {
        healthLabel.text = "" + (int)health;
        float scaleFactor = health / totalHealth;
        healthBar.sizeDelta = new Vector2(scaleFactor * originalBarScale, healthBar.sizeDelta.y);
    }

    private void Kill()
    {
        IsDead = true;
        gameObject.layer = FC.TagAndLayer.GetLayerByName(FC.TagAndLayer.LayerName.Default);
        gameObject.tag = FC.TagAndLayer.TagName.Untagged;
        healthHUD.gameObject.SetActive(false);
        healthHUD.parent.Find("WeaponHUD").gameObject.SetActive(false);
        myAnimator.SetBool(FC.AnimatorKey.Aim, false);
        myAnimator.SetBool(FC.AnimatorKey.Cover, false);
        myAnimator.SetFloat(FC.AnimatorKey.Speed, 0);

        
        foreach (GenericBehaviour behaviour in GetComponents<GenericBehaviour>()) // 원래는 GetComponentsInChildren
        {
            behaviour.enabled = false;
        }
        
        SoundManager.Instance.PlayOneShotEffect((int)deathSound, transform.position, 5f);
    }

    public override void TakeDamage(Vector3 location, Vector3 direction, float damage, Collider bodyPart = null, GameObject origin = null)
    {
        health -= damage;
        UpdateHealthBar();

        if (hurtPrefab && healthHUD)
        {
            hurtHUD.DrawHurtUI(origin.transform, origin.GetHashCode());
        }

        if (health <= 0)
        {
            Kill();
        }
        else if(health <= criticalHealth && !critical)
        {
            critical = true;
            criticalHUD.StartBlink();
        }

        SoundManager.Instance.PlayOneShotEffect((int)hitSound, location, 1f);
    }

    #endregion Method

}
