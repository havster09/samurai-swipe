using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.GoapAttributeComponents;
using Assets.Scripts.GoapHeroActions;
using UnityEngine;

public class SlashRenderer : MonoBehaviour
{
    private GoapHeroAction _goapHeroActionScript;
    private const int SlashZPosition = 3;
    public Material SlashMaterial;
    public Color C1 = Color.yellow;
    public Color C2 = Color.red;
    private GameObject _slashGo;
    private LineRenderer _lineRenderer;
    private int _linePoints = 0;

    private Vector3 _mouseDownPos;
    private Vector3 _mouseUpPos;

    public BoxCollider2D SlashCollider;

    void Awake()
    {
        _goapHeroActionScript = FindObjectOfType<GoapHeroAction>();
    }
    void Start()
    {
        _slashGo = new GameObject("Line");
        _slashGo.AddComponent<LineRenderer>();
        _lineRenderer = _slashGo.GetComponent<LineRenderer>();
        _lineRenderer.material = SlashMaterial;
        // lineRenderer.material = new Material(Shader.Find("Mobile/Particles/Additive"));
        _lineRenderer.startColor = C2;
        _lineRenderer.endColor = C2;
        _lineRenderer.startWidth = .02f;
        _lineRenderer.endWidth = 0;
        _lineRenderer.positionCount = 0;
        _lineRenderer.sortingLayerName = "Slash";
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Moved)
            {
                AddSlash();
            }

            if (touch.phase == TouchPhase.Ended)
            {
                RemoveSlash();
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            _mouseDownPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, SlashZPosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            RemoveSlash();
            _mouseUpPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, SlashZPosition);
            AddMouseSlash();
        }
    }

    public void RemoveSlash()
    {
        _lineRenderer.positionCount = 0;
        _linePoints = 0;

        GameObject slashCollider = GameObject.FindGameObjectWithTag("SlashCollider");
        Destroy(slashCollider);
    }

    private void AddSlash()
    {
        _lineRenderer.positionCount = _linePoints + 1;
        Vector3 mPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, SlashZPosition);
        _lineRenderer.SetPosition(_linePoints, Camera.main.ScreenToWorldPoint(mPosition));
        _linePoints++;

        BoxCollider2D bc = _slashGo.AddComponent<BoxCollider2D>();
        bc.transform.position = _lineRenderer.transform.position;
        bc.size = new Vector2(0.1f, 0.1f);
    }

    private void AddMouseSlash()
    {
        _lineRenderer.positionCount = 2;
        _lineRenderer.SetPosition(0, Camera.main.ScreenToWorldPoint(_mouseDownPos));
        _lineRenderer.SetPosition(1, Camera.main.ScreenToWorldPoint(_mouseUpPos));
        AddColliderToLine(_lineRenderer, Camera.main.ScreenToWorldPoint(_mouseDownPos), Camera.main.ScreenToWorldPoint(_mouseUpPos));
        _goapHeroActionScript.NpcTargetAttributes = new List<NpcAttributesComponent>();
    }

    private void AddColliderToLine(LineRenderer line, Vector3 startPoint, Vector3 endPoint)
    {
        SlashCollider = new GameObject("SlashCollider").AddComponent<BoxCollider2D>();
        SlashCollider.tag = "SlashCollider";
        SlashCollider.transform.position = new Vector3(0f, 0f, 0f);
        float lineWidth = .1f;
        float lineLength = Vector2.Distance(startPoint, endPoint);
        SlashCollider.size = new Vector2(lineLength, lineWidth);
        Vector2 midPoint = (startPoint + endPoint) / 2;
        SlashCollider.transform.position = midPoint;
        float angle = Mathf.Atan2((endPoint.y - startPoint.y), (endPoint.x - startPoint.x));
        angle *= Mathf.Rad2Deg;
        SlashCollider.transform.Rotate(0, 0, angle);
    }
}
