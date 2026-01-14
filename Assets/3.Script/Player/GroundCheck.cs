using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GroundCheck : MonoBehaviour
{
    public bool IsGround { get; private set; }
    public Rigidbody2D UnderPlayerRb { get; private set; }

    private HashSet<Collider2D> groundSet = new HashSet<Collider2D>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.isTrigger) return;

        groundSet.Add(other);
        IsGround = groundSet.Count > 0;

        RecalculateUnderPlayer();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.isTrigger) return;

        groundSet.Remove(other);

        IsGround = groundSet.Count > 0;

        RecalculateUnderPlayer();
    }

    private void RecalculateUnderPlayer()
    {
        UnderPlayerRb = null;

        foreach (var col in groundSet)
        {
            if (col == null) continue;

            if (col.CompareTag("Player"))
            {
                if (col.gameObject != transform.root.gameObject)
                {
                    UnderPlayerRb = col.attachedRigidbody;
                    return; // Player ¿ì¼±
                }
            }
        }
    }
}
