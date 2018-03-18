using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlashRenderer : MonoBehaviour {
    private const int slashZPosition = 3;
    public Material slashMaterial;
    public Color c1 = Color.yellow;
    public Color c2 = Color.red;
    private GameObject slashGO;
    private LineRenderer lineRenderer;
    private int linePoints = 0;

    private Vector3 mouseDownPos;
    private Vector3 mouseUpPos;

    void Start () {
        slashGO = new GameObject("Line");
        slashGO.AddComponent<LineRenderer>();
        lineRenderer = slashGO.GetComponent<LineRenderer>();
        lineRenderer.material = slashMaterial;
        // lineRenderer.material = new Material(Shader.Find("Mobile/Particles/Additive"));
        lineRenderer.startColor = c2;
        lineRenderer.endColor = c2;
        lineRenderer.startWidth = .02f;
        lineRenderer.endWidth = 0;
        lineRenderer.positionCount = 0;
        lineRenderer.sortingLayerName = "Slash";
    }
	
	void Update () {
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
            mouseDownPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, slashZPosition);
        }

	    if (Input.GetMouseButtonUp(0))
	    {
	        RemoveSlash();
            mouseUpPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, slashZPosition);
	        AddMouseSlash();
	    }
    }

    private void RemoveSlash()
    {
        lineRenderer.positionCount = 0;
        linePoints = 0;

        BoxCollider2D[] lineColliders = slashGO.GetComponents<BoxCollider2D>();

        foreach (BoxCollider2D b in lineColliders)
        {
            Destroy(b);
        }
    }

    private void AddSlash()
    {
        lineRenderer.positionCount = linePoints + 1;
        Vector3 mPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, slashZPosition);
        lineRenderer.SetPosition(linePoints, Camera.main.ScreenToWorldPoint(mPosition));
        linePoints++;

        BoxCollider2D bc = slashGO.AddComponent<BoxCollider2D>();
        bc.transform.position = lineRenderer.transform.position;
        bc.size = new Vector2(0.1f, 0.1f);
    }

    private void AddMouseSlash()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, Camera.main.ScreenToWorldPoint(mouseDownPos));
        lineRenderer.SetPosition(1, Camera.main.ScreenToWorldPoint(mouseUpPos));

        BoxCollider2D bc = slashGO.AddComponent<BoxCollider2D>();
        bc.transform.position = lineRenderer.transform.position;
        bc.size = new Vector2(1f, 1f);
    }
}
