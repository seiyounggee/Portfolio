using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using WayPointSystem;

namespace WayPointSystem
{
#if UNITY_EDITOR
    [CustomEditor(typeof(WaypointMeshGenerator))]
    public class WaypointMeshGeneratorEditor : Editor
    {
        private WaypointMeshGenerator meshGenerator = null;
        private WaypointsGroup waypointsGroup = null;

        string input_waypointName = "";
        string input_waypointBoundaryName = "";
        bool overrideSavedFiles = true;
        bool generateOnlyNormalTypeWaypoints = true;
        bool deleteFiles = false;

        bool insertDefaultTexture = true;

        private void OnEnable()
        {
            meshGenerator = target as WaypointMeshGenerator;
            waypointsGroup = FindObjectOfType<WaypointsGroup>();

            string path = "Assets/3.Art/3D/GeneratedMesh/";
            string name = "waypoint_mesh_" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            string fullpath_asset = path + name + ".asset";

            if (System.IO.File.Exists(fullpath_asset))
                input_waypointName = name;

        }

        [MenuItem(CommonDefine.ProjectName + "/Custom Editor/Create Waypoints Mesh Generator")]
        public static void CreateMeshGenerator()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == CommonDefine.InGameScene
                || UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == CommonDefine.OutGameScene)
            {
                Debug.Log("<color=red>Invalid Scene!! Waypoint Generator are set in seperate maps not on ingame or outgame scene</color>");
                return;
            }

