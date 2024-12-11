using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform bodyTransform;
    [SerializeField] private Rigidbody2D rb;

    [Header("Settings")]
    [SerializeField] private float movementSpeed = 4f;
    [SerializeField] private float turningRate = 30f;

    private Vector2 previousInput;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        inputReader.OnMoveEvent += HandleMove;
    }


    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        inputReader.OnMoveEvent -= HandleMove;

    }

    private void Update()
    {
        if (!IsOwner) return;

        float zRotation = previousInput.x * -turningRate * Time.deltaTime;
        bodyTransform.Rotate(0f, 0f, zRotation);
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        rb.linearVelocity = (Vector2)bodyTransform.up * previousInput.y * movementSpeed;
    }

    private void HandleMove(Vector2 movementInput)
    {
        previousInput = movementInput;
    }
}
