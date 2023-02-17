using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ground_Basic : MonoBehaviour
{
    [SerializeField] CollisionChecker collisionChecker = null;

    public enum GroundType { Normal }
    public GroundType CurrentGroundType = GroundType.Normal;

    private void Awake()
    {
        if (collisionChecker != null)
        {
            collisionChecker._OnCollisionEnter = Event_OnCollisionEnter;
            collisionChecker._OnCollisionStay = Event_OnCollisionStay;
            collisionChecker._OnCollisionExit = Event_OnCollisionExit;
        }
    }

    private void Event_OnCollisionEnter(Collision collision)
    {
        if (IsBlockEvent())
            return;

        if (collision.collider.gameObject.CompareTag(CommonDefine.TAG_NetworkPlayer_CollisionChecker))
        {
            var cc = collision.collider.gameObject.GetComponent<PlayerCollisionChecker>();
            if (cc != null && cc.currentType == PlayerCollisionChecker.ColliderType.Ground)
            {
                var pm = cc.pm;
                if (pm == null)
                    return;

                if (CurrentGroundType == GroundType.Normal)
                    pm.SetIsGrounded(true);
            }
        }
    }

    private void Event_OnCollisionStay(Collision collision)
    {
        if (IsBlockEvent())
            return;

        if (collision.collider.gameObject.CompareTag(CommonDefine.TAG_NetworkPlayer_CollisionChecker))
        {
            var cc = collision.collider.gameObject.GetComponent<PlayerCollisionChecker>();
            if (cc != null && cc.currentType == PlayerCollisionChecker.ColliderType.Ground)
            {
                var pm = cc.pm;
                if (pm == null)
                    return;

                if (CurrentGroundType == GroundType.Normal)
                    pm.SetIsGrounded(true);
            }
        }
    }

    private void Event_OnCollisionExit(Collision collision)
    {
        if (IsBlockEvent())
            return;

        if (collision.collider.gameObject.CompareTag(CommonDefine.TAG_NetworkPlayer_CollisionChecker))
        {
            var cc = collision.collider.gameObject.GetComponent<PlayerCollisionChecker>();
            if (cc != null && cc.currentType == PlayerCollisionChecker.ColliderType.Ground)
            {
                var pm = cc.pm;
                if (pm == null)
                    return;

                if (CurrentGroundType == GroundType.Normal)
                    pm.SetIsGrounded(false);
            }
        }
    }

    private bool IsBlockEvent()
    {
        if (PhaseManager.Instance.CurrentPhase != CommonDefine.Phase.InGame)
            return true;

        if (InGameManager.Instance.gameState == InGameManager.GameState.Initialize
        || InGameManager.Instance.gameState == InGameManager.GameState.IsReady
        || InGameManager.Instance.gameState == InGameManager.GameState.StartCountDown
        || InGameManager.Instance.gameState == InGameManager.GameState.EndGame)
            return true;
        else
            return false;

    }
}
