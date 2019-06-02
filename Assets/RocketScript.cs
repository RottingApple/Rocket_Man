using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RocketScript : MonoBehaviour
{
    [SerializeField] float mainThrust = 1000f;
    [SerializeField] float rcsThrust = 200f;
    [SerializeField] float levelLoadDelay = 2f;
    [SerializeField] AudioClip mainEngineSound = null;
    [SerializeField] AudioClip deathSound = null;
    [SerializeField] AudioClip levelCompleteSound = null;

    [SerializeField]
    ParticleSystem mainEngineParticle = null;
    [SerializeField]
    ParticleSystem deathParticle = null;
    [SerializeField]
    ParticleSystem levelCompParticle = null;

    Rigidbody rigidBody;
    AudioSource audioSource;
    enum State { Alive, Dying, Transcending}
    State state = State.Alive;
    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        ProcessInput();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (state != State.Alive) return;

        switch (collision.gameObject.tag)
        {
            case "Enemy":
                DyingSequence();
                break;
            case "Finish":
                FinishSequence();
                break;
            default:
                
                break;
        }
    }

    private void DyingSequence()
    {
        state = State.Dying;
        if (audioSource.isPlaying)
            audioSource.Stop();
        audioSource.PlayOneShot(deathSound);
        deathParticle.Play();
        Invoke("RestartScene", levelLoadDelay);
    }

    private void FinishSequence()
    {
        state = State.Transcending;
        audioSource.PlayOneShot(levelCompleteSound);
        levelCompParticle.Play();
        Invoke("RestartScene", levelLoadDelay);
    }

    private void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex =  (currentSceneIndex + 1) % SceneManager.sceneCountInBuildSettings;
        SceneManager.LoadScene(nextSceneIndex);
    }

    private void LoadStartScene()
    {
        SceneManager.LoadScene(0);
    }

    private void RestartScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    private void ProcessInput()
    {
        if (state == State.Alive)
        {
            RespondToThrustInput();
            RespondToRotateInput();
            if (Debug.isDebugBuild)
            {
                RespondToTurnCollisionsOff();
                RespondToNextLevel();
            }
        }
    }

    private void RespondToNextLevel()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadNextScene();
        }
    }

    private void RespondToTurnCollisionsOff()
    {
        if (Input.GetKeyDown(KeyCode.C))
            if (rigidBody.detectCollisions) rigidBody.detectCollisions = false;
            else rigidBody.detectCollisions = true;
    }

    private void RespondToRotateInput()
    {
        rigidBody.freezeRotation = true; //lock physics control
        float rotationThisFrame = Time.deltaTime * rcsThrust;
        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(Vector3.forward * rotationThisFrame);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(-Vector3.forward * rotationThisFrame);
        }
        rigidBody.freezeRotation = false; //resume physics control
    }

    private void RespondToThrustInput()
    {
        float thrustThisFrame =Time.deltaTime * mainThrust;
        if (Input.GetKey(KeyCode.Space))
        {
            ApplyThrust(thrustThisFrame);
        }
        else
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
            mainEngineParticle.Stop();
        }
    }

    private void ApplyThrust(float thrustThisFrame)
    {
        rigidBody.AddRelativeForce(Vector3.up * thrustThisFrame);
        if (!audioSource.isPlaying)
            audioSource.PlayOneShot(mainEngineSound);
        mainEngineParticle.Play();
    }
}
