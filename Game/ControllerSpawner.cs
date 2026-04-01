using System;
using UnityEngine;

public class ControllerSpawner : MonoBehaviour
{
    public Action<Controller> OnControllerSpawned;
    public Action<Controller> OnControllerDespawned;

    void Awake()
    {

    }

    public Controller SpawnController(string tag, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        GameObject gameObject = Pooler.Instance.SpawnFromPool(tag, spawnPosition, spawnRotation);
        Controller controller = gameObject.GetComponentInChildren<Controller>();
        if (controller != null)
        {
            OnControllerSpawned?.Invoke(controller);
            controller.OnControllerSpawn();
        }

        return controller;
    }

    public void DespawnController(string tag, Controller controller)
    {
        if (controller != null)
        {
            Pooler.Instance.DespawnToPool(tag, controller.gameObject);

            OnControllerDespawned?.Invoke(controller);
        }
    }
}