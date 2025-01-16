using System.Collections;
using System.Collections.Generic;
using TacticsToolkit;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject target;
    public GameObject newTarget;
    public Vector3 StartingPosition;
    public int damage;

    [SerializeField] private float Duration;
    [SerializeField] private float CurrentTime;
    [SerializeField] private AnimationCurve curveHeight;
    [SerializeField] private float curveMaxHeight;

    private Vector3 targetPosition;

    private void Start()
    {
        StartingPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(target != newTarget)
        {
            SetNewTarget(newTarget);
        }

        if (target != null)
        {
            CurrentTime += Time.deltaTime;
            Vector3 newPosition = Move();
            RotateArrow(newPosition - transform.position);
            transform.position = newPosition;


            if(Vector3.Distance(transform.position, targetPosition) < 1)
            {
                target.GetComponent<Entity>().TakeDamage(damage);
                Destroy(gameObject);
            }
        }

        target = newTarget;
    }

    public void SetNewTarget(GameObject newTarget)
    {
        target = newTarget;
        curveMaxHeight = Vector3.Distance(StartingPosition, newTarget.transform.position) / 8;
        Duration = Vector3.Distance(StartingPosition, newTarget.transform.position) / 15;
        CurrentTime = 0;
        targetPosition = new Vector3(newTarget.transform.position.x, newTarget.transform.position.y + StartingPosition.y, newTarget.transform.position.z);
    }

    public Vector3 Move()
    {
        // move towards the target in an arc, taking a constant amount of time
        var linearT = CurrentTime / Duration;
        // we have to move XZ and Y separately so they dont fight each other
        var movePosition = Vector3.Lerp(GetXZPosition(StartingPosition), GetXZPosition(targetPosition), linearT);
        // in case the start and end Y values are different (which they should be since the turret is higher)
        var baseY = Mathf.Lerp(StartingPosition.y, targetPosition.y, linearT);
        // use the animation curve (upside down parabola) to find the magnitude of the Y change
        var arc = curveMaxHeight * curveHeight.Evaluate(linearT);
        // finally, add it all together
        movePosition.y = baseY + arc;
        return movePosition;
    }

    private Vector3 GetXZPosition(Vector3 position)
    {
        return new Vector3(position.x, 0, position.z);
    }

    private void RotateArrow(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction); 
            targetRotation *= Quaternion.Euler(90, 0, 0);
            transform.rotation = targetRotation;
        }
    }
}
