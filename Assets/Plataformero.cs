using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class RedCircle : MonoBehaviour
{
    public float speed = 4f;
    public float jumpSpeed = 6.8f;

    Rigidbody2D rb;
    int groundContacts;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Spawn()
    {
        var cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.white;
            cam.orthographicSize = 9.5f;
            cam.transform.position = new Vector3(5f, 5f, -10f);
        }
        Won = false;
        BuildLevel("Nivel");
    }

    static Transform levelRoot;

    public static GameObject BuildLevel(string rootName)
    {
        var root = new GameObject(rootName);
        levelRoot = root.transform;

        CreateBox("Suelo", new Vector2(5f, -3.5f), new Vector2(24f, 1f), Color.black);
        CreateBox("Techo", new Vector2(5f, 13.5f), new Vector2(24f, 1f), Color.black);
        CreateBox("ParedIzquierda", new Vector2(-6.5f, 5f), new Vector2(1f, 18f), Color.black);
        CreateBox("ParedDerecha", new Vector2(16.5f, 5f), new Vector2(1f, 18f), Color.black);

        var goal = CreateBox("Meta", new Vector2(15.9f, 11f), new Vector2(0.2f, 4f), Color.red);
        goal.GetComponent<BoxCollider2D>().isTrigger = true;
        goal.AddComponent<GoalZone>();

        var pad = CreateBox("CuadroAzul", new Vector2(7f, 8.25f), new Vector2(2f, 9.5f), Color.blue);
        pad.GetComponent<BoxCollider2D>().isTrigger = true;
        pad.AddComponent<BouncePad>();
        CreateBox("Columna", new Vector2(3f, -2.5f), new Vector2(2f, 1f), Color.black);
        CreateBox("Columna2", new Vector2(7f, -1f), new Vector2(2f, 1f), Color.black);
        CreateBox("Columna3", new Vector2(11f, 0f), new Vector2(2f, 1f), Color.black);
        CreateBox("Columna4", new Vector2(11.5f, 7f), new Vector2(1f, 1f), Color.black);
        CreateBox("Plataforma", new Vector2(14f, 3f), new Vector2(4f, 12f), Color.black);

        var lava = CreateBox("SueloAmarillo", new Vector2(8f, -2.85f), new Vector2(8f, 0.3f), Color.yellow);
        lava.GetComponent<BoxCollider2D>().isTrigger = true;
        lava.tag = "Untagged";
        lava.AddComponent<KillZone>();

        var go = new GameObject("RedCircle");
        go.transform.position = StartPos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite(128);
        sr.color = Color.red;
        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.5f;
        col.sharedMaterial = new PhysicsMaterial2D("SinFriccion") { friction = 0f, bounciness = 0f };
        go.transform.SetParent(levelRoot, true);
        go.AddComponent<RedCircle>();
        return root;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public static bool Won;

    void OnGUI()
    {
        if (!Won) return;
        var style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 64,
            fontStyle = FontStyle.Bold
        };
        style.normal.textColor = Color.white;
        GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "¡Ganaste!", style);
    }

    void Update()
    {
        if (Won)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        var kb = Keyboard.current;
        if (kb == null) return;

        float x = 0f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;

        var v = rb.linearVelocity;
        v.x = x * speed;

        bool jump = kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame;
        if (jump && groundContacts > 0) v.y = jumpSpeed;

        rb.linearVelocity = v;
    }

    void OnCollisionStay2D(Collision2D c) => UpdateGrounded(c);
    void OnCollisionEnter2D(Collision2D c) => UpdateGrounded(c);
    void OnCollisionExit2D(Collision2D c) => groundContacts = 0;

    void UpdateGrounded(Collision2D c)
    {
        groundContacts = 0;
        foreach (var p in c.contacts)
            if (p.normal.y > -0.5f) { groundContacts = 1; break; }
    }

    static readonly Vector3 StartPos = new Vector3(-3f, -2.5f, 0f);

    public void Bounce(float speedUp)
    {
        var v = rb.linearVelocity;
        v.y = speedUp;
        rb.linearVelocity = v;
    }

    public void Respawn()
    {
        transform.position = StartPos;
        rb.linearVelocity = Vector2.zero;
    }

    static GameObject CreateBox(string name, Vector2 pos, Vector2 size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(levelRoot, false);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = color;
        go.AddComponent<BoxCollider2D>(); // tamaño 1x1 del sprite, escalado por localScale
        return go;
    }

    static Sprite CreateSquareSprite()
    {
        var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    static Sprite CreateCircleSprite(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float r = size / 2f;
        for (int py = 0; py < size; py++)
        {
            for (int px = 0; px < size; px++)
            {
                float dx = px + 0.5f - r, dy = py + 0.5f - r;
                float a = Mathf.Clamp01(r - Mathf.Sqrt(dx * dx + dy * dy));
                tex.SetPixel(px, py, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}

// Al tocarla, el círculo vuelve al inicio.
public class KillZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponent<RedCircle>();
        if (player != null) player.Respawn();
    }
}

public class BouncePad : MonoBehaviour
{
    public float bounceSpeed = 10.2f;

    void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponent<RedCircle>();
        if (player != null) player.Bounce(bounceSpeed);
    }
}

public class GoalZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<RedCircle>() != null) RedCircle.Won = true;
    }
}
