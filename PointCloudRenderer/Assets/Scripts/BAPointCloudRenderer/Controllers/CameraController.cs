using UnityEngine;

namespace BAPointCloudRenderer.Controllers {
    /*
     * CameraController for flying-controls
     */
    public class CameraController : MonoBehaviour {

        
        public float rotationSpeed = 0.2f;
        public float panSpeed = 0.1f;
        public float zoomSpeed = 0.5f;
        public float minZoomDistance = 5f;
        public float maxZoomDistance = 50f;
        
        #if UNITY_EDITOR
        //Current yaw
        private float yaw = 0.0f;
        //Current pitch
        private float pitch = 0.0f;

        public float normalSpeed = 100;

        void Start() {
            //Hide the cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update() {
            if (Input.GetKey(KeyCode.Escape)) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            if (Input.GetMouseButtonDown(0)) {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        void FixedUpdate() {
            //React to controls. (WASD, EQ and Mouse)
            float moveHorizontal = Input.GetAxis("Horizontal");
            float moveVertical = Input.GetAxis("Vertical");
            float moveUp = Input.GetKey(KeyCode.E) ? 1 : Input.GetKey(KeyCode.Q) ? -1 : 0;

            float speed = normalSpeed;
            if (Input.GetKey(KeyCode.C)) {
                speed /= 10; ;
            } else if (Input.GetKey(KeyCode.LeftShift)) {
                speed *= 5;
            }
            transform.Translate(new Vector3(moveHorizontal * speed * Time.deltaTime, moveUp * speed * Time.deltaTime, moveVertical * speed * Time.deltaTime));

            yaw += 2 * Input.GetAxis("Mouse X");
            pitch -= 2 * Input.GetAxis("Mouse Y");
            transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        }
        #else



    private Vector2 lastTouchPosition;
    private bool isPanning = false;
    private bool isRotating = false;

    void Start() {
       	Application.targetFrameRate = 120;
    }

    void Update()
    {
        if (Input.touchCount == 1) // Single touch for rotation or pan
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                lastTouchPosition = touch.position;
                isRotating = true;
                isPanning = false;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                Vector2 delta = touch.deltaPosition;

                if (isRotating)
                {
                    RotateCamera(delta);
                }
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                isRotating = false;
                isPanning = false;
            }
        }
        else if (Input.touchCount == 2) // Pinch for zoom and pan
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            if (touchZero.phase == TouchPhase.Began || touchOne.phase == TouchPhase.Began)
            {
                lastTouchPosition = (touchZero.position + touchOne.position) / 2;
            }

            if (touchZero.phase == TouchPhase.Moved && touchOne.phase == TouchPhase.Moved)
            {
                // Pan Camera
                Vector2 currentTouchPosition = (touchZero.position + touchOne.position) / 2;
                Vector2 deltaPosition = currentTouchPosition - lastTouchPosition;
                PanCamera(deltaPosition);
                lastTouchPosition = currentTouchPosition;

                // Zoom Camera (Move Forward/Backward)
                float prevTouchDeltaMag = (touchZero.position - touchZero.deltaPosition - (touchOne.position - touchOne.deltaPosition)).magnitude;
                float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;
                float deltaMagnitudeDiff = touchDeltaMag - prevTouchDeltaMag;

                ZoomCamera(deltaMagnitudeDiff);
            }
        }
    }

    void RotateCamera(Vector2 delta)
    {
        float rotationX = delta.x * rotationSpeed * Time.deltaTime;
        float rotationY = delta.y * rotationSpeed * Time.deltaTime;

        transform.eulerAngles += new Vector3(-rotationY, rotationX, 0);
    }

    void PanCamera(Vector2 delta)
    {
        float panX = -delta.x * panSpeed * Time.deltaTime;
        float panY = -delta.y * panSpeed * Time.deltaTime;

        transform.Translate(new Vector3(panX, panY, 0), Space.Self);
    }

    void ZoomCamera(float deltaMagnitudeDiff)
    {
        float zoomAmount = deltaMagnitudeDiff * zoomSpeed * Time.deltaTime;
        Vector3 forwardMove = transform.forward * zoomAmount;

        float currentDistance = Vector3.Distance(transform.position, Vector3.zero);
        float newDistance = Mathf.Clamp(currentDistance - zoomAmount, minZoomDistance, maxZoomDistance);

        // Move the camera forward/backward based on the zoom amount
        transform.position = Vector3.MoveTowards(transform.position, transform.position + forwardMove, Mathf.Abs(currentDistance - newDistance));
    }
        #endif
        
    }

}
