using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UILabel))]
public class FlowNGUILabel : MonoBehaviour
{
    [ReadOnly] public UIPanel parentPanel = null;
    [ReadOnly] public UILabel uILabel = null;

    public MoveType moveType = MoveType.Type_1_Long;
    public float initialWaitTime = 3f;
    public float moveSpeed = 50f;

    private float moveWidth = 0f;
    private float currentMovedLength = 0f;
    private Vector3 originalPosition = Vector3.zero;
    private float initailWaitTimer = 0f;

    public enum MoveType
    { 
        Type_1_Long,
        Type_2_Short,
    }

    private void Awake()
    {
        uILabel = GetComponent<UILabel>();

        if (uILabel != null)
            uILabel.overflowMethod = UILabel.Overflow.ResizeFreely;
    }

    private void Start()
    {
        originalPosition = transform.localPosition;
    }

    private void OnEnable()
    {
        transform.localPosition = originalPosition;

        if (initialWaitTime < 0)
            initialWaitTime = 0.5f;

        if (moveSpeed < 0)
            moveSpeed = 50f;
    }

    void Update()
    {
        if (uILabel == null || parentPanel == null)
            return;

        if (uILabel.width < parentPanel.width)
        {
            transform.localPosition = originalPosition;
            return;
        }

        if (moveType == MoveType.Type_1_Long)
            moveWidth = uILabel.width;
        else if (moveType == MoveType.Type_2_Short)
            moveWidth = (uILabel.width - parentPanel.width) + 10f;


        if (initailWaitTimer < initialWaitTime)
        {
            initailWaitTimer += Time.deltaTime;
        }
        else
        {
            Vector3 currPos = transform.localPosition;
            currPos.x -= moveSpeed * Time.deltaTime;
            currentMovedLength += moveSpeed * Time.deltaTime;

            transform.localPosition = currPos;

            if (moveWidth < currentMovedLength)
            {
                transform.localPosition = originalPosition;
                currentMovedLength = 0f;
                initailWaitTimer = 0f;
            }
        }
    }


}
