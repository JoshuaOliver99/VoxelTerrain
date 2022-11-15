using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SunnyValleyStudio
{
    public class PlayerCamera : MonoBehaviour
    {
        [SerializeField]
        private float sensitivity = 300f;
        [SerializeField]
        private Transform playerBody;
        [SerializeField]
        private PlayerInput playerInput;

        float verticalRotation = 0f;

        private void Awake()
        {
            playerInput = GetComponentInParent<PlayerInput>();
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            float mouseX = playerInput.MousePosition.x * sensitivity * Time.deltaTime;
            float mouseY = playerInput.MousePosition.y * sensitivity * Time.deltaTime;

            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -90, 90);

            transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }
}

// Source: https://www.youtube.com/watch?v=K-wenGRk Gh4&list=PLcRSafycjWFesScBq3JgHMNd9Tidvk9hE&index=11&ab_channel=SunnyValleyStudio