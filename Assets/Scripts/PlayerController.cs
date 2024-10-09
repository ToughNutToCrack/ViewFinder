using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class PlayerController : MonoBehaviour
{
    public float speed = 6.0f;
    public float mouseSensitivity = 100.0f;
    public Transform cameraHolder;
    public bool isMoving;
    public Transform filmPos;
    public Volume ppProfile;
    
    private CharacterController characterController;
    private float verticalVelocity = 0.0f;
    private float gravity = -9.81f;
    private float jumpHeight = 1.5f;
    private float xRotation = 0f;
    bool isPlayerActive;
    LensDistortion lensDistortion;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        isPlayerActive = true;
        ppProfile.sharedProfile.TryGet<LensDistortion>(out lensDistortion);
        lensDistortion.active = false;
    }

    void Update()
    {
        if (isPlayerActive)
        {
            LookAround();
            Move();
        }
    }

    public void ChangePlayerState(bool isActive) {
        isPlayerActive = isActive;
    }

    void LookAround()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void Move()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        isMoving = moveX != 0 || moveZ != 0f;

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        
        // Apply gravity
        if (characterController.isGrounded)
        {
            verticalVelocity = -2f;
            if (Input.GetButtonDown("Jump"))
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 velocity = move * speed + Vector3.up * verticalVelocity;
        characterController.Move(velocity * Time.deltaTime);
    }

    public void MoveIntoFilmPosition(Transform t)
    {
        StartCoroutine(MoveFilm(t));
    }

    IEnumerator MoveFilm(Transform t) 
    {
        t.GetComponent<Animator>().enabled = false;

        var direction = filmPos.position - t.position;
        var prevMag = direction.magnitude + 0.01f;

        while (Vector3.Distance(filmPos.position, t.position) > 0.01f) {

            var dir = filmPos.position - t.position;
            if (dir.magnitude < prevMag)
            {
                prevMag = dir.magnitude;
                t.position += direction * Time.deltaTime * 1.5f;
                t.rotation = Quaternion.RotateTowards(t.rotation, filmPos.rotation, 5);
                yield return null;
            }
            else 
            {
                t.position = filmPos.position;
                t.rotation = filmPos.rotation;
                yield return null;
            }
        }

        t.GetComponent<Animator>().enabled = true;
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.name.Contains("Ending"))
            EndGame();
    }

    void EndGame() {
        if (!lensDistortion.active)
        {
            lensDistortion.active = true;
            StartCoroutine(Ending());
        }
    }

    IEnumerator Ending() {
        float x = 0;
        while (x < 0.5f)
        {
            x+=Time.deltaTime / 6;
            lensDistortion.intensity.value = x;
            yield return null;
        }
    }

    void OnDisable() {
        lensDistortion.intensity.value = 0;
    }
}
