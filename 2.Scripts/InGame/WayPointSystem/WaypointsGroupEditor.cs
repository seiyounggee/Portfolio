using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace WayPointSystem
{
#if UNITY_EDITOR
    [CustomEditor(typeof(WaypointsGroup))]
    public class WaypointsGroupEditor : Editor
    {
        WaypointsGroup waypointsGroup;
        List<Waypoint> waypointsList_0;
        List<Waypoint> waypointsList_1;
        List<Waypoint> waypointsList_2;
        List<Waypoint> waypointsList_3;
        List<Waypoint> waypointsList_4;
        List<Waypoint> waypointsList_5;
        List<Waypoint> waypointsList_6;

        Waypoint selectedWaypoint = null;

        List<Waypoint> multipleSelectedWayPoints = new List<Waypoint>();

        bool doRepaint = false;

        bool showWayPointsList_0 = false;
        bool showWayPointsList_1 = false;
        bool showWayPointsList_2 = false;
        bool showWayPointsList_3 = false;
        bool showWayPointsList_4 = false;
        bool showWayPointsList_5 = false;
        bool showWayPointsList_6 = false;


        bool toggleShowLane_All = true;
        bool toggleShowLane_0 = false;
        bool toggleShowLane_1 = false;
        bool toggleShowLane_2 = false;
        bool toggleShowLane_3 = false;
        bool toggleShowLane_4 = false;
        bool toggleShowLane_5 = false;
        bool toggleShowLane_6 = false;

        private bool isShiftPressed = false;

        public string inputDist = "Set Dist (float)";

        private void OnEnable()
        {
            waypointsGroup = target as WaypointsGroup;
            waypointsList_0 = waypointsGroup.GetWaypointChildren_0();
            waypointsList_1 = waypointsGroup.GetWaypointChildren_1();
            waypointsList_2 = waypointsGroup.GetWaypointChildren_2();
            waypointsList_3 = waypointsGroup.GetWaypointChildren_3();
            waypointsList_4 = waypointsGroup.GetWaypointChildren_4();
            waypointsList_5 = waypointsGroup.GetWaypointChildren_5();
            waypointsList_6 = waypointsGroup.GetWaypointChildren_6();
        }


        private void OnSceneGUI()
        {
            if (toggleShowLane_All == true)
            {
                DrawWaypoints(waypointsList_0);
                DrawWaypoints(waypointsList_1);
                DrawWaypoints(waypointsList_2);
                DrawWaypoints(waypointsList_3);
                DrawWaypoints(waypointsList_4);
                DrawWaypoints(waypointsList_5);
                DrawWaypoints(waypointsList_6);
            }
            else
            {
                if (toggleShowLane_0)
                    DrawWaypoints(waypointsList_0);

                if (toggleShowLane_1)
                    DrawWaypoints(waypointsList_1);

                if (toggleShowLane_2)
                    DrawWaypoints(waypointsList_2);

                if (toggleShowLane_3)
                    DrawWaypoints(waypointsList_3);

                if (toggleShowLane_4)
                    DrawWaypoints(waypointsList_4);

                if (toggleShowLane_5)
                    DrawWaypoints(waypointsList_5);

                if (toggleShowLane_6)
                    DrawWaypoints(waypointsList_6);
            }


            Event e = Event.current;
            switch (e.type)
            {
                case EventType.KeyDown:
                    {
                        if (Event.current.keyCode == (KeyCode.N))
                        {
                            AddWayPoint();
                        }

                        if (Event.current.keyCode == (KeyCode.X))
                        {
                            if (multipleSelectedWayPoints.Contains(selectedWaypoint) == false)
                                multipleSelectedWayPoints.Add(selectedWaypoint);
                        }

                        if (Event.current.keyCode == (KeyCode.C))
                        {
                            multipleSelectedWayPoints.Clear();
                        }


                        if (Event.current.keyCode == (KeyCode.LeftShift))
                        {
                            isShiftPressed = true;
                        }
                    }
                    break;

                case EventType.MouseDown:
                    {

                    }
                    break;

                case EventType.KeyUp:
                    {
                        if (Event.current.keyCode == (KeyCode.LeftShift))
                        {
                            isShiftPressed = false;
                        }
                    }
                    break;
            }


            Handles.BeginGUI();

            if (selectedWaypoint != null)
            {
                GUILayout.BeginArea(new Rect(100, 25, Screen.width - 800, Screen.height - 200));
                GUIStyle SectionNameStyle = new GUIStyle();
                SectionNameStyle.fontSize = 15;
                SectionNameStyle.wordWrap = true;
                SectionNameStyle.fontStyle = FontStyle.Bold;
                SectionNameStyle.normal.background = EditorGUIUtility.whiteTexture;

                if (selectedWaypoint.currentWayPointType == Waypoint.WayPointType.Normal)
                    SectionNameStyle.normal.textColor = Color.black;
                else if (selectedWaypoint.currentWayPointType == Waypoint.WayPointType.Blocked)
                    SectionNameStyle.normal.textColor = Color.magenta;
                else if (selectedWaypoint.currentWayPointType == Waypoint.WayPointType.OutOfBoundary)
                    SectionNameStyle.normal.textColor = Color.red;

                string info = "Waypoint: index- " + selectedWaypoint.index + " | Vector3 " + selectedWaypoint.GetPosition() + " | " + selectedWaypoint.currentWayPointType.ToString();
                GUILayout.Box(info, SectionNameStyle);

                if (GUILayout.Button("Delete"))
                {
                    int deleteIndex = -1;
                    deleteIndex = selectedWaypoint.index;
                    selectedWaypoint = null;

                    if (deleteIndex > -1)
                    {
                        //아래 순서 꼭 지켜야함!!! (RemoveAt 때문에 list에 변동 생김)

                        if (waypointsList_3[deleteIndex] == selectedWaypoint)
                            selectedWaypoint = null;

                        waypointsList_3.RemoveAt(deleteIndex);

                        if (waypointsList_2 != null && waypointsList_2.Count > 0)
                            waypointsList_2.RemoveAt(deleteIndex);

                        if (waypointsList_1 != null && waypointsList_1.Count > 0)
                            waypointsList_1.RemoveAt(deleteIndex);

                        if (waypointsList_0 != null && waypointsList_0.Count > 0)
                            waypointsList_0.RemoveAt(deleteIndex);

                        if (waypointsList_4 != null && waypointsList_4.Count > 0)
                            waypointsList_4.RemoveAt(deleteIndex);

                        if (waypointsList_5 != null && waypointsList_5.Count > 0)
                            waypointsList_5.RemoveAt(deleteIndex);

                        if (waypointsList_6 != null && waypointsList_6.Count > 0)
                            waypointsList_6.RemoveAt(deleteIndex);

                        deleteIndex = -1;

                        SetIndexNumber();

                        SceneView.RepaintAll();
                    }
                }

                if (GUILayout.Button("↔"))
                {
                    if (selectedWaypoint == null)
                        return;

                    var curr = selectedWaypoint.currentWayPointType;
                    if (curr == Waypoint.WayPointType.Normal)
                        selectedWaypoint.currentWayPointType = Waypoint.WayPointType.Blocked;
                    else if (curr == Waypoint.WayPointType.Blocked)
                        selectedWaypoint.currentWayPointType = Waypoint.WayPointType.OutOfBoundary;
                    else if (curr == Waypoint.WayPointType.OutOfBoundary)
                        selectedWaypoint.currentWayPointType = Waypoint.WayPointType.Normal;

                    SceneView.RepaintAll();
                }
                GUILayout.EndArea();
            }


            if (multipleSelectedWayPoints != null && multipleSelectedWayPoints.Count == 2)
            {
                GUILayout.BeginArea(new Rect(100, 150, Screen.width - 800, Screen.height - 200));
                GUIStyle SectionNameStyle = new GUIStyle();
                SectionNameStyle.fontSize = 15;
                SectionNameStyle.wordWrap = true;
                SectionNameStyle.fontStyle = FontStyle.Bold;
                SectionNameStyle.normal.background = EditorGUIUtility.whiteTexture;
                string info = "Mutiple Waypoints: " + multipleSelectedWayPoints[0].GetPosition() + "  |  " + multipleSelectedWayPoints[1].GetPosition();
                GUILayout.Box(info, SectionNameStyle);
                if (GUILayout.Button("Unify All Waypoint Distance"))
                {
                    UnifyWaypoints();
                }
                GUILayout.EndArea();
            }

                    
            Handles.EndGUI();
        }

        override public void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical();

            //GUI.skin.label.fontSize = 50;
            GUIStyle SectionNameStyle = new GUIStyle();
            SectionNameStyle.fontSize = 15;
            SectionNameStyle.wordWrap = true;
            SectionNameStyle.fontStyle = FontStyle.Bold;
            SectionNameStyle.normal.textColor = Color.white;

            bool dorepaint = false;

            EditorGUILayout.LabelField("[Waypoints 0 - Outside]       count: " + waypointsList_0.Count, SectionNameStyle);
            showWayPointsList_0 = EditorGUILayout.Foldout(showWayPointsList_0, "Waypoints 0 List");
            if (showWayPointsList_0)
            {
                if (waypointsList_0 != null)
                {
                    int deleteIndex = -1;
                    for (int cnt = 0; cnt < waypointsList_0.Count; cnt++)
                    {
                        Color guiColor = GUI.color;

                        Waypoint cwp = waypointsList_0[cnt];

                        if (cwp.currentWayPointType == Waypoint.WayPointType.Blocked)
                            GUI.color = Color.yellow;
                        else if (cwp.currentWayPointType == Waypoint.WayPointType.OutOfBoundary)
                            GUI.color = Color.red;
                        else
                            GUI.color = Color.white;

                        if (cwp == selectedWaypoint)
                            GUI.color = Color.green;

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("[" + waypointsList_0[cnt].index + "]", GUILayout.Width(33));
                        if (GUILayout.Button("S", GUILayout.Width(20)))
                        {
                            if (selectedWaypoint == cwp)
                            {
                                selectedWaypoint = null;
                            }
                            else
                            {
                                selectedWaypoint = cwp;
                            }

                            dorepaint = true;

                        }

                        EditorGUI.BeginChangeCheck();
                        Vector3 oldV = cwp.GetPosition(waypointsGroup.XYZConstraint);
                        Vector3 newV = EditorGUILayout.Vector3Field("", oldV);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(waypointsGroup, "Waypoint Moved");
                            cwp.UpdatePosition(newV - oldV, waypointsGroup.XYZConstraint);
                        }

                        if (GUILayout.Button("Del", GUILayout.Width(30)))
                        {
                            deleteIndex = cnt;
                            dorepaint = true;
                            selectedWaypoint = null;
                        }

                        GUILayout.TextField(cwp.currentWayPointType.ToString(), 10);
                        if (GUILayout.Button("↔", GUILayout.Width(25)))
                        {
                            var curr = cwp.currentWayPointType;
                            if (curr == Waypoint.WayPointType.Normal)
                                cwp.currentWayPointType = Waypoint.WayPointType.Blocked;
                            else if (curr == Waypoint.WayPointType.Blocked)
                                cwp.currentWayPointType = Waypoint.WayPointType.OutOfBoundary;
                            else if (curr == Waypoint.WayPointType.OutOfBoundary)
                                cwp.currentWayPointType = Waypoint.WayPointType.Normal;

                            dorepaint = true;
                        }

                        GUI.color = guiColor;
                        EditorGUILayout.EndHorizontal();

                    }

                    if (deleteIndex > -1)
                    {
                        //아래 순서 꼭 지켜야함!!! (RemoveAt 때문에 list에 변동 생김)

                        if (waypointsList_3[deleteIndex] == selectedWaypoint)
                            selectedWaypoint = null;

                        waypointsList_3.RemoveAt(deleteIndex);

                        if (waypointsList_2 != null && waypointsList_2.Count > 0)
                            waypointsList_2.RemoveAt(deleteIndex);

                        if (waypointsList_1 != null && waypointsList_1.Count > 0)
                            waypointsList_1.RemoveAt(deleteIndex);

                        if (waypointsList_0 != null && waypointsList_0.Count > 0)
                            waypointsList_0.RemoveAt(deleteIndex);

                        if (waypointsList_4 != null && waypointsList_4.Count > 0)
                            waypointsList_4.RemoveAt(deleteIndex);

                        if (waypointsList_5 != null && waypointsList_5.Count > 0)
                            waypointsList_5.RemoveAt(deleteIndex);

                        if (waypointsList_6 != null && waypointsList_6.Count > 0)
                            waypointsList_6.RemoveAt(deleteIndex);

                        deleteIndex = -1;

                        SetIndexNumber();
                    }
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("[Waypoints 1]       count: " + waypointsList_1.Count, SectionNameStyle);
            showWayPointsList_1 = EditorGUILayout.Foldout(showWayPointsList_1, "Waypoints 1 List");
            if (showWayPointsList_1)
            {
                if (waypointsList_1 != null)
                {
                    int deleteIndex = -1;
                    for (int cnt = 0; cnt < waypointsList_1.Count; cnt++)
                    {
                        Color guiColor = GUI.color;

                        Waypoint cwp = waypointsList_1[cnt];

                        if (cwp.currentWayPointType == Waypoint.WayPointType.Blocked)
                            GUI.color = Color.yellow;
                        else if (cwp.currentWayPointType == Waypoint.WayPointType.OutOfBoundary)
                            GUI.color = Color.red;
                        else
                            GUI.color = Color.white;

                        if (cwp == selectedWaypoint)
                            GUI.color = Color.green;

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("[" + waypointsList_1[cnt].index + "]", GUILayout.Width(33));
                        if (GUILayout.Button("S", GUILayout.Width(20)))
                        {
                            if (selectedWaypoint == cwp)
                            {
                                selectedWaypoint = null;
                            }
                            else
                            {
                                selectedWaypoint = cwp;
                            }

                            dorepaint = true;

                        }

                        EditorGUI.BeginChangeCheck();
                        Vector3 oldV = cwp.GetPosition(waypointsGroup.XYZConstraint);
                        Vector3 newV = EditorGUILayout.Vector3Field("", oldV);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(waypointsGroup, "Waypoint Moved");
                            cwp.UpdatePosition(newV - oldV, waypointsGroup.XYZConstraint);
                        }

                        if (GUILayout.Button("Del", GUILayout.Width(30)))
                        {
                            deleteIndex = cnt;
                            dorepaint = true;
                            selectedWaypoint = null;
                        }

                        GUILayout.TextField(cwp.currentWayPointType.ToString(), 10);
                        if (GUILayout.Button("↔", GUILayout.Width(25)))
                        {
                            var curr = cwp.currentWayPointType;
                            if (curr == Waypoint.WayPointType.Normal)
                                cwp.currentWayPointType = Waypoint.WayPointType.Blocked;
                            else if (curr == Waypoint.WayPointType.Blocked)
                                cwp.currentWayPointType = Waypoint.WayPointType.OutOfBoundary;
                            else if (curr == Waypoint.WayPointType.OutOfBoundary)
                                cwp.currentWayPointType = Waypoint.WayPointType.Normal;

                            dorepaint = true;
                        }

                        GUI.color = guiColor;
                        EditorGUILayout.EndHorizontal();

                    }

                    if (deleteIndex > -1)
                    {
                        //아래 순서 꼭 지켜야함!!! (RemoveAt 때문에 list에 변동 생김)

                        if (waypointsList_3[deleteIndex] == selectedWaypoint)
                            selectedWaypoint = null;

                        waypointsList_3.RemoveAt(deleteIndex);

                        if (waypointsList_2 != null && waypointsList_2.Count > 0)
                            waypointsList_2.RemoveAt(deleteIndex);

                        if (waypointsList_1 != null && waypointsList_1.Count > 0)
                            waypointsList_1.RemoveAt(deleteIndex);

                        if (waypointsList_0 != null && waypointsList_0.Count > 0)
                            waypointsList_0.RemoveAt(deleteIndex);

                        if (waypointsList_4 != null && waypointsList_4.Count > 0)
                            waypointsList_4.RemoveAt(deleteIndex);

                        if (waypointsList_5 != null && waypointsList_5.Count > 0)
                            waypointsList_5.RemoveAt(deleteIndex);

                        if (waypointsList_6 != null && waypointsList_6.Count > 0)
                            waypointsList_6.RemoveAt(deleteIndex);

                        deleteIndex = -1;

                        SetIndexNumber();
                    }
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("[Waypoints 2]       count: " + waypointsList_2.Count, SectionNameStyle);
            showWayPointsList_2 = EditorGUILayout.Foldout(showWayPointsList_2, "Waypoints 2 List");
            if (showWayPointsList_2)
            {
                if (waypointsList_2 != null)
                {
                    int deleteIndex = -1;
                    for (int cnt = 0; cnt < waypointsList_2.Count; cnt++)
                    {
                        Color guiColor = GUI.color;

                        Waypoint cwp = waypointsList_2[cnt];

                        if (cwp.currentWayPointType == Waypoint.WayPointType.Blocked)
                            GUI.color = Color.yellow;
                        else if (cwp.currentWayPointType == Waypoint.WayPointType.OutOfBoundary)
                            GUI.color = Color.red;
                        else
                            GUI.color = Color.white;

                        if (cwp == selectedWaypoint)
                            GUI.color = Color.green;

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("[" + waypointsList_2[cnt].index + "]", GUILayout.Width(33));
                        if (GUILayout.Button("S", GUILayout.Width(20)))
                        {
                            if (selectedWaypoint == cwp)
                            {
                                selectedWaypoint = null;
                            }
                            else
                            {
                                selectedWaypoint = cwp;
                            }

                            dorepaint = true;

                        }

                        EditorGUI.BeginChangeCheck();
                        Vector3 oldV = cwp.GetPosition(waypointsGroup.XYZConstraint);
                        Vector3 newV = EditorGUILayout.Vector3Field("", oldV);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(waypointsGroup, "Waypoint Moved");
                            cwp.UpdatePosition(newV - oldV, waypointsGroup.XYZConstraint);
                        }



                        if (GUILayout.Button("Del", GUILayout.Width(30)))
                        {
                            deleteIndex = cnt;
                            dorepaint = true;
                            selectedWaypoint = null;
                        }

                        GUILayout.TextField(cwp.currentWayPointType.ToString(), 10);
                        if (GUILayout.Button("↔", GUILayout.Width(25)))
                        {
                            var curr = cwp.currentWayPointType;
                            if (curr == Waypoint.WayPointType.Normal)
                                cwp.currentWayPointType = Waypoint.WayPointType.Blocked;
                            else if (curr == Waypoint.WayPointType.Blocked)
                                cwp.currentWayPointType = Waypoint.WayPointType.OutOfBoundary;
                            else if (curr == Waypoint.WayPointType.OutOfBoundary)
                                cwp.currentWayPointType = Waypoint.WayPointType.Normal;
                        }

                        GUI.color = guiColor;
                        EditorGUILayout.EndHorizontal();

                    }

                    if (deleteIndex > -1)
                    {
                        //아래 순서 꼭 지켜야함!!! (RemoveAt 때문에 list에 변동 생김)

                        if (waypointsList_3[deleteIndex] == selectedWaypoint)
                            selectedWaypoint = null;

                        waypointsList_3.RemoveAt(deleteIndex);

                        if (waypointsList_2 != null && waypointsList_2.Count > 0)
                            waypointsList_2.RemoveAt(deleteIndex);

                        if (waypointsList_1 != null && waypointsList_1.Count > 0)
                            waypointsList_1.RemoveAt(deleteIndex);

                        if (waypointsList_0 != null && waypointsList_0.Count > 0)
                            waypointsList_0.RemoveAt(deleteIndex);

                        if (waypointsList_4 != null && waypointsList_4.Count > 0)
                            waypointsList_4.RemoveAt(deleteIndex);

                        if (waypointsList_5 != null && waypointsList_5.Count > 0)
                            waypointsList_5.RemoveAt(deleteIndex);

                        if (waypointsList_6 != null && waypointsList_6.Count > 0)
                            waypointsList_6.RemoveAt(deleteIndex);

                        deleteIndex = -1;

                        SetIndexNumber();
                    }

                }
            }

   
            EditorGUILayout.Space();
            EditorGUILayout.Space();


            EditorGUILayout.LabelField("[Waypoints 3 - Center]       count: " + waypointsList_3.Count, SectionNameStyle);
            showWayPointsList_3 = EditorGUILayout.Foldout(showWayPointsList_3, "Waypoints 3 List");
            if (showWayPointsList_3)
            {
                if (waypointsList_3 != null)
                {
                    int deleteIndex = -1;
                    for (int cnt = 0; cnt < waypointsList_3.Count; cnt++)
                    {
                        Color guiColor = GUI.color;

                        Waypoint cwp = waypointsList_3[cnt];

                        if (cwp.currentWayPointType == Waypoint.WayPointType.Blocked)
                            GUI.color = Color.yellow;
                        else if (cwp.currentWayPointType == Waypoint.WayPointType.OutOfBoundary)
                            GUI.color = Color.red;
                        else
                            GUI.color = Color.white;

                        if (cwp == selectedWaypoint)
                            GUI.color = Color.green;

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("[" + waypointsList_3[cnt].index + "]", GUILayout.Width(33));
                        if (GUILayout.Button("S", GUILayout.Width(20)))
                        {
                            if (selectedWaypoint == cwp)
                            {
                                selectedWaypoint = null;
                            }
                            else
                            {
                                selectedWaypoint = cwp;
                            }

                            dorepaint = true;

                        }

                        EditorGUI.BeginChangeCheck();
                        Vector3 oldV = cwp.GetPosition(waypointsGroup.XYZConstraint);
                        Vector3 newV = EditorGUILayout.Vector3Field("", oldV);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(waypointsGroup, "Waypoint Moved");
                            cwp.UpdatePosition(newV - oldV, waypointsGroup.XYZConstraint);
                        }



                        if (GUILayout.Button("Del", GUILayout.Width(30)))
                        {
                            deleteIndex = cnt;
                            dorepaint = true;
                            selectedWaypoint = null;
                        }

                        GUILayout.TextField(cwp.currentWayPointType.ToString(), 10);
                        if (GUILayout.Button("↔", GUILayout.Width(25)))
                        {
                            var curr = cwp.currentWayPointType;
                            if (curr == Waypoint.WayPointType.Normal)
                                cwp.currentWayPointType = Waypoint.WayPointType.Blocked;
                            else if (curr == Waypoint.WayPointType.Blocked)
                                cwp.currentWayPointType = Waypoint.WayPointType.OutOfBoundary;
                            else if (curr == Waypoint.WayPointType.OutOfBoundary)
                                cwp.currentWayPointType = Waypoint.WayPointType.Normal;
                        }

                        GUI.color = guiColor;
                        EditorGUILayout.EndHorizontal();

                    }

                    if (deleteIndex > -1)
                    {
                        //아래 순서 꼭 지켜야함!!! (RemoveAt 때문에 list에 변동 생김)

                        if (waypointsList_3[deleteIndex] == selectedWaypoint)
                            selectedWaypoint = null;

                        waypointsList_3.RemoveAt(deleteIndex);

                        if (waypointsList_2 != null && waypointsList_2.Count > 0)
                            waypointsList_2.RemoveAt(deleteIndex);

                        if (waypointsList_1 != null && waypointsList_1.Count > 0)
                            waypointsList_1.RemoveAt(deleteIndex);

                        if (waypointsList_0 != null && waypointsList_0.Count > 0)
                            waypointsList_0.RemoveAt(deleteIndex);

                        if (waypointsList_4 != null && waypointsList_4.Count > 0)
                            waypointsList_4.RemoveAt(deleteIndex);

                        if (waypointsList_5 != null && waypointsList_5.Count > 0)
                            waypointsList_5.RemoveAt(deleteIndex);

                        if (waypointsList_6 != null && waypointsList_6.Count > 0)
                            waypointsList_6.RemoveAt(deleteIndex);

                        deleteIndex = -1;

                        SetIndexNumber();
                    }
                }
            }


            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("[Waypoints 4]       count: " + waypointsList_4.Count, SectionNameStyle);

            showWayPointsList_4 = EditorGUILayout.Foldout(showWayPointsList_4, "Waypoints 4 List");
            if (showWayPointsList_4)
            {
                if (waypointsList_4 != null)
                {
                    int deleteIndex = -1;
                    for (int cnt = 0; cnt < waypointsList_4.Count; cnt++)
                    {
                        Color guiColor = GUI.color;

                        Waypoint cwp = waypointsList_4[cnt];

                        if (cwp.currentWayPointType == Waypoint.WayPointType.Blocked)
                            GUI.color = Color.yellow;
                        else if (cwp.currentWayPointType == Waypoint.WayPointType.OutOfBoundary)
                            GUI.color = Color.red;
                        else
                            GUI.color = Color.white;

                        if (cwp == selectedWaypoint)
                            GUI.color = Color.green;

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("[" + waypointsList_4[cnt].index + "]", GUILayout.Width(33));
                        if (GUILayout.Button("S", GUILayout.Width(20)))
                        {
                            if (selectedWaypoint == cwp)
                            {
                                selectedWaypoint = null;
                            }
                            else
                            {
                                selectedWaypoint = cwp;
                            }

                            dorepaint = true;

                        }

                        EditorGUI.BeginChangeCheck();
                        Vector3 oldV = cwp.GetPosition(waypointsGroup.XYZConstraint);
                        Vector3 newV = EditorGUILayout.Vector3Field("", oldV);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(waypointsGroup, "Waypoint Moved");
                            cwp.UpdatePosition(newV - oldV, waypointsGroup.XYZConstraint);
                        }



                        if (GUILayout.Button("Del", GUILayout.Width(30)))
                        {
                            deleteIndex = cnt;
                            dorepaint = true;
                            selectedWaypoint = null;
                        }

                        GUILayout.TextField(cwp.currentWayPointType.ToString(), 10);
                        if (GUILayout.Button("↔", GUILayout.Width(25)))
                        {
                            var curr = cwp.currentWayPointType;
                            if (curr == Waypoint.WayPointType.Normal)
                                cwp.currentWayPointType = Waypoint.WayPointType.Blocked;
                            else if (curr == Waypoint.WayPointType.Blocked)
                                cwp.currentWayPointType = Waypoint.WayPointType.OutOfBoundary;
                            else if (curr == Waypoint.WayPointType.OutOfBoundary)
                                cwp.currentWayPointType = Waypoint.WayPointType.Normal;
                        }

                        GUI.color = guiColor;
                        EditorGUILayout.EndHorizontal();

                    }

                    if (deleteIndex > -1)
                    {
                        //아래 순서 꼭 지켜야함!!! (RemoveAt 때문에 list에 변동 생김)

                        if (waypointsList_3[deleteIndex] == selectedWaypoint)
                            selectedWaypoint = null;

                        waypointsList_3.RemoveAt(deleteIndex);

                        if (waypointsList_2 != null && waypointsList_2.Count > 0)
                            waypointsList_2.RemoveAt(deleteIndex);

                        if (waypointsList_1 != null && waypointsList_1.Count > 0)
                            waypointsList_1.RemoveAt(deleteIndex);

                        if (waypointsList_0 != null && waypointsList_0.Count > 0)
                            waypointsList_0.RemoveAt(deleteIndex);

                        if (waypointsList_4 != null && waypointsList_4.Count > 0)
                            waypointsList_4.RemoveAt(deleteIndex);

                        if (waypointsList_5 != null && waypointsList_5.Count > 0)
                            waypointsList_5.RemoveAt(deleteIndex);

                        if (waypointsList_6 != null && waypointsList_6.Count > 0)
                            waypointsList_6.RemoveAt(deleteIndex);

                        deleteIndex = -1;

                        SetIndexNumber();
                    }
                }
            }


            EditorGUILayout.Space();
            EditorGUILayout.Space();


            EditorGUILayout.LabelField("[Waypoints 5]       count: " + waypointsList_5.Count, SectionNameStyle);
            showWayPointsList_5 = EditorGUILayout.Foldout(showWayPointsList_5, "Waypoints 5 List");
            if (showWayPointsList_5)
            {
                if (waypointsList_5 != null)
                {
                    int deleteIndex = -1;
                    for (int cnt = 0; cnt < waypointsList_5.Count; cnt++)
                    {
                        Color guiColor = GUI.color;

                        Waypoint cwp = waypointsList_5[cnt];

                        if (cwp.currentWayPointType == Waypoint.WayPointType.Blocked)
                            GUI.color = Color.yellow;
                        else if (cwp.currentWayPointType == Waypoint.WayPointType.OutOfBoundary)
                            GUI.color = Color.red;
                        else
                            GUI.color = Color.white;

                        if (cwp == selectedWaypoint)
                            GUI.color = Color.green;

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("[" + waypointsList_5[cnt].index + "]", GUILayout.Width(33));
                        if (GUILayout.Button("S", GUILayout.Width(20)))
                        {
                            if (selectedWaypoint == cwp)
                            {
                                selectedWaypoint = null;
                            }
                            else
                            {
                                selectedWaypoint = cwp;
                            }

                            dorepaint = true;

                        }

                        EditorGUI.BeginChangeCheck();
                        Vector3 oldV = cwp.GetPosition(waypointsGroup.XYZConstraint);
                        Vector3 newV = EditorGUILayout.Vector3Field("", oldV);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(waypointsGroup, "Waypoint Moved");
                            cwp.UpdatePosition(newV - oldV, waypointsGroup.XYZConstraint);
                        }



                        if (GUILayout.Button("Del", GUILayout.Width(30)))
                        {
                            deleteIndex = cnt;
                            dorepaint = true;
                            selectedWaypoint = null;
                        }

                        GUILayout.TextField(cwp.currentWayPointType.ToString(), 10);
                        if (GUILayout.Button("↔", GUILayout.Width(25)))
                        {
                            var curr = cwp.currentWayPointType;
                            if (curr == Waypoint.WayPointType.Normal)
                                cwp.currentWayPointType = Waypoint.WayPointType.Blocked;
                            else if (curr == Waypoint.WayPointType.Blocked)
                                cwp.currentWayPointType = Waypoint.WayPointType.OutOfBoundary;
                            else if (curr == Waypoint.WayPointType.OutOfBoundary)
                                cwp.currentWayPointType = Waypoint.WayPointType.Normal;
                        }

                        GUI.color = guiColor;
                        EditorGUILayout.EndHorizontal();

                    }

                    if (deleteIndex > -1)
                    {
                        //아래 순서 꼭 지켜야함!!! (RemoveAt 때문에 list에 변동 생김)

                        if (waypointsList_3[deleteIndex] == selectedWaypoint)
                            selectedWaypoint = null;

                        waypointsList_3.RemoveAt(deleteIndex);

                        if (waypointsList_2 != null && waypointsList_2.Count > 0)
                            waypointsList_2.RemoveAt(deleteIndex);

                        if (waypointsList_1 != null && waypointsList_1.Count > 0)
                            waypointsList_1.RemoveAt(deleteIndex);

                        if (waypointsList_0 != null && waypointsList_0.Count > 0)
                            waypointsList_0.RemoveAt(deleteIndex);

                        if (waypointsList_4 != null && waypointsList_4.Count > 0)
                            waypointsList_4.RemoveAt(deleteIndex);

                        if (waypointsList_5 != null && waypointsList_5.Count > 0)
                            waypointsList_5.RemoveAt(deleteIndex);

                        if (waypointsList_6 != null && waypointsList_6.Count > 0)
                            waypointsList_6.RemoveAt(deleteIndex);

                        deleteIndex = -1;

                        SetIndexNumber();
                    }
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("[Waypoints 6 - Outside]       count: " + waypointsList_6.Count, SectionNameStyle);
            showWayPointsList_6 = EditorGUILayout.Foldout(showWayPointsList_6, "Waypoints 6 List");
            if (showWayPointsList_6)
            {
                if (waypointsList_6 != null)
                {
                    int deleteIndex = -1;
                    for (int cnt = 0; cnt < waypointsList_6.Count; cnt++)
                    {
                        Color guiColor = GUI.color;

                        Waypoint cwp = waypointsList_6[cnt];

                        if (cwp.currentWayPointType == Waypoint.WayPointType.Blocked)
                            GUI.color = Color.yellow;
                        else if (cwp.currentWayPointType == Waypoint.WayPointType.OutOfBoundary)
                            GUI.color = Color.red;
                        else
                            GUI.color = Color.white;

                        if (cwp == selectedWaypoint)
                            GUI.color = Color.green;

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("[" + waypointsList_6[cnt].index + "]", GUILayout.Width(33));
                        if (GUILayout.Button("S", GUILayout.Width(20)))
                        {
                            if (selectedWaypoint == cwp)
                            {
                                selectedWaypoint = null;
                            }
                            else
                            {
                                selectedWaypoint = cwp;
                            }

                            dorepaint = true;

                        }

                        EditorGUI.BeginChangeCheck();
                        Vector3 oldV = cwp.GetPosition(waypointsGroup.XYZConstraint);
                        Vector3 newV = EditorGUILayout.Vector3Field("", oldV);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(waypointsGroup, "Waypoint Moved");
                            cwp.UpdatePosition(newV - oldV, waypointsGroup.XYZConstraint);
                        }

                        if (GUILayout.Button("Del", GUILayout.Width(30)))
                        {
                            deleteIndex = cnt;
                            dorepaint = true;
                            selectedWaypoint = null;
                        }

                        GUILayout.TextField(cwp.currentWayPointType.ToString(), 10);
                        if (GUILayout.Button("↔", GUILayout.Width(25)))
                        {
                            var curr = cwp.currentWayPointType;
                            if (curr == Waypoint.WayPointType.Normal)
                                cwp.currentWayPointType = Waypoint.WayPointType.Blocked;
                            else if (curr == Waypoint.WayPointType.Blocked)
                                cwp.currentWayPointType = Waypoint.WayPointType.OutOfBoundary;
                            else if (curr == Waypoint.WayPointType.OutOfBoundary)
                                cwp.currentWayPointType = Waypoint.WayPointType.Normal;

                            dorepaint = true;
                        }

                        GUI.color = guiColor;
                        EditorGUILayout.EndHorizontal();

                    }

                    if (deleteIndex > -1)
                    {
                        //아래 순서 꼭 지켜야함!!! (RemoveAt 때문에 list에 변동 생김)

                        if (waypointsList_3[deleteIndex] == selectedWaypoint)
                            selectedWaypoint = null;

                        waypointsList_3.RemoveAt(deleteIndex);

                        if (waypointsList_2 != null && waypointsList_2.Count > 0)
                            waypointsList_2.RemoveAt(deleteIndex);

                        if (waypointsList_1 != null && waypointsList_1.Count > 0)
                            waypointsList_1.RemoveAt(deleteIndex);

                        if (waypointsList_0 != null && waypointsList_0.Count > 0)
                            waypointsList_0.RemoveAt(deleteIndex);

                        if (waypointsList_4 != null && waypointsList_4.Count > 0)
                            waypointsList_4.RemoveAt(deleteIndex);

                        if (waypointsList_5 != null && waypointsList_5.Count > 0)
                            waypointsList_5.RemoveAt(deleteIndex);

                        if (waypointsList_6 != null && waypointsList_6.Count > 0)
                            waypointsList_6.RemoveAt(deleteIndex);

                        deleteIndex = -1;

                        SetIndexNumber();
                    }
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
                
            EditorGUILayout.LabelField("Press N to Create ");
            if (GUILayout.Button("Add (+1)"))
            {
                multipleSelectedWayPoints.Clear();
                AddWayPoint();
                SetIndexNumber();
                dorepaint = true;

                SaveScene();
            }

            if (GUILayout.Button("Slerp Waypoints (x2)"))
            {
                SlerpWayPoints();
                SetIndexNumber();
                dorepaint = true;

                SaveScene();
            }

            if (GUILayout.Button("Lerp Waypoints (x2)"))
            {
                LerpWayPoints();
                SetIndexNumber();
                dorepaint = true;

                SaveScene();
            }


            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Waypoint 3 ====>  Waypoint 0 & 1 & 2 & 4 & 5 & 6"))
            {
                ExpandWayPoints();
                SetIndexNumber();
                dorepaint = true;

                SaveScene();
            }
            inputDist = GUILayout.TextField(inputDist, 25);
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Set Y(Height) Coordinates Automatically"))
            {
                Undo.RecordObject(waypointsGroup, "Set Y(Height) Coordinates");

                var allWayPoints = new List<List<WayPointSystem.Waypoint>>();
                allWayPoints.Add(waypointsList_0);
                allWayPoints.Add(waypointsList_1);
                allWayPoints.Add(waypointsList_2);
                allWayPoints.Add(waypointsList_3);
                allWayPoints.Add(waypointsList_4);
                allWayPoints.Add(waypointsList_5);
                allWayPoints.Add(waypointsList_6);

                foreach (var j in allWayPoints)
                {
                    foreach (var i in j)
                    {
                        bool isHit = false;
                        Vector3 hitPos = Vector3.zero;

                        float dist = 300f;
                        var rayStartPoint = i.GetPosition();
                        rayStartPoint = new Vector3(rayStartPoint.x, dist, rayStartPoint.z);

                        int layerMask = LayerMask.GetMask("Ground");
                        RaycastHit hit;
                        Ray ray = new Ray(rayStartPoint, Vector3.down);

                        if (Physics.Raycast(ray, out hit, dist * 1.5f, layerMask))
                        {   
                            isHit = true;
                            hitPos = hit.point;
                        }

                        if (isHit)
                        {
                            var newPos = new Vector3(i.GetPosition().x, hitPos.y, i.GetPosition().z);
                            i.SetPosition(newPos, PositionConstraint.XYZ);
                        }
                    }
                }
                dorepaint = true;

                SaveScene();
            }


            if (GUILayout.Button("Set Index Number"))
            {
                SetIndexNumber();
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            if (GUILayout.Button("Clear Waypoints 0 & 1 & 2 & 4 & 5 & 6"))
            {
                Undo.RecordObject(waypointsGroup, "Clear Waypoints 0 &1 & 2 & 4 & 5 & 6");
                waypointsList_0.Clear();
                waypointsList_1.Clear();
                waypointsList_2.Clear();
                waypointsList_4.Clear();
                waypointsList_5.Clear();
                waypointsList_6.Clear();
                waypointsGroup.ClearWayPoints_0_1_2_4_5_6();
                dorepaint = true;

                SaveScene();
            }

            if (GUILayout.Button("Clear All Waypoints"))
            {
                Undo.RecordObject(waypointsGroup, "Clear All Waypoints");
                waypointsList_0.Clear();
                waypointsList_1.Clear();
                waypointsList_2.Clear();
                waypointsList_3.Clear();
                waypointsList_4.Clear();
                waypointsList_5.Clear();
                waypointsList_6.Clear();
                waypointsGroup.ClearAllWaypoints();
                dorepaint = true;

                SaveScene();
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();


            if (multipleSelectedWayPoints != null && multipleSelectedWayPoints.Count > 0)
            {
                EditorGUILayout.LabelField("[SELECTED MULTIPLE WAYPOINTS]       count: " + multipleSelectedWayPoints.Count, SectionNameStyle);

                for (int i = 0; i < multipleSelectedWayPoints.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(multipleSelectedWayPoints[i].GetPosition().ToString());
                    EditorGUILayout.EndHorizontal();
                }

                if (GUILayout.Button("Clear Group Selection"))
                {
                    multipleSelectedWayPoints.Clear();
                    dorepaint = true;

                    SaveScene();
                }


            }
            else
            {
                EditorGUILayout.LabelField("[SELECTED MULTIPLE WAYPOINTS]    Press X to Create and C to Clear  ");
            }

            if (multipleSelectedWayPoints != null && multipleSelectedWayPoints.Count == 2)
            {
                if (GUILayout.Button("Unify All Waypoint Distance"))
                {
                    UnifyWaypoints();
                    dorepaint = true;
                }
            }


            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            toggleShowLane_All = EditorGUILayout.Toggle("Show All Lanes", toggleShowLane_All);
            if (toggleShowLane_All)
            {
                toggleShowLane_All = true;
                toggleShowLane_0 = false;
                toggleShowLane_1 = false;
                toggleShowLane_2 = false;
                toggleShowLane_3 = false;
                toggleShowLane_4 = false;
                toggleShowLane_5 = false;
                toggleShowLane_6 = false;

#if UNITY_EDITOR
                waypointsGroup.ToggleShowLane(WaypointsGroup.ToggleShowLaneType.All);
#endif
            }
            else
            {
#if UNITY_EDITOR
                waypointsGroup.ToggleHideLane(WaypointsGroup.ToggleShowLaneType.All);
#endif
            }

            toggleShowLane_0 = EditorGUILayout.Toggle("Show Lane 0", toggleShowLane_0);
            if (toggleShowLane_0)
            {
                toggleShowLane_All = false;
                toggleShowLane_0 = true;

#if UNITY_EDITOR
                waypointsGroup.ToggleShowLane(WaypointsGroup.ToggleShowLaneType.Zero);
#endif
            }
            else
            {
#if UNITY_EDITOR
                waypointsGroup.ToggleHideLane(WaypointsGroup.ToggleShowLaneType.Zero);
#endif
            }

            toggleShowLane_1 = EditorGUILayout.Toggle("Show Lane 1", toggleShowLane_1);
            if (toggleShowLane_1)
            {
                toggleShowLane_All = false;
                toggleShowLane_1 = true;

#if UNITY_EDITOR
                waypointsGroup.ToggleShowLane(WaypointsGroup.ToggleShowLaneType.One);
#endif
            }
            else
            {
#if UNITY_EDITOR
                waypointsGroup.ToggleHideLane(WaypointsGroup.ToggleShowLaneType.One);
#endif
            }

            toggleShowLane_2 = EditorGUILayout.Toggle("Show Lane 2", toggleShowLane_2);
            if (toggleShowLane_2)
            {
                toggleShowLane_All = false;
                toggleShowLane_2 = true;

#if UNITY_EDITOR
                waypointsGroup.ToggleShowLane(WaypointsGroup.ToggleShowLaneType.Two);
#endif
            }
            else
            {
#if UNITY_EDITOR
                waypointsGroup.ToggleHideLane(WaypointsGroup.ToggleShowLaneType.Two);
#endif
            }

            toggleShowLane_3 = EditorGUILayout.Toggle("Show Lane 3", toggleShowLane_3);
            if (toggleShowLane_3)
            {
                toggleShowLane_All = false;
                toggleShowLane_3 = true;

#if UNITY_EDITOR
                waypointsGroup.ToggleShowLane(WaypointsGroup.ToggleShowLaneType.Three);
#endif
            }
            else
            {
#if UNITY_EDITOR
                waypointsGroup.ToggleHideLane(WaypointsGroup.ToggleShowLaneType.Three);
#endif
            }

            toggleShowLane_4 = EditorGUILayout.Toggle("Show Lane 4", toggleShowLane_4);
            if (toggleShowLane_4)
            {
                toggleShowLane_All = false;
                toggleShowLane_4 = true;

#if UNITY_EDITOR
                waypointsGroup.ToggleShowLane(WaypointsGroup.ToggleShowLaneType.Four);
#endif
            }
            else
            {
#if UNITY_EDITOR
                waypointsGroup.ToggleHideLane(WaypointsGroup.ToggleShowLaneType.Four);
#endif
            }

            toggleShowLane_5 = EditorGUILayout.Toggle("Show Lane 5", toggleShowLane_5);
            if (toggleShowLane_5)
            {
                toggleShowLane_All = false;
                toggleShowLane_5 = true;

#if UNITY_EDITOR
                waypointsGroup.ToggleShowLane(WaypointsGroup.ToggleShowLaneType.Five);
#endif
            }
            else
            {
#if UNITY_EDITOR
                waypointsGroup.ToggleHideLane(WaypointsGroup.ToggleShowLaneType.Five);
#endif
            }

            toggleShowLane_6 = EditorGUILayout.Toggle("Show Lane 6", toggleShowLane_6);
            if (toggleShowLane_6)
            {
                toggleShowLane_All = false;
                toggleShowLane_6 = true;

#if UNITY_EDITOR
                waypointsGroup.ToggleShowLane(WaypointsGroup.ToggleShowLaneType.Six);
#endif
            }
            else
            {
#if UNITY_EDITOR
                waypointsGroup.ToggleHideLane(WaypointsGroup.ToggleShowLaneType.Six);
#endif
            }

            EditorGUILayout.EndVertical();
            if (dorepaint)
            {
                SceneView.RepaintAll();
            }
        }








        public void DrawWaypoints(List<Waypoint> waypoints)
        {
            if (waypoints != null)
            {
                int cnt = 0;
                foreach (Waypoint wp in waypoints)
                {
                    doRepaint |= DrawInScene(wp, multipleSelectedWayPoints);

                    // Draw a pointer line 
                    if(cnt < waypoints.Count-1)
                    {
                        Color c = Handles.color;
                        Handles.color = Color.gray;
                        Waypoint wpnext = waypoints[cnt+1];
                        Handles.DrawLine(wp.GetPosition(waypointsGroup.XYZConstraint), wpnext.GetPosition(waypointsGroup.XYZConstraint));
                        Handles.color = c;
                    }
                    else
                    {
                        Waypoint wpnext = waypoints[0];
                        Color c = Handles.color;
                        Handles.color = Color.gray;
                        Handles.DrawLine(wp.GetPosition(waypointsGroup.XYZConstraint), wpnext.GetPosition(waypointsGroup.XYZConstraint));
                        Handles.color = c;
                    }

                    if (cnt == 0)
                    {
                        Color c = Handles.color;
                        Handles.color = Color.white;
                        Handles.DrawWireCube(wp.GetPosition(waypointsGroup.XYZConstraint), Vector3.one);
                        Handles.color = c;
                    }
                        
                    cnt += 1;
                }
            }

            if(doRepaint)
            {
                Repaint();
            }

        }

        private void AddWayPoint()
        {
            Undo.RecordObject(waypointsGroup, "Waypoint Added");

            //waypoint 3 이 기준...........!

            if (waypointsList_3 == null || waypointsList_3.Count <= 0)
            {
                int index = 0;
                Waypoint wp = new Waypoint();
                waypointsGroup.AddWaypoint_3(wp, index);

                selectedWaypoint = wp;
            }
            else
            {
                int index = -1;
                int insertIndex = -1;

                Waypoint wp_three = new Waypoint();
                Waypoint wp_two = new Waypoint();
                Waypoint wp_one = new Waypoint();
                Waypoint wp_zero = new Waypoint();
                Waypoint wp_four= new Waypoint();
                Waypoint wp_five = new Waypoint();
                Waypoint wp_six = new Waypoint();

                // waypoint 3
                if (selectedWaypoint != null)
                {
                    index = waypointsList_3.IndexOf(selectedWaypoint);

                    if (waypointsList_2 != null && waypointsList_2.Count == waypointsList_3.Count)
                    {
                        if (index == -1) // waypointsList_2에서 찾아보자
                            index = waypointsList_2.IndexOf(selectedWaypoint);
                    }
                    if (waypointsList_4 != null && waypointsList_4.Count == waypointsList_3.Count)
                    {
                        if (index == -1) // waypointsList_4에서 찾아보자
                            index = waypointsList_4.IndexOf(selectedWaypoint);
                    }
                    if (waypointsList_1 != null && waypointsList_1.Count == waypointsList_3.Count)
                    {
                        if (index == -1) // waypointsList_1에서 찾아보자
                            index = waypointsList_1.IndexOf(selectedWaypoint);
                    }
                    if (waypointsList_0 != null && waypointsList_0.Count == waypointsList_3.Count)
                    {
                        if (index == -1) // waypointsList_6에서 찾아보자
                            index = waypointsList_0.IndexOf(selectedWaypoint);
                    }
                    if (waypointsList_5 != null && waypointsList_5.Count == waypointsList_3.Count)
                    {
                        if (index == -1) // waypointsList_5에서 찾아보자
                            index = waypointsList_5.IndexOf(selectedWaypoint);
                    }
                    if (waypointsList_6 != null && waypointsList_6.Count == waypointsList_3.Count)
                    {
                        if (index == -1) // waypointsList_6에서 찾아보자
                            index = waypointsList_6.IndexOf(selectedWaypoint);
                    }

                    insertIndex = index + 1;
                }
                else
                {
                    index = waypointsList_3.Count - 1;
                    insertIndex = waypointsList_3.Count - 1;
                }

                if (index == -1 || insertIndex == -1)
                {
                    Debug.Log("Error in index...!");
                    return;
                }

                if (selectedWaypoint == null)
                {
                    var lastWayPoint = waypointsList_3[waypointsList_3.Count - 1];
                    wp_three.CopyOther(lastWayPoint);

                    selectedWaypoint = lastWayPoint;
                }
                else
                {
                    if (index < waypointsList_3.Count - 1 && waypointsList_3.Count >= 2)
                    {
                        Waypoint p1 = waypointsList_3[index];
                        Waypoint p2 = waypointsList_3[index + 1];

                        wp_three.position = Vector3.Lerp(p1.position, p2.position, 0.5f);
                        wp_three.rotation = Quaternion.identity;
                        wp_three.UpdatePosition(wp_three.position, waypointsGroup.XYZConstraint);
                        wp_three.currentWayPointType = p1.currentWayPointType;
                    }
                    else if (index == waypointsList_3.Count - 1 && waypointsList_3.Count >= 2)
                    {
                        // 마지막 index 인 경우

                        if (waypointsList_3.Count >= 2)
                        {
                            //앞쪽에다가 만들어주자...
                            Waypoint p1 = waypointsList_3[index];
                            Waypoint p2 = waypointsList_3[index - 1];

                            var dir = (p1.position - p2.position).normalized;

                            wp_three.position = p1.position + dir * 5f;
                            wp_three.rotation = Quaternion.identity;
                            wp_three.UpdatePosition(wp_three.position, waypointsGroup.XYZConstraint);
                        }
                        else
                        {
                            Waypoint p1 = waypointsList_3[index];
                            Waypoint p2 = waypointsList_3[0];

                            wp_three.position = Vector3.Lerp(p1.position, p2.position, 0.5f);
                            wp_three.rotation = Quaternion.identity;
                            wp_three.UpdatePosition(wp_three.position, waypointsGroup.XYZConstraint);
                        }
                    }
                    else
                    {
                        wp_three.CopyOther(selectedWaypoint);
                    }

                    selectedWaypoint = wp_three;
                }

                // waypoint 2 list
                if (waypointsList_3.Count == waypointsList_2.Count)
                {
                    if (selectedWaypoint == null)
                    {
                        var lastWayPoint = waypointsList_2[waypointsList_2.Count - 1];
                        wp_two.CopyOther(lastWayPoint);

                        selectedWaypoint = wp_two;
                    }
                    else
                    {
                        if (index < waypointsList_2.Count - 1 && waypointsList_2.Count >= 2)
                        {
                            Waypoint p1 = waypointsList_2[index];
                            Waypoint p2 = waypointsList_2[index + 1];

                            wp_two.position = Vector3.Lerp(p1.position, p2.position, 0.5f);
                            wp_two.rotation = Quaternion.identity;
                            wp_two.UpdatePosition(wp_two.position, waypointsGroup.XYZConstraint);
                            wp_two.currentWayPointType = p1.currentWayPointType;
                        }
                        else if (index == waypointsList_2.Count - 1 && waypointsList_2.Count >= 2)
                        {
                            // 마지막 index 인 경우

                            if (waypointsList_2.Count >= 2)
                            {
                                //앞쪽에다가 만들어주자...
                                Waypoint p1 = waypointsList_2[index];
                                Waypoint p2 = waypointsList_2[index - 1];

                                var dir = (p1.position - p2.position).normalized;

                                wp_two.position = p1.position + dir * 5f;
                                wp_two.rotation = Quaternion.identity;
                                wp_two.UpdatePosition(wp_two.position, waypointsGroup.XYZConstraint);
                                wp_two.currentWayPointType = p1.currentWayPointType;
                            }
                            else
                            {
                                Waypoint p1 = waypointsList_2[index];
                                Waypoint p2 = waypointsList_2[0];

                                wp_two.position = Vector3.Lerp(p1.position, p2.position, 0.5f);
                                wp_two.rotation = Quaternion.identity;
                                wp_two.UpdatePosition(wp_two.position, waypointsGroup.XYZConstraint);
                                wp_two.currentWayPointType = p1.currentWayPointType;
                            }
                        }
                        else
                        {
                            wp_two.CopyOther(selectedWaypoint);
                        }

                        selectedWaypoint = wp_two;
                    }
                }
                else
                {

                }


                // waypoint 1 list
                if (waypointsList_3.Count == waypointsList_1.Count)
                {
                    if (selectedWaypoint == null)
                    {
                        var lastWayPoint = waypointsList_1[waypointsList_1.Count - 1];
                        wp_one.CopyOther(lastWayPoint);

                        selectedWaypoint = wp_one;
                    }
                    else
                    {
                        if (index < waypointsList_1.Count - 1 && waypointsList_1.Count >= 2)
                        {
                            Waypoint p1 = waypointsList_1[index];
                            Waypoint p2 = waypointsList_1[index + 1];

                            wp_one.position = Vector3.Lerp(p1.position, p2.position, 0.5f);
                            wp_one.rotation = Quaternion.identity;
                            wp_one.UpdatePosition(wp_one.position, waypointsGroup.XYZConstraint);
                            wp_one.currentWayPointType = p1.currentWayPointType;
                        }
                        else if (index == waypointsList_1.Count - 1 && waypointsList_1.Count >= 2)
                        {
                            // 마지막 index 인 경우

                            if (waypointsList_1.Count >= 2)
                            {
                                //앞쪽에다가 만들어주자...
                                Waypoint p1 = waypointsList_1[index];
                                Waypoint p2 = waypointsList_1[index - 1];

                                var dir = (p1.position - p2.position).normalized;

                                wp_one.position = p1.position + dir * 5f;
                                wp_one.rotation = Quaternion.identity;
                                wp_one.UpdatePosition(wp_one.position, waypointsGroup.XYZConstraint);
                            }
                            else
                            {
                                Waypoint p1 = waypointsList_1[index];
                                Waypoint p2 = waypointsList_1[0];

                                wp_one.position = Vector3.Lerp(p1.position, p2.position, 0.5f);
                                wp_one.rotation = Quaternion.identity;
                                wp_one.UpdatePosition(wp_one.position, waypointsGroup.XYZConstraint);
                            }
                        }
                        else
                        {
                            wp_one.CopyOther(selectedWaypoint);
                        }

                        selectedWaypoint = wp_one;
                    }
                }
                else
                {

                }

                // waypoint 0 list
                if (waypointsList_3.Count == waypointsList_0.Count)
                {
                    if (selectedWaypoint == null)
                    {
                        var lastWayPoint = waypointsList_0[waypointsList_0.Count - 1];
                        wp_zero.CopyOther(lastWayPoint);

                        selectedWaypoint = wp_zero;
                    }
                    else
                    {
                        if (index < waypointsList_0.Count - 1 && waypointsList_0.Count >= 2)
                        {
                            Waypoint p1 = waypointsList_0[index];
                            Waypoint p2 = waypointsList_0[index + 1];

                            wp_zero.position = Vector3.Lerp(p1.position, p2.position, 0.5f);
                            wp_zero.rotation = Quaternion.identity;
                            wp_zero.UpdatePosition(wp_zero.position, waypointsGroup.XYZConstraint);
                            wp_zero.currentWayPointType = p1.currentWayPointType;
                        }
                        else if (index == waypointsList_0.Count - 1 && waypointsList_0.Count >= 2)
                        {
                            // 마지막 index 인 경우

                            if (waypointsList_0.Count >= 2)
                            {
                                //앞쪽에다가 만들어주자...
                                Waypoint p1 = waypointsList_0[index];
                                Waypoint p2 = waypointsList_0[index - 1];

                                var dir = (p1.position - p2.position).normalized;

                                wp_zero.position = p1.position + dir * 5f;
                                wp_zero.rotation = Quaternion.identity;
                                wp_zero.UpdatePosition(wp_zero.position, waypointsGroup.XYZConstraint);
                                wp_zero.currentWayPointType = p1.currentWayPointType;
                            }
                            else
                            {
                                Waypoint p1 = waypointsList_0[index];
                                Waypoint p2 = waypointsList_0[0];

                                wp_zero.position = Vector3.Lerp(p1.position, p2.position, 0.5f);
                                wp_zero.rotation = Quaternion.identity;
                                wp_zero.UpdatePosition(wp_zero.position, waypointsGroup.XYZConstraint);
                                wp_zero.currentWayPointType = p1.currentWayPointType;
                            }
                        }
                        else
                        {
                            wp_zero.CopyOther(selectedWaypoint);
                        }

                        selectedWaypoint = wp_zero;
                    }
                }
                else
                {

                }


                // waypoint 4 list
                if (waypointsList_3.Count == waypointsList_4.Count)
                {
                    if (selectedWaypoint == null)
                    {
                        var lastWayPoint = waypointsList_4[waypointsList_4.Count - 1];
                        wp_four.CopyOther(lastWayPoint);

                        selectedWaypoint = wp_four;
                    }
                    else
                    {
                        if (index < waypointsList_4.Count - 1 && waypointsList_4.Count >= 2)
                        {
                            Waypoint p1 = waypointsList_4[index];
                            Waypoint p2 = waypointsList_4[index + 1];

                            wp_four.position = Vector3.Lerp(p1.position, p2.position, 0.5f);
                            wp_four.rotation = Quaternion.identity;
                            wp_four.UpdatePosition(wp_four.position, waypointsGroup.XYZConstraint);
                            wp_four.currentWayPointType = p1.currentWayPointType;
                        }
                        else if (index == waypointsList_4.Count - 1 && waypointsList_4.Count >= 2)
                        {
                            // 마지막 index 인 경우

                            if (waypointsList_4.Count >= 2)
                            {
                                //앞쪽에다가 만들어주자...
                                Waypoint p1 = waypointsList_4[index];
                                Waypoint p2 = waypointsList_4[index - 1];

                                var dir = (p1.position - p2.position).normalized;

                                wp_four.position = p1.position + dir * 5f;
                                wp_four.rotation = Quaternion.identity;
                                wp_four.UpdatePosition(wp_four.position, waypointsGroup.XYZConstraint);
                                wp_four.currentWayPointType = p1.currentWayPointType;
                            }
                            else
                            {
                                Waypoint p1 = waypointsList_4[index];
                                Waypoint p2 = waypointsList_4[0];

                                wp_four.position = Vector3.Lerp(p1.position, p2.position, 0.5f);
                                wp_four.rotation = Quaternion.identity;
                                wp_four.UpdatePosition(wp_four.position, waypointsGroup.XYZConstraint);
                                wp_four.currentWayPointType = p1.currentWayPointType;
                            }


                        }
                        else
                        {
                            wp_four.CopyOther(selectedWaypoint);
                        }

                        selectedWaypoint = wp_four;
                    }
                }
                else
                {

                }


                // waypoint 5 list
                if (waypointsList_3.Count == waypointsList_5.Count)
                {
                    if (selectedWaypoint == null)
                    {
                        var lastWayPoint = waypointsList_5[waypointsList_5.Count - 1];
                        wp_five.CopyOther(lastWayPoint);

                        selectedWaypoint = wp_five;
                    }
                    else
                    {
                        if (index < waypointsList_5.Count - 1 && waypointsList_5.Count >= 2)
                        {
                            Waypoint p1 = waypointsList_5[index];
                            Waypoint p2 = waypointsList_5[index + 1];

                            wp_five.position = Vector3.Lerp(p1.position, p2.position, 0.5f);
                            wp_five.rotation = Quaternion.identity;
                            wp_five.UpdatePosition(wp_five.position, waypointsGroup.XYZConstraint);
                            wp_five.currentWayPointType = p1.currentWayPointType;
                        }
                        else if (index == waypointsList_5.Count - 1 && waypointsList_5.Count >= 2)
                        {
                            // 마지막 index 인 경우

                            if (waypointsList_5.Count >= 2)
                            {
                                //앞쪽에다가 만들어주자...
                                Waypoint p1 = waypointsList_5[index];
                                Waypoint p2 = waypointsList_5[index - 1];

                                var dir = (p1.position - p2.position).normalized;

                                wp_five.position = p1.position + dir * 5f;
                                wp_five.rotation = Quaternion.identity;
                                wp_five.UpdatePosition(wp_five.position, waypointsGroup.XYZConstraint);
                                wp_five.currentWayPointType = p1.currentWayPointType;
                            }
                            else
                            {
                                Waypoint p1 = waypointsList_5[index];
                                Waypoint p2 = waypointsList_5[0];

                                wp_five.position = Vector3.Lerp(p1.position, p2.position, 0.5f);
                                wp_five.rotation = Quaternion.identity;
                                wp_five.UpdatePosition(wp_five.position, waypointsGroup.XYZConstraint);
                                wp_five.currentWayPointType = p1.currentWayPointType;
                            }

                        }
                        else
                        {
                            wp_five.CopyOther(selectedWaypoint);
                        }

                        selectedWaypoint = wp_five;
                    }
                }
                else
                {

                }

                // waypoint 6 list
                if (waypointsList_3.Count == waypointsList_6.Count)
                {
                    if (selectedWaypoint == null)
                    {
                        var lastWayPoint = waypointsList_6[waypointsList_6.Count - 1];
                        wp_six.CopyOther(lastWayPoint);

                        selectedWaypoint = wp_six;
                    }
                    else
                    {
                        if (index < waypointsList_6.Count - 1 && waypointsList_6.Count >= 2)
                        {
                            Waypoint p1 = waypointsList_6[index];
                            Waypoint p2 = waypointsList_6[index + 1];

                            wp_six.position = Vector3.Lerp(p1.position, p2.position, 0.5f);
                            wp_six.rotation = Quaternion.identity;
                            wp_six.UpdatePosition(wp_six.position, waypointsGroup.XYZConstraint);
                            wp_six.currentWayPointType = p1.currentWayPointType;
                        }
                        else if (index == waypointsList_6.Count - 1 && waypointsList_6.Count >= 2)
                        {
                            // 마지막 index 인 경우

                            if (waypointsList_6.Count >= 2)
                            {
                                //앞쪽에다가 만들어주자...
                                Waypoint p1 = waypointsList_6[index];
                                Waypoint p2 = waypointsList_6[index - 1];

                                var dir = (p1.position - p2.position).normalized;

                                wp_six.position = p1.position + dir * 5f;
                                wp_six.rotation = Quaternion.identity;
                                wp_six.UpdatePosition(wp_six.position, waypointsGroup.XYZConstraint);
                                wp_six.currentWayPointType = p1.currentWayPointType;
                            }
                            else
                            {
                                Waypoint p1 = waypointsList_6[index];
                                Waypoint p2 = waypointsList_6[0];

                                wp_six.position = Vector3.Lerp(p1.position, p2.position, 0.5f);
                                wp_six.rotation = Quaternion.identity;
                                wp_six.UpdatePosition(wp_six.position, waypointsGroup.XYZConstraint);
                                wp_six.currentWayPointType = p1.currentWayPointType;
                            }

                        }
                        else
                        {
                            wp_six.CopyOther(selectedWaypoint);
                        }

                        selectedWaypoint = wp_six;
                    }
                }
                else
                {

                }


                waypointsGroup.AddWaypoint_3(wp_three, insertIndex);

                if (waypointsList_2 != null && waypointsList_2.Count > 0)
                    waypointsGroup.AddWaypoint_2(wp_two, insertIndex);
                if (waypointsList_1 != null && waypointsList_1.Count > 0)
                    waypointsGroup.AddWaypoint_1(wp_one, insertIndex);
                if (waypointsList_0 != null && waypointsList_0.Count > 0)
                    waypointsGroup.AddWaypoint_0(wp_zero, insertIndex);
                if (waypointsList_4 != null && waypointsList_4.Count > 0)
                    waypointsGroup.AddWaypoint_4(wp_four, insertIndex);
                if (waypointsList_5 != null && waypointsList_5.Count > 0)
                    waypointsGroup.AddWaypoint_5(wp_five, insertIndex);
                if (waypointsList_6 != null && waypointsList_6.Count > 0)
                    waypointsGroup.AddWaypoint_6(wp_six, insertIndex);
            }

        }

        private void SlerpWayPoints()
        {
            Undo.RecordObject(waypointsGroup, "Slerp Waypoints Added");

            if (waypointsList_3.Count > 0)
            {
                var currList = waypointsList_3.ToList();
                var additionalList = new List<Waypoint>();

                for (int i = 0; i < waypointsList_3.Count - 1; i++)
                {
                    Waypoint p1 = waypointsList_3[i];
                    Waypoint p2 = waypointsList_3[i + 1];

                    Waypoint newP = new Waypoint();
                    newP.position = Vector3.Slerp(p1.position, p2.position, 0.5f);
                    newP.rotation = Quaternion.identity;
                    newP.UpdatePosition(newP.position, waypointsGroup.XYZConstraint);

                    additionalList.Add(newP);
                }

                if (waypointsList_3.Count > 2)
                {
                    Waypoint p1 = waypointsList_3[waypointsList_3.Count - 1];
                    Waypoint p2 = waypointsList_3[0];

                    Waypoint newP = new Waypoint();
                    newP.position = Vector3.Slerp(p1.position, p2.position, 0.5f);
                    newP.rotation = Quaternion.identity;
                    newP.UpdatePosition(newP.position, waypointsGroup.XYZConstraint);

                    additionalList.Add(newP);
                }

                for (int i = 0; i < additionalList.Count; i++)
                {
                    int index = i + i * 1;
                    waypointsGroup.AddWaypoint_3(additionalList[i], index + 1);
                }
            }


            if (waypointsList_0.Count > 0)
            {
                var currList = waypointsList_0.ToList();
                var additionalList = new List<Waypoint>();

                for (int i = 0; i < waypointsList_0.Count - 1; i++)
                {
                    Waypoint p1 = waypointsList_0[i];
                    Waypoint p2 = waypointsList_0[i + 1];

                    Waypoint newP = new Waypoint();
                    newP.position = Vector3.Slerp(p1.position, p2.position, 0.5f);
                    newP.rotation = Quaternion.identity;
                    newP.UpdatePosition(newP.position, waypointsGroup.XYZConstraint);
                    newP.currentWayPointType = p1.currentWayPointType;

                    additionalList.Add(newP);
                }

                if (waypointsList_0.Count > 2)
                {
                    Waypoint p1 = waypointsList_0[waypointsList_0.Count - 1];
                    Waypoint p2 = waypointsList_0[0];

                    Waypoint newP = new Waypoint();
                    newP.position = Vector3.Slerp(p1.position, p2.position, 0.5f);
                    newP.rotation = Quaternion.identity;
                    newP.UpdatePosition(newP.position, waypointsGroup.XYZConstraint);
                    newP.currentWayPointType = p1.currentWayPointType;

                    additionalList.Add(newP);
                }

                for (int i = 0; i < additionalList.Count; i++)
                {
                    int index = i + i * 1;
                    waypointsGroup.AddWaypoint_0(additionalList[i], index + 1);
                }
            }

            if (waypointsList_1.Count > 0)
            {
                var currList = waypointsList_1.ToList();
                var additionalList = new List<Waypoint>();

                for (int i = 0; i < waypointsList_1.Count - 1; i++)
                {
                    Waypoint p1 = waypointsList_1[i];
                    Waypoint p2 = waypointsList_1[i + 1];

                    Waypoint newP = new Waypoint();
                    newP.position = Vector3.Slerp(p1.position, p2.position, 0.5f);
                    newP.rotation = Quaternion.identity;
                    newP.UpdatePosition(newP.position, waypointsGroup.XYZConstraint);
                    newP.currentWayPointType = p1.currentWayPointType;

                    additionalList.Add(newP);
                }

                if (waypointsList_1.Count > 2)
                {
                    Waypoint p1 = waypointsList_1[waypointsList_1.Count - 1];
                    Waypoint p2 = waypointsList_1[0];

                    Waypoint newP = new Waypoint();
                    newP.position = Vector3.Slerp(p1.position, p2.position, 0.5f);
                    newP.rotation = Quaternion.identity;
                    newP.UpdatePosition(newP.position, waypointsGroup.XYZConstraint);
                    newP.currentWayPointType = p1.currentWayPointType;

                    additionalList.Add(newP);
                }

                for (int i = 0; i < additionalList.Count; i++)
                {
                    int index = i + i * 1;
                    waypointsGroup.AddWaypoint_1(additionalList[i], index + 1);
                }
            }

            if (waypointsList_2.Count > 0)
            {
                var currList = waypointsList_2.ToList();
                var additionalList = new List<Waypoint>();

                for (int i = 0; i < waypointsList_2.Count - 1; i++)
                {
                    Waypoint p1 = waypointsList_2[i];
                    Waypoint p2 = waypointsList_2[i + 1];

                    Waypoint newP = new Waypoint();
                    newP.position = Vector3.Slerp(p1.position, p2.position, 0.5f);
                    newP.rotation = Quaternion.identity;
                    newP.UpdatePosition(newP.position, waypointsGroup.XYZConstraint);
                    newP.currentWayPointType = p1.currentWayPointType;

                    additionalList.Add(newP);
                }

                if (waypointsList_2.Count > 2)
                {
                    Waypoint p1 = waypointsList_2[waypointsList_2.Count - 1];
                    Waypoint p2 = waypointsList_2[0];

                    Waypoint newP = new Waypoint();
                    newP.position = Vector3.Slerp(p1.position, p2.position, 0.5f);
                    newP.rotation = Quaternion.identity;
                    newP.UpdatePosition(newP.position, waypointsGroup.XYZConstraint);
                    newP.currentWayPointType = p1.currentWayPointType;

                    additionalList.Add(newP);
                }

                for (int i = 0; i < additionalList.Count; i++)
                {
                    int index = i + i * 1;
                    waypointsGroup.AddWaypoint_2(additionalList[i], index + 1);
                }
            }

            if (waypointsList_4.Count > 0)
            {
                var currList = waypointsList_4.ToList();
                var additionalList = new List<Waypoint>();

                for (int i = 0; i < waypointsList_4.Count - 1; i++)
                {
                    Waypoint p1 = waypointsList_4[i];
                    Waypoint p2 = waypointsList_4[i + 1];

                    Waypoint newP = new Waypoint();
                    newP.position = Vector3.Slerp(p1.position, p2.position, 0.5f);
                    newP.rotation = Quaternion.identity;
                    newP.UpdatePosition(newP.position, waypointsGroup.XYZConstraint);
                    newP.currentWayPointType = p1.currentWayPointType;

                    additionalList.Add(newP);
                }

                if (waypointsList_4.Count > 2)
                {
                    Waypoint p1 = waypointsList_4[waypointsList_4.Count - 1];
                    Waypoint p2 = waypointsList_4[0];

                    Waypoint newP = new Waypoint();
                    newP.position = Vector3.Slerp(p1.position, p2.position, 0.5f);
                    newP.rotation = Quaternion.identity;
                    newP.UpdatePosition(newP.position, waypointsGroup.XYZConstraint);
                    newP.currentWayPointType = p1.currentWayPointType;

                    additionalList.Add(newP);
                }

                for (int i = 0; i < additionalList.Count; i++)
                {
                    int index = i + i * 1;
                    waypointsGroup.AddWaypoint_4(additionalList[i], index + 1);
                }
            }

            if (waypointsList_5.Count > 0)
            {
                var currList = waypointsList_5.ToList();
                var additionalList = new List<Waypoint>();

                for (int i = 0; i < waypointsList_5.Count - 1; i++)
                {
                    Waypoint p1 = waypointsList_5[i];
                    Waypoint p2 = waypointsList_5[i + 1];

                    Waypoint newP = new Waypoint();
                    newP.position = Vector3.Slerp(p1.position, p2.position, 0.5f);
                    newP.rotation = Quaternion.identity;
                    newP.UpdatePosition(newP.position, waypointsGroup.XYZConstraint);
                    newP.currentWayPointType = p1.currentWayPointType;

                    additionalList.Add(newP);
                }

                if (waypointsList_5.Count > 2)
                {
                    Waypoint p1 = waypointsList_5[waypointsList_5.Count - 1];
                    Waypoint p2 = waypointsList_5[0];

                    Waypoint newP = new Waypoint();
                    newP.position = Vector3.Slerp(p1.position, p2.position, 0.5f);
                    newP.rotation = Quaternion.identity;
                    newP.UpdatePosition(newP.position, waypointsGroup.XYZConstraint);
                    newP.currentWayPointType = p1.currentWayPointType;

                    additionalList.Add(newP);
                }

                for (int i = 0; i < additionalList.Count; i++)
                {
                    int index = i + i * 1;
                    waypointsGroup.AddWaypoint_5(additionalList[i], index + 1);
                }
            }

            if (waypointsList_6.Count > 0)
            {
                var currList = waypointsList_6.ToList();
                var additionalList = new List<Waypoint>();

                for (int i = 0; i < waypointsList_6.Count - 1; i++)
                {
                    Waypoint p1 = waypointsList_6[i];
                    Waypoint p2 = waypointsList_6[i + 1];

                    Waypoint newP = new Waypoint();
                    newP.position = Vector3.Slerp(p1.position, p2.position, 0.5f);
                    newP.rotation = Quaternion.identity;
                    newP.UpdatePosition(newP.position, waypointsGroup.XYZConstraint);
                    newP.currentWayPointType = p1.currentWayPointType;

                    additionalList.Add(newP);
                }

                if (waypointsList_6.Count > 2)
                {
                    Waypoint p1 = waypointsList_6[waypointsList_6.Count - 1];
                    Waypoint p2 = waypointsList_6[0];

                    Waypoint newP = new Waypoint();
                    newP.position = Vector3.Slerp(p1.position, p2.position, 0.5f);
                    newP.rotation = Quaternion.identity;
                    newP.UpdatePosition(newP.position, waypointsGroup.XYZConstraint);
                    newP.currentWayPointType = p1.currentWayPointType;

                    additionalList.Add(newP);
                }

                for (int i = 0; i < additionalList.Count; i++)
                {
                    int index = i + i * 1;
                    waypointsGroup.AddWaypoint_6(additionalList[i], index + 1);
                }
            }
        }

        private void LerpWayPoints()
        {
            Undo.RecordObject(waypointsGroup, "Lerp Waypoints Added");

            if (waypointsList_3.Count > 0)
            {
                var currList = waypointsList_3.ToList();
                var additionalList = new List<Waypoint>();

                for (int i = 0; i < waypointsList_3.Count - 1; i++)
                {
                    Waypoint p1 = waypointsList_3[i];
                    Waypoint p2 = waypointsList_3[i + 1];

                    Waypoint newP = new Waypoint();
                    newP.position = Vector3.Lerp(p1.position, p2.position, 0.5f);
                    newP.rotation = Quaternion.identity;
                    newP.UpdatePosition(newP.position, waypointsGroup.XYZConstraint);
                    newP.currentWayPointType = p1.currentWayPointType;

                    additionalList.Add(newP);
                }

                if (waypointsList_3.Count > 2)
                {
                    Waypoint p1 = waypointsList_3[waypointsList_3.Count - 1];
                    Waypoint p2 = waypointsList_3[0];

                    Waypoint newP = new Waypoint();
                    newP.position = Vector3.Lerp(p1.position, p2.position, 0.5f);
                    newP.rotation = Quaternion.identity;
                    newP.UpdatePosition(newP.position, waypointsGroup.XYZConstraint);
                    newP.currentWayPointType = p1.currentWayPointType;

                    additionalList.Add(newP);
                }

                for (int i = 0; i < additionalList.Count; i++)
                {
                    int index = i + i * 1;
                    waypointsGroup.AddWaypoint_3(additionalList[i], index + 1);
                }
            }

            if (waypointsList_0.Count > 0)
            {
                var currList = waypointsList_0.ToList();
                var additionalList = new List<Waypoint>();

                for (int i = 0; i < waypointsList_0.Count - 1; i++)
                {
                    Waypoint p1 = waypointsList_0[i];
                    Waypoint p2 = waypointsList_0[i + 1];

                    Waypoint newP = new Waypoint();
                    newP.position = Vector3.Lerp(p1.position, p2.position, 0.5f);
                    newP.rotation = Quaternion.identity;
                    newP.UpdatePosition(newP.position, waypointsGroup.XYZConstraint);
                    newP.currentWayPointType = p1.currentWayPointType;

                    additionalList.Add(newP);
                }

                if (waypointsList_0.Count > 2)
                {
                    Waypoint p1 = waypointsList_0[waypointsList_0.Count - 1];
                    Waypoint p2 = waypointsList_0[0];

                    Waypoint newP = new Waypoint();
                    newP.position = Vector3.Lerp(p1.position, p2.position, 0.5f);
                    newP.rotation = Quaternion.identity;
                    newP.UpdatePosition(newP.position, waypointsGroup.XYZConstraint);
                    newP.currentWayPointType = p1.currentWayPointType;

                    additionalList.Add(newP);
                }

                for (int i = 0; i < additionalList.Count; i++)
                {
                    int index = i + i * 1;
                    waypointsGroup.AddWaypoint_0(additionalList[i], index + 1);
                }
            }

            if (waypointsList_1.Count > 0)
            {
                var currList = waypointsList_1.ToList();
                var additionalList = new List<Waypoint>();

                for (int i = 0; i < waypointsList_1.Count - 1; i++)
                {
                    Waypoint p1 = waypointsList_1[i];
                    Waypoint p2 = waypointsList_1[i + 1];

                    Waypoint newP = new Waypoint();
                    newP.position = Vector3.Lerp(p1.position, p2.position, 0.5f);
                    newP.rotation = Quaternion.identity;
                    newP.UpdatePosition(newP.position, waypointsGroup.XYZConstraint);
                    newP.currentWayPointType = p1.currentWayPointType;

                    additionalList.Add(newP);
                }

                if (waypointsList_1.Count > 2)
                {
                    Waypoint p1 = waypointsList_1[waypointsList_1.Count - 1];
                    Waypoint p2 = waypointsList_1[0];

                    Waypoint newP = new Waypoint();
                    newP.position = Vector3.Lerp(p1.position, p2.position, 0.5f);
                    newP.rotation = Quaternion.identity;
                    newP.UpdatePosition(newP.position, waypointsGroup.XYZConstraint);
                    newP.currentWayPointType = p1.currentWayPointType;

                    additionalList.Add(newP);
                }

                for (int i = 0; i < additionalList.Count; i++)
                {
                    int index = i + i * 1;
                    waypointsGroup.AddWaypoint_1(additionalList[i], index + 1);
                }
            }

            if (waypointsList_2.Count > 0)
            {
                var currList = waypointsList_2.ToList();
                var additionalList = new List<Waypoint>();

                for (int i = 0; i < waypointsList_2.Count - 1; i++)
                {
                    Waypoint p1 = waypointsList_2[i];
                    Waypoint p2 = waypointsList_2[i + 1];

                    Waypoint newP = new Waypoint();
                    newP.position = Vector3.Lerp(p1.position, p2.position, 0.5f);
                    newP.rotation = Quaternion.identity;
                    newP.UpdatePosition(newP.position, waypointsGroup.XYZConstraint);
                    newP.currentWayPointType = p1.currentWayPointType;

                    additionalList.Add(newP);
                }

                if (waypointsList_2.Count > 2)
                {
                    Waypoint p1 = waypointsList_2[waypointsList_2.Count - 1];
                    Waypoint p2 = waypointsList_2[0];

                    Waypoint newP = new Waypoint();
                    newP.position = Vector3.Lerp(p1.position, p2.position, 0.5f);
                    newP.rotation = Quaternion.identity;
                    newP.UpdatePosition(newP.position, waypointsGroup.XYZConstraint);
                    newP.currentWayPointType = p1.currentWayPointType;

                    additionalList.Add(newP);
                }

                for (int i = 0; i < additionalList.Count; i++)
                {
                    int index = i + i * 1;
                    waypointsGroup.AddWaypoint_2(additionalList[i], index + 1);
                }
            }

            if (waypointsList_4.Count > 0)
            {
                var currList = waypointsList_4.ToList();
                var additionalList = new List<Waypoint>();

                for (int i = 0; i < waypointsList_4.Count - 1; i++)
                {
                    Waypoint p1 = waypointsList_4[i];
                    Waypoint p2 = waypointsList_4[i + 1];

                    Waypoint newP = new Waypoint();
                    newP.position = Vector3.Lerp(p1.position, p2.position, 0.5f);
                    newP.rotation = Quaternion.identity;
                    newP.UpdatePosition(newP.position, waypointsGroup.XYZConstraint);
                    newP.currentWayPointType = p1.currentWayPointType;

                    additionalList.Add(newP);
                }

                if (waypointsList_4.Count > 2)
                {
                    Waypoint p1 = waypointsList_4[waypointsList_4.Count - 1];
                    Waypoint p2 = waypointsList_4[0];

                    Waypoint newP = new Waypoint();
                    newP.position = Vector3.Lerp(p1.position, p2.position, 0.5f);
                    newP.rotation = Quaternion.identity;
                    newP.UpdatePosition(newP.position, waypointsGroup.XYZConstraint);
                    newP.currentWayPointType = p1.currentWayPointType;

                    additionalList.Add(newP);
                }

                for (int i = 0; i < additionalList.Count; i++)
                {
                    int index = i + i * 1;
                    waypointsGroup.AddWaypoint_4(additionalList[i], index + 1);
                }
            }

            if (waypointsList_5.Count > 0)
            {
                var currList = waypointsList_5.ToList();
                var additionalList = new List<Waypoint>();

                for (int i = 0; i < waypointsList_5.Count - 1; i++)
                {
                    Waypoint p1 = waypointsList_5[i];
                    Waypoint p2 = waypointsList_5[i + 1];

                    Waypoint newP = new Waypoint();
                    newP.position = Vector3.Lerp(p1.position, p2.position, 0.5f);
                    newP.rotation = Quaternion.identity;
                    newP.UpdatePosition(newP.position, waypointsGroup.XYZConstraint);
                    newP.currentWayPointType = p1.currentWayPointType;

                    additionalList.Add(newP);
                }

                if (waypointsList_5.Count > 2)
                {
                    Waypoint p1 = waypointsList_5[waypointsList_5.Count - 1];
                    Waypoint p2 = waypointsList_5[0];

                    Waypoint newP = new Waypoint();
                    newP.position = Vector3.Lerp(p1.position, p2.position, 0.5f);
                    newP.rotation = Quaternion.identity;
                    newP.UpdatePosition(newP.position, waypointsGroup.XYZConstraint);
                    newP.currentWayPointType = p1.currentWayPointType;

                    additionalList.Add(newP);
                }

                for (int i = 0; i < additionalList.Count; i++)
                {
                    int index = i + i * 1;
                    waypointsGroup.AddWaypoint_5(additionalList[i], index + 1);
                }
            }

            if (waypointsList_6.Count > 0)
            {
                var currList = waypointsList_6.ToList();
                var additionalList = new List<Waypoint>();

                for (int i = 0; i < waypointsList_6.Count - 1; i++)
                {
                    Waypoint p1 = waypointsList_6[i];
                    Waypoint p2 = waypointsList_6[i + 1];

                    Waypoint newP = new Waypoint();
                    newP.position = Vector3.Lerp(p1.position, p2.position, 0.5f);
                    newP.rotation = Quaternion.identity;
                    newP.UpdatePosition(newP.position, waypointsGroup.XYZConstraint);
                    newP.currentWayPointType = p1.currentWayPointType;

                    additionalList.Add(newP);
                }

                if (waypointsList_6.Count > 2)
                {
                    Waypoint p1 = waypointsList_6[waypointsList_6.Count - 1];
                    Waypoint p2 = waypointsList_6[0];

                    Waypoint newP = new Waypoint();
                    newP.position = Vector3.Lerp(p1.position, p2.position, 0.5f);
                    newP.rotation = Quaternion.identity;
                    newP.UpdatePosition(newP.position, waypointsGroup.XYZConstraint);
                    newP.currentWayPointType = p1.currentWayPointType;

                    additionalList.Add(newP);
                }

                for (int i = 0; i < additionalList.Count; i++)
                {
                    int index = i + i * 1;
                    waypointsGroup.AddWaypoint_6(additionalList[i], index + 1);
                }
            }
        }

        private void ExpandWayPoints()
        {
            Undo.RecordObject(waypointsGroup, "Waypoint 3 ====>  Waypoint 0 & 1 & 2 & 4 & 5 & 6");

            if (waypointsList_2.Count > 0 || waypointsList_1.Count > 0
                || waypointsList_4.Count > 0 || waypointsList_5.Count > 0
                || waypointsList_0.Count > 0 || waypointsList_6.Count > 0)
            {
                Debug.Log("<color=red>There is a WayPoint List ALREADY. Clear your 0 & 1 & 2 & 4 & 5 & 6 waypoints first!!</color>");
                return;
            }

            if (waypointsList_3.Count <= 2)
            {
                Debug.Log("Too little waypoints!  Add more waypoints!");
                return;
            }

            float _dist = 0f;
            if (float.TryParse(inputDist, out _dist))
            {
                if (_dist <= 0f)
                    _dist = 3.5f;
            }
            else
            {
                Debug.Log("<color=red>_dist float parse Error! using default dist: 3.5f </color>");

                _dist = 3.5f;
            }


            Vector3 centerPoint = Vector3.zero;
            foreach (var i in waypointsList_3)
                centerPoint += i.GetPosition();
            centerPoint /= waypointsList_3.Count;

            waypointsList_2 = new List<Waypoint>();
            waypointsList_1 = new List<Waypoint>();
            waypointsList_0 = new List<Waypoint>();
            waypointsList_4 = new List<Waypoint>();
            waypointsList_5 = new List<Waypoint>();
            waypointsList_6 = new List<Waypoint>();

            for (int i = 0; i < waypointsList_3.Count; i++)
            {
                Vector3 orginalPos = waypointsList_3[i].position;
                Vector3 dir = Vector3.zero;

                //dir = (orginalPos - centerPoint).normalized;

                Vector3 movingDir = Vector3.zero;
                if (i < waypointsList_3.Count - 1)
                {
                    movingDir = (waypointsList_3[i + 1].position - waypointsList_3[i].position).normalized;
                }
                else
                {
                    movingDir = (waypointsList_3[0].position - waypointsList_3[i].position).normalized;
                }

                //dir = UtilityCommon.GetDirectionByAngle_XZ(90, movingDir);
                dir = Quaternion.AngleAxis(90, Vector3.up) * movingDir;

                Vector3 newPos_2 = orginalPos - dir * (_dist * 1);

                var newWayPoint_2 = new Waypoint() { position = newPos_2 };
                newWayPoint_2.UpdatePosition(newPos_2, PositionConstraint.XYZ);

                waypointsGroup.AddWaypoint_2(newWayPoint_2, i);
                waypointsList_2.Add(newWayPoint_2);


                Vector3 newPos_1 = orginalPos - dir * (_dist * 2);
                var newWayPoint_1 = new Waypoint() { position = newPos_1 };
                newWayPoint_1.UpdatePosition(newPos_1, PositionConstraint.XYZ);

                waypointsGroup.AddWaypoint_1(newWayPoint_1, i);
                waypointsList_1.Add(newWayPoint_1);

                Vector3 newPos_0 = orginalPos - dir * (_dist * 3);
                var newWayPoint_0 = new Waypoint() { position = newPos_0 };
                newWayPoint_0.UpdatePosition(newPos_0, PositionConstraint.XYZ);
                newWayPoint_0.currentWayPointType = Waypoint.WayPointType.Blocked;

                waypointsGroup.AddWaypoint_0(newWayPoint_0, i);
                waypointsList_0.Add(newWayPoint_0);

                Vector3 newPos_4 = orginalPos + dir * (_dist * 1);
                var newWayPoint_4 = new Waypoint() { position = newPos_4 };
                newWayPoint_4.UpdatePosition(newPos_4, PositionConstraint.XYZ);

                waypointsGroup.AddWaypoint_4(newWayPoint_4, i);
                waypointsList_4.Add(newWayPoint_4);


                Vector3 newPos_5 = orginalPos + dir * (_dist * 2);
                var newWayPoint_5 = new Waypoint() { position = newPos_5 };
                newWayPoint_5.UpdatePosition(newPos_5, PositionConstraint.XYZ);

                waypointsGroup.AddWaypoint_5(newWayPoint_5, i);
                waypointsList_5.Add(newWayPoint_5);

                Vector3 newPos_6 = orginalPos + dir * (_dist * 3);
                var newWayPoint_6 = new Waypoint() { position = newPos_6 };
                newWayPoint_6.UpdatePosition(newPos_6, PositionConstraint.XYZ);
                newWayPoint_6.currentWayPointType = Waypoint.WayPointType.Blocked;

                waypointsGroup.AddWaypoint_6(newWayPoint_6, i);
                waypointsList_6.Add(newWayPoint_6);
            }
        }

        private void SetIndexNumber()
        {
            if (waypointsList_0 != null)
            {
                for (int i = 0; i < waypointsList_0.Count; i++)
                {
                    waypointsList_0[i].index = i;
                }
            }
            if (waypointsList_1 != null)
            {
                for (int i = 0; i < waypointsList_1.Count; i++)
                {
                    waypointsList_1[i].index = i;
                }
            }
            if (waypointsList_2 != null)
            {
                for (int i = 0; i < waypointsList_2.Count; i++)
                {
                    waypointsList_2[i].index = i;
                }
            }
            if (waypointsList_3 != null)
            {
                for (int i = 0; i < waypointsList_3.Count; i++)
                {
                    waypointsList_3[i].index = i;
                }
            }
            if (waypointsList_4 != null)
            {
                for (int i = 0; i < waypointsList_4.Count; i++)
                {
                    waypointsList_4[i].index = i;
                }
            }
            if (waypointsList_5 != null)
            {
                for (int i = 0; i < waypointsList_5.Count; i++)
                {
                    waypointsList_5[i].index = i;
                }
            }
            if (waypointsList_6 != null)
            {
                for (int i = 0; i < waypointsList_6.Count; i++)
                {
                    waypointsList_6[i].index = i;
                }
            }

            if (waypointsList_3 != null)
            {
                Debug.Log("Index Number Set....!!!! total num: " + waypointsList_3.Count);
            }
            else
            {
                Debug.Log("Error.... No Index Number to set...");
            }
        }

        private void UnifyWaypoints()
        {
            if (waypointsList_3.Count <= 0)
            {
                Debug.Log("Set waypoints 3 first...!");
                return;
            }

            if (waypointsList_3 != null)
            {
                if (waypointsList_3.Find(x => x.index.Equals(-1)) != null)
                {
                    SetIndexNumber();
                }
            }

            Undo.RecordObject(waypointsGroup, "Unify All Waypoint Distance");
            //X,Y 축 사이 거리 통일 및 보정...

            var wp_1 = multipleSelectedWayPoints[0];
            var wp_2 = multipleSelectedWayPoints[1];

            var startIndex = wp_1.index >= wp_2.index ? wp_2.index : wp_1.index;
            var endIndex = wp_1.index < wp_2.index ? wp_2.index : wp_1.index;


            List<List<Waypoint>> lw = new List<List<Waypoint>>();
            if (waypointsList_0.Count > 0)
                lw.Add(waypointsList_0);
            if (waypointsList_1.Count > 0)
                lw.Add(waypointsList_1);
            if (waypointsList_2.Count > 0)
                lw.Add(waypointsList_2);
            if (waypointsList_3.Count > 0)
                lw.Add(waypointsList_3);
            if (waypointsList_4.Count > 0)
                lw.Add(waypointsList_4);
            if (waypointsList_5.Count > 0)
                lw.Add(waypointsList_5);
            if (waypointsList_6.Count > 0)
                lw.Add(waypointsList_6);

            if (endIndex - startIndex > 1)
            {
                int counter = 1;
                for (int i = startIndex + 1; i < endIndex; i++)
                {
                    for (int j = 0; j < lw.Count; j++)
                    {
                        var dist = Vector3.Distance(lw[j][endIndex].GetPosition(), lw[j][startIndex].GetPosition());
                        var distPerWayPoint = dist / (endIndex - startIndex);
                        var direction = (lw[j][endIndex].GetPosition() - lw[j][startIndex].GetPosition()).normalized;

                        var newPos = lw[j][startIndex].GetPosition() + counter * (direction * distPerWayPoint);
                        //var newPos = lw[j][startIndex].position + counter * (direction * distPerWayPoint);

                        Debug.Log("original: " + lw[j][i].GetPosition() + "   new: " + newPos);
                        lw[j][i].SetPosition(newPos, waypointsGroup.XYZConstraint);
                    }
                    counter++;
                }
                SaveScene();

                Debug.Log("All Waypoint Distance is Unified! start index: " + startIndex + "  end index: " + endIndex);
            }
            else
            {
                Debug.Log("No index between your selected waypoints!");
            }

        }


        public bool DrawInScene(Waypoint waypoint, List<Waypoint> listOfMutipleWayPoints)
        {
            if (waypoint == null)
            {
                Debug.Log("NO WP!");
                return false;
            }

            bool doRepaint = false;
            //None serialized field, gets "lost" during serailize updates;
            waypoint.SetWaypointGroup(waypointsGroup);

            if (waypoint == selectedWaypoint)
            {
                Handles.color = Color.green;

                EditorGUI.BeginChangeCheck();
                Vector3 oldpos = waypoint.GetPosition(waypointsGroup.XYZConstraint);
                Vector3 newPos = Handles.PositionHandle(oldpos, waypoint.rotation);

                float handleSize = HandleUtility.GetHandleSize(newPos);

                Handles.SphereHandleCap(-1, newPos, waypoint.rotation, 0.3f * handleSize, EventType.Repaint);
                if (EditorGUI.EndChangeCheck())
                {
                    waypoint.UpdatePosition(newPos - oldpos, waypointsGroup.XYZConstraint);
                }
            }


            Waypoint wpToMove = null;
            bool isMultipleMove = false;
            if (listOfMutipleWayPoints != null && listOfMutipleWayPoints.Count > 0)
            {
                var w = listOfMutipleWayPoints.Find(x => x.Equals(waypoint));
                if (w != null)
                {
                    wpToMove = w;
                    isMultipleMove = true;
                }
            }
            else
            {
                wpToMove = selectedWaypoint;
                isMultipleMove = false;
            }


            if (wpToMove == waypoint)
            {
                Color c = Handles.color;
                if(isMultipleMove)
                    Handles.color = Color.magenta;
                else
                    Handles.color = Color.green;

                //Vector3 newPos = Handles.FreeMoveHandle(waypoint.GetPosition(), waypoint.rotation, 1.0f, Vector3.zero, Handles.SphereHandleCap);
                EditorGUI.BeginChangeCheck();
                Vector3 oldpos = waypoint.GetPosition(waypointsGroup.XYZConstraint);
                Vector3 newPos = Handles.PositionHandle(oldpos, waypoint.rotation);

                float handleSize = HandleUtility.GetHandleSize(newPos);

                Handles.SphereHandleCap(-1, newPos, waypoint.rotation, 0.3f * handleSize, EventType.Repaint);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(waypointsGroup, "Waypoint Moved");

                    if (isMultipleMove == true)
                    {
                        foreach (var i in listOfMutipleWayPoints)
                        {
                            i.UpdatePosition(newPos - oldpos, waypointsGroup.XYZConstraint);
                        }
                    }
                    else
                    {
                        waypoint.UpdatePosition(newPos - oldpos, waypointsGroup.XYZConstraint);
                    }
                }
                
                Handles.color = c;
                
            }
            else
            {
                Color c = Handles.color;

                if (waypoint.currentWayPointType == Waypoint.WayPointType.Blocked)
                    Handles.color = Color.yellow;
                else if (waypoint.currentWayPointType == Waypoint.WayPointType.OutOfBoundary)
                    Handles.color = Color.red;
                else
                    Handles.color = Color.white;


                Vector3 currPos = waypoint.GetPosition(waypointsGroup.XYZConstraint);
                float handleSize = HandleUtility.GetHandleSize(currPos);
                if (Handles.Button(currPos, waypoint.rotation, 0.15f * handleSize, 0.20f * handleSize, Handles.SphereHandleCap))
                {
                    doRepaint = true;
                    selectedWaypoint = waypoint;
                }

                Handles.color = c;
            }
            return doRepaint;
        }

        private void SaveScene()
        {
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            Debug.Log("Saved Scene");
        }

        // Menu item for creating a waypoints group
        [MenuItem(CommonDefine.ProjectName + "/Create WaypointsGroup")]
        public static void CreateRFPathManager()
        {
            GameObject go = new GameObject("Waypoints");
            go.AddComponent<WaypointsGroup>();
            // Select it:
            Selection.activeGameObject = go;


        }


    }
#endif
}
