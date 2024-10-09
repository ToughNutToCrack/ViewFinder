using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class Polaroid : MonoBehaviour
{
    public GameObject viewFinder;
    public GameObject polaroidCamera;
    public GameObject film;
    public Transform filmParentT;
    public int photoNumber = 5;
    public TextMeshPro frames;
    public PlayerController playerController;
    public Volume ppProfile;
    public CustomFrustumLocalSpace customFrustumLocalSpace;
    public GameObject objectToRotate;
    Animator filmAnimator;
    Animator animator;
    float checkTimer, waitTime = 0.25f;
    bool isFilmSpawned;
    GameObject spawnedFilm;
    bool isRotating;

    DepthOfField depthOfField;

    void Start() {
        animator = GetComponent<Animator>();
        frames.text = photoNumber.ToString();
        checkTimer = waitTime;
        ppProfile.sharedProfile.TryGet<DepthOfField>(out depthOfField);
        depthOfField.active = false;
        polaroidCamera.SetActive(false);
        viewFinder.SetActive(false);
    }

    void Update() {

        if (Input.GetKeyDown(KeyCode.T))
            Rotate();

        if (!isRotating)
        {
            animator.SetFloat("Velocity", playerController.isMoving ? 1 : 0);
            if (filmAnimator != null) //tocheck
                filmAnimator.SetFloat("Velocity", playerController.isMoving ? 1 : 0);

            checkTimer += Time.deltaTime; 
            if (checkTimer > waitTime)
            {
                if (!isFilmSpawned)
                {
                    if (Input.GetKeyDown(KeyCode.Mouse0) && animator.GetCurrentAnimatorStateInfo(0).IsName("Main"))
                    {
                        polaroidCamera.SetActive(true);
                        viewFinder.SetActive(true);
                        animator.SetTrigger("CameraIn");
                        depthOfField.active = true;
                        checkTimer = 0;
                        isFilmSpawned = false;
                    }
                    else if (Input.GetKeyDown(KeyCode.Mouse0) && animator.GetCurrentAnimatorStateInfo(0).IsName("Look") && photoNumber != 0)
                    {
                        polaroidCamera.SetActive(false);
                        StartCoroutine(DeactivateViewFinder());
                        if (photoNumber>0)
                            photoNumber --;
                        frames.text = photoNumber.ToString();
                        animator.SetTrigger("Film");
                        isFilmSpawned = true;
                        checkTimer = 0;
                        customFrustumLocalSpace.Cut(true);
                    }            
                    else if (Input.GetKeyDown(KeyCode.Mouse1) && animator.GetCurrentAnimatorStateInfo(0).IsName("Look"))
                    {
                        polaroidCamera.SetActive(false);
                        viewFinder.SetActive(false);
                        isFilmSpawned = false;
                        animator.SetTrigger("CameraOut");
                        depthOfField.active = false;
                        checkTimer = 0;
                    }
                }
                else {
                    if (Input.GetKeyDown(KeyCode.Mouse0) && filmAnimator != null && filmAnimator.GetCurrentAnimatorStateInfo(0).IsName("Main"))
                    {
                        filmAnimator.SetTrigger("PolaroidIn");
                        checkTimer = 0;
                    }
                    else if (Input.GetKeyDown(KeyCode.Mouse0) && filmAnimator != null && filmAnimator.GetCurrentAnimatorStateInfo(0).IsName("Look"))
                    {
                        customFrustumLocalSpace.Cut(false);
                        StartCoroutine(FilmOut());
                    }            
                    else if (Input.GetKeyDown(KeyCode.Mouse1) && filmAnimator != null && filmAnimator.GetCurrentAnimatorStateInfo(0).IsName("Look"))
                    {
                        filmAnimator.SetTrigger("PolaroidOut");
                        checkTimer = 0;
                    }
                }
            }
        }
    }

    IEnumerator DeactivateViewFinder() {
        yield return new WaitForSeconds(1f);
        viewFinder.SetActive(false);
        depthOfField.active = false;

    } 

    IEnumerator FilmOut() {
        yield return new WaitForSeconds(0.5f);
        spawnedFilm.SetActive(false);
        checkTimer = 0;
        isFilmSpawned = false;
        animator.SetTrigger("FilmOut");
        // playerController.ChangePlayerState(true);
    } 

    void Rotate() {
        if (spawnedFilm != null)
        {
            StartCoroutine(RotateOnAxis());
        }
    }

    IEnumerator RotateOnAxis() {
        var child = spawnedFilm.transform.GetChild(0);

        var initialRotation = child.localRotation;
        var finalRotation = initialRotation * Quaternion.Euler(0, 0, -30);

        float elapsed = 0;

        while (elapsed < 1) {
            elapsed += Time.deltaTime*2;
            child.localRotation = Quaternion.Slerp(initialRotation, finalRotation, elapsed);
            yield return null;
        }

        objectToRotate.transform.localRotation *= Quaternion.Euler(0, 0, -30);

        child.localRotation = finalRotation;
        isRotating = false;
    }

    public void PrintEvent()
    {
        spawnedFilm = Instantiate(film, filmParentT);
        filmAnimator = spawnedFilm.GetComponent<Animator>();
        spawnedFilm.transform.SetParent(playerController.cameraHolder);
        playerController.MoveIntoFilmPosition(spawnedFilm.transform);
    }
}
