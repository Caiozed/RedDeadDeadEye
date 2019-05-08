using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
using Cinemachine;
using DG.Tweening;
public class WeaponController : MonoBehaviour
{
    // Start is called before the first frame update
    PlayerMovement _player;
    public PostProcessVolume PPVolume;
    public CinemachineFreeLook freeLookCamera;
    public GameObject crossPrefab, MuzzleFlashObj;
    public List<RaycastHit> Marks;
    public Image Recticle;
    public bool isAiming, isInDeadEye = false;
    List<GameObject> Crosses;
    public ParticleSystem particles;
    RaycastHit hit;

    bool enemyHit = false;
    void Start()
    {
        _player = GetComponent<PlayerMovement>();
        Marks = new List<RaycastHit>();
        Crosses = new List<GameObject>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        isAiming = Input.GetButton("Fire2") || isInDeadEye;
        _player.anim.SetBool("isAiming", isAiming);

        PPVolume.weight = Mathf.Lerp(PPVolume.weight, isAiming ? 1 : 0, 0.1f);
        var slowMotionSpeed = isAiming ? 0.3f : 1f;
        Time.timeScale = slowMotionSpeed;
        Time.fixedDeltaTime = slowMotionSpeed * 0.02f;
        freeLookCamera.m_Lens.FieldOfView = Mathf.Lerp(freeLookCamera.m_Lens.FieldOfView, isAiming && !isInDeadEye ? 30f : 40f, 0.1f);
        _player.anim.speed = isInDeadEye ? 3 : 1;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit))
        {
            if (hit.transform.tag == "Enemy")
            {
                enemyHit = true;
            }
            else
            {
                enemyHit = false;
            }
        }
        Recticle.DOColor(enemyHit ? Color.red : Color.white, 0.1f);


        if (Input.GetButtonDown("Fire1") && !isAiming)
        {
            Fire();
        }
        else if (isAiming && Input.GetButtonDown("Fire1"))
        {
            Mark();
        }

        if (!isAiming)
        {
            if (Marks.Count > 0)
            {
                isInDeadEye = true;
                Sequence seq = DOTween.Sequence();
                for (int i = 0; i < Marks.Count; i++)
                {
                    var mark = Marks[i];
                    var x = i;
                    seq.Append(transform.DOLookAt(Marks[i].transform.root.position, .05f).SetUpdate(true));
                    seq.AppendCallback(() => _player.anim.SetTrigger("Fire"));
                    seq.AppendCallback(() => MarkFire(mark, x));
                    seq.AppendInterval(.35f);
                }
                seq.AppendCallback(() => { Destroy(Crosses[Crosses.Count - 1].gameObject); });
                seq.AppendCallback(() => { Marks = new List<RaycastHit>(); Crosses = new List<GameObject>(); });
                seq.AppendCallback(() => { isInDeadEye = false; });
            }
        }
    }

    public void Fire()
    {
        _player.anim.SetTrigger("Fire");
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit))
        {
            transform.DORotate(new Vector3(0, Camera.main.transform.eulerAngles.y, transform.eulerAngles.z), .2f);
            if (hit.transform.tag == "Enemy")
            {
                hit.transform.root.GetComponent<EnemyController>().ActivateRagdoll();
                hit.transform.GetComponent<Rigidbody>().AddForce(transform.forward * 35, ForceMode.Impulse);
            }
        }
    }

    public void MarkFire(RaycastHit hit, int x)
    {
        hit.transform.root.GetComponent<EnemyController>().ActivateRagdoll();
        hit.transform.GetComponent<Rigidbody>().AddForce(transform.forward * 35, ForceMode.Impulse);
        Destroy(Crosses[x].gameObject); Crosses.RemoveAt(x);
    }

    public void Mark()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit))
        {
            if (hit.transform.tag == "Enemy")
            {
                var obj = Instantiate(crossPrefab, hit.point, Quaternion.identity);
                obj.transform.LookAt(Camera.main.transform.position);
                obj.transform.SetParent(hit.transform);
                Crosses.Add(obj);
                Marks.Add(hit);
            }
        }
    }

    public void PlayFlashEffect()
    {
        MuzzleFlashObj.SetActive(true);
        particles.Play();
    }

    public void StopFlashEffect()
    {
        MuzzleFlashObj.SetActive(false);
    }
}
