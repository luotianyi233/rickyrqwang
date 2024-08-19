using System.Collections;
using UnityEngine;

public class FloatingBaseController : Switchable
{
    private Vector3 startPoint;
    public float moveDistanceX; // 平台移动的距离
    public float moveDistanceY; // 平台移动的距离
    public float moveDistanceZ; // 平台移动的距离
    public float moveSpeed; // 平台移动的速度

    private Vector3 targetPosition; // 平台的目标位置
    private Coroutine moveCoroutine; // 用于存储当前的移动协程


    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player")|| other.gameObject.CompareTag("Moveable"))
        {
            other.transform.SetParent(transform);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Moveable"))
        {
            other.transform.SetParent(null);
        }
    }

    private void Awake()
    {
        startPoint = transform.position;
        targetPosition = startPoint + new Vector3(moveDistanceX, moveDistanceY, moveDistanceZ);
        RespawnController.Instance.RegisterRespawnable(this);
    }

    public override void Open()
    {
        if (state == SwitchState.OPEN)
            return;
        state = SwitchState.OPEN;
        //向x方向移动
        // 停止当前的移动协程
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        // 启动新的移动协程，向目标位置移动
        moveCoroutine = StartCoroutine(Move(targetPosition));
    }

    public override void Close()
    {
        if (state == SwitchState.CLOSED)
            return;
        state = SwitchState.CLOSED;
        //向x轴反方向移动，到起点停止
        // 停止当前的移动协程
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        // 启动新的移动协程，返回起点位置
         moveCoroutine = StartCoroutine(Move(startPoint));
    }

    public override void Respawn()
    {
        if (state == SwitchState.CLOSED) return;
        Close();
        state = SwitchState.CLOSED;
    }

    private IEnumerator Move(Vector3 destination)
    {
        while (Vector3.Distance(transform.position, destination) > 0.01f)
        {
            Vector3 newPosition = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
            transform.position = newPosition;
            yield return null;
        }

        // 确保平台精确到达目标位置
        transform.position = destination;
    }
}
