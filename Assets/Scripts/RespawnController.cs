using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IRespawnable
{
    void Respawn();
}
public class RespawnController: MonoBehaviour
{
    public static RespawnController Instance { get; private set; }  //单例的全局访问点

    [SerializeField]
    public List<IRespawnable> respawnables = new List<IRespawnable>();

    private void Awake()    //单例
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterRespawnable(IRespawnable respawnable)
    {
        if (!respawnables.Contains(respawnable))
        {
            respawnables.Add(respawnable);
        }
    }

    public void UnregisterRespawnable(IRespawnable respawnable)
    {
        if (respawnables.Contains(respawnable))
        {
            respawnables.Remove(respawnable);
        }
    }

    public void RespawnAll()
    {
        foreach (var respawnable in respawnables)
        {
            respawnable.Respawn();
        }
    }

    public void RespawnSelf(IRespawnable respawnable)
    {
        respawnable.Respawn();
    }
}
