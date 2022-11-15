using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SunnyValleyStudio
{
    public class PlayerInput : MonoBehaviour
    {
        public event Action OnMouseClick, OnFly;
        public bool RunningPressed { get; private set; }
        public Vector3 MovementInput { get; private set; }
        public Vector2 MousePosition { get; private set; }
        public bool IsJumping { get; private set; }

        private void Update()
        {
            GetMouseClick();
            GetMousePosition();
            GetMovementInput();
            GetJumpInput();
            GetRunInput();
            GetFlyInput();
        }

        private void GetMouseClick()
        {
            if (Input.GetMouseButtonDown(0))
            {
                OnFly?.Invoke();
            }
        }

        private void GetMousePosition()
        {
            MousePosition = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        }

        private void GetMovementInput()
        {
            MovementInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        }

        private void GetJumpInput()
        {
            IsJumping = Input.GetButton("Jump");
        }

        private void GetRunInput()
        {
            RunningPressed = Input.GetKey(KeyCode.LeftShift);
        }

        private void GetFlyInput()
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                OnFly?.Invoke();
            }
        }
    }
}

// Source: https://www.youtube.com/watch?v=K-wenGRkGh4&list=PLcRSafycjWFesScBq3JgHMNd9Tidvk9hE&index=11&ab_channel=SunnyValleyStudio