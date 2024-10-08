using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraggablePuzzlePiece : MonoBehaviour
{
    private Vector3 startPosition;  // Starting position in case the piece is not dropped correctly
    private Camera mainCamera;      // Reference to the main camera

    void Start()
    {
        startPosition = transform.position;   // Store the initial position of the puzzle piece
        mainCamera = GameObject.FindGameObjectWithTag("BuildItCam").GetComponent<Camera>();
    }

    private void Update()
    {
        // Check if the puzzle piece is out of bounds of the orthographic camera view
        if (!IsWithinCameraBounds())
        {
            ResetToStartPosition();  // Reset if out of bounds
        }
    }

    // Check if the puzzle piece is within the bounds of the orthographic camera
    private bool IsWithinCameraBounds()
    {
        // Get the orthographic bounds of the camera
        float camHeight = mainCamera.orthographicSize * 2f;        // Height based on orthographic size
        float camWidth = camHeight * mainCamera.aspect;            // Width based on aspect ratio

        Vector3 camPosition = mainCamera.transform.position;       // Camera position
        Vector3 objectPosition = transform.position;               // Puzzle piece position

        // Calculate the boundaries for x and y based on the camera's orthographic size
        float minX = camPosition.x - camWidth / 2f;
        float maxX = camPosition.x + camWidth / 2f;
        float minY = camPosition.y - camHeight / 2f;
        float maxY = camPosition.y + camHeight / 2f;

        // Check if the object's position is within the camera's bounds
        return objectPosition.x >= minX && objectPosition.x <= maxX && objectPosition.y >= minY && objectPosition.y <= maxY;
    }

    // Method to reset the piece to its original starting position
    private void ResetToStartPosition()
    {
        transform.position = startPosition;
    }
}
