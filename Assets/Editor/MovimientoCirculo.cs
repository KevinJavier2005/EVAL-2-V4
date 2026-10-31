using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class MovimientoCirculo : MonoBehaviour
{
    public float velocidad = 5f;

    void Update()
    {
        Vector2 dir = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
        var teclado = Keyboard.current;
        if (teclado == null) return;
        if (teclado.wKey.isPressed) dir.y += 1;
        if (teclado.sKey.isPressed) dir.y -= 1;
        if (teclado.aKey.isPressed) dir.x -= 1;
        if (teclado.dKey.isPressed) dir.x += 1;
#else
        if (Input.GetKey(KeyCode.W)) dir.y += 1;
        if (Input.GetKey(KeyCode.S)) dir.y -= 1;
        if (Input.GetKey(KeyCode.A)) dir.x -= 1;
        if (Input.GetKey(KeyCode.D)) dir.x += 1;
#endif
        transform.Translate(dir.normalized * velocidad * Time.deltaTime);
    }
}