            GameObject go = new GameObject("Waypoint Mesh Baker");
            go.AddComponent<WaypointMeshGenerator>();
            Selection.activeGameObject = go;
        }

        public override void OnInspectorGUI()
        {
            if (Application.isPlaying)
                return;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Inset Waypoint Name: ");
            input_waypointName = GUILayout.TextField(string.IsNullOrEmpty(input_waypointName) ? "waypoint mesh" : input_waypointName);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            overrideSavedFiles = GUILayout.Toggle(overrideSavedFiles, "Override Saved Files");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            generateOnlyNormalTypeWaypoints = GUILayout.Toggle(generateOnlyNormalTypeWaypoints, "Generate Only Normal Waypoints");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            deleteFiles = GUILayout.Toggle(deleteFiles, "Delete Files When Removing Mesh");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            insertDefaultTexture = GUILayout.Toggle(insertDefaultTexture, "Auto Material Texture");
            GUILayout.EndHorizontal();


            if (GUILayout.Button("Generate Road Mesh", GUILayout.Height(30)))
            {
                GenerateWaypointMesh_Editor();
            }

            if (GUILayout.Button("Remove Road Mesh", GUILayout.Height(30)))
            {
                DeleteWaypointMesh_Editor();
            }

            GUILayout.Space(50);

            if (meshGenerator != null && meshGenerator.roadMesh != null && meshGenerator.roadMesh.generatedMeshWaypoints != null && meshGenerator.roadMesh.generatedMeshWaypoints.Count > 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Inset Waypoint Boundary Name: ");
                input_waypointBoundaryName = GUILayout.TextField(string.IsNullOrEmpty(input_waypointBoundaryName) ? "waypoint boundary mesh" : input_waypointBoundaryName);
                GUILayout.EndHorizontal();

                if (GUILayout.Button("Generate Boundary Mesh", GUILayout.Height(30)))
                {
                    GenerateBoundaryMesh_Editor();
                }

                if (GUILayout.Button("Remove Boundary Mesh", GUILayout.Height(30)))
                {
                    DeleteBoundaryMesh_Editor();
                }
            }
        }

        private void OnSceneGUI()
        {
            if (Application.isPlaying)
                return;

            GUILayout.BeginArea(new Rect(100, 25, Screen.width - 800, Screen.height - 200));

            if (GUILayout.Button("Generate Road Mesh"))
            {
                GenerateWaypointMesh_Editor();
            }

            GUILayout.EndArea();
        }


        public void GenerateWaypointMesh_Editor()
        {
            if (waypointsGroup == null)
            {
                Debug.Log("error...! waypointsGroup is null");
                return;
            }

            if (waypointsGroup.IsWaypointValidAndSet() == false)
            {
                Debug.Log("<color=red>Error...! waypoints is not set....! Finish your waypoint first</color>");
                return;
            }

            if (meshGenerator == null)
            {
                Debug.Log("error...! meshGenerator is null");
                return;
            }

            if (input_waypointName == "waypoint mesh") //기본값인 경우
            {
                input_waypointName = "waypoint_mesh_" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            }

            var mesh = meshGenerator.GenerateWaypointMesh(waypointsGroup.allWaypoints, input_waypointName, generateOnlyNormalTypeWaypoints);

            if (mesh != null)
            {
                string path = "Assets/3.Art/3D/GeneratedMesh/";
                string fullpath_asset = path + mesh.name + ".asset";
                string fullpath_prefab = path + mesh.name + ".prefab";
                string fullpath_mat = path + mesh.name + ".mat";

                if (overrideSavedFiles == false && System.IO.File.Exists(fullpath_asset))
                {
                    Debug.Log("Same File Exits!!!!!!! Not Saved...");
                    return;
                }

                AssetDatabase.CreateAsset(mesh, fullpath_asset);
                AssetDatabase.SaveAssets();

                var shader = Shader.Find("Shader Graphs/" + CommonDefine.ShaderName_DTRBasicLitShader);
                Material mat = new Material(shader);
                if (insertDefaultTexture)
                {
                    Texture texture = (Texture)AssetDatabase.LoadAssetAtPath("Assets/3.Art/3D/GeneratedMesh/waypoint_map_texture.psd", typeof(Texture));
                    if (texture != null)
                        mat.SetTexture("_Base_Map_Texture", texture);
                }
                AssetDatabase.CreateAsset(mat, fullpath_mat);
                if (meshGenerator != null && meshGenerator.roadMesh.meshRenderer != null)
                    meshGenerator.roadMesh.meshRenderer.material = mat;


                bool prefabSuccess = false;
                PrefabUtility.SaveAsPrefabAsset(meshGenerator.roadMesh.gameObject, fullpath_prefab, out prefabSuccess);

                if (prefabSuccess == true)
                    Debug.Log("Map Mesh is generated and saved at :" + path);

                SaveScene();

            }
            else
                Debug.Log("Error.... no mesh generated!");
        }


        private void DeleteWaypointMesh_Editor()
        {
            var meshName = "";
            if (meshGenerator != null && meshGenerator.roadMesh != null && meshGenerator.roadMesh.meshFilter != null && meshGenerator.roadMesh.meshFilter.sharedMesh != null) 
            {
                meshName = meshGenerator.roadMesh.meshFilter.sharedMesh.name;
            }

            if (string.IsNullOrEmpty(meshName))
            {
                return;
            }

            string path = "Assets/3.Art/3D/GeneratedMesh/";
            string fullpath_asset = path + meshName + ".asset";
            string fullpath_prefab = path + meshName + ".prefab";
            string fullpath_mat = path + meshName + ".mat";

            if (deleteFiles)
            {
                if (System.IO.File.Exists(fullpath_asset))
                    System.IO.File.Delete(fullpath_asset);

                if (System.IO.File.Exists(fullpath_prefab))
                    System.IO.File.Delete(fullpath_prefab);

                if (System.IO.File.Exists(fullpath_mat))
                    System.IO.File.Delete(fullpath_mat);

                meshGenerator.roadMesh.meshFilter.sharedMesh = null;
                meshGenerator.roadMesh.meshRenderer.material = null;

                Debug.Log("Map mesh and files deleted!!");
            }
            else
            {
                meshGenerator.roadMesh.meshFilter.sharedMesh = null;
                meshGenerator.roadMesh.meshRenderer.material = null;

                Debug.Log("Map mesh deleted!!");
            }

            AssetDatabase.Refresh();
            SaveScene();

        }

        private void GenerateBoundaryMesh_Editor()
        {
            if (waypointsGroup == null)
            {
                Debug.Log("error...! waypointsGroup is null");
                return;
            }

            if (waypointsGroup.IsWaypointValidAndSet() == false)
            {
                Debug.Log("<color=red>Error...! waypoints is not set....! Finish your waypoint first</color>");
                return;
            }

            if (meshGenerator == null)
            {
                Debug.Log("error...! meshGenerator is null");
                return;
            }

            if (input_waypointBoundaryName == "waypoint boundary mesh") //기본값인 경우
            {
                input_waypointBoundaryName = "waypoint_mesh_" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "_boundary";
            }

            if (meshGenerator.roadMesh == null)
            {
                Debug.Log("<color=red>Error...! meshGenerator.roadMesh is null !! set your road first!!!</color>");
                return;
            }

            var mesh = meshGenerator.GenerateWaypointBoundaryMesh(input_waypointBoundaryName);

            if (mesh != null)
            {
                string path = "Assets/3.Art/3D/GeneratedMesh/";
                string fullpath_asset = path + mesh.name + ".asset";
                string fullpath_prefab = path + mesh.name + ".prefab";
                string fullpath_mat = path + mesh.name + ".mat";

                if (overrideSavedFiles == false && System.IO.File.Exists(fullpath_asset))
                {
                    Debug.Log("Same File Exits!!!!!!! Not Saved...");
                    return;
                }

                AssetDatabase.CreateAsset(mesh, fullpath_asset);
                AssetDatabase.SaveAssets();

                var shader = Shader.Find("Shader Graphs/" + CommonDefine.ShaderName_DTRBasicLitShader);
                Material mat = new Material(shader);
                if (insertDefaultTexture)
                {
                    Texture texture = (Texture)AssetDatabase.LoadAssetAtPath("Assets/3.Art/3D/GeneratedMesh/waypoint_boundary_texture.tif", typeof(Texture));
                    if (texture != null)
                        mat.SetTexture("_Base_Map_Texture", texture);
                }
                AssetDatabase.CreateAsset(mat, fullpath_mat);
                if (meshGenerator != null && meshGenerator.boundaryMesh.meshRenderer != null)
                    meshGenerator.boundaryMesh.meshRenderer.material = mat;


                bool prefabSuccess = false;
                PrefabUtility.SaveAsPrefabAsset(meshGenerator.boundaryMesh.gameObject, fullpath_prefab, out prefabSuccess);

                if (prefabSuccess == true)
                    Debug.Log("Map Boundary Mesh is generated and saved at :" + path);

                SaveScene();

            }
            else
                Debug.Log("Error.... no mesh generated!");

        }

        private void DeleteBoundaryMesh_Editor()
        {
            var meshName = "";
            if (meshGenerator != null && meshGenerator.boundaryMesh.meshFilter != null && meshGenerator.boundaryMesh.meshFilter.sharedMesh != null)
            {
                meshName = meshGenerator.boundaryMesh.meshFilter.sharedMesh.name;
            }

            if (string.IsNullOrEmpty(meshName))
            {
                return;
            }

            string path = "Assets/3.Art/3D/GeneratedMesh/";
            string fullpath_asset = path + meshName + ".asset";
            string fullpath_prefab = path + meshName + ".prefab";
            string fullpath_mat = path + meshName + ".mat";

            if (deleteFiles)
            {
                if (System.IO.File.Exists(fullpath_asset))
                    System.IO.File.Delete(fullpath_asset);

                if (System.IO.File.Exists(fullpath_prefab))
                    System.IO.File.Delete(fullpath_prefab);

                if (System.IO.File.Exists(fullpath_mat))
                    System.IO.File.Delete(fullpath_mat);

                meshGenerator.boundaryMesh.meshFilter.sharedMesh = null;
                meshGenerator.boundaryMesh.meshRenderer.material = null;

                Debug.Log("Map mesh and files deleted!!");
            }
            else
            {
                meshGenerator.boundaryMesh.meshFilter.sharedMesh = null;
                meshGenerator.boundaryMesh.meshRenderer.material = null;

                Debug.Log("Map mesh deleted!!");
            }

            AssetDatabase.Refresh();
            SaveScene();
        }
        
        
        private void SaveScene()
        {
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            Debug.Log("Saved Scene");
        }
    }

#endif
}