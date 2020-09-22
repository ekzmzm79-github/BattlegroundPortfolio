using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class EnemyHealth : HealthBase
{
    #region Variable
    public float health = 100f;
    public GameObject healthHUD;
    public GameObject bloodSample;
    public bool headShot;

    private float totalHealth;
    private Transform weapon;
    private Transform hud;
    private RectTransform healthBar;
    private float originalBarScale;
    private HealthHUD healthUI;
    private Animator anim;
    private StateController controller;
    private GameObject gameController;

    #endregion Variable


    #region Method

    private void Awake()
    {
        hud = Instantiate(healthHUD, transform).transform;
        if(!hud.gameObject.activeSelf)
        {
            hud.gameObject.SetActive(true);
        }

        totalHealth = health;
        healthBar = hud.transform.Find("Bar").GetComponent<RectTransform>();
        healthUI = hud.GetComponent<HealthHUD>();
        originalBarScale = healthBar.sizeDelta.x;
        anim = GetComponent<Animator>();
        controller = GetComponent<StateController>();
        gameController = GameObject.FindGameObjectWithTag("GameController");

        foreach (Transform child in anim.GetBoneTransform(HumanBodyBones.RightHand))
        {
            weapon = child.Find("muzzle");
            if (weapon != null)
            {
                break;
            }
        }
        weapon = weapon.parent;
    }

    private void UpdateHealthBar()
    {
        float scaleFactor = health / totalHealth; // 현재 비율 구하기
        healthBar.sizeDelta = new Vector2(scaleFactor * originalBarScale, healthBar.sizeDelta.y); // x축 비율로 조절
    }

    private void RemoveAllForce()
    {
        foreach (Rigidbody body in GetComponentsInChildren<Rigidbody>())
        {
            body.isKinematic = false;
            body.velocity = Vector3.zero;
        }
    }

    private void Kill()
    {
        foreach (MonoBehaviour mb in GetComponents<MonoBehaviour>())
        {
            // 모든 스크립트 제거
            if(this != mb)
            {
                Destroy(mb);
            }
        }
        Destroy(GetComponent<NavMeshAgent>());
        RemoveAllForce();
        controller.focusSight = false;
        anim.SetBool(FC.AnimatorKey.Aim, false);
        anim.SetBool(FC.AnimatorKey.Crouch, false);
        anim.enabled = false;
        Destroy(weapon.gameObject);
        Destroy(hud.gameObject);
        IsDead = true;
    }

    public override void TakeDamage(Vector3 location, Vector3 direction, float damage, Collider bodyPart = null, GameObject origin = null)
    {
        if (!IsDead && headShot && bodyPart.transform == anim.GetBoneTransform(HumanBodyBones.Head))
        {
            damage *= 10;
            gameController.SendMessage("HeadShotCallback", SendMessageOptions.DontRequireReceiver);
        }
        Instantiate(bloodSample, location, Quaternion.LookRotation(-direction), transform);
        health -= damage;
        if(!IsDead)
        {
            anim.SetTrigger("Hit");
            healthUI.SetVisible();
            UpdateHealthBar();
            controller.variables.feelAlert = true;
            controller.personalTarget = controller.aimTarget.position;
        }
        if(health <= 0)
        {
            if(!IsDead)
            {
                Kill();
            }

            //래그돌 유무에 따라 반응이 달라짐
            Rigidbody rigidbody = bodyPart.GetComponent<Rigidbody>();
            rigidbody.mass = 40;
            rigidbody.AddForce(100f * direction.normalized, ForceMode.Impulse);
        }
    }

    #endregion Method
}
