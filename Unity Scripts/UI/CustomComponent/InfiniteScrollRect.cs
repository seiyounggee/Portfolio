﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu("UI/Infinite Scroll Rect", 37)]
public class InfiniteScrollRect : ScrollRect
{
    public Func<int, bool> onVerifyIndex;

    [SerializeField]
    private float m_buffer = 100f;

    [SerializeField]
    private bool m_autoInactive = false;

    [SerializeField]
    private bool m_loopMode = true;

    [SerializeField]
    private bool m_snapshotMode = false;

    /// <summary>
    /// Key: Content
    /// Value: Index
    /// </summary>
    private Dictionary<RectTransform, int> m_contentsForLoopMode;
    private RectTransform[] m_contentsForSnapshotMode;
    private List<RectTransform> m_autoInactives;
    private Vector2 m_delta;
    private bool m_isDrag;
    private bool m_isUpdate;
    private int m_indexMin;
    private int m_indexMax;
    private LayoutGroup m_layoutGroup;

    protected override void Awake()
    {
        base.Awake();

        onValueChanged.AddListener(OnValueChanged);

        m_autoInactives = new List<RectTransform>();
        m_contentsForLoopMode = new Dictionary<RectTransform, int>();
        m_contentsForSnapshotMode = new RectTransform[content.childCount];

        m_layoutGroup = content.GetComponent<LayoutGroup>();

        Init();
    }

