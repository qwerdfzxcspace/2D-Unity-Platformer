using UnityEngine;

/* <summary>
Этот класс отвечает за проверку того, стоит ли персонаж на земле.
</summary> */

public class Groundcheck : MonoBehaviour
{
    public delegate void OnGroundDelegate(GameObject other, float groundcontacts);
    public static event OnGroundDelegate OnGround;

    public int groundcontacts = 0; // кол-во прикосновений к тайлам

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Collissions"))
        {
            groundcontacts++;
            OnGround?.Invoke(collision.gameObject, groundcontacts); // оповещает остальные скрипты о том, что персонаж на полу
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Collissions"))
        {
            groundcontacts--;
            if (groundcontacts <= 0)
            {
                groundcontacts = 0;
                OnGround?.Invoke(collision.gameObject, groundcontacts); // оповещает остальные скрипты о том, что персонаж не на полу
            }
        }
    }
}
