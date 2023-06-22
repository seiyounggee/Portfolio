using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Track_Basic : MonoBehaviour
{
    [SerializeField] public CollisionChecker collisionChecker = null;

    public enum GroundType 
    {
        Normal, 
        OutOfBoundary
    }
    public GroundType CurrentTrackType = GroundType.Normal;

    private void Awake()
    {
        if (collisionChecker != null)
        {
            collisionChecker._OnCollisionEnter = Event_OnCollisionEnter;
            collisionChecker._OnCollisionStay = Event_OnCollisionStay;
            collisionChecker._OnCollisionExit = Event_OnCollisionExit;
        }

        gameObject.layer = LayerMask.NameToLayer(CommonDefine.LayerName_Ground);
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

                if (CurrentTrackType == GroundType.Normal)
                {
                    pm.SetGroundType(PlayerMovement.TrackType.Normal);
                }
                else if (CurrentTrackType == GroundType.OutOfBoundary)
                {
                    pm.SetGroundType(PlayerMovement.TrackType.OutOfBound);
                }
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

                if (CurrentTrackType == GroundType.Normal)
                {
                    pm.SetGroundType(PlayerMovement.TrackType.Normal);
                    pm.SetUpwardDirection(collision.contacts);
                }
                else if (CurrentTrackType == GroundType.OutOfBoundary)
                {
                    pm.SetGroundType(PlayerMovement.TrackType.OutOfBound);
                }
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

                if (CurrentTrackType == GroundType.Normal)
                {
                    pm.SetGroundType(PlayerMovement.TrackType.None);
                }
                else if (CurrentTrackType == GroundType.OutOfBoundary)
                {
                    pm.SetGroundType(PlayerMovement.TrackType.None);
                }
            }
        }
    }

    private bool IsBlockEvent()
    {
        if (PhaseManager.Instance.CurrentPhase != CommonDefine.Phase.InGame)
            return true;

        if (InGameManager.Instance.gameState == InGameManager.GameState.Initialize
        || InGameManager.Instance.gameState == InGameManager.GameState.IsGameReady
        || InGameManager.Instance.gameState == InGameManager.GameState.StartCountDown
        || InGameManager.Instance.gameState == InGameManager.GameState.EndGame)
            return true;
        else
            return false;

    }
}
