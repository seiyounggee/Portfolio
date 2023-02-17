using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WayPointSystem
{
    public class WaypointsGroup : MonoBehaviour
    {
        public PositionConstraint XYZConstraint = PositionConstraint.XYZ;

        [HideInInspector] public List<Waypoint> waypoints_0;

        [HideInInspector] public List<Waypoint> waypoints_1;   // The waypoint components controlled by this WaypointsGroupl IMMEDIATE children only

        [HideInInspector] public List<Waypoint> waypoints_2;

        [HideInInspector] public List<Waypoint> waypoints_3;

        [HideInInspector] public List<Waypoint> waypoints_4;

        [HideInInspector] public List<Waypoint> waypoints_5;

        [HideInInspector] public List<Waypoint> waypoints_6;

        private void Awake()
        {
            if (waypoints_0 != null)
            {
                foreach (Waypoint wp in waypoints_0)
                    wp.SetWaypointGroup(this);
            }

            if (waypoints_1 != null)
            {
                foreach (Waypoint wp in waypoints_1)
                    wp.SetWaypointGroup(this);
            }

            if (waypoints_2 != null)
            {
                foreach (Waypoint wp in waypoints_2)
                    wp.SetWaypointGroup(this);
            }

            if (waypoints_3 != null)
            {
                foreach (Waypoint wp in waypoints_3)
                    wp.SetWaypointGroup(this);
            }

            if (waypoints_4 != null)
            {
                foreach (Waypoint wp in waypoints_4)
                    wp.SetWaypointGroup(this);
            }

            if (waypoints_5 != null)
            {
                foreach (Waypoint wp in waypoints_5)
                    wp.SetWaypointGroup(this);
            }

            if (waypoints_6 != null)
            {
                foreach (Waypoint wp in waypoints_6)
                    wp.SetWaypointGroup(this);
            }
        }

        /// <summary>
        /// Returns a list of  Waypoints; resets the parent transform if reparent == true
        /// </summary>
        /// <returns></returns>
        /// 

        public List<Waypoint> GetWaypointChildren_0(bool reparent = true)
        {
            if (waypoints_0 == null)
                waypoints_0 = new List<Waypoint>();

            if (reparent == true)
            {
                foreach (Waypoint wp in waypoints_0)
                    wp.SetWaypointGroup(this);
            }


            return waypoints_0;
        }

        public List<Waypoint> GetWaypointChildren_1(bool reparent = true)
        {
            if (waypoints_1 == null)
                waypoints_1 = new List<Waypoint>();

            if(reparent == true)
            { 
                foreach (Waypoint wp in waypoints_1)
                    wp.SetWaypointGroup(this);
             }


            return waypoints_1;
        }

        public List<Waypoint> GetWaypointChildren_2(bool reparent = true)
        {
            if (waypoints_2 == null)
                waypoints_2 = new List<Waypoint>();

            if (reparent == true)
            {
                foreach (Waypoint wp in waypoints_2)
                    wp.SetWaypointGroup(this);
            }


            return waypoints_2;
        }

        public List<Waypoint> GetWaypointChildren_3(bool reparent = true)
        {
            if (waypoints_3 == null)
                waypoints_3 = new List<Waypoint>();

            if (reparent == true)
            {
                foreach (Waypoint wp in waypoints_3)
                    wp.SetWaypointGroup(this);
            }


            return waypoints_3;
        }

        public List<Waypoint> GetWaypointChildren_4(bool reparent = true)
        {
            if (waypoints_4 == null)
                waypoints_4 = new List<Waypoint>();

            if (reparent == true)
            {
                foreach (Waypoint wp in waypoints_4)
                    wp.SetWaypointGroup(this);
            }


            return waypoints_4;
        }

        public List<Waypoint> GetWaypointChildren_5(bool reparent = true)
        {
            if (waypoints_5 == null)
                waypoints_5 = new List<Waypoint>();

            if (reparent == true)
            {
                foreach (Waypoint wp in waypoints_5)
                    wp.SetWaypointGroup(this);
            }


            return waypoints_5;
        }

        public List<Waypoint> GetWaypointChildren_6(bool reparent = true)
        {
            if (waypoints_6 == null)
                waypoints_6 = new List<Waypoint>();

            if (reparent == true)
            {
                foreach (Waypoint wp in waypoints_6)
                    wp.SetWaypointGroup(this);
            }


            return waypoints_6;
        }

        public void AddWaypoint_0(Waypoint wp, int index = -1)
        {
            if (waypoints_0 == null) waypoints_0 = new List<Waypoint>();
            if (index == -1)
                waypoints_0.Add(wp);
            else
                waypoints_0.Insert(index, wp);
            wp.SetWaypointGroup(this);
        }

        public void AddWaypoint_1(Waypoint wp, int index = -1)
        {
            if (waypoints_1 == null) waypoints_1 = new List<Waypoint>();
            if (index == -1)
                waypoints_1.Add(wp);
            else
                waypoints_1.Insert(index, wp);
            wp.SetWaypointGroup(this);
        }

        public void AddWaypoint_2(Waypoint wp, int index = -1)
        {
            if (waypoints_2 == null) waypoints_2 = new List<Waypoint>();
            if (index == -1)
                waypoints_2.Add(wp);
            else
                waypoints_2.Insert(index, wp);
            wp.SetWaypointGroup(this);
        }

        public void AddWaypoint_3(Waypoint wp, int index = -1)
        {
            if (waypoints_3 == null) waypoints_3 = new List<Waypoint>();
            if (index == -1)
                waypoints_3.Add(wp);
            else
                waypoints_3.Insert(index, wp);
            wp.SetWaypointGroup(this);
        }

        public void AddWaypoint_4(Waypoint wp, int index = -1)
        {
            if (waypoints_4 == null) waypoints_4 = new List<Waypoint>();
            if (index == -1)
                waypoints_4.Add(wp);
            else
                waypoints_4.Insert(index, wp);
            wp.SetWaypointGroup(this);
        }

        public void AddWaypoint_5(Waypoint wp, int index = -1)
        {
            if (waypoints_5 == null) waypoints_5 = new List<Waypoint>();
            if (index == -1)
                waypoints_5.Add(wp);
            else
                waypoints_5.Insert(index, wp);
            wp.SetWaypointGroup(this);
        }

        public void AddWaypoint_6(Waypoint wp, int index = -1)
        {
            if (waypoints_6 == null) waypoints_6 = new List<Waypoint>();
            if (index == -1)
                waypoints_6.Add(wp);
            else
                waypoints_6.Insert(index, wp);
            wp.SetWaypointGroup(this);
        }

        public void ClearWayPoints_0_1_2_4_5_6()
        {
            waypoints_0.Clear();
            waypoints_1.Clear();
            waypoints_2.Clear();
            waypoints_4.Clear();
            waypoints_5.Clear();
            waypoints_6.Clear();
        }


        public void ClearAllWaypoints()
        {
            waypoints_0.Clear();
            waypoints_1.Clear();
            waypoints_2.Clear();
            waypoints_3.Clear();
            waypoints_4.Clear();
            waypoints_5.Clear();
            waypoints_6.Clear();
        }

        public Vector3 GetCenterOfWayPoints()
        {
            Vector3 centerPoint = Vector3.zero;

            if (waypoints_3 != null && waypoints_3.Count > 0)
            {
                foreach (var i in waypoints_3)
                    centerPoint += i.GetPosition();
                centerPoint /= waypoints_1.Count;
            }

            return centerPoint;
        }

#if UNITY_EDITOR

        public enum ToggleShowLaneType { All, Zero, One, Two, Three, Four, Five, Six}

        bool toggleShowLane_All = true;
        bool toggleShowLane_0 = false;
        bool toggleShowLane_1 = false;
        bool toggleShowLane_2 = false;
        bool toggleShowLane_3 = false;
        bool toggleShowLane_4 = false;
        bool toggleShowLane_5 = false;
        bool toggleShowLane_6 = false;

        public void ToggleShowLane(ToggleShowLaneType type)
        {
            switch (type)
            {
                case ToggleShowLaneType.All:
                    toggleShowLane_All = true;
                    toggleShowLane_0 = true;
                    toggleShowLane_1 = true;
                    toggleShowLane_2 = true;
                    toggleShowLane_3 = true;
                    toggleShowLane_4 = true;
                    toggleShowLane_5 = true;
                    toggleShowLane_6 = true;
                    break;
                case ToggleShowLaneType.Zero:
                    toggleShowLane_0 = true;
                    toggleShowLane_All = false;
                    break;
                case ToggleShowLaneType.One:
                    toggleShowLane_1 = true;
                    toggleShowLane_All = false;
                    break;
                case ToggleShowLaneType.Two:
                    toggleShowLane_2 = true;
                    toggleShowLane_All = false;
                    break;
                case ToggleShowLaneType.Three:
                    toggleShowLane_3 = true;
                    toggleShowLane_All = false;
                    break;
                case ToggleShowLaneType.Four:
                    toggleShowLane_4 = true;
                    toggleShowLane_All = false;
                    break;
                case ToggleShowLaneType.Five:
                    toggleShowLane_5 = true;
                    toggleShowLane_All = false;
                    break;
                case ToggleShowLaneType.Six:
                    toggleShowLane_6 = true;
                    toggleShowLane_All = false;
                    break;
            }
        }


        public void ToggleHideLane(ToggleShowLaneType type)
        {
            switch (type)
            {
                case ToggleShowLaneType.All:
                    toggleShowLane_All = false;
                    break;
                case ToggleShowLaneType.Zero:
                    toggleShowLane_0 = false;
                    break;
                case ToggleShowLaneType.One:
                    toggleShowLane_1 = false;
                    break;
                case ToggleShowLaneType.Two:
                    toggleShowLane_2 = false;
                    break;
                case ToggleShowLaneType.Three:
                    toggleShowLane_3 = false;
                    break;
                case ToggleShowLaneType.Four:
                    toggleShowLane_4 = false;
                    break;
                case ToggleShowLaneType.Five:
                    toggleShowLane_5 = false;
                    break;
                case ToggleShowLaneType.Six:
                    toggleShowLane_6 = false;
                    break;
            }
        }


        private void OnDrawGizmos()
        {
            if (waypoints_0 != null && waypoints_0.Count > 0 && waypoints_1 != null && waypoints_1.Count > 0
                && waypoints_2 != null && waypoints_2.Count > 0 && waypoints_3 != null && waypoints_3.Count > 0
                && waypoints_4 != null && waypoints_4.Count > 0 && waypoints_5 != null && waypoints_5.Count > 0
                && waypoints_6 != null && waypoints_6.Count > 0)
            {
                if (toggleShowLane_All || toggleShowLane_0)
                {
                    Gizmos.color = Color.red;

                    for (int i = 0; i < waypoints_0.Count - 1; i++)
                    {
                        Gizmos.DrawLine(waypoints_0[i].GetPosition(), waypoints_0[i + 1].GetPosition());
                    }

                    Gizmos.DrawLine(waypoints_0[waypoints_0.Count - 1].GetPosition(), waypoints_0[0].GetPosition());

                    for (int i = 0; i < waypoints_0.Count; i++)
                    {
                        Gizmos.DrawSphere(waypoints_0[i].GetPosition(), 0.3f);
                    }
                }

                if (toggleShowLane_All || toggleShowLane_1)
                {
                    Gizmos.color = Color.red;

                    for (int i = 0; i < waypoints_1.Count - 1; i++)
                    {
                        Gizmos.DrawLine(waypoints_1[i].GetPosition(), waypoints_1[i + 1].GetPosition());
                    }

                    Gizmos.DrawLine(waypoints_1[waypoints_1.Count - 1].GetPosition(), waypoints_1[0].GetPosition());

                    for (int i = 0; i < waypoints_1.Count; i++)
                    {
                        Gizmos.DrawSphere(waypoints_1[i].GetPosition(), 0.3f);
                    }
                }

                if (toggleShowLane_All || toggleShowLane_2)
                {
                    Gizmos.color = Color.green;

                    if (waypoints_2 != null && waypoints_2.Count > 0)
                    {
                        for (int i = 0; i < waypoints_2.Count - 1; i++)
                        {
                            Gizmos.DrawLine(waypoints_2[i].GetPosition(), waypoints_2[i + 1].GetPosition());
                        }

                        Gizmos.DrawLine(waypoints_2[waypoints_2.Count - 1].GetPosition(), waypoints_2[0].GetPosition());

                        for (int i = 0; i < waypoints_2.Count; i++)
                        {
                            Gizmos.DrawSphere(waypoints_2[i].GetPosition(), 0.3f);
                        }
                    }
                }

                if (toggleShowLane_All || toggleShowLane_3)
                {
                    Gizmos.color = Color.blue;
                    if (waypoints_3 != null && waypoints_3.Count > 0)
                    {
                        for (int i = 0; i < waypoints_3.Count - 1; i++)
                        {
                            Gizmos.DrawLine(waypoints_3[i].GetPosition(), waypoints_3[i + 1].GetPosition());
                        }

                        Gizmos.DrawLine(waypoints_3[waypoints_3.Count - 1].GetPosition(), waypoints_3[0].GetPosition());

                        for (int i = 0; i < waypoints_3.Count; i++)
                        {
                            Gizmos.DrawSphere(waypoints_3[i].GetPosition(), 0.3f);
                        }
                    }
                }

                if (toggleShowLane_All || toggleShowLane_4)
                {
                    Gizmos.color = Color.gray;
                    if (waypoints_4 != null && waypoints_4.Count > 0)
                    {
                        for (int i = 0; i < waypoints_4.Count - 1; i++)
                        {
                            Gizmos.DrawLine(waypoints_4[i].GetPosition(), waypoints_4[i + 1].GetPosition());
                        }

                        Gizmos.DrawLine(waypoints_4[waypoints_4.Count - 1].GetPosition(), waypoints_4[0].GetPosition());

                        for (int i = 0; i < waypoints_4.Count; i++)
                        {
                            Gizmos.DrawSphere(waypoints_4[i].GetPosition(), 0.3f);
                        }
                    }
                }

                if (toggleShowLane_All || toggleShowLane_5)
                {
                    Gizmos.color = Color.cyan;
                    if (waypoints_5 != null && waypoints_5.Count > 0)
                    {
                        for (int i = 0; i < waypoints_5.Count - 1; i++)
                        {
                            Gizmos.DrawLine(waypoints_5[i].GetPosition(), waypoints_5[i + 1].GetPosition());
                        }

                        Gizmos.DrawLine(waypoints_5[waypoints_5.Count - 1].GetPosition(), waypoints_5[0].GetPosition());

                        for (int i = 0; i < waypoints_5.Count; i++)
                        {
                            Gizmos.DrawSphere(waypoints_5[i].GetPosition(), 0.3f);
                        }
                    }
                }

                if (toggleShowLane_All || toggleShowLane_6)
                {
                    Gizmos.color = Color.red;

                    for (int i = 0; i < waypoints_6.Count - 1; i++)
                    {
                        Gizmos.DrawLine(waypoints_6[i].GetPosition(), waypoints_6[i + 1].GetPosition());
                    }

                    Gizmos.DrawLine(waypoints_6[waypoints_6.Count - 1].GetPosition(), waypoints_6[0].GetPosition());

                    for (int i = 0; i < waypoints_6.Count; i++)
                    {
                        Gizmos.DrawSphere(waypoints_6[i].GetPosition(), 0.3f);
                    }
                }

                if (toggleShowLane_All)
                {
                    Gizmos.color = Color.cyan;
                    if (waypoints_6 != null && waypoints_6.Count > 0)
                    {
                        for (int i = 0; i < waypoints_6.Count - 1; i++)
                        {
                            Gizmos.DrawLine(waypoints_6[i].GetPosition(), waypoints_6[i + 1].GetPosition());
                        }

                        Gizmos.DrawLine(waypoints_6[waypoints_6.Count - 1].GetPosition(), waypoints_6[0].GetPosition());

                        for (int i = 0; i < waypoints_6.Count; i++)
                        {
                            Gizmos.DrawSphere(waypoints_6[i].GetPosition(), 0.3f);
                        }
                    }
                }


                if (waypoints_3.Count >= 3)
                {
                    Vector3 centerPoint = Vector3.zero;
                    foreach (var i in waypoints_3)
                        centerPoint += i.GetPosition();
                    centerPoint /= waypoints_3.Count;

                    Gizmos.color = Color.cyan;
                    Gizmos.DrawSphere(centerPoint, 0.55f);
                }

                if (toggleShowLane_All)
                {
                    Gizmos.color = Color.grey;
                    for (int i = 0; i < waypoints_3.Count; i++)
                    {
                        if (waypoints_2 != null && waypoints_2.Count > 0 && waypoints_1 != null && waypoints_1.Count > 0
                            && waypoints_4 != null && waypoints_4.Count > 0 && waypoints_5 != null && waypoints_5.Count > 0
                            && waypoints_6 != null && waypoints_6.Count > 0 && waypoints_0 != null && waypoints_0.Count > 0)
                        {
                            Gizmos.DrawLine(waypoints_0[i].GetPosition(), waypoints_1[i].GetPosition());
                            Gizmos.DrawLine(waypoints_1[i].GetPosition(), waypoints_2[i].GetPosition());
                            Gizmos.DrawLine(waypoints_2[i].GetPosition(), waypoints_3[i].GetPosition());
                            Gizmos.DrawLine(waypoints_3[i].GetPosition(), waypoints_4[i].GetPosition());
                            Gizmos.DrawLine(waypoints_4[i].GetPosition(), waypoints_5[i].GetPosition());
                            Gizmos.DrawLine(waypoints_5[i].GetPosition(), waypoints_6[i].GetPosition());
                        }
                    }
                }
            }


        }

        void OnApplicationQuit()
        {
            Debug.Log("Application ending after " + UtilityCommon.GetTimeString_TYPE_1(Time.time));

        }


#endif
    }
}