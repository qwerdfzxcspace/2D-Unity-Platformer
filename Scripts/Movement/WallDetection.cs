using UnityEngine;

/* <summary>
Этот класс отвечает за проверку того, находится ли он у стены.
</summary> */

public class wallcheck : MonoBehaviour
{
    public delegate void OnWallDelegate(GameObject other, float wallcontacts);
    public static event OnWallDelegate OnWall;

    public int wallcontacts = 0; // кол-во прикосновений к стенам

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Collissions"))
        {
            wallcontacts++;
            OnWall?.Invoke(collision.gameObject, wallcontacts); // оповещает остальные скрипты о том, что персонаж у стены
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Collissions"))
        {
            wallcontacts--;
            if (wallcontacts <= 0)
            {
                wallcontacts = 0;
                OnWall?.Invoke(collision.gameObject, wallcontacts); // оповещает остальные скрипты о том, что персонаж не у стены
            }
        }
    }
}
