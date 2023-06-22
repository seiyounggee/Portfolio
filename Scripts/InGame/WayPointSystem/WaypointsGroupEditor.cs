using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.IO;

namespace WayPointSystem
{
#if UNITY_EDITOR
    [CustomEditor(typeof(WaypointsGroup))]
    public class WaypointsGroupEditor : Editor
    {
        WaypointsGroup waypointsGroup;
        [SerializeField] List<Waypoint> waypointsList_0;
        [SerializeField] List<Waypoint> waypointsList_1;
        [SerializeField] List<Waypoint> waypointsList_2;
        [SerializeField] List<Waypoint> waypointsList_3;
        [SerializeField] List<Waypoint> waypointsList_4;
        [SerializeField] List<Waypoint> waypointsList_5;
        [SerializeField] List<Waypoint> waypointsList_6;

        public List<List<Waypoint>> allWaypoints
        {
            get
            {
                var all = new List<List<Waypoint>>();
                if (waypointsList_0 != null) all.Add(waypointsList_0);
                if (waypointsList_1 != null) all.Add(waypointsList_1);
                if (waypointsList_2 != null) all.Add(waypointsList_2);
                if (waypointsList_3 != null) all.Add(waypointsList_3);
                if (waypointsList_4 != null) all.Add(waypointsList_4);
                if (waypointsList_5 != null) all.Add(waypointsList_5);
                if (waypointsList_6 != null) all.Add(waypointsList_6);

                return all;
            }
        }


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

        public string inputDist = "4.2";


        Vector3? centerAxis = null;
        Vector3 movableCenter;

        public string input_addwaypointCount_Curved;
        public string input_addwaypointCount_Straight;
        public int addWaypointCount_Straight = 0;
        public int addWaypointCount_Curved = 0;
        public bool autoWaypointCount_Straight = true;
        public bool autoWaypointCount_Curved = true;
        public string input_addWaypointDegree;
        public float addWaypointDegree = 45;
        public string input_addWaypointDist;
        private float addWaypointLength_Curved = 75f;
        private float addWaypointLength_Straight = 75f;

        private string input_addWaypointLength_Curved = "";
        private float input_addWaypointLength_scrollbar_Curved;
        private string input_addWaypointLength_Straight = "";
        private float input_addWaypointLength_scrollbar_Straight;
        private string input_addWaypointDegree_Curved = "";
        private float input_addWaypointDegree_scrollbar_Curved;

        private bool showOnSceneGUI = false;

        string input_saveFileName = "";
        string input_loadFileName = "";

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
            /*
            if (Application.isPlaying)
                return;
            */

            if (waypointsGroup != null)
            {
                waypointsGroup.transform.position = Vector3.zero;
            }

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

            DrawCenterAxis();

            Event e = Event.current;
            switch (e.type)
            {
                case EventType.KeyDown:
                    {
                        if (Event.current.keyCode == (KeyCode.C))
                        {
                            multipleSelectedWayPoints.Clear();
                            AddWayPoint();
                            SetIndexNumber();
                        }

                        if (Event.current.keyCode == (KeyCode.Backspace))
                        {
                            if (selectedWaypoint != null)
                            {
                                multipleSelectedWayPoints.Clear();
                                int deleteIndex = -1;
                                deleteIndex = selectedWaypoint.index;
                                selectedWaypoint = null;
                                if (deleteIndex > -1)
                                    DeleteIndex(deleteIndex);
                                SetIndexNumber();
                            }
                        }

                        if (Event.current.keyCode == (KeyCode.X))
                        {
                            if (multipleSelectedWayPoints.Contains(selectedWaypoint) == false)
                                multipleSelectedWayPoints.Add(selectedWaypoint);
                        }
                         
                        if(Event.current.keyCode == (KeyCode.Q))                        
                        {
                            ChangeSelectedWaypointType();
                        }

                        if (Event.current.keyCode == (KeyCode.Escape))
                        {
                            if (multipleSelectedWayPoints != null && multipleSelectedWayPoints.Count > 0)
                                multipleSelectedWayPoints.Clear();

                            if (selectedWaypoint != null)
                                selectedWaypoint = null;
                        }

                        if (Event.current.keyCode == KeyCode.I)
                        {
                            if (selectedWaypoint != null && multipleSelectedWayPoints.Count <= 0)
                            {
                                SetIndexNumber();
                                CreateChargePad();
                            }
                        }

                        if (Event.current.keyCode == KeyCode.O)
                        {
                            if (selectedWaypoint != null && multipleSelectedWayPoints.Count <= 0)
                            {
                                SetIndexNumber();
                                CreateTimingPad();
                            }
                        }

                        if (Event.current.keyCode == KeyCode.P)
                        {
                            if (selectedWaypoint != null && multipleSelectedWayPoints.Count <= 0)
                            {
                                SetIndexNumber();
                                CreateObstacle();
                            }
                        }
                    }
                    break;

                case EventType.MouseDown:
                    {

                    }
                    break;

                case EventType.KeyUp:
                    {

                    }
                    break;
            }


            Handles.BeginGUI();

            GUIStyle SectionNameStyle_0 = new GUIStyle();
            SectionNameStyle_0.fontSize = 10;
            SectionNameStyle_0.wordWrap = true;
            SectionNameStyle_0.fontStyle = FontStyle.Bold;
            SectionNameStyle_0.normal.textColor = Color.white;
            SectionNameStyle_0.alignment = TextAnchor.LowerRight;

            EditorGUILayout.LabelField("[HOTKEYS]", SectionNameStyle_0);
            SectionNameStyle_0.normal.textColor = Color.white;
            EditorGUILayout.LabelField("Create Single Waypoint  >>>>>   Press C to Create", SectionNameStyle_0);
            EditorGUILayout.LabelField("Delete Waypoint  >>>>>   Press Backspace to Delete", SectionNameStyle_0);
            EditorGUILayout.LabelField("Change Waypoint Type  >>>>>   Press Q to Change", SectionNameStyle_0);
            EditorGUILayout.LabelField("Select Multiple Waypoints  >>>>>   Press X to Select", SectionNameStyle_0);
            EditorGUILayout.LabelField("Clear Selected Waypoints  >>>>>   Press ESC to Clear", SectionNameStyle_0);
            EditorGUILayout.LabelField("Add Charge Pad  >>>>>   Press I to Add", SectionNameStyle_0);
            EditorGUILayout.LabelField("Add Timing Pad  >>>>>   Press O to Add", SectionNameStyle_0);
            EditorGUILayout.LabelField("Add Obstacle >>>>>   Press P to Add", SectionNameStyle_0);
            EditorGUILayout.LabelField("-------------------------------------------", SectionNameStyle_0);

            if (showOnSceneGUI)
            {
                if (selectedWaypoint != null && multipleSelectedWayPoints.Count <= 0)
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
                    else if (selectedWaypoint.currentWayPointType == Waypoint.WayPointType.Sky)
                        SectionNameStyle.normal.textColor = Color.blue;
                    else if (selectedWaypoint.currentWayPointType == Waypoint.WayPointType.OnlyFront)
                        SectionNameStyle.normal.textColor = Color.gray;

                    string info = "Waypoint: index- " + selectedWaypoint.index + " | Vector3 " + selectedWaypoint.GetPosition() + " | " + selectedWaypoint.currentWayPointType.ToString();
                    GUILayout.Box(info, SectionNameStyle);

                    GUILayoutOption[] btnOptions0 = new[] {
                    GUILayout.Height (40),
                    };

                    if (GUILayout.Button("↔\nChange Waypoint Type", btnOptions0))
                    {
                        ChangeSelectedWaypointType();
                    }

                    GUILayout.BeginHorizontal();
                    GUILayoutOption[] btnOptions = new[] {
                    GUILayout.Height (110),
                    GUILayout.Width (140),
                };
                    if (GUILayout.Button("●\n●\n●\n●\n●\nVertical Aline", btnOptions))
                    {
                        AlineWayPointsVertical();
                    }
                    if (GUILayout.Button("●●●●●\nHorizontal Aline", btnOptions))
                    {
                        AlineWayPointsHorizontal();
                    }
                    if (GUILayout.Button("|\n|\n● ↔ ●\n|\n|\nBilateral Symmertry\n(Left basis)", btnOptions))
                    {
                        AlineWayPointsSymmetry();
                    }

                    if (GUILayout.Button("●↔●↔●↔●↔●\nSpacing Unify", btnOptions))
                    {
                        SetIndexNumber();
                        AlineWayPointsIntervals();
                        SaveScene();
                        SceneView.RepaintAll();
                    }
                    GUILayout.EndHorizontal();


                    GUILayout.BeginHorizontal();
                    GUILayout.Box("Set Invertal Distance: ");
                    inputDist = GUILayout.TextField(inputDist, 35);
                    GUILayout.EndHorizontal();

                    if (GUILayout.Button("Delete"))
                    {
                        int deleteIndex = -1;
                        deleteIndex = selectedWaypoint.index;
                        selectedWaypoint = null;

                        if (deleteIndex > -1)
                        {
                            DeleteIndex(deleteIndex);
                        }
                    }

                    GUILayout.EndArea();
                }


                if (multipleSelectedWayPoints != null && multipleSelectedWayPoints.Count >= 2)
                {
                    GUILayout.BeginArea(new Rect(100, 150, Screen.width - 800, Screen.height - 200));
                    GUIStyle SectionNameStyle = new GUIStyle();
                    SectionNameStyle.fontSize = 15;
                    SectionNameStyle.wordWrap = true;
                    SectionNameStyle.fontStyle = FontStyle.Bold;
                    SectionNameStyle.normal.background = EditorGUIUtility.whiteTexture;
                    string info = "Mutiple Waypoints: " + multipleSelectedWayPoints[0].GetPosition() + "  |  " + multipleSelectedWayPoints[1].GetPosition();
                    GUILayout.Box(info, SectionNameStyle);

                    if (GUILayout.Button("Change All Waypoint Type To Normal"))
                    {
                        UnifyWayPointType(Waypoint.WayPointType.Normal);
                    }
                    if (GUILayout.Button("Change All Waypoint Type To Blocked"))
                    {
                        UnifyWayPointType(Waypoint.WayPointType.Blocked);
                    }
                    if (GUILayout.Button("Change All Waypoint Type To OutOfBoundary"))
                    {
                        UnifyWayPointType(Waypoint.WayPointType.OutOfBoundary);
                    }
                    if (GUILayout.Button("Change All Waypoint Type To OnlyFront"))
                    {
                        UnifyWayPointType(Waypoint.WayPointType.OnlyFront);
                    }
                    if (GUILayout.Button("Change All Waypoint Type To Sky"))
                    {
                        UnifyWayPointType(Waypoint.WayPointType.Sky);
                    }

                    if (multipleSelectedWayPoints != null && multipleSelectedWayPoints.Count == 2)
                    {
                        GUILayout.Space(10);

                        if (GUILayout.Button("Unify All Waypoint Distance"))
                        {
                            UnifyWaypointsLength();
                        }

                        if (GUILayout.Button("Create Curved Waypoint"))
                        {
                            CreateCurvedWaypoints();
                        }

                    }

                    GUILayout.EndArea();
                }


                if (selectedWaypoint != null && multipleSelectedWayPoints.Count <= 0)
                {
                    GUILayout.BeginArea(new Rect(100, 250, Screen.width - 800, Screen.height - 200));

                    GUILayout.BeginHorizontal();
                    GUILayoutOption[] btnOptions2 = new[] {
                    GUILayout.Height (80),
                    GUILayout.Width (80),
                };
                    if (GUILayout.Button("Add\nCharge\nPad", btnOptions2))
                    {
                        SetIndexNumber();
                        CreateChargePad();
                    }

                    if (GUILayout.Button("Add\nTiming Pad", btnOptions2))
                    {
                        SetIndexNumber();
                        CreateTimingPad();
                    }

                    if (GUILayout.Button("Add\nObstacle", btnOptions2))
                    {
                        SetIndexNumber();
                        CreateObstacle();
                    }


                    GUILayout.EndHorizontal();


                    GUILayout.EndArea();
                }
            }
            else
            {
                showOnSceneGUI = GUILayout.Toggle(showOnSceneGUI, "Show Scene UI");
            }


