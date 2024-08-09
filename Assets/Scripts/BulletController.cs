using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public PlayerController target;
    public GameObject hitEffect;
    Animator animator;
    Rigidbody rb;
    Vector3 direction;
    private float flyingTime;
    private bool isLaunched;

    private enum BulletState
    {
        Moving,
        Hit,
    };
    BulletState bulletState;

    private void FixedUpdate()
    {
        if (isLaunched && bulletState != BulletState.Hit)
            bulletState = BulletState.Moving;
    }

    void Awake()
    {
        rb= GetComponent<Rigidbody>();
        rb.velocity = Vector3.one;
        rb.useGravity = false;
        isLaunched = false;
    }

    void Update()
    {
        flyingTime += Time.deltaTime;
        if (flyingTime > 5)
            Destroy(this.gameObject);
    }
    
    public void MoveToTarget()
    {
        if(target != null)  //极限情况发射子弹后玩家target可能丢失
        {            
            isLaunched = true;
            direction = (target.transform.position - transform.position + new Vector3(0,1.3f,0)).normalized;
            rb.velocity = direction * 45;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.CompareTag("Player"))//|| other.gameObject.CompareTag("Obstacle"))  TODO:场景障碍物破坏
        {
            direction = (target.transform.position - transform.position + new Vector3(0, 1.3f, 0)).normalized;
            target.GetComponent<Animator>().SetTrigger("Dizzy");
            target.GetComponent<Rigidbody>().mass = 45000;
            target.GetComponent<PlayerController>().enabled = false;
            target.GetComponent<Rigidbody>().velocity = 200 * direction;
            StartCoroutine(ResetKnockBack());
            //other.rigidbody.AddForce(1000f*direction,ForceMode.Impulse);
            Instantiate(hitEffect,transform.position, Quaternion.identity);
            bulletState = BulletState.Hit;
        }
    }

    IEnumerator ResetKnockBack()
    {
        yield return new WaitForSeconds(1f);
        target.GetComponent<PlayerController>().enabled = true;
        target.GetComponent<Rigidbody>().mass = 45;
        Destroy(gameObject, 1f);
    }
}
