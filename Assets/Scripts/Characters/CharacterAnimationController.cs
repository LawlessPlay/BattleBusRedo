using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TacticsToolkit;
using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
    public int direction = 1;

    public Animator animator;
    public Entity entity;
    public Camera camera;
    public AttackFx basicAttack;
    public Animator attackFX;

    public Vector2 cameraDirection;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        camera = Camera.main;

        if (entity == null)
            entity = GetComponent<Entity>();
    }

    public void Update()
    {
        cameraDirection = CalculateDirection();

        animator.SetFloat("DirectionX", cameraDirection.x);
        animator.SetFloat("DirectionY", cameraDirection.y);

    }

    // Method to calculate the resulting direction
    // Method to calculate the resulting direction based on camera and character's facing direction
    Vector2 CalculateDirection()
    {
        // Get camera's forward direction
        Vector3 cameraForward = camera.transform.forward;
        var sprite = GetComponent<SpriteRenderer>();

        // Ignore Y axis for camera
        cameraForward.y = 0f;
        cameraForward.Normalize();

        // Get character's facing direction (up, down, left, right)
        Vector2 characterForward = entity.facingDirection;
        Vector2 right = new Vector2(-characterForward.y, characterForward.x);

        // Get the dot products for forward and right directions
        float dot = Vector2.Dot(new Vector2(cameraForward.x, cameraForward.z), characterForward);
        float rightDot = Vector2.Dot(new Vector2(cameraForward.x, cameraForward.z), right);

        // Round the results to whole numbers (either -1, 0, or 1)
        var roundRight = SetToWholeNumber(rightDot);
        var roundDot = SetToWholeNumber(dot);

        Vector2 result = new Vector2(roundRight, roundDot);

        // Determine flip type based on the character's facing direction and adjust accordingly
        FlipType flipType = FlipType.NegativeNegative;

        // Facing down (0, -1)
        if (characterForward == new Vector2(0, -1))
        {
            if (result.x == 1 && result.y == 1)
            {
                result = new Vector2(1, 0);
                flipType = FlipType.NegativePositive;
                sprite.flipX = false;
            }
            else if (result.x == 1 && result.y == -1)
            {
                result = new Vector2(0, -1);
                flipType = FlipType.NegativeNegative;
                sprite.flipX = false;
            }
            else if (result.x == -1 && result.y == -1)
            {
                result = new Vector2(-1, 0);
                flipType = FlipType.PositiveNegative;
                sprite.flipX = true;
            }
            else if (result.x == -1 && result.y == 1)
            {
                result = new Vector2(0, 1);
                flipType = FlipType.PositivePositive;
                sprite.flipX = true;
            }
        }
        // Facing up (0, 1)
        else if (characterForward == new Vector2(0, 1))
        {
            if (result.x == 1 && result.y == 1)
            {
                result = new Vector2(1, 0);
                flipType = FlipType.PositiveNegative;
                sprite.flipX = false;
            }
            else if (result.x == 1 && result.y == -1)
            {
                result = new Vector2(0, -1);
                flipType = FlipType.PositivePositive;
                sprite.flipX = false;
            }
            else if (result.x == -1 && result.y == -1)
            {
                result = new Vector2(-1, 0);
                flipType = FlipType.NegativePositive;
                sprite.flipX = true;
            }
            else if (result.x == -1 && result.y == 1)
            {
                result = new Vector2(0, 1);
                flipType = FlipType.NegativeNegative;
                sprite.flipX = true;
            }
        }
        // Facing right (1, 0)
        else if (characterForward == new Vector2(1, 0))
        {
            if (result.x == 1 && result.y == 1)
            {
                result = new Vector2(1, 0);
                flipType = FlipType.NegativeNegative;
                sprite.flipX = false;
            }
            else if (result.x == 1 && result.y == -1)
            {
                result = new Vector2(0, -1);
                flipType = FlipType.PositiveNegative;
                sprite.flipX = false;
            }
            else if (result.x == -1 && result.y == -1)
            {
                result = new Vector2(-1, 0);
                flipType = FlipType.PositivePositive;
                sprite.flipX = true;
            }
            else if (result.x == -1 && result.y == 1)
            {
                result = new Vector2(0, 1);
                flipType = FlipType.NegativePositive;
                sprite.flipX = true;
            }
        }
        // Facing left (-1, 0)
        else if (characterForward == new Vector2(-1, 0))
        {
            if (result.x == 1 && result.y == 1)
            {
                result = new Vector2(1, 0);
                flipType = FlipType.PositivePositive;
                sprite.flipX = false;
            }
            else if (result.x == 1 && result.y == -1)
            {
                result = new Vector2(0, -1);
                flipType = FlipType.NegativePositive;
                sprite.flipX = false;
            }
            else if (result.x == -1 && result.y == -1)
            {
                result = new Vector2(-1, 0);
                flipType = FlipType.NegativeNegative;
                sprite.flipX = true;
            }
            else if (result.x == -1 && result.y == 1)
            {
                result = new Vector2(0, 1);
                flipType = FlipType.PositiveNegative;
                sprite.flipX = true;
            }
        }

        // Update the entity's sprite offset with the selected flip type
        entity.UpdateOffset(FlipVector(entity.spriteOffset, flipType));

        return result;
    }


    private float SetToWholeNumber(float number)
    {
        if (number > 0)
            return 1;

        if(number < 0)
            return -1;

        return number;
    }

    public enum FlipType
    {
        NegativeNegative,
        PositivePositive,
        NegativePositive,
        PositiveNegative
    }

    // Function to flip the vector in different ways based on the FlipType
    public static Vector3 FlipVector(Vector3 input, FlipType flipType)
    {
        switch (flipType)
        {
            case FlipType.NegativeNegative:
                return new Vector3(-Mathf.Abs(input.x), 0, -Mathf.Abs(input.z));

            case FlipType.PositivePositive:
                return new Vector3(Mathf.Abs(input.x), 0, Mathf.Abs(input.z));

            case FlipType.NegativePositive:
                return new Vector3(-Mathf.Abs(input.x), 0, Mathf.Abs(input.z));

            case FlipType.PositiveNegative:
                return new Vector3(Mathf.Abs(input.x), 0, -Mathf.Abs(input.z));

            default:
                return input; // Default case returns the input as is
        }
    }
}
