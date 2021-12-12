using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIDetector : MonoBehaviour
{
    [Range(1, 15)]
    [SerializeField]
    private float viewRadius = 11;
    [SerializeField]
    private float detectionCheckDelay = 0.1f;
    [SerializeField]
    private Transform target = null;
    [SerializeField]
    private LayerMask playerLayerMask;
    [SerializeField]
    private LayerMask visibilityLayer;

    [field: SerializeField]
    public bool TargetVisible { get; private set; }
    public Transform Target
    {
        get => target;
        set
        {
            target = value;
            TargetVisible = false;
        }
    }

    private void Start()
    {
        StartCoroutine(DetectionCoroutine());
    }

    private void Update()
    {
        if (Target != null)
            TargetVisible = CheckTargetVisible(Target);
    }

    private bool CheckTargetVisible(Transform target)
    {
        var result = Physics2D.Raycast(transform.position, target.position - transform.position, viewRadius, visibilityLayer);
        if (result.collider != null)
        {
            return (playerLayerMask & (1 << result.collider.gameObject.layer)) != 0;
        }
        return false;
    }

    private void DetectTarget()
    {
        /*if (Target == null)
            CheckIfPlayerInRange();
        else if (Target != null)
            DetectIfOutOfRange();
        */
        
        if (Target != null)
            DetectIfOutOfRange();
        CheckIfPlayerInRange();
    }

    private void DetectIfOutOfRange()
    {
        if (Target.gameObject.activeInHierarchy == false || Target.gameObject.activeSelf == false || Vector2.Distance(transform.position, Target.position) > viewRadius + 1)
        {
            Target = null;
        }
    }

    private void CheckIfPlayerInRange()
    {
        /*Collider2D[] collisions = Physics2D.OverlapCircleAll(transform.position, viewRadius, playerLayerMask);
        float minDistance = float.PositiveInfinity;
        if (collisions != null) //&& collision.transform != this.transform.parent)
        {
            foreach (Collider2D collider in collisions)
            {
                if (Vector2.Distance(collider.transform.position, transform.position) < minDistance && CheckTargetVisible(collider.transform))
                {
                    minDistance = Vector2.Distance(collider.transform.position, transform.position);
                    Target = collider.transform;
                    TargetVisible = true;
                }
            }
            //Debug.Log(this.transform.parent.ToString());
        }*/
        for (int i = 1; i <= viewRadius; i++)
        {
            Collider2D collision = Physics2D.OverlapCircle(transform.position, i, playerLayerMask);
            if (collision != null && CheckTargetVisible(collision.transform))
            {
                Target = collision.transform;
                TargetVisible = true;
                break;
            }
        }

    }

    IEnumerator DetectionCoroutine()
    {
        yield return new WaitForSeconds(detectionCheckDelay);
        DetectTarget();
        StartCoroutine(DetectionCoroutine());

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
    }
}
