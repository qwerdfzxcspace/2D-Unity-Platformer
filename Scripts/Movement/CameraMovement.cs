using System;
using Unity.VisualScripting;
using UnityEngine;

/* <summary>
Этот класс отвечает за передвежением камера за персонажем
</summary> */

public class CameraMovement : MonoBehaviour
{
    private Camera mainCamera;
    private float positionY = 0;
    [SerializeField] private GameObject Character;
    [SerializeField] private GameObject MinCamera; // Самое крайне-левое положение камеры которое может быть
    [SerializeField] private GameObject MaxCamera; // Самое крайне-правое положение камеры которое может быть

    public void Start()
    {
        mainCamera = Camera.main;
    }

    public void Update()
    {
        /*if (Character.transform.position.y > 0)
        {
            positionY = Character.transform.position.y;
        }
        else
        {
            positionY = mainCamera.transform.position.y;
        }*/

        if (Character.transform.position.x > MinCamera.transform.position.x &&
            Character.transform.position.x < MaxCamera.transform.position.x)
        {
            mainCamera.transform.position = new Vector3(Character.transform.position.x, positionY, -10);
        }
    }
}
