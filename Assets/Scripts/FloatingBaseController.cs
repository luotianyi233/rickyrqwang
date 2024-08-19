using System.Collections;
using UnityEngine;

public class FloatingBaseController : Switchable
{
    private Vector3 startPoint;
    public float moveDistanceX; // ƽ̨�ƶ��ľ���
    public float moveDistanceY; // ƽ̨�ƶ��ľ���
    public float moveDistanceZ; // ƽ̨�ƶ��ľ���
    public float moveSpeed; // ƽ̨�ƶ����ٶ�

    private Vector3 targetPosition; // ƽ̨��Ŀ��λ��
    private Coroutine moveCoroutine; // ���ڴ洢��ǰ���ƶ�Э��


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
        //��x�����ƶ�
        // ֹͣ��ǰ���ƶ�Э��
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        // �����µ��ƶ�Э�̣���Ŀ��λ���ƶ�
        moveCoroutine = StartCoroutine(Move(targetPosition));
    }

    public override void Close()
    {
        if (state == SwitchState.CLOSED)
            return;
        state = SwitchState.CLOSED;
        //��x�ᷴ�����ƶ��������ֹͣ
        // ֹͣ��ǰ���ƶ�Э��
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        // �����µ��ƶ�Э�̣��������λ��
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

        // ȷ��ƽ̨��ȷ����Ŀ��λ��
        transform.position = destination;
    }
}