    private void Init()
    {
        m_contentsForLoopMode.Clear();
        m_contentsForSnapshotMode.Clone();

        for (int i = 0; i < content.childCount; i++)
        {
            m_contentsForLoopMode.Add((RectTransform)content.GetChild(i), i);
            m_contentsForSnapshotMode[i] = (RectTransform)content.GetChild(i);
        }

        m_indexMin = 0;
        m_indexMax = content.childCount;

        if (vertical)
        {
            SetNormalizedPosition(1, 1);
        }

        if (horizontal)
        {
            SetNormalizedPosition(0, 0);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        onValueChanged.RemoveAllListeners();
        onVerifyIndex = default;
        m_contentsForLoopMode?.Clear();
        m_contentsForLoopMode = default;

        for (int i = 0; i < m_contentsForSnapshotMode.Length; i++)
        {
            m_contentsForSnapshotMode[i] = default;
        }
        m_contentsForSnapshotMode = default;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        m_isUpdate = true;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        m_isDrag = false;
    }

    private void Update()
    {
        if (m_autoInactive && m_autoInactives.Any())
        {
            foreach (var target in m_autoInactives)
            {
                target.gameObject.SetActive(onVerifyIndex?.Invoke(m_contentsForLoopMode[target]) ?? true);

                ExecuteEvents.Execute<IInfiniteScrollContent>(target.gameObject, null, (handler, data) =>
                {
                    handler.Update(m_contentsForLoopMode[target]);
                });
            }
            m_autoInactives.Clear();
        }

        if (m_isUpdate)
        {
            m_isUpdate = false;
            UpdateContent();
        }
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
        m_isDrag = true;
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        m_delta = Vector2.zero;
        m_isDrag = false;
        base.OnEndDrag(eventData);
    }

    protected override void SetContentAnchoredPosition(Vector2 position)
    {
        // 계산 순서상 반드시 base보다 먼저 해야함
        position.x -= m_delta.x;
        position.y -= m_delta.y;
        base.SetContentAnchoredPosition(position);
    }

    private void UpdateContent()
    {
        if (m_contentsForLoopMode == default)
        {
            return;
        }

        for (int i = 0; i < content.childCount; i++)
        {
            RectTransform target = (RectTransform)content.GetChild(i);

            if (m_autoInactive)
            {
                target.gameObject.SetActive(onVerifyIndex?.Invoke(m_contentsForLoopMode[target]) ?? true);
            }
            else
            {
                target.gameObject.SetActive(true);
            }

            ExecuteEvents.Execute<IInfiniteScrollContent>(target.gameObject, null, (handler, data) =>
            {
                handler.Update(m_contentsForLoopMode[target]);
            });
        }
    }

    private void OnValueChanged(Vector2 value)
    {
        if (m_loopMode)
        {
            UpdateVerticalLoopMode();
            UpdateHorizontalLoopMode();
        }
        else if (m_snapshotMode)
        {
            UpdateVerticalSnapshotMode();
        }
    }

    private void UpdateVerticalLoopMode()
    {
        if (!vertical)
        {
            return;
        }

        if (velocity.y > 0)
        {
            var targets = m_contentsForLoopMode.Where(t =>
            {
                bool result = t.Key.offsetMin.y > content.InverseTransformPoint(viewport.position).y + viewport.rect.yMax + m_buffer;
                return result;
            }).OrderBy(t => t.Value);

            bool isUpdated = targets.Any(t =>
            {
                var result = onVerifyIndex?.Invoke(t.Value + content.childCount) ?? true;
                return result;
            });

            if (isUpdated)
            {
                int lastIndex = m_contentsForLoopMode.Max(t => t.Value);
                foreach (var target in targets)
                {
                    m_contentsForLoopMode[target.Key] = ++lastIndex;
                    target.Key.SetAsLastSibling();

                    if (m_autoInactive)
                    {
                        m_autoInactives.Add(target.Key);
                    }
                    else
                    {
                        target.Key.gameObject.SetActive(true);

                        ExecuteEvents.Execute<IInfiniteScrollContent>(target.Key.gameObject, null, (handler, data) =>
                        {
                            handler.Update(lastIndex);
                        });
                    }
                }

                Vector3 pos = content.position;
                RectTransform child = (RectTransform)content.GetChild(0);
                Vector2 childWorldTopPos = content.TransformPoint(child.offsetMax);
                Vector3 localPos = content.parent.InverseTransformPoint(childWorldTopPos);

                if (m_isDrag)
                {
                    m_delta += content.offsetMax - (Vector2)localPos;
                }

                localPos.x = content.localPosition.x;

                content.localPosition = localPos;
            }
        }
        else if (velocity.y < 0)
        {
            var targets = m_contentsForLoopMode.Where(t =>
            {
                bool result = t.Key.offsetMax.y < content.InverseTransformPoint(viewport.position).y + viewport.rect.yMin - m_buffer;
                return result;
            }).OrderByDescending(t => t.Value);

            bool isUpdated = targets.Any(t =>
            {
                var result = onVerifyIndex?.Invoke(t.Value - content.childCount) ?? true;
                return result;
            });

            if (isUpdated)
            {
                int firstIndex = m_contentsForLoopMode.Min(t => t.Value);
                foreach (var target in targets)
                {
                    m_contentsForLoopMode[target.Key] = --firstIndex;
                    target.Key.SetAsFirstSibling();

                    if (m_autoInactive)
                    {
                        m_autoInactives.Add(target.Key);
                    }
                    else
                    {
                        target.Key.gameObject.SetActive(true);

                        ExecuteEvents.Execute<IInfiniteScrollContent>(target.Key.gameObject, null, (handler, data) =>
                        {
                            handler.Update(firstIndex);
                        });
                    }
                }

                RectTransform child = (RectTransform)content.GetChild(content.childCount - 1);
                Vector2 childWorldButtomPos = content.TransformPoint(child.offsetMin);
                Vector3 localPos = content.parent.InverseTransformPoint(childWorldButtomPos);

                if (m_isDrag)
                {
                    m_delta += content.offsetMin - (Vector2)localPos;
                }

                localPos.x = content.localPosition.x;
                localPos.y += content.rect.height;

                content.localPosition = localPos;
            }
        }
    }

    private void UpdateVerticalSnapshotMode()
    {
        if (velocity.y > 0)
        {
            var targets = m_contentsForSnapshotMode.Where(t =>
            {
                bool result = t.offsetMin.y > content.InverseTransformPoint(viewport.position).y + viewport.rect.yMax + m_buffer;
                return result;
            });

            bool isUpdated = false;

            for (int i = m_indexMax + 1; i <= m_indexMax + targets.Count(); i++)
            {
                isUpdated = onVerifyIndex?.Invoke(i) ?? true;
                if (isUpdated)
                {
                    break;
                }
            }

            if (isUpdated)
            {
                m_indexMin += targets.Count();
                m_indexMax += targets.Count();

                for (int targetIndex = 0, dataIndex = m_indexMin; targetIndex < m_contentsForSnapshotMode.Length; targetIndex++, dataIndex++)
                {
                    RectTransform target = m_contentsForSnapshotMode[targetIndex];

                    target.gameObject.SetActive(!m_autoInactive || (onVerifyIndex?.Invoke(dataIndex) ?? true));

                    ExecuteEvents.Execute<IInfiniteScrollContent>(target.gameObject, null, (handler, data) =>
                    {
                        handler.Update(dataIndex);
                    });
                }

                Vector3 pos = content.position;
                RectTransform child = (RectTransform)content.GetChild(targets.Count());
                Vector2 childWorldTopPos = content.TransformPoint(child.offsetMax);
                Vector3 localPos = content.parent.InverseTransformPoint(childWorldTopPos);

                if (m_isDrag)
                {
                    m_delta += content.offsetMax - (Vector2)localPos;
                }

                localPos.x = content.localPosition.x;

                content.localPosition = localPos;
            }
        }
        else if (velocity.y < 0)
        {
            var targets = m_contentsForSnapshotMode.Where(t =>
            {
                bool result = t.offsetMax.y < content.InverseTransformPoint(viewport.position).y + viewport.rect.yMin - m_buffer;
                return result;
            });

            bool isUpdated = false;

            for (int i = m_indexMin - 1; i >= m_indexMin - targets.Count(); i--)
            {
                isUpdated = onVerifyIndex?.Invoke(i) ?? true;
                if (isUpdated)
                {
                    break;
                }
            }

            if (isUpdated)
            {
                m_indexMin -= targets.Count();
                m_indexMax -= targets.Count();

                for (int targetIndex = 0, dataIndex = m_indexMin; targetIndex < m_contentsForSnapshotMode.Length; targetIndex++, dataIndex++)
                {
                    RectTransform target = m_contentsForSnapshotMode[targetIndex];

                    target.gameObject.SetActive(!m_autoInactive || (onVerifyIndex?.Invoke(dataIndex) ?? true));

                    ExecuteEvents.Execute<IInfiniteScrollContent>(target.gameObject, null, (handler, data) =>
                    {
                        handler.Update(dataIndex);
                    });
                }

                RectTransform child = (RectTransform)content.GetChild(content.childCount - targets.Count() - 1);
                Vector2 childWorldButtomPos = content.TransformPoint(child.offsetMin);
                Vector3 localPos = content.parent.InverseTransformPoint(childWorldButtomPos);

                if (m_isDrag)
                {
                    m_delta += content.offsetMin - (Vector2)localPos;
                }

                localPos.x = content.localPosition.x;
                localPos.y += content.rect.height;

                content.localPosition = localPos;
            }
        }
    }

    private void UpdateHorizontalLoopMode()
    {
        if (!horizontal)
        {
            return;
        }

        if (velocity.x < 0)
        {
            var targets = m_contentsForLoopMode.Where(t =>
            {
                bool result = t.Key.offsetMax.x < content.InverseTransformPoint(viewport.position).x + viewport.rect.xMin - m_buffer;
                return result;
            }).OrderBy(t => t.Value);

            bool isUpdated = targets.Any(t =>
            {
                var result = onVerifyIndex?.Invoke(t.Value + content.childCount) ?? true;
                return result;
            });

            if (isUpdated)
            {
                int lastIndex = m_contentsForLoopMode.Max(t => t.Value);
                foreach (var target in targets)
                {
                    m_contentsForLoopMode[target.Key] = ++lastIndex;
                    target.Key.SetAsLastSibling();

                    if (m_autoInactive)
                    {
                        m_autoInactives.Add(target.Key);
                    }
                    else
                    {
                        target.Key.gameObject.SetActive(true);

                        ExecuteEvents.Execute<IInfiniteScrollContent>(target.Key.gameObject, null, (handler, data) =>
                        {
                            handler.Update(lastIndex);
                        });
                    }
                }

                RectTransform child = (RectTransform)content.GetChild(0);
                Vector2 childWorldLeftPos = content.TransformPoint(child.offsetMin);
                Vector3 localPos = content.parent.InverseTransformPoint(childWorldLeftPos);

                if (m_isDrag)
                {
                    m_delta += content.offsetMin - (Vector2)localPos;
                }

                localPos.y = content.localPosition.y;

                content.localPosition = localPos;
            }
        }
        else if (velocity.x > 0)
        {
            var targets = m_contentsForLoopMode.Where(t =>
            {
                bool result = t.Key.offsetMin.x > content.InverseTransformPoint(viewport.position).x + viewport.rect.xMax + m_buffer;
                return result;
            }).OrderByDescending(t => t.Value);

            bool isUpdated = targets.Any(t =>
            {
                var result = onVerifyIndex?.Invoke(t.Value - content.childCount) ?? true;
                return result;
            });

            if (isUpdated)
            {
                int firstIndex = m_contentsForLoopMode.Min(t => t.Value);
                foreach (var target in targets)
                {
                    m_contentsForLoopMode[target.Key] = --firstIndex;
                    target.Key.SetAsFirstSibling();

                    if (m_autoInactive)
                    {
                        m_autoInactives.Add(target.Key);
                    }
                    else
                    {
                        target.Key.gameObject.SetActive(true);

                        ExecuteEvents.Execute<IInfiniteScrollContent>(target.Key.gameObject, null, (handler, data) =>
                        {
                            handler.Update(firstIndex);
                        });
                    }
                }

                RectTransform child = (RectTransform)content.GetChild(content.childCount - 1);
                Vector2 childWorldRightPos = content.TransformPoint(child.offsetMax);
                Vector3 localPos = content.parent.InverseTransformPoint(childWorldRightPos);

                if (m_isDrag)
                {
                    m_delta += content.offsetMax - (Vector2)localPos;
                }

                localPos.y = content.localPosition.y;
                localPos.x -= content.rect.width;

                content.localPosition = localPos;
            }
        }
    }

    private void SetAsFirstSibiling(RectTransform transform)
    {
        if (m_layoutGroup is GridLayoutGroup)
        {
            var grid = m_layoutGroup as GridLayoutGroup;

            if (horizontal)
            {
                if (grid.startAxis == GridLayoutGroup.Axis.Horizontal &&
                    (grid.startCorner == GridLayoutGroup.Corner.LowerRight || grid.startCorner == GridLayoutGroup.Corner.UpperRight))
                {
                    var last = m_contentsForLoopMode.Where(t => t.Key.position.y == transform.position.y).OrderByDescending(t => t.Key.position.x).FirstOrDefault();
                }
                else
                {
                }
            }
        }
    }
}