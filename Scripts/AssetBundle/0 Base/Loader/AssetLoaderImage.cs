using System.Collections;
using UnityEngine;
#if NGUI
public class AssetLoaderImage : MonoBehaviour 
{
    [SerializeField] UITexture texTarget;

    string m_filename;
    bool m_isMakePixel;
    GameObject m_objPanel;
    bool m_updateImmediately;

    //
    void Awake()
    {
        if (texTarget == null)
        {
            texTarget = GetComponent<UITexture>();
        }

        if (texTarget != null)
        {
            var _panel = texTarget.gameObject.GetComponentInParent<UIPanel>();
            if (_panel != null)
            {
                m_objPanel = _panel.gameObject;
            }
        }
    }

    public void SetUIImageWithDefault( AssetDefines.AssetObjectEnum type, string name, SaveFlag save_type, bool load_first, bool update_immediately = false, bool makepixel = false )
    {
        Clear();

        m_filename = name;
        m_isMakePixel = makepixel;
        m_updateImmediately = update_immediately;

        AssetManager.Instance.GetImage( name, OnResCallbackImg, ( int )save_type, type, load_first );
    }

    public void SetUIImageNoDefault( string name, SaveFlag save_type, bool load_first, bool update_immediately = false, bool makepixel = false )
    {
        Clear();

        m_filename = name;
        m_isMakePixel = makepixel;
        m_updateImmediately = update_immediately;
        AssetManager.Instance.LoadImage( name, (int)save_type, OnResCallbackImg, load_first );
    }

    public void Clear( bool clear_target = false )
    {
        if (string.IsNullOrEmpty(m_filename) == true)
            return;

        AssetManager.Instance.CancelReserve<delegateImageLoading>(m_filename, OnResCallbackImg);
        m_filename = null;

        if (texTarget != null && clear_target == true)
        {
            texTarget.mainTexture = null;
        }
    }

    //
    void OnResCallbackImg( bool is_default, string fullname, Texture2D tex )
    {
        if (texTarget == null)
        {
            return;
        }

        if (tex == null)
        {
            texTarget.gameObject.SetActive(false);
        }
        else
        {
            texTarget.gameObject.SetActive(true);

            if (m_updateImmediately == true && m_objPanel != null)
            {
                m_objPanel.SendMessage( "FillAllDrawCalls", SendMessageOptions.DontRequireReceiver);
                texTarget.material = null;
            }

            texTarget.mainTexture = tex;

            if (m_isMakePixel == true)
            {
                texTarget.MakePixelPerfect();
            }
        }
    }
	
}
#endif
