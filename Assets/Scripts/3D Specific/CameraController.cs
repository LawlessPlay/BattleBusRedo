using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

namespace TacticsToolkit
{
    public class CameraController : MonoBehaviour
    {
        [Header("Movement")]
        private bool isMoving;
        public int movementSpeed = 16;
        private Vector3 desiredPosition;

        public Vector2 TopBounds = Vector2.zero;
        public Vector2 BottomBounds = Vector2.zero;
        public Vector2 LeftBounds = Vector2.zero;
        public Vector2 RightBounds = Vector2.zero;


        [Header("Zooming")]
        public float zoomSpeed = 4.0f;
        public float size = 10.0f;
        public float smoothing = 5.0f;
        public float minSize = 2.0f;
        public float maxSize = 20.0f;


        [Header("Rotation")]
        public float rotationSpeed = 90.0f;
        private bool isRotating;


        [Header("Target")]
        public Transform target;
        private IEnumerator activeMoveToTarget;


        [Header("Easing")]
        public AnimationCurve easeCurve;

        private void Start()
        {
            // Set the desired position and rotation to the current values
            desiredPosition = transform.position;
        }

        private void Update()
        {
            UpdatePixelPerfectZoom();
            MoveCamera(); 


            if (Input.GetKeyDown(KeyCode.E) && !isRotating)
            {
                StartCoroutine(StartRotation(1));
            }
            if (Input.GetKeyDown(KeyCode.Q) && !isRotating)
            {
                StartCoroutine(StartRotation(-1));
            }
        }

        private void MoveCamera()
        {
            float horizontalInput = Input.GetAxis("Horizontal"); // A & D keys
            float verticalInput = Input.GetAxis("Vertical");     // W & S keys

            // Get camera's forward and right vectors in world space
            Vector3 cameraForward = Camera.main.transform.forward;
            cameraForward.y = 0f; // Ignore Y axis for movement
            cameraForward.Normalize();

            Vector3 cameraRight = Camera.main.transform.right;
            cameraRight.y = 0f; // Ignore Y axis for movement
            cameraRight.Normalize();

            // Stop target-based movement if manual input is detected
            if (verticalInput != 0 || horizontalInput != 0 && activeMoveToTarget != null)
            {
                isRotating = false;
            }

            // Calculate movement based on input
            Vector3 movement = Vector3.zero;
            if (verticalInput > 0)
            {
                movement += cameraForward * movementSpeed * Time.deltaTime;
            }
            if (verticalInput < 0)
            {
                movement -= cameraForward * movementSpeed * Time.deltaTime;
            }
            if (horizontalInput > 0)
            {
                movement += cameraRight * movementSpeed * Time.deltaTime;
            }
            if (horizontalInput < 0)
            {
                movement -= cameraRight * movementSpeed * Time.deltaTime;
            }

            // Move the camera
            transform.Translate(movement, Space.World);

            // Transform camera position to the local space of an unrotated reference frame
            Vector3 cameraPosition = transform.position;

            // Define a reference rotation (inverse of the 45-degree camera rotation)
            Quaternion inverseRotation = Quaternion.Euler(0, -45f, 0);

            // Apply the inverse rotation to the camera's position to bring it into local space
            Vector3 localPosition = inverseRotation * cameraPosition;

            // Clamp the local position within the bounds (now axis-aligned)
            localPosition.x = Mathf.Clamp(localPosition.x, LeftBounds.x, RightBounds.x);
            localPosition.z = Mathf.Clamp(localPosition.z, BottomBounds.y, TopBounds.y);

            // Convert the clamped local position back to world space by applying the original rotation
            Vector3 clampedPosition = Quaternion.Euler(0, 45f, 0) * localPosition;

            // Apply the clamped world space position back to the camera
            transform.position = clampedPosition;
        }

        private IEnumerator StartRotation(int dir)
        {
            isRotating = true;

            // Calculate the target rotation based on the specified direction
            Quaternion targetRotation = Quaternion.Euler(30f, transform.eulerAngles.y + dir * 90f, transform.rotation.z);

            float duration = 0.5f;  // Adjust based on how long you want the rotation to take
            float elapsedTime = 0f;
            Quaternion startRotation = transform.rotation;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;

                // Evaluate the curve to get the eased interpolation factor
                float easedT = easeCurve.Evaluate(t);

                // Slerp the rotation using the eased time
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, easedT);
                yield return null;
            }

            transform.rotation = targetRotation;
            isRotating = false;
        }

        private void UpdateZoom()
        {
            // Zoom the camera in and out based on scroll wheel input
            float zoom = Input.GetAxis("Mouse ScrollWheel");
            if (zoom != 0f)
            {
                size = Mathf.Round(Mathf.Clamp(size - (zoom * zoomSpeed), minSize, maxSize));
                GetComponent<Camera>().orthographicSize = size;
            }
        }

        public int minZoom = 15;
        public int maxZoom = 25;
        public int minZoomRotation = 20;
        public int maxZoomRotation = 30;

        float smooth = 5.0f;
        private void UpdatePixelPerfectZoom()
        {
            var ppCam = GetComponent<PixelPerfectCamera>();
            int currentSize = ppCam.assetsPPU;
            float currentX = transform.rotation.eulerAngles.x;

            // Zoom the camera in and out based on scroll wheel input
            float zoom = Input.GetAxis("Mouse ScrollWheel");
            if (zoom != 0f)
            {
                currentSize = Mathf.Clamp(currentSize - Mathf.RoundToInt(zoom * zoomSpeed), minZoom, maxZoom);
                currentX = Mathf.Clamp(currentX - Mathf.Round(zoom * zoomSpeed), minZoomRotation, maxZoomRotation);
                GetComponent<PixelPerfectCamera>().assetsPPU = currentSize;

                var percentage = CalculatePercentage(currentSize, minZoom, maxZoom);

                var xRotation = CalculateValueFromPercentage(percentage, minZoomRotation, maxZoomRotation);

                Quaternion target = Quaternion.Euler(xRotation, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
                transform.rotation = target;
            }
        }

        public static float CalculatePercentage(float currentValue, float min, float max)
        {
            if (min == max)
            {
                Debug.LogWarning("Min and Max values are equal. Returning 0 to avoid division by zero.");
                return 0f;
            }

            float clampedValue = Mathf.Clamp(currentValue, min, max);
            return ((clampedValue - min) / (max - min)) * 100f;
        }

        public static float CalculateValueFromPercentage(float percentage, float min, float max)
        {
            if (min == max)
            {
                Debug.LogWarning("Min and Max values are equal. Returning Min value as result.");
                return min;
            }

            float clampedPercentage = Mathf.Clamp01(percentage / 100f);
            return Mathf.Lerp(min, max, clampedPercentage);
        }

        private IEnumerator MoveToTarget()
        {
            // Calculate the desired position based on the target's position
            Vector3 startPosition = transform.position;
            Vector3 desiredPosition = target.position;

            float duration = 1.0f;  // Adjust based on how long you want the movement to take
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;

                // Evaluate the curve to get the eased interpolation factor
                float easedT = easeCurve.Evaluate(t);

                // Lerp the position using the eased time
                transform.position = Vector3.Lerp(startPosition, desiredPosition, easedT);
                yield return null;
            }

            // Ensure the final position is exactly the desired position
            transform.position = desiredPosition;
            isMoving = false;
        }

        // Function to focus the camera on a new target
        public void FocusOnTarget(GameObject newTarget)
        {
            target = newTarget.transform;
            isMoving = true;
            activeMoveToTarget = MoveToTarget();
            StartCoroutine(activeMoveToTarget);
        }
    }
}