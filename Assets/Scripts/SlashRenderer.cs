using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.GoapHeroActions;
using UnityEngine;

namespace Assets.Scripts
{
    public class SlashRenderer : MonoBehaviour
    {
        public static SlashRenderer Instance;
        private const int SlashZPosition = 3;
        public Material SlashMaterial;
        public Color C1 = Color.yellow;
        public Color C2 = Color.red;
        private GameObject _slashGo;
        private LineRenderer _lineRenderer;
        private int _linePoints = 0;

        private Vector3 _touchDownPos;
        private Vector3 _touchUpPos;
        public Vector3 LastTouchPos;
        public int CrossSlashCounter;
        private bool _rightTouchDirection;
        public bool RightTouchDirection
        {
            get { return _rightTouchDirection; }
            set
            {
                if (value != _rightTouchDirection)
                {
                    CrossSlashCounter++;
                }
                _rightTouchDirection = value;
                
            }
        }

        private List<Vector3> _linePositions = new List<Vector3>();
        

        public BoxCollider2D SlashCollider;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
            DontDestroyOnLoad(gameObject);
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

                if (touch.phase == TouchPhase.Stationary)
                {
                    Debug.Log("Stationary");
                    // use for power moves power up
                }

                if (touch.phase == TouchPhase.Began)
                {
                    HandleTouchBegan(touch);
                }

                if (touch.phase == TouchPhase.Moved)
                {
                    HandleTouchMoved(touch);
                }

                if (touch.phase == TouchPhase.Ended)
                {
                    HandleTouchEnded(touch);
                }
            }

             if (Input.GetMouseButtonDown(0))
            {
                _touchDownPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, SlashZPosition);
            }

            if (Input.GetMouseButtonUp(0))
            {
                RemoveSlash();
                _touchUpPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, SlashZPosition);
                AddMouseSlash();
            }
        }

        private void HandleTouchBegan(Touch touch)
        {
            Debug.Log("Began");
            CrossSlashCounter = 0;
            _touchDownPos = new Vector3(touch.position.x, touch.position.y, SlashZPosition);
        }

        private void HandleTouchEnded(Touch touch)
        {
            RemoveSlash();
            _touchUpPos = new Vector3(touch.position.x, touch.position.y, SlashZPosition);
            _linePositions.Add(_touchUpPos);

            var startPoint = _linePositions.OrderByDescending(lp => lp.x).FirstOrDefault();
            var endPoint = _linePositions.OrderByDescending(lp => lp.x).LastOrDefault();

            Debug.Log("===========");
            Debug.Log(Vector3.Distance(startPoint, endPoint));
            Debug.Log("===========");

            AddColliderToLine(_lineRenderer, Camera.main.ScreenToWorldPoint(startPoint),
                Camera.main.ScreenToWorldPoint(endPoint));
            _linePositions.Clear();

            Debug.Log("Ended");
            Debug.Log(CrossSlashCounter);
        }

        private void HandleTouchMoved(Touch touch)
        {
            _touchUpPos = new Vector3(touch.position.x, touch.position.y, SlashZPosition);

            RightTouchDirection = _touchDownPos.x < LastTouchPos.x;
            _linePositions.Add(LastTouchPos);

            LastTouchPos = new Vector3(touch.position.x, touch.position.y, SlashZPosition);

            AddSlash();
            Debug.Log("Moved");
        }

        private void AddSlash()
        {
            GoapHeroAction.Instance.ClearAllTargetsFromList();
            _lineRenderer.positionCount = _linePoints + 1;
            Vector3 mPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, SlashZPosition);
            _lineRenderer.SetPosition(_linePoints, Camera.main.ScreenToWorldPoint(mPosition));
            _linePoints++;
        }

        public void RemoveSlash()
        {
            if (_lineRenderer.positionCount == 0)
            {
                return;
            }

            _lineRenderer.positionCount = 0;
            _linePoints = 0;

            GameObject[] slashColliders = GameObject.FindGameObjectsWithTag("SlashCollider");
            foreach (GameObject s in slashColliders)
            {
                Destroy(s);
            }
        }

        private void AddMouseSlash()
        {
            _lineRenderer.positionCount = 2;
            _lineRenderer.SetPosition(0, Camera.main.ScreenToWorldPoint(_touchDownPos));
            _lineRenderer.SetPosition(1, Camera.main.ScreenToWorldPoint(_touchUpPos));
            AddColliderToLine(_lineRenderer, Camera.main.ScreenToWorldPoint(_touchDownPos), Camera.main.ScreenToWorldPoint(_touchUpPos));
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
}
