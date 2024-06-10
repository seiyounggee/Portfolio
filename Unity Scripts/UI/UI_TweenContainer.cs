using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UI_TweenContainer : UIBase
{
    [SerializeField] RawImage tweenObjImage = null;

    private List<RawImage> tweenObjList = new List<RawImage>();

    private bool isTweenFinished = false;

    private const int TWEEN_OBJ_COUNT_TOTAL = 20;

    private void Awake()
    {
        tweenObjImage.SafeSetActive(false);

        for (int i = 0; i < TWEEN_OBJ_COUNT_TOTAL; i++)
        {
            var obj = GameObject.Instantiate(tweenObjImage, this.transform);
            obj.transform.localScale = Vector3.one;
            obj.SafeSetActive(false);
            tweenObjList.Add(obj);
        }
    }

    public override void Show()
    {
        base.Show();


    }

    public void StopTween()
    {
        isTweenFinished = true;
        foreach (var i in tweenObjList)
        {
             i.SafeSetActive(false);
        }

        gameObject.SafeSetActive(false);
    }

    public void Tween(Texture texture, Transform startTrans, Transform endTrans,
        int startValue, int endValue,
        Action<int> interval_callback = null,
        Action final_callback = null)
    {
        gameObject.SafeSetActive(true);
        transform.SetAsLastSibling();
        isTweenFinished = false;

        //Set Posiiton
        foreach (var i in tweenObjList)
        {
            i.texture = texture;
            i.transform.position = startTrans.transform.position;
            i.SafeSetActive(false);
        }

        UtilityCoroutine.StartCoroutine(ref tweenCoroutine, TweenCoroutine(startTrans, endTrans, startValue, endValue, interval_callback, final_callback), this);
    }

    private IEnumerator tweenCoroutine = null;
    private IEnumerator TweenCoroutine(Transform startTrans, Transform endTrans,
        int startValue, int endValue,
        Action<int> interval_callback = null,
        Action final_callback = null)
    {
        yield return null; //1프레임 기다리고 시작하자

        int groupCounter = 0; //5개씩 묶어서 보여주자
        int count = tweenObjList.Count;
        int intervalValue = (endValue - startValue) / count;
        for (int i = 0; i < tweenObjList.Count; i++)
        {
            tweenObjList[i].transform.position = startTrans.transform.position;
            tweenObjList[i].SafeSetActive(true);

            Vector3 randomPos = new Vector3(UnityEngine.Random.Range(-50f, 50f), UnityEngine.Random.Range(-50f, 50f), 0f);
            Sequence tweenSequence = DOTween.Sequence();

            if (i != count - 1)
            {
                tweenSequence.Append(tweenObjList[i].transform.DOMove(startTrans.transform.position + randomPos, 0.3f));
                tweenSequence.Append(tweenObjList[i].transform.DOMove(endTrans.transform.position, 0.4f).OnComplete(() =>
                {
                    interval_callback?.Invoke(startValue + intervalValue * i);
                }));
            }
            else //마지막!
            {
                tweenSequence.Append(tweenObjList[i].transform.DOMove(startTrans.transform.position + randomPos, 0.3f));
                tweenSequence.Append(tweenObjList[i].transform.DOMove(endTrans.transform.position, 0.4f).OnComplete(() =>
                {
                    final_callback?.Invoke();
                    isTweenFinished = true;
                }));
            }

            ++groupCounter;

            if (groupCounter < 5)
                continue;
            else
            {
                groupCounter = 0;
                yield return new WaitForSeconds(0.15f);
            }
        }

        while (isTweenFinished == false)
        {
            yield return null;
        }

        Hide();
    }
}
