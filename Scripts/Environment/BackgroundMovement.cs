using UnityEngine;

/* <summary>
Этот класс добавляет параллаксное перемещение заднего фона в игру
</summary> */

public class BackgroundMovement : MonoBehaviour
{
    Camera mainCamera;
    Vector3 lastCameraPosition;

    [Header("Settings")]
    [SerializeField] float resetDistance = 20f; // Дистанция от камеры, при которой кусок фона телепортируется в противоположную сторону
    [SerializeField] float spriteWidth = 20f; // Ширина одного спрайта фона для расчета шага телепортации

    void Start()
    {
        mainCamera = Camera.main;
        lastCameraPosition = mainCamera.transform.position;

        Transform[] children = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++) children[i] = transform.GetChild(i);

        foreach (Transform child in children)
        {
            Instantiate(child.gameObject, child.position + new Vector3(spriteWidth, 0, 0), child.rotation, transform);
        }
    }

    void LateUpdate()
    {
        Vector3 currentCameraPos = mainCamera.transform.position;
        Vector3 deltaPosition = currentCameraPos - lastCameraPosition;

        // Отвечает за перемещение бэкграунда
        foreach (Transform child in transform)
        {
            SpriteRenderer sprite = child.GetComponentInChildren<SpriteRenderer>();
            if (sprite == null) continue;

            float factor = GetParallaxFactor(sprite.sortingOrder);

            child.position += new Vector3(deltaPosition.x * factor, 0f, 0f);

            float relativeDist = child.position.x - currentCameraPos.x;
            
            if (relativeDist < -resetDistance)
                child.position += new Vector3(spriteWidth * 2, 0, 0);
            else if (relativeDist > resetDistance)
                child.position -= new Vector3(spriteWidth * 2, 0, 0);
        }

        lastCameraPosition = currentCameraPos;
    }

    // Возвращает коэфицент скорости перемещения смотря на .sortingOrder объекта
    float GetParallaxFactor(int order)
    {
        return order switch
        {
            -5 => 0.8f, // 0.1 = 10% скорости камеры
            -4 => 0.5f,
            -3 => 0.4f,
            -2 => 0.3f,
            -1 => 0.2f,
            _  => 0f
        };
    }
}