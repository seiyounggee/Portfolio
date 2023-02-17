using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowNGUITexture : MonoBehaviour
{
    public enum FlowNGUITextureType
    {
        XMove = 0,
        YMove,
        XYMove
    }

    public float m_Speed = 1f;
    public FlowNGUITextureType m_MoveType = FlowNGUITextureType.XMove;

    private Rect m_OriRect;
    private Rect m_FlowRect;
    private UITexture m_Texture = null;

    void Awake()
    {
        m_Texture = GetComponent< UITexture >();
        if( m_Texture != null )
            m_OriRect = m_Texture.uvRect;
    }

    void OnEnable()
    {
        StopAllCoroutines();
        if( m_Texture != null )
            StartCoroutine( FlowTextureCoroutine() );
    }

    IEnumerator FlowTextureCoroutine()
    {
        m_FlowRect = m_OriRect;
        while( true )
        {
            switch( m_MoveType )
            {
            case FlowNGUITextureType.XMove:
                {
                    m_FlowRect.x += ( Time.smoothDeltaTime * m_Speed );
                    if( m_FlowRect.x > 1f )
                        m_FlowRect.x = 0f;
                    else if( m_FlowRect.x < 0f )
                        m_FlowRect.x = 1f;
                }
                break;
            case FlowNGUITextureType.YMove:
                {
                    m_FlowRect.y += ( Time.smoothDeltaTime * m_Speed );
                    if( m_FlowRect.y > 1f )
                        m_FlowRect.y = 0f;
                    else if( m_FlowRect.y < 0f )
                        m_FlowRect.y = 1f;
                }
                break;
            case FlowNGUITextureType.XYMove:
                {
                    m_FlowRect.x += ( Time.smoothDeltaTime * m_Speed );
                    if( m_FlowRect.x > 1f )
                        m_FlowRect.x = 0f;
                    else if( m_FlowRect.x < 0f )
                        m_FlowRect.x = 1f;

                    m_FlowRect.y += ( Time.smoothDeltaTime * m_Speed );
                    if( m_FlowRect.y > 1f )
                        m_FlowRect.y = 0f;
                    else if( m_FlowRect.y < 0f )
                        m_FlowRect.y = 1f;
                }
                break;
            }

            m_Texture.uvRect = m_FlowRect;
            yield return null;
        }
    }
}