            Handles.EndGUI();
        }

        override public void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            /*
            if (Application.isPlaying)
                return;
            */

            if (waypointsGroup != null)
            {
                waypointsGroup.transform.position = Vector3.zero;
            }

            bool dorepaint = false;

            showOnSceneGUI = GUILayout.Toggle(showOnSceneGUI, "Show Scene UI");
            EditorGUILayout.Space(20);

            GUIStyle SectionNameStyle_0 = new GUIStyle();
            SectionNameStyle_0.fontSize = 15;
            SectionNameStyle_0.wordWrap = true;
            SectionNameStyle_0.fontStyle = FontStyle.Bold;
            SectionNameStyle_0.normal.textColor = Color.white;

            EditorGUILayout.LabelField("[HOTKEYS]", SectionNameStyle_0);
            EditorGUILayout.LabelField("Create Single Waypoint  >>>>>   Press C to Create");
            EditorGUILayout.LabelField("Delete Waypoint  >>>>>   Press Backspace to Delete");
            EditorGUILayout.LabelField("Change Waypoint Type  >>>>>   Press Q to Change");
            EditorGUILayout.LabelField("Select Multiple Waypoints  >>>>>   Press X to Select");
            EditorGUILayout.LabelField("Clear Multiple Waypoints  >>>>>   Press ESC to Clear");
            EditorGUILayout.LabelField("Add Charge Pad  >>>>>   Press I to Add");
            EditorGUILayout.LabelField("Add Timing Pad  >>>>>   Press O to Add");
            EditorGUILayout.LabelField("Add Obstacle >>>>>   Press P to Add");
            EditorGUILayout.LabelField("-------------------------------------------", SectionNameStyle_0);

            EditorGUILayout.LabelField("[INFO]", SectionNameStyle_0);
            EditorGUILayout.LabelField("Current Waypoint Count: " + GetWaypointCount());
            EditorGUILayout.LabelField("Current Waypoint Total Length(Approximately): " + (int)GetWaypointTotalLength());
            EditorGUILayout.LabelField("-------------------------------------------", SectionNameStyle_0);

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            GUIStyle SectionNameStyle_1 = new GUIStyle();
            SectionNameStyle_1.fontSize = 13;
            SectionNameStyle_1.wordWrap = true;
            SectionNameStyle_1.fontStyle = FontStyle.Bold;
            SectionNameStyle_1.normal.textColor = Color.cyan;

            if (selectedWaypoint != null)
            {
                bool isValid = true;
                if (multipleSelectedWayPoints != null && multipleSelectedWayPoints.Count >= 2)
                    isValid = false;


                if (isValid)
                {
                    EditorGUILayout.LabelField("[SINGLE SELECTED WAYPOINT] | index: " + selectedWaypoint.index + " | type: " + selectedWaypoint.currentWayPointType, SectionNameStyle_1);

                    GUILayout.BeginHorizontal();

                    EditorGUI.BeginChangeCheck();
                    Vector3 oldV = selectedWaypoint.GetPosition(waypointsGroup.XYZConstraint);
                    Vector3 newV = EditorGUILayout.Vector3Field("", oldV);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(waypointsGroup, "Waypoint Moved");
                        selectedWaypoint.UpdatePosition(newV - oldV, waypointsGroup.XYZConstraint);
                    }

                    GUILayout.EndHorizontal();

                    GUILayoutOption[] btnOptions0 = new[] {
                    GUILayout.Height (40),
                    };

                    if (GUILayout.Button("↔\nChange Waypoint Type", btnOptions0))
                    {
                        ChangeSelectedWaypointType();
                    }

                    GUILayout.BeginHorizontal();
                    GUILayoutOption[] btnOptions = new[] {
                    GUILayout.Height (110),
                    GUILayout.MinWidth (50),
                    GUILayout.MaxWidth (140),
                    };
                    if (GUILayout.Button("●\n●\n●\n●\n●\nVertical Aline", btnOptions))
                    {
                        SetIndexNumber();
                        AlineWayPointsVertical();
                        SaveScene();
                        SceneView.RepaintAll();
                    }
                    if (GUILayout.Button("●●●●●\nHorizontal Aline", btnOptions))
                    {
                        SetIndexNumber();
                        AlineWayPointsHorizontal();
                        SaveScene();
                        SceneView.RepaintAll();
                    }
                    if (GUILayout.Button("|\n|\n● ↔ ●\n|\n|\nBilateral Symmertry\n(Left basis)", btnOptions))
                    {
                        SetIndexNumber();
                        AlineWayPointsSymmetry();
                        SaveScene();
                        SceneView.RepaintAll();
                    }

                    if (GUILayout.Button("●↔●↔●↔●↔●\nSpacing Unify", btnOptions))
                    {
                        SetIndexNumber();
                        AlineWayPointsIntervals();
                        SaveScene();
                        SceneView.RepaintAll();
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Set Invertal Dist: ");
                    inputDist = GUILayout.TextField(inputDist, 35);
                    GUILayout.EndHorizontal();


                    GUILayout.BeginHorizontal();
                    GUILayoutOption[] btnOptions2 = new[] {
                        GUILayout.Height (50),
                        GUILayout.MinWidth (50),
                        GUILayout.MaxWidth (100),
                    };
                    if (GUILayout.Button("Add\nCharge Pad", btnOptions2))
                    {
                        SetIndexNumber();
                        CreateChargePad();
                    }

                    if (GUILayout.Button("Add\nTiming Pad", btnOptions2))
                    {
                        SetIndexNumber();
                        CreateTimingPad();
                    }

                    if (GUILayout.Button("Add\nObstacle", btnOptions2))
                    {
                        SetIndexNumber();
                        CreateObstacle();
                    }

                    GUILayout.EndHorizontal();
                }

                EditorGUILayout.Space(20);


                EditorGUILayout.LabelField("[Add Waypoints]");

                GUILayoutOption[] scrollBarOption = new[] {
                GUILayout.MinWidth (150),
                GUILayout.MaxWidth (300),
                GUILayout.ExpandWidth(true)
                };

                GUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField("Add Waypoint Length (Straight) : " + addWaypointLength_Straight);
                GUILayout.Label("Input: ");
                input_addWaypointLength_Straight = GUILayout.TextField(input_addWaypointLength_Straight);
                if (GetInput(input_addWaypointLength_Straight) > 1f)
                    addWaypointLength_Straight = Mathf.Clamp(GetInput(input_addWaypointLength_Straight), 1f, 200f);

                if (EditorGUI.EndChangeCheck())
                {
                    if (GetInput(input_addWaypointLength_Straight) > 1f)
                        addWaypointLength_Straight = (int)Mathf.Clamp(GetInput(input_addWaypointLength_Straight), 1f, 200f);

                    input_addWaypointLength_scrollbar_Straight = addWaypointLength_Straight;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                input_addWaypointLength_scrollbar_Straight = (int)GUILayout.HorizontalScrollbar(input_addWaypointLength_scrollbar_Straight, 1f, 1f, 201f, scrollBarOption);
                if (EditorGUI.EndChangeCheck())
                {
                    input_addWaypointLength_Straight = input_addWaypointLength_scrollbar_Straight.ToString();
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Add Waypoint Count (Straight) : " + addWaypointCount_Straight);
                if (autoWaypointCount_Straight)
                {
                    autoWaypointCount_Straight = GUILayout.Toggle(autoWaypointCount_Straight, "Auto");
                    addWaypointCount_Straight = (int)(addWaypointLength_Straight / 1.65f);

                    if (addWaypointCount_Straight <= 0)
                        addWaypointCount_Straight = 1;
                }
                else
                {
                    GUILayout.Label("Input: ");
                    input_addwaypointCount_Straight = GUILayout.TextField(input_addwaypointCount_Straight);
                    addWaypointCount_Straight = (int)GetInput(input_addwaypointCount_Straight);
                    if (addWaypointCount_Straight <= 0)
                        addWaypointCount_Straight = 1;
                    autoWaypointCount_Straight = GUILayout.Toggle(autoWaypointCount_Straight, "Auto");
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayoutOption[] btnOptions3 = new[] {
                GUILayout.Height (80),
                GUILayout.MinWidth (40),
                GUILayout.MaxWidth (80),
                };
                Texture texture = (Texture)AssetDatabase.LoadAssetAtPath("Assets/3.Art/UI/EditorUI/UI_Icon_Point.png", typeof(Texture));
                if (GUILayout.Button(texture, btnOptions3))
                {
                    multipleSelectedWayPoints.Clear();
                    AddWayPoint();
                    SetIndexNumber();
                    dorepaint = true;

                    SaveScene();
                }
                Texture texture2 = (Texture)AssetDatabase.LoadAssetAtPath("Assets/3.Art/UI/EditorUI/UI_Icon_ArrowUp_Single.png", typeof(Texture));
                if (GUILayout.Button(texture2, btnOptions3))
                {
                    multipleSelectedWayPoints.Clear();
                    AddMultipleWaypoints_Straight(addWaypointCount_Straight, addWaypointLength_Straight, 0f);
                    SetIndexNumber();
                    dorepaint = true;

                    SaveScene();
                }
                GUILayout.EndHorizontal();

                EditorGUILayout.Space(20);
                GUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField("Add Waypoint Length (Curved) : " + addWaypointLength_Curved);
                GUILayout.Label("Input: ");
                input_addWaypointLength_Curved = GUILayout.TextField(input_addWaypointLength_Curved);
                if (GetInput(input_addWaypointLength_Curved) > 1f)
                    addWaypointLength_Curved = Mathf.Clamp(GetInput(input_addWaypointLength_Curved), 1f, 200f);

                if (EditorGUI.EndChangeCheck())
                {
                    if (GetInput(input_addWaypointLength_Curved) > 1f)
                        addWaypointLength_Curved = (int)Mathf.Clamp(GetInput(input_addWaypointLength_Curved), 1f, 200f);

                    input_addWaypointLength_scrollbar_Curved = addWaypointLength_Curved;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                input_addWaypointLength_scrollbar_Curved = (int)GUILayout.HorizontalScrollbar(input_addWaypointLength_scrollbar_Curved, 1f, 1f, 201f, scrollBarOption);
                if (EditorGUI.EndChangeCheck())
                {
                    input_addWaypointLength_Curved = input_addWaypointLength_scrollbar_Curved.ToString();
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Add Waypoint Count (Curved) : " + addWaypointCount_Curved);
                if (autoWaypointCount_Curved)
                {
                    autoWaypointCount_Curved = GUILayout.Toggle(autoWaypointCount_Curved, "Auto");

                    if (addWaypointDegree < 100)
                        addWaypointCount_Curved = (int)((addWaypointLength_Curved / 1.3f) * (5f - (5f * (100 - addWaypointDegree) / 100)));
                    else
                        addWaypointCount_Curved = (int)(addWaypointLength_Curved / 1.3f);

                    if (addWaypointCount_Curved <= 0)
                        addWaypointCount_Curved = 1;
                }
                else
                {
                    GUILayout.Label("Input: ");
                    input_addwaypointCount_Curved = GUILayout.TextField(input_addwaypointCount_Curved);
                    addWaypointCount_Curved = (int)GetInput(input_addwaypointCount_Curved);
                    if (addWaypointCount_Curved <= 0)
                        addWaypointCount_Curved = 1;
                    autoWaypointCount_Curved = GUILayout.Toggle(autoWaypointCount_Curved, "Auto");
                }

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField("Add Waypoint Degree (Curved) : " + addWaypointDegree);
                GUILayout.Label("Input: ");
                input_addWaypointDegree_Curved = GUILayout.TextField(input_addWaypointDegree_Curved);

                if (EditorGUI.EndChangeCheck())
                {
                    if (GetInput(input_addWaypointDegree_Curved) > 1f)
                        addWaypointDegree = (int)Mathf.Clamp(GetInput(input_addWaypointDegree_Curved), 1f, 200f);

                    input_addWaypointDegree_scrollbar_Curved = addWaypointDegree;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                input_addWaypointDegree_scrollbar_Curved = (int)GUILayout.HorizontalScrollbar(input_addWaypointDegree_scrollbar_Curved, 1f, 0f, 91f, scrollBarOption);
                if (EditorGUI.EndChangeCheck())
                {
                    input_addWaypointDegree_Curved = input_addWaypointDegree_scrollbar_Curved.ToString();
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(10);

                /*
                Texture texture3 = (Texture)AssetDatabase.LoadAssetAtPath("Assets/3.Art/UI/EditorUI/UI_Icon_ArrowFront_Left.png", typeof(Texture));
                if (GUILayout.Button(texture3, btnOptions3))
                {
                    multipleSelectedWayPoints.Clear();
                    AddMultipleWaypoints_Straight(addWaypointCount_Straight, addWaypointLength_Straight, -addWaypointDegree);
                    SetIndexNumber();
                    dorepaint = true;

                    SaveScene();
                }
                Texture texture4 = (Texture)AssetDatabase.LoadAssetAtPath("Assets/3.Art/UI/EditorUI/UI_Icon_ArrowFront_Right.png", typeof(Texture));
                if (GUILayout.Button(texture4, btnOptions3))
                {
                    multipleSelectedWayPoints.Clear();
                    AddMultipleWaypoints_Straight(addWaypointCount_Straight, addWaypointLength_Straight, addWaypointDegree);
                    SetIndexNumber();
                    dorepaint = true;

                    SaveScene();
                }
                */

                GUILayout.BeginHorizontal();
                Texture texture5 = (Texture)AssetDatabase.LoadAssetAtPath("Assets/3.Art/UI/EditorUI/UI_Icon_TurnLeft.png", typeof(Texture));
                if (GUILayout.Button(texture5, btnOptions3))
                {
                    multipleSelectedWayPoints.Clear();
                    AddMultipleWaypoints_Curved(addWaypointCount_Curved, addWaypointLength_Curved, -1f * addWaypointDegree * 2f);
                    SetIndexNumber();
                    dorepaint = true;

                    SaveScene();
                }
                Texture texture6 = (Texture)AssetDatabase.LoadAssetAtPath("Assets/3.Art/UI/EditorUI/UI_Icon_TurnRight.png", typeof(Texture));
                if (GUILayout.Button(texture6, btnOptions3))
                {
                    multipleSelectedWayPoints.Clear();
                    AddMultipleWaypoints_Curved(addWaypointCount_Curved, addWaypointLength_Curved, 1f * addWaypointDegree * 2f);
                    SetIndexNumber();
                    dorepaint = true;

                    SaveScene();
                }

                GUILayout.EndHorizontal();


                EditorGUILayout.Space();
            }

            if (waypointsList_3 != null && waypointsList_3.Count > 0)
            {
                if (GUILayout.Button("Remove Half (1/2)"))
                {
                    RemoveHalfWaypoint();
                    SetIndexNumber();
                    dorepaint = true;
                    SaveScene();
                }

                //TODO...
                /*
                if (GUILayout.Button("Slerp Waypoints (x2)"))
                {
                    SlerpWayPoints();
                    SetIndexNumber();
                    dorepaint = true;

                    SaveScene();
                }
                */

                if (GUILayout.Button("Lerp Waypoints (x2)"))
                {
                    LerpWayPoints();
                    SetIndexNumber();
                    dorepaint = true;

                    SaveScene();
                }

                /*
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Aline Waypoint"))
                {
                    SetIndexNumber();
                    AlineWayPointsIntervals();
                    dorepaint = true;

                    SaveScene();
                }
                inputDist = GUILayout.TextField(inputDist, 35);
                GUILayout.EndHorizontal();
                */

                GUILayout.BeginHorizontal();

                if (waypointsList_0 != null && waypointsList_0.Count <= 0)
                {
                    if (GUILayout.Button("Waypoint 3 ====>  Waypoint 0 & 1 & 2 & 4 & 5 & 6"))
                    {
                        ExpandWayPoints();
                        SetIndexNumber();
                        dorepaint = true;

                        SaveScene();
                    }
                    inputDist = GUILayout.TextField(inputDist, 35);
                }
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

                    GameObject ground = GameObject.FindGameObjectWithTag(CommonDefine.TAG_ROAD_Normal);
                    MeshCollider coll = null;
                    MeshFilter meshfilter = null;

                    if (ground != null)
                        coll = ground.GetComponent<MeshCollider>();
                    if (ground != null)
                        meshfilter = ground.GetComponent<MeshFilter>();

                    foreach (var j in allWayPoints)
                    {
                        foreach (var i in j)
                        {
                            /*
                             //가장 가까운 colider 방식
                            if (coll != null)
                            {
                                Vector3 closestPoint = coll.ClosestPoint(i.GetPosition());

                                var newPos = new Vector3(i.GetPosition().x, closestPoint.y, i.GetPosition().z);
                                i.SetPosition(newPos, PositionConstraint.XYZ);
                                Debug.Log("index " + i.index + "  height is set");
                            }
                            */


                            /*
                             //가장 가까운 vertex 방식
                            if (meshfilter != null)
                            {
                                float minDistanceSqr = Mathf.Infinity;
                                Vector3 nearestVertex = Vector3.zero;

                                foreach (Vector3 vertex in meshfilter.mesh.vertices)
                                {
                                    Vector3 diff = i.GetPosition() - vertex;
                                    float distSqr = diff.sqrMagnitude;
                                    if (distSqr <= minDistanceSqr)
                                    {
                                        minDistanceSqr = distSqr;
                                        nearestVertex = vertex;
                                    }
                                }

                                var newPos = new Vector3(i.GetPosition().x, nearestVertex.y, i.GetPosition().z);
                                i.SetPosition(nearestVertex, PositionConstraint.XYZ);
                                // convert nearest vertex back to world space
                            }
                            */


                            //수직 ray 방식              
                            bool isHit = false;
                            Vector3 hitPos = Vector3.zero;

                            float dist = 10f;
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
                                Debug.Log("index " + i.index + "  height is set");
                            }
                            else
                            {
                                //없으면 위쪽에서 한번더!!

                                rayStartPoint = new Vector3(rayStartPoint.x, 3000f, rayStartPoint.z);
                                Ray ray_2 = new Ray(rayStartPoint, Vector3.down);
                                if (Physics.Raycast(ray_2, out hit, 3100f, layerMask))
                                {
                                    isHit = true;
                                    hitPos = hit.point;
                                }

                                if (isHit)
                                {
                                    var newPos = new Vector3(i.GetPosition().x, hitPos.y, i.GetPosition().z);
                                    i.SetPosition(newPos, PositionConstraint.XYZ);
                                    Debug.Log("index " + i.index + "  height is set");
                                }
                            }
                        }
                    }
                    dorepaint = true;

                    SaveScene();
                }

                if (GUILayout.Button("Set All Waypoints Y(Height) to 0"))
                {
                    SetWaypointsYtoZero();
                }

                    
                if (GUILayout.Button("Set Index Number"))
                {
                    SetIndexNumber();
                }

                if (GUILayout.Button("Reverse All Waypoints"))
                {
                    ReverseAllWaypoints();
                }

                if (selectedWaypoint != null )
                {
                    EditorGUILayout.Space(20);
                    if (GUILayout.Button("Delete Waypoint"))
                    {
                        int deleteIndex = -1;
                        deleteIndex = selectedWaypoint.index;
                        selectedWaypoint = null;

                        if (deleteIndex > -1)
                        {
                            DeleteIndex(deleteIndex);
                        }
                    }
                }

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
                    selectedWaypoint = null;
                    multipleSelectedWayPoints.Clear();

                    SaveScene();
                }

                GUILayout.Space(20);

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Save Waypoint to Txt file"))
                {
                    SaveWaypointToJsonString(input_saveFileName);
                }
                input_saveFileName = GUILayout.TextField(input_saveFileName);
                if (string.IsNullOrEmpty(input_saveFileName))
                {
                    input_saveFileName = "Waypoint_Data_" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                }

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Load Waypoint Txt file"))
                {
                    var data = ReadJsonData(input_loadFileName);
                    if (data != null)
                    {
                        //성공한 경우
                        waypointsList_0 = data.waypointsList_0;
                        waypointsList_1 = data.waypointsList_1;
                        waypointsList_2 = data.waypointsList_2;
                        waypointsList_3 = data.waypointsList_3;
                        waypointsList_4 = data.waypointsList_4;
                        waypointsList_5 = data.waypointsList_5;
                        waypointsList_6 = data.waypointsList_6;
                        waypointsGroup.waypoints_0 = waypointsList_0;
                        waypointsGroup.waypoints_1 = waypointsList_1;
                        waypointsGroup.waypoints_2 = waypointsList_2;
                        waypointsGroup.waypoints_3 = waypointsList_3;
                        waypointsGroup.waypoints_4 = waypointsList_4;
                        waypointsGroup.waypoints_5 = waypointsList_5;
                        waypointsGroup.waypoints_6 = waypointsList_6;

                        if (waypointsList_0 != null && waypointsList_0.Count > 0)
                            selectedWaypoint = waypointsList_0[0];

                        Debug.Log("Waypoint Successfully Loaded!");
                    }
                    else
                    {
                        //실패할경우...
                        Debug.Log("<color=red>Faild to load</color>");
                    }

                    SetIndexNumber();
                    dorepaint = true;
                    SaveScene();
                }
                input_loadFileName = GUILayout.TextField(input_loadFileName);
                GUILayout.EndHorizontal();
            }


            if (multipleSelectedWayPoints != null && multipleSelectedWayPoints.Count > 0)
            {
                EditorGUILayout.LabelField("[SELECTED MULTIPLE WAYPOINTS]       count: " + multipleSelectedWayPoints.Count, SectionNameStyle_1);

                for (int i = 0; i < multipleSelectedWayPoints.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("index: " + multipleSelectedWayPoints[i].index + "   |   " + multipleSelectedWayPoints[i].GetPosition().ToString());
                    EditorGUILayout.EndHorizontal();
                }

                if (GUILayout.Button("Clear Group Selection"))
                {
                    multipleSelectedWayPoints.Clear();
                    dorepaint = true;

                    SaveScene();
                }
                GUILayout.Space(20);
            }
            else
            {
            }

            if (multipleSelectedWayPoints != null && multipleSelectedWayPoints.Count == 2)
            {
                if (GUILayout.Button("Unify All Waypoint Type To Normal"))
                {
                    UnifyWayPointType(Waypoint.WayPointType.Normal);
                    dorepaint = true;
                }
                if (GUILayout.Button("Unify All Waypoint Type To Blocked"))
                {
                    UnifyWayPointType(Waypoint.WayPointType.Blocked);
                    dorepaint = true;
                }
                if (GUILayout.Button("Unify All Waypoint Type To OutOfBoundary"))
                {
                    UnifyWayPointType(Waypoint.WayPointType.OutOfBoundary);
                    dorepaint = true;
                }
                if (GUILayout.Button("Unify All Waypoint Type To OnlyFront"))
                {
                    UnifyWayPointType(Waypoint.WayPointType.OnlyFront);
                    dorepaint = true;
                }
                if (GUILayout.Button("Unify All Waypoint Type To Sky"))
                {
                    UnifyWayPointType(Waypoint.WayPointType.Sky);
                    dorepaint = true;
                }
                GUILayout.Space(20);

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Unify All Waypoint Distance"))
                {
                    UnifyWaypointsLength();
                    dorepaint = true;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Create Curved Waypoint"))
                {
                    CreateCurvedWaypoints();
                    dorepaint = true;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Delete Range Waypoint"))
                {
                    RemoveRangeWaypoint();
                    dorepaint = true;
                }
                GUILayout.EndHorizontal();
            }


            EditorGUILayout.Space();
            EditorGUILayout.Space();



            EditorGUILayout.BeginVertical();
            //GUI.skin.label.fontSize = 50;
            GUIStyle SectionNameStyle_2 = new GUIStyle();
            SectionNameStyle_2.fontSize = 15;
            SectionNameStyle_2.wordWrap = true;
            SectionNameStyle_2.fontStyle = FontStyle.Bold;
            SectionNameStyle_2.normal.textColor = Color.white;

            EditorGUILayout.LabelField("-------------------------------------------", SectionNameStyle_2);


            EditorGUILayout.LabelField("[Waypoints 0 - Outside]       count: " + waypointsList_0.Count, SectionNameStyle_2);
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
                        else if (cwp.currentWayPointType == Waypoint.WayPointType.OnlyFront)
                            GUI.color = Color.gray;
                        else if (cwp.currentWayPointType == Waypoint.WayPointType.Sky)
                            GUI.color = Color.blue;
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
                            else if (curr == Waypoint.WayPointType.Sky)
                                cwp.currentWayPointType = Waypoint.WayPointType.Sky;
                            else if (curr == Waypoint.WayPointType.OnlyFront)
                                cwp.currentWayPointType = Waypoint.WayPointType.OnlyFront;

                            dorepaint = true;
                        }

                        GUI.color = guiColor;
                        EditorGUILayout.EndHorizontal();

                    }

                    if (deleteIndex > -1)
                    {
                        DeleteIndex(deleteIndex);
                    }
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("[Waypoints 1]       count: " + waypointsList_1.Count, SectionNameStyle_2);
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
                        else if (cwp.currentWayPointType == Waypoint.WayPointType.OnlyFront)
                            GUI.color = Color.gray;
                        else if (cwp.currentWayPointType == Waypoint.WayPointType.Sky)
                            GUI.color = Color.blue;
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
                                cwp.currentWayPointType = Waypoint.WayPointType.Sky;
                            else if (curr == Waypoint.WayPointType.Sky)
                                cwp.currentWayPointType = Waypoint.WayPointType.OnlyFront;
                            else if (curr == Waypoint.WayPointType.OnlyFront)
                                cwp.currentWayPointType = Waypoint.WayPointType.Normal;

                            dorepaint = true;
                        }

                        GUI.color = guiColor;
                        EditorGUILayout.EndHorizontal();

                    }

                    if (deleteIndex > -1)
                    {
                        DeleteIndex(deleteIndex);
                    }
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("[Waypoints 2]       count: " + waypointsList_2.Count, SectionNameStyle_2);
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
                        else if (cwp.currentWayPointType == Waypoint.WayPointType.OnlyFront)
                            GUI.color = Color.gray;
                        else if (cwp.currentWayPointType == Waypoint.WayPointType.Sky)
                            GUI.color = Color.blue;
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
                                cwp.currentWayPointType = Waypoint.WayPointType.Sky;
                            else if (curr == Waypoint.WayPointType.Sky)
                                cwp.currentWayPointType = Waypoint.WayPointType.OnlyFront;
                            else if (curr == Waypoint.WayPointType.OnlyFront)
                                cwp.currentWayPointType = Waypoint.WayPointType.Normal;
                        }

                        GUI.color = guiColor;
                        EditorGUILayout.EndHorizontal();

                    }

                    if (deleteIndex > -1)
                    {
                        DeleteIndex(deleteIndex);
                    }

                }
            }

   
            EditorGUILayout.Space();
            EditorGUILayout.Space();


            EditorGUILayout.LabelField("[Waypoints 3 - Center]       count: " + waypointsList_3.Count, SectionNameStyle_2);
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
                        else if (cwp.currentWayPointType == Waypoint.WayPointType.OnlyFront)
                            GUI.color = Color.gray;
                        else if (cwp.currentWayPointType == Waypoint.WayPointType.Sky)
                            GUI.color = Color.blue;
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
                                cwp.currentWayPointType = Waypoint.WayPointType.Sky;
                            else if (curr == Waypoint.WayPointType.Sky)
                                cwp.currentWayPointType = Waypoint.WayPointType.OnlyFront;
                            else if (curr == Waypoint.WayPointType.OnlyFront)
                                cwp.currentWayPointType = Waypoint.WayPointType.Normal;
                        }

                        GUI.color = guiColor;
                        EditorGUILayout.EndHorizontal();

                    }

                    if (deleteIndex > -1)
                    {
                        DeleteIndex(deleteIndex);
                    }
                }
            }


            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("[Waypoints 4]       count: " + waypointsList_4.Count, SectionNameStyle_2);

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
                        else if (cwp.currentWayPointType == Waypoint.WayPointType.OnlyFront)
                            GUI.color = Color.gray;
                        else if (cwp.currentWayPointType == Waypoint.WayPointType.Sky)
                            GUI.color = Color.blue;
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
                                cwp.currentWayPointType = Waypoint.WayPointType.Sky;
                            else if (curr == Waypoint.WayPointType.Sky)
                                cwp.currentWayPointType = Waypoint.WayPointType.OnlyFront;
                            else if (curr == Waypoint.WayPointType.OnlyFront)
                                cwp.currentWayPointType = Waypoint.WayPointType.Normal;
                        }

                        GUI.color = guiColor;
                        EditorGUILayout.EndHorizontal();

                    }

                    if (deleteIndex > -1)
                    {
                        DeleteIndex(deleteIndex);
                    }
                }
            }


            EditorGUILayout.Space();
            EditorGUILayout.Space();


            EditorGUILayout.LabelField("[Waypoints 5]       count: " + waypointsList_5.Count, SectionNameStyle_2);
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
                        else if (cwp.currentWayPointType == Waypoint.WayPointType.OnlyFront)
                            GUI.color = Color.gray;
                        else if (cwp.currentWayPointType == Waypoint.WayPointType.Sky)
                            GUI.color = Color.blue;
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
                                cwp.currentWayPointType = Waypoint.WayPointType.Sky;
                            else if (curr == Waypoint.WayPointType.Sky)
                                cwp.currentWayPointType = Waypoint.WayPointType.OnlyFront;
                            else if (curr == Waypoint.WayPointType.OnlyFront)
                                cwp.currentWayPointType = Waypoint.WayPointType.Normal;
                        }

                        GUI.color = guiColor;
                        EditorGUILayout.EndHorizontal();

                    }

                    if (deleteIndex > -1)
                    {
                        DeleteIndex(deleteIndex);
                    }
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("[Waypoints 6 - Outside]       count: " + waypointsList_6.Count, SectionNameStyle_2);
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
                        else if (cwp.currentWayPointType == Waypoint.WayPointType.OnlyFront)
                            GUI.color = Color.gray;
                        else if (cwp.currentWayPointType == Waypoint.WayPointType.Sky)
                            GUI.color = Color.blue;
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
                                cwp.currentWayPointType = Waypoint.WayPointType.Sky;
                            else if (curr == Waypoint.WayPointType.Sky)
                                cwp.currentWayPointType = Waypoint.WayPointType.OnlyFront;
                            else if (curr == Waypoint.WayPointType.OnlyFront)
                                cwp.currentWayPointType = Waypoint.WayPointType.Normal;

                            dorepaint = true;
                        }

                        GUI.color = guiColor;
                        EditorGUILayout.EndHorizontal();

                    }

                    if (deleteIndex > -1)
                    {
                        DeleteIndex(deleteIndex);
                    }
                }
            }


            EditorGUILayout.LabelField("-------------------------------------------", SectionNameStyle_2);


            GUILayout.Space(40);

            if (waypointsList_3.Count <= 0)
            {
                GUILayoutOption[] btnOptions_initialWaypoint = new[] {
                        GUILayout.Height (50),
                    };
                if (GUILayout.Button("Create Initial Waypoint", btnOptions_initialWaypoint))
                {
                    var data = ReadJsonData("Waypoint_Default");
                    if (data != null)
                    {
                        //성공한 경우
                        waypointsList_0 = data.waypointsList_0;
                        waypointsList_1 = data.waypointsList_1;
                        waypointsList_2 = data.waypointsList_2;
                        waypointsList_3 = data.waypointsList_3;
                        waypointsList_4 = data.waypointsList_4;
                        waypointsList_5 = data.waypointsList_5;
                        waypointsList_6 = data.waypointsList_6;
                        waypointsGroup.waypoints_0 = waypointsList_0;
                        waypointsGroup.waypoints_1 = waypointsList_1;
                        waypointsGroup.waypoints_2 = waypointsList_2;
                        waypointsGroup.waypoints_3 = waypointsList_3;
                        waypointsGroup.waypoints_4 = waypointsList_4;
                        waypointsGroup.waypoints_5 = waypointsList_5;
                        waypointsGroup.waypoints_6 = waypointsList_6;

                        if (waypointsList_0 != null && waypointsList_0.Count > 0)
                            selectedWaypoint = waypointsList_0[0];
                    }
                    else
                    {
                        //실패할경우...
                        CreateSampleWaypoint();
                        ExpandWayPoints();
                    }

                    SetIndexNumber();
                    dorepaint = true;
                    SaveScene();
                }
            }
            else
            {

            }

            /*
            GUILayout.Space(40);

            if (GUILayout.Button("Create Sample Waypoints (square)"))
            {
                Undo.RecordObject(waypointsGroup, "Create Sample Waypoints (square)");
                CreateSampleWaypoint();
                dorepaint = true;
            }
            */

            GUILayout.Space(40);


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
                        //Handles.DrawLine(wp.GetPosition(waypointsGroup.XYZConstraint), wpnext.GetPosition(waypointsGroup.XYZConstraint));
                        Handles.color = c;
                    }
                    else
                    {
                        Waypoint wpnext = waypoints[0];
                        Color c = Handles.color;
                        Handles.color = Color.gray;
                        //Handles.DrawLine(wp.GetPosition(waypointsGroup.XYZConstraint), wpnext.GetPosition(waypointsGroup.XYZConstraint));
                        Handles.color = c;
                    }

                    if (cnt == 0)
                    {
                        Color c = Handles.color;
                        Handles.color = Color.white;
                        float handleSize = HandleUtility.GetHandleSize(wp.GetPosition(waypointsGroup.XYZConstraint));
                        Handles.DrawWireDisc(wp.GetPosition(waypointsGroup.XYZConstraint), Vector3.up, 0.1f * handleSize, 5f);
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

        private void DrawCenterAxis()
        {
            if (waypointsList_3 != null && multipleSelectedWayPoints != null && multipleSelectedWayPoints.Count == 2)
            {
                var wp_1 = multipleSelectedWayPoints[0];
                var wp_2 = multipleSelectedWayPoints[1];

                if (Mathf.Abs(wp_1.index - wp_2.index) < 2)
                    return;

                if (centerAxis.HasValue)
                {
                    int startIndex = wp_1.index > wp_2.index ? wp_1.index : wp_2.index;
                    int endIndex = wp_1.index > wp_2.index ? wp_2.index : wp_1.index;

                    if (startIndex >= waypointsList_3.Count || endIndex >= waypointsList_3.Count)
                        return;

                    Color c = Handles.color;
                    Handles.color = Color.magenta;
                    float lineThickness = 2f;

                    EditorGUI.BeginChangeCheck();
                    Vector3 oldpos = centerAxis.Value;
                    Vector3 newPos = Handles.PositionHandle(oldpos, Quaternion.identity);
                    float handleSize = HandleUtility.GetHandleSize(centerAxis.Value);
                    Handles.SphereHandleCap(-1, centerAxis.Value, Quaternion.identity, 0.3f * handleSize, EventType.Repaint);
                    Handles.DrawLine(waypointsList_3[startIndex].GetPosition(), centerAxis.Value, lineThickness);
                    Handles.DrawLine(waypointsList_3[endIndex].GetPosition(), centerAxis.Value, lineThickness);

                    Handles.DrawLine(waypointsList_3[startIndex].GetPosition(), waypointsList_3[endIndex].GetPosition(), lineThickness);

                    var orthoLineDir = Vector3.Cross((waypointsList_3[endIndex].GetPosition() - waypointsList_3[startIndex].GetPosition()).normalized, Vector3.up).normalized;
                    var diagonalDist = Vector3.Distance(waypointsList_3[endIndex].GetPosition(), waypointsList_3[startIndex].GetPosition());
                    var radiusDist = Vector3.Distance(centerAxis.Value, waypointsList_3[startIndex].GetPosition());
                    var offset = radiusDist * orthoLineDir;
                    Handles.color = Color.magenta;

                    Vector3 lineV_1 = (centerAxis.Value - waypointsList_3[startIndex].GetPosition()).normalized;
                    Vector3 lineV_2 = (centerAxis.Value - waypointsList_3[endIndex].GetPosition()).normalized;

                    float degree = Vector3.Angle(lineV_1, lineV_2);
                    Handles.Label(centerAxis.Value + orthoLineDir * 10f, degree.ToString() + " degree");


                    var length_1 = Vector3.Distance(centerAxis.Value, waypointsList_3[startIndex].GetPosition());
                    var length_2 = Vector3.Distance(centerAxis.Value, waypointsList_3[endIndex].GetPosition());
                    Handles.Label((centerAxis.Value + waypointsList_3[startIndex].GetPosition()) / 2, "length: " + length_1.ToString());
                    Handles.Label((centerAxis.Value + waypointsList_3[endIndex].GetPosition()) / 2, "length: " + length_2.ToString());


                    int drawIntervalsCount = 50;
                    float perDegree = degree / drawIntervalsCount;
                    List<Vector3> drawIntervalList = new List<Vector3>();


                    bool isInner = true;
                    if (waypointsList_0 != null && waypointsList_6 != null && waypointsList_0.Count > 0)
                    {
                        if (Mathf.Abs(length_1 - length_2) <= 0.2f)
                        {
                            var s_dist_1 = Vector3.Distance(centerAxis.Value, waypointsList_0[startIndex].GetPosition());
                            var s_dist_2 = Vector3.Distance(centerAxis.Value, waypointsList_6[startIndex].GetPosition());
                            if (s_dist_1 < s_dist_2)
                                isInner = true; //안쪽 차선 기준인경우
                            else
                                isInner = false; //바깥 차선 기준인경우

                            var originalDir = (waypointsList_3[startIndex].GetPosition() - centerAxis.Value).normalized;
                            for (int index = 0; index < drawIntervalsCount; index++)
                            {
                                Quaternion rotation;

                                if (isInner)
                                    rotation = Quaternion.AngleAxis(perDegree * 1 * index, Vector3.up);
                                else
                                    rotation = Quaternion.AngleAxis(perDegree * -1 * index, Vector3.up);
                                var dir = (rotation * originalDir).normalized;
                                var drawIntervalPos = centerAxis.Value + dir * radiusDist;
                                drawIntervalList.Add(drawIntervalPos);
                            }

                            for (int i = 0; i < drawIntervalList.Count; i++)
                            {
                                if (i > 0)
                                {
                                    Handles.DrawLine(drawIntervalList[i - 1], drawIntervalList[i], lineThickness);
                                }
                            }

                            if (isInner)
                                Handles.DrawLine(centerAxis.Value + offset, centerAxis.Value, lineThickness);
                            else
                                Handles.DrawLine(centerAxis.Value - offset, centerAxis.Value, lineThickness);
                        }
                    }



                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(waypointsGroup, "centerAxis Moved");
                        doRepaint = true;
                        newPos.y = oldpos.y; //높이 값은 고정시키자...!
                        newPos = new Vector3(Mathf.Floor(newPos.x), Mathf.Floor(newPos.y), Mathf.Floor(newPos.z));

                        centerAxis = newPos;
                    }

                    Handles.color = c;
                }
                else
                {
                    var startIndex = wp_1.index >= wp_2.index ? wp_2.index : wp_1.index;
                    var endIndex = wp_1.index < wp_2.index ? wp_2.index : wp_1.index;

                    Vector3 startPos = waypointsList_3[startIndex].GetPosition();
                    Vector3 startNextPos;
                    if (startIndex < waypointsList_3.Count - 1)
                        startNextPos = waypointsList_3[++startIndex].GetPosition();
                    else
                        startNextPos = waypointsList_3[0].GetPosition();

                    Vector3 endPos = waypointsList_3[endIndex].GetPosition();
                    Vector3 endNextPos;
                    if (endIndex < waypointsList_3.Count - 1)
                        endNextPos = waypointsList_3[++endIndex].GetPosition();
                    else
                        endNextPos = waypointsList_3[0].GetPosition();

                    Vector3 lineV_1 = (startNextPos - startPos).normalized;
                    Vector3 lineV_2 = (endNextPos - endPos).normalized;
                    Vector3 cross = Vector3.Cross(lineV_1, lineV_2);

                    if (cross.magnitude < 0.1f) //거의 수평인경우
                    {
                        //수평인 경우 보여주지말자...!

                    }
                    else
                    {
                        centerAxis = GetInitialCenterAxis(startIndex, endIndex);
                        movableCenter = centerAxis.Value;
                    }
                }
            }
            else
            {
                centerAxis = null;
            }
        }

        private void AddWayPoint()
        {
            if (waypointsList_3 != null && waypointsList_3.Count > 0 && selectedWaypoint == null)
                return;

            waypointsList_0 = waypointsGroup.GetWaypointChildren_0();
            waypointsList_1 = waypointsGroup.GetWaypointChildren_1();
            waypointsList_2 = waypointsGroup.GetWaypointChildren_2();
            waypointsList_3 = waypointsGroup.GetWaypointChildren_3();
            waypointsList_4 = waypointsGroup.GetWaypointChildren_4();
            waypointsList_5 = waypointsGroup.GetWaypointChildren_5();
            waypointsList_6 = waypointsGroup.GetWaypointChildren_6();

            Undo.RecordObject(waypointsGroup, "AddWayPoint");

            SetIndexNumber();

            if (waypointsList_3 == null || waypointsList_3.Count <= 0)
            {
                int index = 0;
                Waypoint wp = new Waypoint();
                waypointsGroup.AddWaypoint_3(wp, index);

                selectedWaypoint = wp;
            }
            else
            {
                foreach (var list in allWaypoints)
                {
                    if (list == null || list.Count <= 0)
                        continue;

                    int index = -1;
                    int insertIndex = -1;

                    if (selectedWaypoint != null)
                    {
                        index = selectedWaypoint.index;
                        insertIndex = index + 1;
                    }

                    Waypoint wp = new Waypoint();

                    if (selectedWaypoint != null)
                    {
                        if (index < list.Count - 1 && list.Count >= 2)
                        {
                            Waypoint p1 = list[index];
                            Waypoint p2 = list[index + 1];

                            wp.position = Vector3.Lerp(p1.position, p2.position, 0.5f);
                            wp.rotation = Quaternion.identity;
                            wp.UpdatePosition(wp.position, waypointsGroup.XYZConstraint);
                            wp.currentWayPointType = p1.currentWayPointType;
                            wp.laneNumber = p1.laneNumber;
                            wp.index = insertIndex;
                        }
                        else if (index == list.Count - 1 && list.Count >= 2)
                        {
                            // 마지막 index 인 경우

                            if (list.Count >= 2 && list.Count <= 4)
                            {
                                //앞쪽에다가 만들어주자...
                                Waypoint p1 = list[index];
                                Waypoint p2 = list[index - 1];

                                var dir = (p1.position - p2.position).normalized;

                                wp.position = p1.position + dir * 10f;
                                wp.rotation = Quaternion.identity;
                                wp.UpdatePosition(wp.position, waypointsGroup.XYZConstraint);
                                wp.currentWayPointType = p1.currentWayPointType;
                                wp.laneNumber = p1.laneNumber;
                                wp.index = insertIndex;
                                insertIndex = -1;
                            }
                            else if (list.Count > 4)
                            {
                                //사이에만들어주자
                                Waypoint p1 = list[index];
                                Waypoint p2 = list[0];

                                wp.position = Vector3.Lerp(p1.position, p2.position, 0.5f);
                                wp.rotation = Quaternion.identity;
                                wp.UpdatePosition(wp.position, waypointsGroup.XYZConstraint);
                                wp.currentWayPointType = p1.currentWayPointType;
                                wp.laneNumber = p1.laneNumber;
                                wp.index = insertIndex;
                                insertIndex = -1;
                            }
                            else
                            {
                                wp.CopyOther(list[index]);
                                insertIndex = -1;
                            }
                        }
                        else
                        {
                            wp.CopyOther(list[index]);
                            insertIndex = -1;
                        }

                    }

                    waypointsGroup.AddWaypoint(wp, insertIndex, wp.laneNumber);
                }

                bool breakFlag = false;
                foreach (var i in allWaypoints)
                {
                    foreach (var j in i)
                    {
                        if (j.index.Equals(selectedWaypoint.index + 1)
                            && j.laneNumber.Equals(selectedWaypoint.laneNumber))
                        {
                            selectedWaypoint = j;
                            breakFlag = true;
                            break;
                        }
                    }
                    if (breakFlag)
                        break;
                }
            }

            SetIndexNumber();
        }

        private void AddMultipleWaypoints_Straight(int count, float length, float degree = 0f)
        {
            if (selectedWaypoint == null || count <= 0)
                return;

            waypointsList_0 = waypointsGroup.GetWaypointChildren_0();
            waypointsList_1 = waypointsGroup.GetWaypointChildren_1();
            waypointsList_2 = waypointsGroup.GetWaypointChildren_2();
            waypointsList_3 = waypointsGroup.GetWaypointChildren_3();
            waypointsList_4 = waypointsGroup.GetWaypointChildren_4();
            waypointsList_5 = waypointsGroup.GetWaypointChildren_5();
            waypointsList_6 = waypointsGroup.GetWaypointChildren_6();

            Undo.RecordObject(waypointsGroup, "AddMultipleWaypoints_Straight");

            SetIndexNumber();

            if (waypointsList_3 == null || waypointsList_3.Count <= 0)
            {
                int index = 0;
                Waypoint wp = new Waypoint();
                waypointsGroup.AddWaypoint_3(wp, index);

                selectedWaypoint = wp;
            }
            else
            {
                foreach (var list in allWaypoints)
                {
                    if (list == null || list.Count <= 0)
                        continue;

                    int index = -1;
                    int insertIndex = -1;
                    int laneNumber = -1;

                    if (selectedWaypoint != null)
                    {
                        index = selectedWaypoint.index;
                        insertIndex = index + 1;
                        laneNumber = list[0].laneNumber;
                    }

                    List<Waypoint> wpList = new List<Waypoint>();

                    if (index >= 0 && index <= list.Count - 1 && list.Count >= 2)
                    {
                        Waypoint p1 = list[index];
                        Waypoint p2 = (index > 0) ? list[index - 1] : list[list.Count - 1];

                        Vector3 dir;

                        if (waypointsList_2 != null && index < waypointsList_2.Count)
                        {
                            var newDir = (waypointsList_3[index].GetPosition() - waypointsList_2[index].GetPosition()).normalized;
                            var newDirOrtho = Vector3.Cross(newDir, Vector3.up).normalized;
                            dir = newDirOrtho;
                        }
                        else
                        {
                            dir = (p1.position - p2.position).normalized;
                        }
                        


                        var distPerWay = length / count;
                        Quaternion rotation = Quaternion.AngleAxis(degree, Vector3.up);
                        dir = (rotation * dir).normalized;

                        Vector3 startP;

                        if (degree < 0)
                        {
                            var tempDir = (allWaypoints[allWaypoints.Count - 1][index].GetPosition() - allWaypoints[0][index].GetPosition()).normalized;
                            var rotatedTempDir = (Quaternion.AngleAxis(degree, Vector3.up) * tempDir).normalized;
                            startP = allWaypoints[0][index].GetPosition() + GetInputDist() * rotatedTempDir * laneNumber;
                        }
                        else if (degree > 0)
                        {
                            var tempDir = (allWaypoints[allWaypoints.Count - 1][index].GetPosition() - allWaypoints[0][index].GetPosition()).normalized;
                            var rotatedTempDir = (Quaternion.AngleAxis(degree, Vector3.up) * tempDir).normalized;
                            startP = allWaypoints[allWaypoints.Count - 1][index].GetPosition() - GetInputDist() * rotatedTempDir * (allWaypoints.Count - laneNumber - 1);
                        }
                        else
                        {
                            startP = p1.GetPosition();
                        }

                        for (int i = 0; i < count; i++)
                        {
                            Waypoint _wp = new Waypoint();

                            _wp.position = startP + dir * distPerWay * (i + 1);
                            _wp.rotation = Quaternion.identity;
                            _wp.UpdatePosition(_wp.position, waypointsGroup.XYZConstraint);
                            _wp.currentWayPointType = p1.currentWayPointType;
                            _wp.laneNumber = p1.laneNumber;
                            _wp.index = insertIndex + count;
                            wpList.Add(_wp);
                        }

                        //마지막index의 경우...!
                        if(index == list.Count - 1)
                            insertIndex = -1;
                    }
                    else
                    {
                        for (int i = 0; i < wpList.Count; i++)
                        {
                            Waypoint _wp = new Waypoint();
                            _wp.CopyOther(list[index]);
                            _wp.position = _wp.GetPosition() + Vector3.right * 10f * (i + 1);
                            wpList.Add(_wp);
                        }

                        insertIndex = -1;
                    }

                    waypointsGroup.AddMultipleWaypoint(wpList, insertIndex, laneNumber);
                }

                SetIndexNumber();
                bool breakFlag = false;
                foreach (var i in allWaypoints)
                {
                    foreach (var j in i)
                    {
                        if (j.index.Equals(selectedWaypoint.index + count)
                            && j.laneNumber.Equals(selectedWaypoint.laneNumber))
                        {
                            selectedWaypoint = j;
                            breakFlag = true;
                            break;
                        }
                    }
                    if (breakFlag)
                        break;
                }
            }

            SetIndexNumber();
            SetWayPointListFromGroup();

            Debug.Log("Mulitple Waypoint Added!");
        }

        private void AddMultipleWaypoints_Curved(int count, float length, float degree)
        {
            if (selectedWaypoint == null || count <= 0)
                return;

            waypointsList_0 = waypointsGroup.GetWaypointChildren_0();
            waypointsList_1 = waypointsGroup.GetWaypointChildren_1();
            waypointsList_2 = waypointsGroup.GetWaypointChildren_2();
            waypointsList_3 = waypointsGroup.GetWaypointChildren_3();
            waypointsList_4 = waypointsGroup.GetWaypointChildren_4();
            waypointsList_5 = waypointsGroup.GetWaypointChildren_5();
            waypointsList_6 = waypointsGroup.GetWaypointChildren_6();

            Undo.RecordObject(waypointsGroup, "AddMultipleWaypoints_Curved");

            SetIndexNumber();

            if (waypointsList_3 == null || waypointsList_3.Count <= 0)
            {
                int index = 0;
                Waypoint wp = new Waypoint();
                waypointsGroup.AddWaypoint_3(wp, index);

                selectedWaypoint = wp;
            }
            else
            {
                foreach (var list in allWaypoints)
                {
                    if (list == null || list.Count <= 0)
                        continue;

                    int index = -1;
                    int insertIndex = -1;
                    int laneNumber = -1;

                    index = selectedWaypoint.index;
                    insertIndex = index + 1;
                    laneNumber = list[0].laneNumber;

                    List<Waypoint> wpList = new List<Waypoint>();

                    if (index >= 0 && index <= list.Count - 1 && list.Count >= 2)
                    {
                        Waypoint p1 = list[index];
                        Waypoint p2 = (index > 0) ? list[index - 1] : list[list.Count - 1];
                        var dir = (p1.position - p2.position).normalized;
                        var orthoDir = Vector3.Cross(dir, Vector3.up).normalized;

                        Vector3 a_pos;
                        Vector3 c_pos;
                        float arc_radius;
                        Vector3 arc_dir;

                        if (degree < 0) // 왼쪽 방향
                        {
                            if(waypointsList_0 != null && waypointsList_0.Count > 0)
                                a_pos = allWaypoints[0][index].GetPosition();
                            else
                                a_pos = allWaypoints[3][index].GetPosition() + orthoDir * 3 * GetInputDist();

                            c_pos = a_pos + orthoDir * length;
                            arc_radius = Vector3.Distance(c_pos, a_pos);
                            arc_dir = (a_pos - c_pos).normalized;
                        }
                        else if (degree > 0) //오른쪽
                        {
                            if (waypointsList_6 != null && waypointsList_6.Count > 0)
                                a_pos = allWaypoints[allWaypoints.Count - 1][index].GetPosition();
                            else
                                a_pos = allWaypoints[3][index].GetPosition() - orthoDir * 3 * GetInputDist();


                            c_pos = a_pos - orthoDir * length;
                            arc_radius = Vector3.Distance(c_pos, a_pos);
                            arc_dir = (a_pos - c_pos).normalized;
                        }
                        else
                        {
                            return;
                        }

                        float perDegree = degree / count;
                        for (int i = 0; i < count; i++)
                        {
                            Waypoint _wp = new Waypoint();

                            Quaternion _rot = Quaternion.AngleAxis(perDegree * (i + 1), Vector3.up);
                            var _rotatedDir = (_rot * arc_dir).normalized;

                            if (degree < 0) // 왼쪽 방향
                                _wp.position = c_pos + _rotatedDir * arc_radius + _rotatedDir * GetInputDist() * laneNumber;
                            else if (degree > 0) //오른쪽
                                _wp.position = c_pos + _rotatedDir * arc_radius + _rotatedDir * GetInputDist() * (allWaypoints.Count - laneNumber - 1);

                            _wp.rotation = Quaternion.identity;
                            _wp.UpdatePosition(_wp.position, waypointsGroup.XYZConstraint);
                            _wp.currentWayPointType = p1.currentWayPointType;
                            _wp.laneNumber = p1.laneNumber;
                            _wp.index = insertIndex + count;
                            wpList.Add(_wp);
                        }

                        //마지막index의 경우...!
                        if (index == list.Count - 1)
                            insertIndex = -1;
                    }
                    else
                    {
                        for (int i = 0; i < wpList.Count; i++)
                        {
                            Waypoint _wp = new Waypoint();
                            _wp.CopyOther(list[index]);
                            _wp.position = _wp.GetPosition() + Vector3.right * 10f * (i + 1);
                            wpList.Add(_wp);
                        }

                        insertIndex = -1;
                    }

                    waypointsGroup.AddMultipleWaypoint(wpList, insertIndex, laneNumber);
                }

                SetIndexNumber();
                bool breakFlag = false;
                foreach (var i in allWaypoints)
                {
                    foreach (var j in i)
                    {
                        if (j.index.Equals(selectedWaypoint.index + count)
                            && j.laneNumber.Equals(selectedWaypoint.laneNumber))
                        {
                            selectedWaypoint = j;
                            breakFlag = true;
                            break;
                        }
                    }
                    if (breakFlag)
                        break;
                }
            }

            SetIndexNumber();

            SetWayPointListFromGroup();

            Debug.Log("Mulitple Waypoint Added!");
        }

        private void RemoveHalfWaypoint()
        {
            if (waypointsList_3 == null || waypointsList_3.Count <= 3)
                return;

            Undo.RecordObject(waypointsGroup, "RemoveHalfWaypoint");

            SetIndexNumber();

            int cnt = 0;
            int listCount = waypointsList_3.Count;
            for (int index = listCount - 1; index >= 0; index--)
            {
                if (cnt % 2 == 1)
                    DeleteIndex(waypointsList_3[index].index);

                cnt++;
            }
        }

        private void RemoveRangeWaypoint()
        {
            Undo.RecordObject(waypointsGroup, "RemoveRangeWaypoint");

            if (waypointsList_3 != null && multipleSelectedWayPoints != null && multipleSelectedWayPoints.Count == 2)
            {
                var wp_1 = multipleSelectedWayPoints[0];
                var wp_2 = multipleSelectedWayPoints[1];

                //지워주는거니까 list뒤에서부터...
                int startIndex = wp_1.index < wp_2.index ? wp_1.index : wp_2.index;
                int endIndex = wp_1.index < wp_2.index ? wp_2.index : wp_1.index;

                Debug.Log("startIndex: " + startIndex);
                Debug.Log("endIndex: " + endIndex);

                for (int index = endIndex; index >= startIndex; --index)
                {
                    if (waypointsList_3.Count > index && index >= 0)
                    {
                        Debug.Log("delete index: " + index);
                        DeleteIndex(waypointsList_3[index].index);
                    }

                }

                multipleSelectedWayPoints.Clear();
                selectedWaypoint = null;
            }
            else
            {
                Debug.Log("Error...! RemoveRangeWaypoint");
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
                    newP.laneNumber = 3;

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
                    newP.laneNumber = 3;

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
                    newP.laneNumber = 0;

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
                    newP.laneNumber = 0;

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
                    newP.laneNumber = 1;

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
                    newP.laneNumber = 1;

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
                    newP.laneNumber = 2;

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
                    newP.laneNumber = 2;

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
                    newP.laneNumber = 4;

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
                    newP.laneNumber = 4;

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
                    newP.laneNumber = 5;

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
                    newP.laneNumber = 5;

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
                    newP.laneNumber = 6;

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
                    newP.laneNumber = 6;

                    additionalList.Add(newP);
                }

                for (int i = 0; i < additionalList.Count; i++)
                {
                    int index = i + i * 1;
                    waypointsGroup.AddWaypoint_6(additionalList[i], index + 1);
                }
            }
        }

        private void ChangeSelectedWaypointType()
        {
            if (selectedWaypoint == null)
                return;

            var curr = selectedWaypoint.currentWayPointType;
            if (curr == Waypoint.WayPointType.Normal)
                selectedWaypoint.currentWayPointType = Waypoint.WayPointType.Blocked;
            else if (curr == Waypoint.WayPointType.Blocked)
                selectedWaypoint.currentWayPointType = Waypoint.WayPointType.OutOfBoundary;
            else if (curr == Waypoint.WayPointType.OutOfBoundary)
                selectedWaypoint.currentWayPointType = Waypoint.WayPointType.OnlyFront;
            else if (curr == Waypoint.WayPointType.OnlyFront)
                selectedWaypoint.currentWayPointType = Waypoint.WayPointType.Sky;
            else if (curr == Waypoint.WayPointType.Sky)
                selectedWaypoint.currentWayPointType = Waypoint.WayPointType.Normal;

            SceneView.RepaintAll();
        }

        private void AlineWayPointsVertical()
        {
            if (selectedWaypoint == null && selectedWaypoint.index != -1)
                return;

            if (allWaypoints == null || allWaypoints.Count <= 1 || allWaypoints[0] == null || allWaypoints[0].Count <= 1)
            {
                Debug.Log("Error...! Wayponts need to be set first!!!");
                return;
            }

            Undo.RecordObject(waypointsGroup, "AlineWayPointsVertical");

            int nextIndex;

            if (selectedWaypoint.index < allWaypoints[3].Count - 1)
                nextIndex = selectedWaypoint.index + 1;
            else
                nextIndex = 0;

            Vector3 initialPosition = allWaypoints[3][selectedWaypoint.index].GetPosition();
            Vector3 nextPosition = allWaypoints[3][nextIndex].GetPosition();

            Vector3 direction;

            if (UtilityCommon.IsFront(Vector3.left, initialPosition, nextPosition))
                direction = Vector3.forward;
            else
                direction = Vector3.back;

            float distPerWaypoint = GetInputDist();

            allWaypoints[0][selectedWaypoint.index].SetPosition(initialPosition - direction * (distPerWaypoint * 3));
            allWaypoints[1][selectedWaypoint.index].SetPosition(initialPosition - direction * (distPerWaypoint * 2));
            allWaypoints[2][selectedWaypoint.index].SetPosition(initialPosition - direction * (distPerWaypoint * 1));
            allWaypoints[3][selectedWaypoint.index].SetPosition(initialPosition);
            allWaypoints[4][selectedWaypoint.index].SetPosition(initialPosition + direction * (distPerWaypoint * 1));
            allWaypoints[5][selectedWaypoint.index].SetPosition(initialPosition + direction * (distPerWaypoint * 2));
            allWaypoints[6][selectedWaypoint.index].SetPosition(initialPosition + direction * (distPerWaypoint * 3));

            Debug.Log("Waypoint successfully Vertical alined!");
        }

        private void AlineWayPointsHorizontal()
        {
            if (selectedWaypoint == null && selectedWaypoint.index != -1)
                return;

            if (allWaypoints == null || allWaypoints.Count <= 1 || allWaypoints[0] == null || allWaypoints[0].Count <= 1)
            {
                Debug.Log("Error...! Wayponts need to be set first!!!");
                return;
            }

            Undo.RecordObject(waypointsGroup, "AlineWayPointsHorizontal");

            int nextIndex;

            if (selectedWaypoint.index < allWaypoints[3].Count - 1)
                nextIndex = selectedWaypoint.index + 1;
            else
                nextIndex = 0;

            Vector3 initialPosition = allWaypoints[3][selectedWaypoint.index].GetPosition();
            Vector3 nextPosition = allWaypoints[3][nextIndex].GetPosition();

            Vector3 direction;

            if (UtilityCommon.IsFront(Vector3.forward, initialPosition, nextPosition))
                direction = Vector3.right;
            else
                direction = Vector3.left;

            float distPerWaypoint = GetInputDist();

            allWaypoints[0][selectedWaypoint.index].SetPosition(initialPosition - direction * (distPerWaypoint * 3));
            allWaypoints[1][selectedWaypoint.index].SetPosition(initialPosition - direction * (distPerWaypoint * 2));
            allWaypoints[2][selectedWaypoint.index].SetPosition(initialPosition - direction * (distPerWaypoint * 1));
            allWaypoints[3][selectedWaypoint.index].SetPosition(initialPosition);
            allWaypoints[4][selectedWaypoint.index].SetPosition(initialPosition + direction * (distPerWaypoint * 1 ));
            allWaypoints[5][selectedWaypoint.index].SetPosition(initialPosition + direction * (distPerWaypoint * 2));
            allWaypoints[6][selectedWaypoint.index].SetPosition(initialPosition + direction * (distPerWaypoint * 3));

            Debug.Log("Waypoint successfully Horizontal alined!");
        }

        private void AlineWayPointsSymmetry()
        {
            if (selectedWaypoint == null && selectedWaypoint.index != -1)
                return;

            if (allWaypoints == null || allWaypoints.Count <= 1 || allWaypoints[0] == null || allWaypoints[0].Count <= 1)
            {
                Debug.Log("Error...! Wayponts need to be set first!!!");
                return;
            }

            Undo.RecordObject(waypointsGroup, "AlineWayPointsSymmetry");

            int nextIndex;

            if (selectedWaypoint.index < allWaypoints[3].Count - 1)
                nextIndex = selectedWaypoint.index + 1;
            else
                nextIndex = 0;

            Vector3 initialPosition = allWaypoints[3][selectedWaypoint.index].GetPosition();

            float dist_0 = Vector3.Distance(allWaypoints[0][selectedWaypoint.index].GetPosition(), initialPosition);
            Vector3 dir_0 = (allWaypoints[0][selectedWaypoint.index].GetPosition() - initialPosition).normalized * -1f;
            float dist_1 = Vector3.Distance(allWaypoints[1][selectedWaypoint.index].GetPosition(), initialPosition);
            Vector3 dir_1 = (allWaypoints[1][selectedWaypoint.index].GetPosition() - initialPosition).normalized * -1f;
            float dist_2 = Vector3.Distance(allWaypoints[2][selectedWaypoint.index].GetPosition(), initialPosition);
            Vector3 dir_2 = (allWaypoints[2][selectedWaypoint.index].GetPosition() - initialPosition).normalized * -1f;

            allWaypoints[4][selectedWaypoint.index].SetPosition(initialPosition + dir_2 * dist_2);
            allWaypoints[5][selectedWaypoint.index].SetPosition(initialPosition + dir_1 * dist_1);
            allWaypoints[6][selectedWaypoint.index].SetPosition(initialPosition + dir_0 * dist_0);

            Debug.Log("Waypoint successfully Symmetried!");
        }

        private void AlineWayPointsIntervals()
        {
            if (selectedWaypoint == null && selectedWaypoint.index != -1)
                return;

            if (allWaypoints == null || allWaypoints.Count <= 1 || allWaypoints[0] == null || allWaypoints[0].Count <= 1)
            {
                Debug.Log("Error...! Wayponts need to be set first!!!");
                return;
            }

            Undo.RecordObject(waypointsGroup, "AlineWayPointsIntervals");

            Vector3 initialPosition = allWaypoints[3][selectedWaypoint.index].GetPosition();
            Vector3 direction = (allWaypoints[3][selectedWaypoint.index].GetPosition() - allWaypoints[2][selectedWaypoint.index].GetPosition()).normalized;

            float distPerWaypoint = GetInputDist();

            allWaypoints[0][selectedWaypoint.index].SetPosition(initialPosition - direction * (distPerWaypoint * 3));
            allWaypoints[1][selectedWaypoint.index].SetPosition(initialPosition - direction * (distPerWaypoint * 2));
            allWaypoints[2][selectedWaypoint.index].SetPosition(initialPosition - direction * (distPerWaypoint * 1));
            allWaypoints[3][selectedWaypoint.index].SetPosition(initialPosition);
            allWaypoints[4][selectedWaypoint.index].SetPosition(initialPosition + direction * (distPerWaypoint * 1));
            allWaypoints[5][selectedWaypoint.index].SetPosition(initialPosition + direction * (distPerWaypoint * 2));
            allWaypoints[6][selectedWaypoint.index].SetPosition(initialPosition + direction * (distPerWaypoint * 3));

            Debug.Log("Waypoint intervals successfully alined!");
        }

        private void ExpandWayPoints()
        {
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

            Undo.RecordObject(waypointsGroup, "Waypoint 3 ====>  Waypoint 0 & 1 & 2 & 4 & 5 & 6");

            float dist = GetInputDist();

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

                Vector3 newPos_0 = orginalPos - dir * (dist * 3);
                var newWayPoint_0 = new Waypoint() { position = newPos_0 };
                newWayPoint_0.UpdatePosition(newPos_0, PositionConstraint.XYZ);
                newWayPoint_0.currentWayPointType = Waypoint.WayPointType.Blocked;
                newWayPoint_0.laneNumber = 0;

                waypointsGroup.AddWaypoint_0(newWayPoint_0, i);
                waypointsList_0.Add(newWayPoint_0);

                Vector3 newPos_1 = orginalPos - dir * (dist * 2);
                var newWayPoint_1 = new Waypoint() { position = newPos_1 };
                newWayPoint_1.UpdatePosition(newPos_1, PositionConstraint.XYZ);
                newWayPoint_1.laneNumber = 1;

                waypointsGroup.AddWaypoint_1(newWayPoint_1, i);
                waypointsList_1.Add(newWayPoint_1);

                Vector3 newPos_2 = orginalPos - dir * (dist * 1);
                var newWayPoint_2 = new Waypoint() { position = newPos_2 };
                newWayPoint_2.UpdatePosition(newPos_2, PositionConstraint.XYZ);
                newWayPoint_2.laneNumber = 2;

                waypointsGroup.AddWaypoint_2(newWayPoint_2, i);
                waypointsList_2.Add(newWayPoint_2);

                Vector3 newPos_4 = orginalPos + dir * (dist * 1);
                var newWayPoint_4 = new Waypoint() { position = newPos_4 };
                newWayPoint_4.UpdatePosition(newPos_4, PositionConstraint.XYZ);
                newWayPoint_4.laneNumber = 4;

                waypointsGroup.AddWaypoint_4(newWayPoint_4, i);
                waypointsList_4.Add(newWayPoint_4);

                Vector3 newPos_5 = orginalPos + dir * (dist * 2);
                var newWayPoint_5 = new Waypoint() { position = newPos_5 };
                newWayPoint_5.UpdatePosition(newPos_5, PositionConstraint.XYZ);
                newWayPoint_5.laneNumber = 5;

                waypointsGroup.AddWaypoint_5(newWayPoint_5, i);
                waypointsList_5.Add(newWayPoint_5);

                Vector3 newPos_6 = orginalPos + dir * (dist * 3);
                var newWayPoint_6 = new Waypoint() { position = newPos_6 };
                newWayPoint_6.UpdatePosition(newPos_6, PositionConstraint.XYZ);
                newWayPoint_6.currentWayPointType = Waypoint.WayPointType.Blocked;
                newWayPoint_6.laneNumber = 6;

                waypointsGroup.AddWaypoint_6(newWayPoint_6, i);
                waypointsList_6.Add(newWayPoint_6);
            }

            SetIndexNumber();
            SceneView.RepaintAll();
        }

        private void DeleteIndex(int deleteIndex)
        {
            //아래 순서 꼭 지켜야함!!! (RemoveAt 때문에 list에 변동 생김)

            Undo.RecordObject(waypointsGroup, "DeleteIndex");

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

        private void SetWaypointsYtoZero()
        {
            Undo.RecordObject(waypointsGroup, "SetWaypointsYtoZero");

            foreach (var i in allWaypoints)
            {
                foreach (var j in i)
                {
                    var newPos = new Vector3(j.GetPosition().x, 0f, j.GetPosition().z);
                    j.SetPosition(newPos, PositionConstraint.XYZ);
                }
            }
        } 

        private void SetIndexNumber()
        {
            int verticesCnt = 0;

            if (waypointsList_0 != null)
            {
                for (int i = 0; i < waypointsList_0.Count; i++)
                {
                    waypointsList_0[i].index = i;
                    waypointsList_0[i].laneNumber = 0;
                    waypointsList_0[i].verticesNumber = verticesCnt++;
                }
            }
            if (waypointsList_1 != null)
            {
                for (int i = 0; i < waypointsList_1.Count; i++)
                {
                    waypointsList_1[i].index = i;
                    waypointsList_1[i].laneNumber = 1;
                    waypointsList_1[i].verticesNumber = verticesCnt++;
                }
            }
            if (waypointsList_2 != null)
            {
                for (int i = 0; i < waypointsList_2.Count; i++)
                {
                    waypointsList_2[i].index = i;
                    waypointsList_2[i].laneNumber = 2;
                    waypointsList_2[i].verticesNumber = verticesCnt++;
                }
            }
            if (waypointsList_3 != null)
            {
                for (int i = 0; i < waypointsList_3.Count; i++)
                {
                    waypointsList_3[i].index = i;
                    waypointsList_3[i].laneNumber = 3;
                    waypointsList_3[i].verticesNumber = verticesCnt++;
                }
            }
            if (waypointsList_4 != null)
            {
                for (int i = 0; i < waypointsList_4.Count; i++)
                {
                    waypointsList_4[i].index = i;
                    waypointsList_4[i].laneNumber = 4;
                    waypointsList_4[i].verticesNumber = verticesCnt++;
                }
            }
            if (waypointsList_5 != null)
            {
                for (int i = 0; i < waypointsList_5.Count; i++)
                {
                    waypointsList_5[i].index = i;
                    waypointsList_5[i].laneNumber = 5;
                    waypointsList_5[i].verticesNumber = verticesCnt++;
                }
            }
            if (waypointsList_6 != null)
            {
                for (int i = 0; i < waypointsList_6.Count; i++)
                {
                    waypointsList_6[i].index = i;
                    waypointsList_6[i].laneNumber = 6;
                    waypointsList_6[i].verticesNumber = verticesCnt++;
                }
            }

            if (waypointsList_3 != null)
            {
                //Debug.Log("Index Number Set....!!!! total num: " + waypointsList_3.Count);
            }
            else
            {
                Debug.Log("Error.... No Index Number to set...");
            }
        }

        private void ReverseAllWaypoints()
        {
            if (waypointsList_2.Count <= 0 || waypointsList_1.Count <= 0 || waypointsList_0.Count <= 0
                || waypointsList_4.Count <= 0 || waypointsList_5.Count <= 0 || waypointsList_6.Count <= 0)
            {
                Debug.Log("Set other waypoints first...!");
                return;
            }

            foreach (var i in allWaypoints)
            {
                if (i != null)
                {
                    i.Reverse();
                }
            }

            var deepCopyWayList = new List<List<Waypoint>>();

            for (int i = allWaypoints.Count - 1; i >= 0; --i) 
            {
                var _temp = new List<Waypoint>();
                foreach (var p in allWaypoints[i])
                {
                    var wp = new Waypoint();
                    wp.position = p.position;
                    wp.rotation = p.rotation;
                    wp.XY = p.XY;
                    wp.XYZ = p.XYZ;
                    wp.XZ = p.XZ;

                    wp.currentWayPointType = p.currentWayPointType;
                    wp.index = p.index;
                    wp.laneNumber = p.laneNumber;
                    wp.verticesNumber = p.verticesNumber;

                    _temp.Add(wp);
                }

                deepCopyWayList.Add(_temp);
            }


            for(int i = 0; i< deepCopyWayList.Count; i++)
            {
                waypointsList_0 = deepCopyWayList[0];
                waypointsList_1 = deepCopyWayList[1];
                waypointsList_2 = deepCopyWayList[2];
                waypointsList_3 = deepCopyWayList[3];
                waypointsList_4 = deepCopyWayList[4];
                waypointsList_5 = deepCopyWayList[5];
                waypointsList_6 = deepCopyWayList[6];

                if (waypointsGroup != null)
                {
                    waypointsGroup.waypoints_0 = waypointsList_0;                 
                    waypointsGroup.waypoints_1 = waypointsList_1;                 
                    waypointsGroup.waypoints_2 = waypointsList_2;                 
                    waypointsGroup.waypoints_3 = waypointsList_3;                 
                    waypointsGroup.waypoints_4 = waypointsList_4;                 
                    waypointsGroup.waypoints_5 = waypointsList_5;                 
                    waypointsGroup.waypoints_6 = waypointsList_6;                 
                }
            }

            SetIndexNumber();


            Debug.Log("Waypoint Reverse Complete!!!!");
        }

        private void SetWayPointListFromGroup()
        {
            waypointsList_0 = waypointsGroup.GetWaypointChildren_0();
            waypointsList_1 = waypointsGroup.GetWaypointChildren_1();
            waypointsList_2 = waypointsGroup.GetWaypointChildren_2();
            waypointsList_3 = waypointsGroup.GetWaypointChildren_3();
            waypointsList_4 = waypointsGroup.GetWaypointChildren_4();
            waypointsList_5 = waypointsGroup.GetWaypointChildren_5();
            waypointsList_6 = waypointsGroup.GetWaypointChildren_6();
        }

        //웨이포인트 간 거리 통일 및 보정
        private void UnifyWaypointsLength()
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

            bool unifyIncludingIndexZero = false;
            if (waypointsList_3.Count >= 4 
                && MathF.Abs(startIndex - endIndex) > waypointsList_3.Count * 0.5f)
            {
                if ((waypointsList_3.Count - (int)MathF.Abs(endIndex - startIndex)) >= 4)
                {
                    unifyIncludingIndexZero = true;
                    startIndex = wp_1.index >= wp_2.index ? wp_1.index : wp_2.index;
                    endIndex = wp_1.index < wp_2.index ? wp_1.index : wp_2.index;
                }
                else
                {
                    Debug.Log("Nothing Happened...! Select Longer index Waypoint!");
                    return;
                }
            }
            else
                unifyIncludingIndexZero = false;



            int indexCount = waypointsList_3.Count;
            int laneCount = allWaypoints.Count;
            int centerLane = (int)laneCount / 2;

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

            if (MathF.Abs(endIndex - startIndex) > 1)
            {
                var initialPos = lw[3][startIndex].GetPosition();
                var dir = (lw[3][endIndex].GetPosition() - lw[3][startIndex].GetPosition()).normalized;
                var othDir = Vector3.Cross(dir, Vector3.up).normalized;


                int laneCounter = 1;
                int totalCounter = 0;
                int totalIndexMakeCount = 0;

                if (unifyIncludingIndexZero == false)
                    totalIndexMakeCount = (endIndex - startIndex) * lw.Count;
                else
                    totalIndexMakeCount = (indexCount - (int)MathF.Abs(endIndex - startIndex)) * lw.Count;

                for (int index = startIndex + 1;;)
                {
                    for (int lane = 0; lane < lw.Count; lane++)
                    {
                        var dist = Vector3.Distance(lw[3][endIndex].GetPosition(), lw[3][startIndex].GetPosition());
                        var distPerWayPoint = 0f;

                        if (unifyIncludingIndexZero == false)
                            distPerWayPoint = dist / (endIndex - startIndex);
                        else
                            distPerWayPoint = dist / (indexCount - MathF.Abs(endIndex - startIndex));

                        Vector3 newPos;
                        if (lane < centerLane)
                            newPos = initialPos + othDir * (centerLane - lane) * GetInputDist() + laneCounter * (dir * distPerWayPoint);
                        else if (lane == centerLane)
                            newPos = initialPos + laneCounter * (dir * distPerWayPoint);
                        else
                            newPos = initialPos - othDir * (lane - centerLane) * GetInputDist() + laneCounter * (dir * distPerWayPoint);

                        lw[lane][index].SetPosition(newPos);
                        ++totalCounter;
                    }
                    laneCounter++;

                    if (totalCounter >= totalIndexMakeCount)
                    {
                        break;
                    }
                    else
                    {
                        if (index < indexCount - 1)
                            ++index;
                        else
                            index = 0;
                    }
                }
                SaveScene();

                Debug.Log("All Waypoint Distance is Unified! start index: " + startIndex + "  end index: " + endIndex);
            }
            else
            {
                Debug.Log("No index between your selected waypoints!");
            }

        }

        //X,Y 웨이포인트 타입 통일
        private void UnifyWayPointType(Waypoint.WayPointType type)
        {
            if (waypointsList_3.Count <= 0)
            {
                Debug.Log("Set waypoints 3 first...!");
                return;
            }

            if (waypointsList_2.Count <= 0 || waypointsList_1.Count <= 0 || waypointsList_0.Count <= 0
                || waypointsList_4.Count <= 0 || waypointsList_5.Count <= 0 || waypointsList_6.Count <= 0)
            {
                Debug.Log("Set other waypoints first...!");
                return;
            }

            if (waypointsList_3 != null)
            {
                SetIndexNumber();
            }

            if (multipleSelectedWayPoints == null || multipleSelectedWayPoints.Count <= 0)
                return;

            Undo.RecordObject(waypointsGroup, "Unify All Waypoint Type" + type);
            //X,Y 웨이포인트 타입 통일

            multipleSelectedWayPoints.Sort((x, y) => x.index.CompareTo(y.index));
            var wp_1 = multipleSelectedWayPoints[0];
            var wp_2 = multipleSelectedWayPoints[multipleSelectedWayPoints.Count - 1];

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

            var startLane = wp_1.laneNumber >= wp_2.laneNumber ? wp_2.laneNumber : wp_1.laneNumber;
            var endLane = wp_1.laneNumber < wp_2.laneNumber ? wp_2.laneNumber : wp_1.laneNumber;

            if (endIndex - startIndex >= 1)
            {
                for (int i = startIndex; i <= endIndex; i++)
                {
                    for (int j = 0; j < lw.Count; j++)
                    {
                        if (lw[j][i].laneNumber >= startLane
                            && lw[j][i].laneNumber <= endLane)
                            lw[j][i].currentWayPointType = type;
                    }
                }
                SaveScene();
                multipleSelectedWayPoints.Clear();

                Debug.Log("All Waypoint Type is Unified to " + type + "! start index: " + startIndex + "  end index: " + endIndex);
            }
            else
            {
                Debug.Log("No index between your selected waypoints!");
            }
        }

        //곡선 웨이포인트
        private void CreateCurvedWaypoints()
        {
            if (multipleSelectedWayPoints == null || multipleSelectedWayPoints.Count != 2)
            {
                return;
            }

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

            if (centerAxis.HasValue == false)
                return;
                    

            Undo.RecordObject(waypointsGroup, "CreateCurvedWaypoints");
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

            if (endIndex - startIndex >= 2)
            {
                Vector3 startPos = waypointsList_3[startIndex].GetPosition();
                Vector3 endPos = waypointsList_3[endIndex].GetPosition();

                Vector3 lineV_1 = (centerAxis.Value - startPos).normalized;
                Vector3 lineV_2 = (centerAxis.Value - endPos).normalized;

                float degree = Vector3.Angle(lineV_1, lineV_2);

                var s_dist_1 = Vector3.Distance(centerAxis.Value, lw[0][startIndex].GetPosition());
                var s_dist_2 = Vector3.Distance(centerAxis.Value, lw[6][startIndex].GetPosition());

                var startIndex_dist = Mathf.Min(s_dist_1, s_dist_2);

                float perDegree = degree / (float)(endIndex - startIndex);
                bool isInner = true;
                if (s_dist_1 < s_dist_2)
                {
                    //안쪽 차선 기준인경우
                    isInner = true;
                }
                else
                {
                    //바깥 차선 기준인경우
                    isInner = false;
                }

                var originalDir = (lw[3][startIndex].GetPosition() - centerAxis.Value).normalized;

                int counter = 1;
                for (int index = startIndex + 1; index < endIndex; index++)
                {
                    for (int lane = 0; lane < lw.Count; lane++)
                    {
                        Quaternion rotation;
                        if(isInner)
                            rotation = Quaternion.AngleAxis(perDegree * -1 * counter, Vector3.up);
                        else
                            rotation = Quaternion.AngleAxis(perDegree * 1 * counter, Vector3.up);

                        var dir = (rotation * originalDir).normalized;
                        var newPos = centerAxis.Value + dir * startIndex_dist + dir * GetInputDist() * lane;
                        //Debug.Log("original: " + lw[lane][index].GetPosition() + "   new: " + newPos);

                        if(isInner)
                            lw[lane][index].SetPosition(newPos);
                        else
                            lw[lw.Count - 1 - lane][index].SetPosition(newPos);
                    }
                    counter++;
                }
                SaveScene();
                                
                Debug.Log("Curved Waypoints Created! degree: " + degree +   "   start index: " + startIndex + "  end index: " + endIndex);
            }
            else
            {
                Debug.Log("No index between your selected waypoints!");
            }
        }

        private Vector3 GetInitialCenterAxis(int startIndex, int endIndex)
        {
            Vector3 startPos = waypointsList_3[startIndex].GetPosition();
            Vector3 endPos = waypointsList_3[endIndex].GetPosition();
            Vector3 midPosBetweenStartAndEndPos = (startPos + endPos) / 2f;

            Vector3 result;
            int centerIndex = startIndex + (endIndex - startIndex) / 2;


            float dist = Vector3.Distance(startPos, endPos) / 2f;

            Vector3 direction = (endPos - startPos).normalized;
            Vector3 orthoDirection = Vector3.Cross(direction, Vector3.up).normalized;


            Vector3 vector1 = midPosBetweenStartAndEndPos + orthoDirection * dist * 1f;
            Vector3 vector2 = midPosBetweenStartAndEndPos + orthoDirection * dist * -1f;


            var s_dist_1 = Vector3.Distance(vector1, waypointsList_3[centerIndex].GetPosition());
            var s_dist_2 = Vector3.Distance(vector2, waypointsList_3[centerIndex].GetPosition());

            if (s_dist_1 > s_dist_2)
            {
               //isInner = true;
                result = vector1;
            }
            else
            {
                //isOuter
                result = vector2;
            }

            return result;



            /*
            Vector3 startPos = waypointsList_3[startIndex].GetPosition();
            Vector3 startNextPos = waypointsList_2[startIndex].GetPosition();

            Vector3 endPos = waypointsList_3[endIndex].GetPosition();
            Vector3 endNextPos = waypointsList_2[endIndex].GetPosition();

            Vector3 lineV_1 = (startNextPos - startPos).normalized;
            Vector3 lineV_2 = (endNextPos - endPos).normalized;
            Vector3 cross = Vector3.Cross(lineV_1, lineV_2);

            Vector3 centerIntersection;

            if (cross.magnitude < 0.2f) //거의 수평인경우
            {
                //수평인 경우 두 지점의 중간을.... 중앙으로 계산하자
                centerIntersection = (endPos - startPos) / 2f;
            }
            else
            {
                float temp = Vector3.Cross((endPos - startPos), lineV_2).magnitude / cross.magnitude;
                centerIntersection = startPos + lineV_1 * temp; //두 방향벡터의 교차점!!!
            }
            */
        }

        private float GetWaypointTotalLength()
        { 
            float totalLength = 0f;

            if (waypointsList_3 != null && waypointsList_3.Count > 0)
            {
                for (int i = 0; i < waypointsList_3.Count - 1; i++)
                {
                    totalLength += Vector3.Distance(waypointsList_3[i].GetPosition(), waypointsList_3[i + 1].GetPosition());
                }
            }

            return totalLength;
        }

        private int GetWaypointCount()
        {
            int cnt = 0;

            if (waypointsList_3 != null && waypointsList_3.Count > 0)
            {
                cnt = waypointsList_3.Count;
            }

            return cnt;
        }


        public void CreateSampleWaypoint()
        {
            if (waypointsGroup != null
                && waypointsGroup.waypoints_0.Count <= 0
                && waypointsGroup.waypoints_3.Count <= 0)
            {
                Undo.RecordObject(waypointsGroup, "CreateSampleWaypoint");

                multipleSelectedWayPoints.Clear();
                selectedWaypoint = null;

                for (int i = 0; i < sqaure_example_lane3.Length; i++)
                {
                    AddWayPoint();
                }
                for (int i = 0; i < waypointsGroup.waypoints_3.Count; i++)
                {
                    if (i < sqaure_example_lane3.Length)
                        waypointsGroup.waypoints_3[i].SetPosition(sqaure_example_lane3[i]);
                }

                //TODO.. 아래 코드 사용하면 뭔가 이상함... 문제가 있음
                /*
                ExpandWayPoints();


                for (int i = 0; i < waypointsGroup.waypoints_0.Count; i++)
                {
                    if (i < sqaure_example_lane0.Length)
                        waypointsGroup.waypoints_0[i].SetPosition(sqaure_example_lane0[i]);
                }
                for (int i = 0; i < waypointsGroup.waypoints_1.Count; i++)
                {
                    if (i < sqaure_example_lane1.Length)
                        waypointsGroup.waypoints_1[i].SetPosition(sqaure_example_lane1[i]);
                }
                for (int i = 0; i < waypointsGroup.waypoints_2.Count; i++)
                {
                    if (i < sqaure_example_lane2.Length)
                        waypointsGroup.waypoints_2[i].SetPosition(sqaure_example_lane2[i]);
                }


                for (int i = 0; i < waypointsGroup.waypoints_4.Count; i++)
                {
                    if (i < sqaure_example_lane4.Length)
                        waypointsGroup.waypoints_4[i].SetPosition(sqaure_example_lane4[i]);
                }
                for (int i = 0; i < waypointsGroup.waypoints_5.Count; i++)
                {
                    if (i < sqaure_example_lane5.Length)
                        waypointsGroup.waypoints_5[i].SetPosition(sqaure_example_lane5[i]);
                }
                for (int i = 0; i < waypointsGroup.waypoints_6.Count; i++)
                {
                    if (i < sqaure_example_lane6.Length)
                        waypointsGroup.waypoints_6[i].SetPosition(sqaure_example_lane6[i]);
                }
                */

                SetIndexNumber();

                SaveScene();
            }
            else
            {
                Debug.Log("<color=red>Error...! Clear your waypoint first!</color>");
            }

        }

        private void CreateChargePad()
        {
            if (selectedWaypoint == null || waypointsList_3 == null || waypointsList_3.Count <= 0)
                return;

            Undo.RecordObject(waypointsGroup, "CreateChargePad");

            int startIndex = selectedWaypoint.index;
            int nextIndex = startIndex;

            if (nextIndex < waypointsList_3.Count - 1)
                nextIndex = startIndex + 1;
            else
                nextIndex = 0;

            List<Waypoint> lane = new List<Waypoint>();
            foreach (var i in allWaypoints)
            {
                foreach (var j in i)
                {
                    if (selectedWaypoint == j)
                    {
                        lane = i;
                        break;
                    }
                }
            }

            if (lane.Count <= 0)
                return;

            Vector3 dir = (lane[nextIndex].GetPosition() - lane[startIndex].GetPosition()).normalized;

            string chargePadPath = "Assets/Resources/Prefabs/InGame/MapObject_ChargePad.prefab";
            if (System.IO.File.Exists(chargePadPath))
            {
                var resource = Resources.Load("Prefabs/InGame/MapObject_ChargePad");
                if (resource != null)
                {
                    GameObject cp = PrefabUtility.InstantiatePrefab(resource) as GameObject;
                    cp.transform.rotation = Quaternion.LookRotation(dir);
                    cp.transform.localScale = Vector3.one;
                    cp.transform.position = selectedWaypoint.GetPosition();

                    var mapObjectManager = GameObject.FindObjectOfType<MapObjectManager>();
                    if (mapObjectManager != null)
                    {
                        mapObjectManager.chargePadList = new List<MapObject_ChargePad>();

                        var arr = FindObjectsOfType<MapObject_ChargePad>();
                        foreach (var i in arr)
                        {
                            if (mapObjectManager.chargePadList.Contains(i) == false)
                                mapObjectManager.chargePadList.Add(i);
                        }
                    }

                    Debug.Log("Charge Pad Added!");
                }
            }
            else
                Debug.Log("chargePadPath: " + chargePadPath + "is null");
        }

        private void CreateObstacle()
        {
            if (selectedWaypoint == null || waypointsList_3 == null || waypointsList_3.Count <= 0)
                return;

            Undo.RecordObject(waypointsGroup, "CreateObstacle");

            int startIndex = selectedWaypoint.index;
            int nextIndex = startIndex;

            if (nextIndex < waypointsList_3.Count - 1)
                nextIndex = startIndex + 1;
            else
                nextIndex = 0;

            List<Waypoint> lane = new List<Waypoint>();
            foreach (var i in allWaypoints)
            {
                foreach (var j in i)
                {
                    if (selectedWaypoint == j)
                    {
                        lane = i;
                        break;
                    }
                }
            }

            if (lane.Count <= 0)
                return;

            Vector3 dir = (lane[nextIndex].GetPosition() - lane[startIndex].GetPosition()).normalized;

            string path = "Assets/Resources/Prefabs/InGame/MapObject_Obstacle.prefab";
            if (System.IO.File.Exists(path))
            {
                var resource = Resources.Load("Prefabs/InGame/MapObject_Obstacle");
                if (resource != null)
                {
                    GameObject obj = PrefabUtility.InstantiatePrefab(resource) as GameObject;
                    obj.transform.rotation = Quaternion.LookRotation(dir);
                    obj.transform.localScale = Vector3.one;
                    obj.transform.position = selectedWaypoint.GetPosition();

                    var mapObjectManager = GameObject.FindObjectOfType<MapObjectManager>();
                    if (mapObjectManager != null)
                    {
                        mapObjectManager.obstacleList = new List<MapObject_Obstacle>();

                        var arr = FindObjectsOfType<MapObject_Obstacle>();
                        foreach (var i in arr)
                        {
                            if (mapObjectManager.obstacleList.Contains(i) == false)
                                mapObjectManager.obstacleList.Add(i);
                        }
                    }

                    Debug.Log("Obstacle Added!");
                }
            }
            else
                Debug.Log("path: " + path + "is null");
        }

        private void CreateTimingPad()
        {
            if (selectedWaypoint == null || waypointsList_3 == null || waypointsList_3.Count <= 0)
                return;

            Undo.RecordObject(waypointsGroup, "CreateTimingPad");

            int startIndex = selectedWaypoint.index;
            int nextIndex = startIndex;

            if (nextIndex < waypointsList_3.Count - 1)
                nextIndex = startIndex + 1;
            else
                nextIndex = 0;

            List<Waypoint> lane = new List<Waypoint>();
            foreach (var i in allWaypoints)
            {
                foreach (var j in i)
                {
                    if (selectedWaypoint == j)
                    {
                        lane = i;
                        break;
                    }
                }
            }

            if (lane.Count <= 0)
                return;

            Vector3 dir = (lane[nextIndex].GetPosition() - lane[startIndex].GetPosition()).normalized;

            string path = "Assets/Resources/Prefabs/InGame/MapObject_TimingPad.prefab";
            if (System.IO.File.Exists(path))
            {
                var resource = Resources.Load("Prefabs/InGame/MapObject_TimingPad");
                if (resource != null)
                {
                    GameObject obj = PrefabUtility.InstantiatePrefab(resource) as GameObject;
                    obj.transform.rotation = Quaternion.LookRotation(dir);
                    obj.transform.localScale = Vector3.one;
                    obj.transform.position = selectedWaypoint.GetPosition();

                    var mapObjectManager = GameObject.FindObjectOfType<MapObjectManager>();
                    if (mapObjectManager != null)
                    {
                        mapObjectManager.timingPadList = new List<MapObject_TimingPad>();

                        var arr = FindObjectsOfType<MapObject_TimingPad>();
                        foreach (var i in arr)
                        {
                            if (mapObjectManager.timingPadList.Contains(i) == false)
                                mapObjectManager.timingPadList.Add(i);
                        }
                    }

                    Debug.Log("Timing Pad Added!");
                }
            }
            else
                Debug.Log("path: " + path + "is null");
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
                EditorGUI.BeginChangeCheck();
                Vector3 oldpos = waypoint.GetPosition(waypointsGroup.XYZConstraint);
                Vector3 newPos = Handles.PositionHandle(oldpos, waypoint.rotation);

                float handleSize = HandleUtility.GetHandleSize(newPos);

                Handles.DrawWireDisc(newPos, Vector3.up, 0.15f * handleSize, 2f);
                //Handles.DrawWireCube(newPos, Vector3.one * 0.3f * handleSize);
                //Handles.SphereHandleCap(-1, newPos, waypoint.rotation, 0.3f * handleSize, EventType.Repaint);
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
                    Handles.color = Color.black;

                //Vector3 newPos = Handles.FreeMoveHandle(waypoint.GetPosition(), waypoint.rotation, 1.0f, Vector3.zero, Handles.SphereHandleCap);
                EditorGUI.BeginChangeCheck();
                Vector3 oldpos = waypoint.GetPosition(waypointsGroup.XYZConstraint);
                Vector3 newPos = Handles.PositionHandle(oldpos, waypoint.rotation);

                float handleSize = HandleUtility.GetHandleSize(newPos);

                Handles.DrawWireDisc(newPos, Vector3.up, 0.15f * handleSize, 2f);
                //Handles.SphereHandleCap(-1, newPos, waypoint.rotation, 0.3f * handleSize, EventType.Repaint);
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
                else if (waypoint.currentWayPointType == Waypoint.WayPointType.Sky)
                    Handles.color = Color.blue;
                else if (waypoint.currentWayPointType == Waypoint.WayPointType.OnlyFront)
                    Handles.color = Color.gray;
                else
                    Handles.color = Color.white;

                EditorGUI.BeginChangeCheck();
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
        [MenuItem(CommonDefine.ProjectName + "/Custom Editor/Create Waypoints Group")]
        public static void CreateWaypoint()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == CommonDefine.InGameScene
                || UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == CommonDefine.OutGameScene)
            {
                Debug.Log("<color=red>Invalid Scene!! Waypoints are set in seperate maps not on ingame or outgame scene</color>");
                return;
            }

            GameObject go = new GameObject("Waypoints Info");
            go.AddComponent<WaypointsGroup>();

            // Select it:
            Selection.activeGameObject = go;
        }


        private float GetInputDist()
        {
            float _dist = 0f;
            string input = inputDist;
            if (float.TryParse(input, out _dist))
            {
                if (_dist <= 0f)
                    _dist = CommonDefine.DEFAULT_LANE_BY_LANE_DIST;
            }
            else
            {
               // Debug.Log("<color=red>_dist float parse using default dist: 3.5f </color>");
                _dist = CommonDefine.DEFAULT_LANE_BY_LANE_DIST;
            }

            return _dist;
        }

        private float GetInput(string input)
        {
            float result = 0f;
            if (float.TryParse(input, out result))
            {

            }
            else
            {
                result = 0f;
            }

            return result;
        }

        private List<Vector3> CalculatePoints(Vector3 startPoint, Vector3 endPoint, Vector3 controlPoint, int numPoints)
        {
            List<Vector3> list = new List<Vector3>();

            for (int i = 0; i < numPoints; i++)
            {
                float t = i / (float)numPoints;
                Vector3 point = CalculateBezierPoint(t, startPoint, endPoint, controlPoint);
                list.Add(point);
            }

            return list;
        }

        // 베지어 곡선을 계산하는 함수
        Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            Vector3 p = uu * p0;
            p += 2 * u * t * p1;
            p += tt * p2;
            return p;
        }


        [Serializable]
        public class WaypointData
        {
            [SerializeField] public List<Waypoint> waypointsList_0;
            [SerializeField] public List<Waypoint> waypointsList_1;
            [SerializeField] public List<Waypoint> waypointsList_2;
            [SerializeField] public List<Waypoint> waypointsList_3;
            [SerializeField] public List<Waypoint> waypointsList_4;
            [SerializeField] public List<Waypoint> waypointsList_5;
            [SerializeField] public List<Waypoint> waypointsList_6;
        }

        public string SaveWaypointToJsonString(string _fileName)
        {
            WaypointData data = new WaypointData();
            data.waypointsList_0 = new List<Waypoint>();
            data.waypointsList_1 = new List<Waypoint>();
            data.waypointsList_2 = new List<Waypoint>();
            data.waypointsList_3 = new List<Waypoint>();
            data.waypointsList_4 = new List<Waypoint>();
            data.waypointsList_5 = new List<Waypoint>();
            data.waypointsList_6 = new List<Waypoint>();

            data.waypointsList_0 = waypointsList_0;
            data.waypointsList_1 = waypointsList_1;
            data.waypointsList_2 = waypointsList_2;
            data.waypointsList_3 = waypointsList_3;
            data.waypointsList_4 = waypointsList_4;
            data.waypointsList_5 = waypointsList_5;
            data.waypointsList_6 = waypointsList_6;

            string json = JsonUtility.ToJson(data);



            string fileName = "";
            if (string.IsNullOrEmpty(_fileName) == false)
            {
                fileName = _fileName + ".txt";
            }
            else
            {
                fileName = "Waypoint_Data" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + ".txt";
            }

            string filePath = Path.Combine("Assets/Resources/Data", fileName);
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine(json);
            }

            if (string.IsNullOrEmpty(json))
            {
                Debug.Log("waypointData json is null... ");
            }
            else
            {

                Debug.Log("Saved... path: " + filePath);
                Debug.Log("waypointData json :" + json);
            }

            return json;
        }


        public WaypointData ReadJsonData(string _fileName)
        {
            WaypointData data = null;

            string fileName = "";
            if (string.IsNullOrEmpty(_fileName) == false)
            {
                if (_fileName.Contains(".txt") == false)
                    fileName = _fileName + ".txt";
                else
                    fileName = _fileName;
            }
            else
            {
                fileName = "Waypoint_Default.txt";
            }

            string filePath = Path.Combine("Assets/Resources/Data", fileName);

            if (File.Exists(filePath))
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrEmpty(line) == false)
                            data = JsonUtility.FromJson<WaypointData>(line);
                    }
                }
            }
            else
            {
                Debug.Log("<color=red>No Such File Exists!</color>");
            }


            return data;
        }


        #region Memo
        //사각형 example

        private Vector3[] sqaure_example_lane3 = new Vector3[]
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(75f, 0f, 0f),
            new Vector3(150f, 0f, 0f),
            new Vector3(150f, 0f, 75f),
            new Vector3(150f, 0f, 150f),
            new Vector3(150f, 0f, 2250f),
            new Vector3(150f, 0f, 300f),
            new Vector3(75f, 0f, 300f),
            new Vector3(0f, 0f, 300f),
            new Vector3(-75f, 0f, 300f),
            new Vector3(-150f, 0f, 300f),
            new Vector3(-150f, 0f, 225f),
            new Vector3(-150f, 0f, 150f),
            new Vector3(-150f, 0f, 75f),
            new Vector3(-150f, 0f, 0f),
        };

        #endregion
    }
#endif
}
