using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WayPointSystem;
using System.Linq;


namespace WayPointSystem
{
    public class WaypointMeshGenerator : MonoBehaviour
    {
        public WaypointMesh_Road roadMesh = null;
        public WaypointMesh_Boundary boundaryMesh = null;

        private void Awake()
        {

        }

        public enum Floor { Top = 0, Bottom = 1 }

        public Mesh GenerateWaypointMesh(List<List<Waypoint>> wayList, string meshName = "", bool onlyNomralType = true)
        {
            if (roadMesh == null)
            {
                var roadMeshGo = new GameObject();
                roadMesh = roadMeshGo.AddComponent<WaypointMesh_Road>();
                roadMeshGo.transform.parent = this.transform;
                roadMeshGo.name = "Waypoint Mesh";
            }

            roadMesh.SetRoad();
            roadMesh.generatedMeshWaypoints.Clear();

            List<List<List<Waypoint>>> newWaypoints = new List<List<List<Waypoint>>>();
            newWaypoints.Add(new List<List<Waypoint>>()); //���� 
            newWaypoints.Add(new List<List<Waypoint>>()); //�Ʒ���

            int centerIndex = 3;
            int generatedFloors = 2;

            //���̿� waypoint �߰����� (mesh ������...!)
            for (int floor = 0; floor < generatedFloors; floor++)
            {
                //deep copy ����....
                var deepCopyWayList = new List<List<Waypoint>>();

                foreach (var w in wayList)
                {
                    var tempList = new List<Waypoint>();
                    foreach (var p in w)
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

                        tempList.Add(wp);
                    }
                    deepCopyWayList.Add(tempList);
                }

                for (int i = 0; i < deepCopyWayList.Count; i++)
                {
                    if (i < centerIndex)
                    {
                        newWaypoints[floor].Add(deepCopyWayList[i]);

                        if (true)
                        {
                            var additionalWaypoints = deepCopyWayList[i].ConvertAll(x => new Waypoint());
                            for (int j = 0; j < additionalWaypoints.Count; j++)
                            {
                                Vector3 newPos = (deepCopyWayList[i][j].GetPosition() + deepCopyWayList[i + 1][j].GetPosition()) / 2;

                                if (IsValidWayPoint(deepCopyWayList[i + 1][j].currentWayPointType) == true
                                    && IsValidWayPoint(deepCopyWayList[i][j].currentWayPointType) == false
                                    && i == centerIndex - 1)
                                {
                                    var dir = (deepCopyWayList[i][j].GetPosition() - deepCopyWayList[i + 1][j].GetPosition()).normalized;
                                    newPos = deepCopyWayList[i + 1][j].GetPosition() + dir * CommonDefine.DEFAULT_LANE_BY_LANE_DIST / 2f;
                                }

                                newPos.y = deepCopyWayList[i][j].GetPosition().y;
                                additionalWaypoints[j].SetPosition(newPos, PositionConstraint.XYZ); ;

                                if (deepCopyWayList[i + 1][j].currentWayPointType != Waypoint.WayPointType.Normal)
                                {
                                    additionalWaypoints[j].currentWayPointType = deepCopyWayList[i][j].currentWayPointType;
                                }
                            }
                            newWaypoints[floor].Add(additionalWaypoints);
                        }
                    }
                    else if (i == centerIndex)
                    {
                        newWaypoints[floor].Add(deepCopyWayList[i]);
                    }
                    else if (i > centerIndex)
                    {
                        if (true)
                        {
                            var additionalWaypoints = deepCopyWayList[i - 1].ConvertAll(x => new Waypoint());
                            for (int j = 0; j < additionalWaypoints.Count; j++)
                            {
                                Vector3 newPos = (deepCopyWayList[i - 1][j].GetPosition() + deepCopyWayList[i][j].GetPosition()) / 2;

                                if (IsValidWayPoint(deepCopyWayList[i - 1][j].currentWayPointType) == true
                                && IsValidWayPoint(deepCopyWayList[i][j].currentWayPointType) == false
                                && i == centerIndex + 1)
                                {
                                    var dir = (deepCopyWayList[i][j].GetPosition() - deepCopyWayList[i - 1][j].GetPosition()).normalized;
                                    newPos = deepCopyWayList[i - 1][j].GetPosition() + dir * CommonDefine.DEFAULT_LANE_BY_LANE_DIST / 2f;
                                }

                                newPos.y = deepCopyWayList[i - 1][j].GetPosition().y;
                                additionalWaypoints[j].SetPosition(newPos);

                                if (deepCopyWayList[i][j].currentWayPointType != Waypoint.WayPointType.Normal)
                                {
                                    additionalWaypoints[j].currentWayPointType = deepCopyWayList[i - 1][j].currentWayPointType;
                                }
                            }
                            newWaypoints[floor].Add(additionalWaypoints);
                        }
                        newWaypoints[floor].Add(deepCopyWayList[i]);
                    }
                }
            }

            for (int floor = 0; floor < generatedFloors; floor++)
            {
                for (int i = newWaypoints[floor].Count - 1; i >= 0; i--)
                {
                    for (int j = 0; j < newWaypoints[floor][i].Count; j++)
                    {
                        if (floor == (int)Floor.Top)
                        {
                            var pos = newWaypoints[floor][i][j].GetPosition();
                            newWaypoints[floor][i][j].SetPosition(pos);

                        }
                        else if (floor == (int)Floor.Bottom)
                        {
                            var pos = newWaypoints[floor][i][j].GetPosition();
                            pos += Vector3.down * 2.5f;
                            newWaypoints[floor][i][j].SetPosition(pos);
                        }
                    }
                }
            }


            var mesh = new Mesh
            {
                name = meshName
            };

            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();


            int cnt = 0;
            int uvCount = 0;
            for (int floor = 0; floor < generatedFloors; floor++)
            {
                for (int i = newWaypoints[floor].Count - 1; i >= 0; i--)
                {
                    for (int j = 0; j < newWaypoints[floor][i].Count; j++)
                    {
                        if (floor == (int)Floor.Top)
                        {
                            vertices.Add(newWaypoints[floor][i][j].GetPosition());
                            normals.Add(Vector3.up);

                            if (uvCount >= quadUVCoord.Length)
                                uvCount = 0; //0�� �ʱ�ȭ
                            uvs.Add(quadUVCoord[uvCount++]);

                        }
                        else if (floor == (int)Floor.Bottom)
                        {
                            vertices.Add(newWaypoints[floor][i][j].GetPosition());
                            normals.Add(Vector3.down);

                            if (uvCount >= quadUVCoord.Length)
                                uvCount = 0; //0�� �ʱ�ȭ
                            uvs.Add(quadUVCoord[uvCount++]);

                        }

                        newWaypoints[floor][i][j].verticesNumber = cnt;
                        ++cnt;
                    }
                }
            }


            for (int floor = 0; floor < generatedFloors; floor++)
            {
                for (int i = 0; i < newWaypoints[floor].Count - 1; i++)
                {
                    for (int j = 0; j < newWaypoints[floor][i].Count; j++)
                    {
                        if (j < newWaypoints[floor][i].Count - 1)
                        {
                            if (onlyNomralType) //Valid�� ������ ������
                            {
                                //���
                                #region Flat Part 
                                if (IsValidWayPoint(newWaypoints[floor][i][j].currentWayPointType)
                                    && IsValidWayPoint(newWaypoints[floor][i][j + 1].currentWayPointType)
                                    && IsValidWayPoint(newWaypoints[floor][i + 1][j].currentWayPointType))
                                {
                                    if (floor == (int)Floor.Top)
                                    {
                                        triangles.Add(newWaypoints[floor][i][j].verticesNumber);
                                        triangles.Add(newWaypoints[floor][i][j + 1].verticesNumber);
                                        triangles.Add(newWaypoints[floor][i + 1][j].verticesNumber);
                                    }
                                    else if (floor == (int)Floor.Bottom)
                                    {
                                        triangles.Add(newWaypoints[floor][i + 1][j].verticesNumber);
                                        triangles.Add(newWaypoints[floor][i][j + 1].verticesNumber);
                                        triangles.Add(newWaypoints[floor][i][j].verticesNumber);
                                    }
                                }
                                else
                                {
                                    if (IsValidWayPoint(newWaypoints[floor][i][j].currentWayPointType)
                                        && IsValidWayPoint(newWaypoints[floor][i + 1][j + 1].currentWayPointType)
                                        && IsValidWayPoint(newWaypoints[floor][i + 1][j].currentWayPointType))
                                    {
                                        if (floor == (int)Floor.Top)
                                        {
                                            triangles.Add(newWaypoints[floor][i][j].verticesNumber);
                                            triangles.Add(newWaypoints[floor][i + 1][j + 1].verticesNumber);
                                            triangles.Add(newWaypoints[floor][i + 1][j].verticesNumber);
                                        }
                                        else if (floor == (int)Floor.Bottom)
                                        {
                                            triangles.Add(newWaypoints[floor][i + 1][j].verticesNumber);
                                            triangles.Add(newWaypoints[floor][i + 1][j + 1].verticesNumber);
                                            triangles.Add(newWaypoints[floor][i][j].verticesNumber);
                                        }
                                    }
                                }

                                if (IsValidWayPoint(newWaypoints[floor][i + 1][j + 1].currentWayPointType)
                                    && IsValidWayPoint(newWaypoints[floor][i + 1][j].currentWayPointType)
                                    && IsValidWayPoint(newWaypoints[floor][i][j + 1].currentWayPointType))
                                {
                                    if (floor == (int)Floor.Top)
                                    {
                                        triangles.Add(newWaypoints[floor][i + 1][j + 1].verticesNumber);
                                        triangles.Add(newWaypoints[floor][i + 1][j].verticesNumber);
                                        triangles.Add(newWaypoints[floor][i][j + 1].verticesNumber);
                                    }
                                    else if (floor == (int)Floor.Bottom)
                                    {
                                        triangles.Add(newWaypoints[floor][i][j + 1].verticesNumber);
                                        triangles.Add(newWaypoints[floor][i + 1][j].verticesNumber);
                                        triangles.Add(newWaypoints[floor][i + 1][j + 1].verticesNumber);
                                    }
                                }
                                else
                                {
                                    if (IsValidWayPoint(newWaypoints[floor][i + 1][j + 1].currentWayPointType)
                                        && IsValidWayPoint(newWaypoints[floor][i][j + 1].currentWayPointType)
                                        && IsValidWayPoint(newWaypoints[floor][i][j].currentWayPointType))
                                    {
                                        if (floor == (int)Floor.Top)
                                        {
                                            triangles.Add(newWaypoints[floor][i][j].verticesNumber);
                                            triangles.Add(newWaypoints[floor][i][j + 1].verticesNumber);
                                            triangles.Add(newWaypoints[floor][i + 1][j + 1].verticesNumber);
                                        }
                                        else if (floor == (int)Floor.Bottom)
                                        {
                                            triangles.Add(newWaypoints[floor][i + 1][j + 1].verticesNumber);
                                            triangles.Add(newWaypoints[floor][i][j + 1].verticesNumber);
                                            triangles.Add(newWaypoints[floor][i][j].verticesNumber);
                                        }
                                    }
                                }
                                #endregion

                                //����
                                #region Side Part
                                if (floor == (int)Floor.Top) //Top�������� ������
                                {
                                    if (IsValidWayPoint(newWaypoints[floor][i][j].currentWayPointType)
                                        && IsValidWayPoint(newWaypoints[floor][i][j + 1].currentWayPointType))
                                    {
                                        if (i > 0 && i < newWaypoints[floor].Count - 1) //���� point ����
                                        {
                                            //���� �¿���� normal�� ��� ���ʿ��� triangle�� ��������� ����
                                            if (IsValidWayPoint(newWaypoints[floor][i - 1][j].currentWayPointType)
                                                && IsValidWayPoint(newWaypoints[floor][i + 1][j].currentWayPointType))
                                            {
                                                //���� �¿���� normal�� ��� ���ʿ��� triangle�� ��������� ����
                                            }
                                            else
                                            {
                                                //���̴� �κ� üũ
                                                bool validLeft = IsValidWayPoint(newWaypoints[floor][i - 1][j + 1].currentWayPointType);
                                                bool validFront = IsValidWayPoint(newWaypoints[floor][i][j + 1].currentWayPointType);
                                                bool validRight = IsValidWayPoint(newWaypoints[floor][i + 1][j + 1].currentWayPointType);

                                                //�о����� ����
                                                if (validLeft && validRight)
                                                {
                                                    //�������� ���̴� �κ�
                                                    if (validLeft)
                                                    {
                                                        //���� ���� ���� ��������
                                                        triangles.Add(newWaypoints[floor][i - 1][j + 1].verticesNumber);
                                                        triangles.Add(newWaypoints[floor][i][j].verticesNumber);
                                                        triangles.Add(newWaypoints[floor + 1][i - 1][j + 1].verticesNumber);

                                                        triangles.Add(newWaypoints[floor][i][j].verticesNumber);
                                                        triangles.Add(newWaypoints[floor + 1][i][j].verticesNumber);
                                                        triangles.Add(newWaypoints[floor + 1][i - 1][j + 1].verticesNumber);

                                                        //������ ���� ���ص� ��������
                                                        triangles.Add(newWaypoints[floor + 1][i - 1][j + 1].verticesNumber);
                                                        triangles.Add(newWaypoints[floor][i][j].verticesNumber);
                                                        triangles.Add(newWaypoints[floor][i - 1][j + 1].verticesNumber);

                                                        triangles.Add(newWaypoints[floor + 1][i - 1][j + 1].verticesNumber);
                                                        triangles.Add(newWaypoints[floor + 1][i][j].verticesNumber);
                                                        triangles.Add(newWaypoints[floor][i][j].verticesNumber);
                                                    }

                                                    //���������� ���̴� �κ�
                                                    if (validRight)
                                                    {
                                                        //���� ���� ���� ��������
                                                        triangles.Add(newWaypoints[floor][i + 1][j + 1].verticesNumber);
                                                        triangles.Add(newWaypoints[floor][i][j].verticesNumber);
                                                        triangles.Add(newWaypoints[floor + 1][i + 1][j + 1].verticesNumber);

                                                        triangles.Add(newWaypoints[floor][i][j].verticesNumber);
                                                        triangles.Add(newWaypoints[floor + 1][i][j].verticesNumber);
                                                        triangles.Add(newWaypoints[floor + 1][i + 1][j + 1].verticesNumber);

                                                        //������ ���� ���ص� ��������
                                                        triangles.Add(newWaypoints[floor + 1][i + 1][j + 1].verticesNumber);
                                                        triangles.Add(newWaypoints[floor][i][j].verticesNumber);
                                                        triangles.Add(newWaypoints[floor][i + 1][j + 1].verticesNumber);

                                                        triangles.Add(newWaypoints[floor + 1][i + 1][j + 1].verticesNumber);
                                                        triangles.Add(newWaypoints[floor + 1][i][j].verticesNumber);
                                                        triangles.Add(newWaypoints[floor][i][j].verticesNumber);
                                                    }
                                                }
                                                else //������ ���̴� �ʴ� �κ�
                                                {
                                                    //���� ���� ���� ��������
                                                    triangles.Add(newWaypoints[floor][i][j + 1].verticesNumber);
                                                    triangles.Add(newWaypoints[floor][i][j].verticesNumber);
                                                    triangles.Add(newWaypoints[floor + 1][i][j].verticesNumber);

                                                    triangles.Add(newWaypoints[floor][i][j + 1].verticesNumber);
                                                    triangles.Add(newWaypoints[floor + 1][i][j].verticesNumber);
                                                    triangles.Add(newWaypoints[floor + 1][i][j + 1].verticesNumber);


                                                    //������ ���� ���ص� ��������
                                                    triangles.Add(newWaypoints[floor + 1][i][j].verticesNumber);
                                                    triangles.Add(newWaypoints[floor][i][j].verticesNumber);
                                                    triangles.Add(newWaypoints[floor][i][j + 1].verticesNumber);

                                                    triangles.Add(newWaypoints[floor + 1][i][j + 1].verticesNumber);
                                                    triangles.Add(newWaypoints[floor + 1][i][j].verticesNumber);
                                                    triangles.Add(newWaypoints[floor][i][j + 1].verticesNumber);
                                                }
                                            }
                                        }
                                        //��Ÿ ���� ����
                                        else
                                        {
                                            //���� ���� ���� ��������
                                            triangles.Add(newWaypoints[floor][i][j + 1].verticesNumber);
                                            triangles.Add(newWaypoints[floor][i][j].verticesNumber);
                                            triangles.Add(newWaypoints[floor + 1][i][j].verticesNumber);

                                            triangles.Add(newWaypoints[floor][i][j + 1].verticesNumber);
                                            triangles.Add(newWaypoints[floor + 1][i][j].verticesNumber);
                                            triangles.Add(newWaypoints[floor + 1][i][j + 1].verticesNumber);

                                            //������ ���� ���ص� ��������
                                            triangles.Add(newWaypoints[floor + 1][i][j].verticesNumber);
                                            triangles.Add(newWaypoints[floor][i][j].verticesNumber);
                                            triangles.Add(newWaypoints[floor][i][j + 1].verticesNumber);

                                            triangles.Add(newWaypoints[floor + 1][i][j + 1].verticesNumber);
                                            triangles.Add(newWaypoints[floor + 1][i][j].verticesNumber);
                                            triangles.Add(newWaypoints[floor][i][j + 1].verticesNumber);
                                        }
                                    }
                                    else if (IsValidWayPoint(newWaypoints[floor][i][j].currentWayPointType))
                                    {
                                        if (i > 0 && i < newWaypoints[floor].Count - 1)
                                        {
                                            bool validLeft = IsValidWayPoint(newWaypoints[floor][i - 1][j + 1].currentWayPointType);
                                            bool validFront = IsValidWayPoint(newWaypoints[floor][i][j + 1].currentWayPointType);
                                            bool validRight = IsValidWayPoint(newWaypoints[floor][i + 1][j + 1].currentWayPointType);
                                            if (validFront == false)
                                            {
                                                //�������� ����
                                                if (validFront == false && (validLeft == true || validRight == true))
                                                {
                                                    if (validLeft)
                                                    {
                                                        //���� ���� ���� ��������
                                                        triangles.Add(newWaypoints[floor][i][j].verticesNumber);
                                                        triangles.Add(newWaypoints[floor][i - 1][j + 1].verticesNumber);
                                                        triangles.Add(newWaypoints[floor + 1][i - 1][j + 1].verticesNumber);

                                                        triangles.Add(newWaypoints[floor + 1][i - 1][j + 1].verticesNumber);
                                                        triangles.Add(newWaypoints[floor + 1][i][j].verticesNumber);
                                                        triangles.Add(newWaypoints[floor][i][j].verticesNumber);

                                                        //������ ���� ���ص� ��������
                                                        triangles.Add(newWaypoints[floor + 1][i - 1][j + 1].verticesNumber);
                                                        triangles.Add(newWaypoints[floor][i - 1][j + 1].verticesNumber);
                                                        triangles.Add(newWaypoints[floor][i][j].verticesNumber);

                                                        triangles.Add(newWaypoints[floor][i][j].verticesNumber);
                                                        triangles.Add(newWaypoints[floor + 1][i][j].verticesNumber);
                                                        triangles.Add(newWaypoints[floor + 1][i - 1][j + 1].verticesNumber);
                                                    }

                                                    if (validRight)
                                                    {
                                                        //���� ���� ���� ��������
                                                        triangles.Add(newWaypoints[floor][i][j].verticesNumber);
                                                        triangles.Add(newWaypoints[floor][i + 1][j + 1].verticesNumber);
                                                        triangles.Add(newWaypoints[floor + 1][i + 1][j + 1].verticesNumber);

                                                        triangles.Add(newWaypoints[floor + 1][i + 1][j + 1].verticesNumber);
                                                        triangles.Add(newWaypoints[floor + 1][i][j].verticesNumber);
                                                        triangles.Add(newWaypoints[floor][i][j].verticesNumber);

                                                        //������ ���� ���ص� ��������
                                                        triangles.Add(newWaypoints[floor + 1][i + 1][j + 1].verticesNumber);
                                                        triangles.Add(newWaypoints[floor][i + 1][j + 1].verticesNumber);
                                                        triangles.Add(newWaypoints[floor][i][j].verticesNumber);

                                                        triangles.Add(newWaypoints[floor][i][j].verticesNumber);
                                                        triangles.Add(newWaypoints[floor + 1][i][j].verticesNumber);
                                                        triangles.Add(newWaypoints[floor + 1][i + 1][j + 1].verticesNumber);
                                                    }
                                                }
                                            }
                                        }
                                    }


                                    if (i < newWaypoints[floor].Count - 1)
                                    {
                                        if (IsValidWayPoint(newWaypoints[floor][i][j].currentWayPointType)
                                            && IsValidWayPoint(newWaypoints[floor][i + 1][j].currentWayPointType))
                                        {
                                            if (j < newWaypoints[floor][i].Count - 1 && j > 0)
                                            {
                                                if (IsValidWayPoint(newWaypoints[floor][i][j - 1].currentWayPointType)
                                                 && IsValidWayPoint(newWaypoints[floor][i][j].currentWayPointType)
                                                 && IsValidWayPoint(newWaypoints[floor][i][j + 1].currentWayPointType))
                                                {
                                                    //���� �յڰ� normal�� ��� ���ʿ��� triangle�� ��������� ����
                                                }
                                                else
                                                {
                                                    //���� ����
                                                    triangles.Add(newWaypoints[floor][i + 1][j].verticesNumber);
                                                    triangles.Add(newWaypoints[floor][i][j].verticesNumber);
                                                    triangles.Add(newWaypoints[floor + 1][i][j].verticesNumber);

                                                    triangles.Add(newWaypoints[floor][i + 1][j].verticesNumber);
                                                    triangles.Add(newWaypoints[floor + 1][i][j].verticesNumber);
                                                    triangles.Add(newWaypoints[floor + 1][i + 1][j].verticesNumber);

                                                    //���� ����
                                                    triangles.Add(newWaypoints[floor + 1][i][j].verticesNumber);
                                                    triangles.Add(newWaypoints[floor][i][j].verticesNumber);
                                                    triangles.Add(newWaypoints[floor][i + 1][j].verticesNumber);

                                                    triangles.Add(newWaypoints[floor + 1][i + 1][j].verticesNumber);
                                                    triangles.Add(newWaypoints[floor + 1][i][j].verticesNumber);
                                                    triangles.Add(newWaypoints[floor][i + 1][j].verticesNumber);
                                                }
                                            }
                                            else
                                            {
                                                //���� ����
                                                triangles.Add(newWaypoints[floor][i + 1][j].verticesNumber);
                                                triangles.Add(newWaypoints[floor][i][j].verticesNumber);
                                                triangles.Add(newWaypoints[floor + 1][i][j].verticesNumber);

                                                triangles.Add(newWaypoints[floor][i + 1][j].verticesNumber);
                                                triangles.Add(newWaypoints[floor + 1][i][j].verticesNumber);
                                                triangles.Add(newWaypoints[floor + 1][i + 1][j].verticesNumber);

                                                //���� ����
                                                triangles.Add(newWaypoints[floor + 1][i][j].verticesNumber);
                                                triangles.Add(newWaypoints[floor][i][j].verticesNumber);
                                                triangles.Add(newWaypoints[floor][i + 1][j].verticesNumber);

                                                triangles.Add(newWaypoints[floor + 1][i + 1][j].verticesNumber);
                                                triangles.Add(newWaypoints[floor + 1][i][j].verticesNumber);
                                                triangles.Add(newWaypoints[floor][i + 1][j].verticesNumber);
                                            }
                                        }
                                    }
                                }

                                #endregion



                            }
                            else  //waypoint ���� ������� ������
                            {
                                if (floor == (int)Floor.Top)
                                {
                                    triangles.Add(newWaypoints[floor][i][j].verticesNumber);
                                    triangles.Add(newWaypoints[floor][i][j + 1].verticesNumber);
                                    triangles.Add(newWaypoints[floor][i + 1][j].verticesNumber);

                                    triangles.Add(newWaypoints[floor][i + 1][j + 1].verticesNumber);
                                    triangles.Add(newWaypoints[floor][i + 1][j].verticesNumber);
                                    triangles.Add(newWaypoints[floor][i][j + 1].verticesNumber);
                                }
                                else if (floor == (int)Floor.Bottom)
                                {
                                    triangles.Add(newWaypoints[floor][i + 1][j].verticesNumber);
                                    triangles.Add(newWaypoints[floor][i][j + 1].verticesNumber);
                                    triangles.Add(newWaypoints[floor][i][j].verticesNumber);

                                    triangles.Add(newWaypoints[floor][i][j + 1].verticesNumber);
                                    triangles.Add(newWaypoints[floor][i + 1][j].verticesNumber);
                                    triangles.Add(newWaypoints[floor][i + 1][j + 1].verticesNumber);
                                }
                            }
                        }
                        else //������ �κ� ���ۺκ��̶� �̾�����...
                        {
                            if (onlyNomralType) //Valid�� ������ ������
                            {
                                #region Top & Bottom
                                if (floor == (int)Floor.Top)
                                {
                                    if (IsValidWayPoint(newWaypoints[floor][i][j].currentWayPointType)
                                    && IsValidWayPoint(newWaypoints[floor][i][0].currentWayPointType)
                                    && IsValidWayPoint(newWaypoints[floor][i + 1][0].currentWayPointType))
                                    {
                                        triangles.Add(newWaypoints[floor][i][j].verticesNumber);
                                        triangles.Add(newWaypoints[floor][i][0].verticesNumber);
                                        triangles.Add(newWaypoints[floor][i + 1][j].verticesNumber);

                                        triangles.Add(newWaypoints[floor][i + 1][j].verticesNumber);
                                        triangles.Add(newWaypoints[floor][i][0].verticesNumber);
                                        triangles.Add(newWaypoints[floor][i + 1][0].verticesNumber);
                                    }
                                }
                                else if (floor == (int)Floor.Bottom)
                                {
                                    if (IsValidWayPoint(newWaypoints[floor][i + 1][j].currentWayPointType)
                                        && IsValidWayPoint(newWaypoints[floor][i][0].currentWayPointType)
                                        && IsValidWayPoint(newWaypoints[floor][i][j].currentWayPointType))
                                    {
                                        triangles.Add(newWaypoints[floor][i + 1][j].verticesNumber);
                                        triangles.Add(newWaypoints[floor][i][0].verticesNumber);
                                        triangles.Add(newWaypoints[floor][i][j].verticesNumber);

                                        triangles.Add(newWaypoints[floor][i + 1][0].verticesNumber);
                                        triangles.Add(newWaypoints[floor][i][0].verticesNumber);
                                        triangles.Add(newWaypoints[floor][i + 1][j].verticesNumber);
                                    }
                                }
                                #endregion


                                #region Side
                                if (floor == (int)Floor.Top) //Top�������� ������
                                {
                                    if (IsValidWayPoint(newWaypoints[floor][i][j].currentWayPointType)
                                        && IsValidWayPoint(newWaypoints[floor][i][0].currentWayPointType))
                                    {
                                        if (i > 0 && i < newWaypoints[floor].Count - 1)
                                        {
                                            if (IsValidWayPoint(newWaypoints[floor][i - 1][j].currentWayPointType)
                                                && IsValidWayPoint(newWaypoints[floor][i + 1][j].currentWayPointType))
                                            {

                                            }
                                            else
                                            {
                                                //���� ���� ���� ��������
                                                triangles.Add(newWaypoints[floor][i][0].verticesNumber);
                                                triangles.Add(newWaypoints[floor][i][j].verticesNumber);
                                                triangles.Add(newWaypoints[floor + 1][i][j].verticesNumber);

                                                triangles.Add(newWaypoints[floor][i][0].verticesNumber);
                                                triangles.Add(newWaypoints[floor + 1][i][j].verticesNumber);
                                                triangles.Add(newWaypoints[floor + 1][i][0].verticesNumber);


                                                //������ ���� ���ص� ��������
                                                triangles.Add(newWaypoints[floor + 1][i][j].verticesNumber);
                                                triangles.Add(newWaypoints[floor][i][j].verticesNumber);
                                                triangles.Add(newWaypoints[floor][i][0].verticesNumber);

                                                triangles.Add(newWaypoints[floor + 1][i][0].verticesNumber);
                                                triangles.Add(newWaypoints[floor + 1][i][j].verticesNumber);
                                                triangles.Add(newWaypoints[floor][i][0].verticesNumber);
                                            }
                                        }
                                        else
                                        {
                                            //���� ���� ���� ��������
                                            triangles.Add(newWaypoints[floor][i][0].verticesNumber);
                                            triangles.Add(newWaypoints[floor][i][j].verticesNumber);
                                            triangles.Add(newWaypoints[floor + 1][i][j].verticesNumber);

                                            triangles.Add(newWaypoints[floor][i][0].verticesNumber);
                                            triangles.Add(newWaypoints[floor + 1][i][j].verticesNumber);
                                            triangles.Add(newWaypoints[floor + 1][i][0].verticesNumber);


                                            //������ ���� ���ص� ��������
                                            triangles.Add(newWaypoints[floor + 1][i][j].verticesNumber);
                                            triangles.Add(newWaypoints[floor][i][j].verticesNumber);
                                            triangles.Add(newWaypoints[floor][i][0].verticesNumber);

                                            triangles.Add(newWaypoints[floor + 1][i][0].verticesNumber);
                                            triangles.Add(newWaypoints[floor + 1][i][j].verticesNumber);
                                            triangles.Add(newWaypoints[floor][i][0].verticesNumber);
                                        }
                                    }
                                }
                                #endregion
                            }
                            else
                            {
                                if (floor == (int)Floor.Top)
                                {
                                    triangles.Add(newWaypoints[floor][i][j].verticesNumber);
                                    triangles.Add(newWaypoints[floor][i][0].verticesNumber);
                                    triangles.Add(newWaypoints[floor][i + 1][j].verticesNumber);

                                    triangles.Add(newWaypoints[floor][i + 1][j].verticesNumber);
                                    triangles.Add(newWaypoints[floor][i][0].verticesNumber);
                                    triangles.Add(newWaypoints[floor][i + 1][0].verticesNumber);
                                }
                                else if (floor == (int)Floor.Bottom)
                                {
                                    triangles.Add(newWaypoints[floor][i + 1][j].verticesNumber);
                                    triangles.Add(newWaypoints[floor][i][0].verticesNumber);
                                    triangles.Add(newWaypoints[floor][i][j].verticesNumber);

                                    triangles.Add(newWaypoints[floor][i + 1][0].verticesNumber);
                                    triangles.Add(newWaypoints[floor][i][0].verticesNumber);
                                    triangles.Add(newWaypoints[floor][i + 1][j].verticesNumber);
                                }
                            }
                        }
                    }
                }
            }



            mesh.vertices = vertices.ToArray();
            mesh.normals = normals.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();

            roadMesh.meshFilter.mesh = mesh;
            roadMesh.meshCollider.sharedMesh = mesh;
            roadMesh.generatedMeshWaypoints = newWaypoints;

            return mesh;
        }

        public bool IsValidWayPoint(Waypoint.WayPointType type)
        {
            if (type == Waypoint.WayPointType.Normal || type == Waypoint.WayPointType.OnlyFront)
                return true;
            else
                return false;
        }

        public bool IsBlockedWayPoint(Waypoint.WayPointType type)
        {
            if (type == Waypoint.WayPointType.Blocked)
                return true;
            else
                return false;
        }


        Vector2[] quadUVCoord = new Vector2[]
        {
            /*
            new Vector2(0f, 0f),
            new Vector2(0f, 1f),
            new Vector2(1f, 0f),
            new Vector2(1f, 1f),
            */
            
            new Vector2(0f, 1f),
            new Vector2(1f, 0f),
            new Vector2(0f, 0f),
            new Vector2(1f, 1f),
            
        };










        public Mesh GenerateWaypointBoundaryMesh(string meshName = "")
        {
            if (boundaryMesh == null)
            {
                var boundaryMeshGo = new GameObject();
                boundaryMesh = boundaryMeshGo.AddComponent<WaypointMesh_Boundary>();
                boundaryMeshGo.transform.parent = this.transform;
                boundaryMeshGo.name = "Waypoint Boundary Mesh";
            }

            boundaryMesh.SetBoundary();

            if (roadMesh.generatedMeshWaypoints == null || roadMesh.generatedMeshWaypoints.Count <= 0)
                return null;

            var mesh = new Mesh
            {
                name = meshName
            };

            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();

            List<CombineInstance> combineList = new List<CombineInstance>();

            int counter = 0;
            for (int floor = 0; floor < roadMesh.generatedMeshWaypoints.Count; floor++)
            {
                if (floor == (int)Floor.Top)
                {
                    for (int lane = 1; lane < roadMesh.generatedMeshWaypoints[floor].Count - 1; lane++)
                    {
                        for (int index = 0; index < roadMesh.generatedMeshWaypoints[floor][lane].Count; index++)
                        {
                            int nextIndex;
                            if (index != roadMesh.generatedMeshWaypoints[floor][lane].Count - 1)
                                nextIndex = index + 1;
                            else
                                nextIndex = 0;

                            int prevIndex;
                            if (index != 0)
                                prevIndex = index - 1;
                            else
                                prevIndex = roadMesh.generatedMeshWaypoints[floor][lane].Count - 1;

                            //����
                            if (IsValidWayPoint(roadMesh.generatedMeshWaypoints[floor][lane][index].currentWayPointType)
                                && IsValidWayPoint(roadMesh.generatedMeshWaypoints[floor][lane][nextIndex].currentWayPointType)
                                && (IsBlockedWayPoint(roadMesh.generatedMeshWaypoints[floor][lane - 1][index].currentWayPointType) == true || IsBlockedWayPoint(roadMesh.generatedMeshWaypoints[floor][lane + 1][index].currentWayPointType) == true)
                                && (IsBlockedWayPoint(roadMesh.generatedMeshWaypoints[floor][lane - 1][nextIndex].currentWayPointType) == true || IsBlockedWayPoint(roadMesh.generatedMeshWaypoints[floor][lane + 1][nextIndex].currentWayPointType) == true)
                                )
                            {
                                Vector3 pos1 = roadMesh.generatedMeshWaypoints[floor][lane][index].GetPosition();
                                Vector3 pos2 = roadMesh.generatedMeshWaypoints[floor][lane][nextIndex].GetPosition();

                                float length = Vector3.Distance(roadMesh.generatedMeshWaypoints[floor][lane][index].GetPosition(), roadMesh.generatedMeshWaypoints[floor][lane][nextIndex].GetPosition());
                                float width = 0.5f;
                                float height = 0.7f;


                                vertices.AddRange(CubeMesh_vertices(pos1, pos2, length, width, height));
                                triangles.AddRange(CubeMesh_triangles(counter));
                                uvs.AddRange(CubeMesh_uv());

                                counter += CubMesh_vertices_count;
                            }


                            //����
                            if (lane < roadMesh.generatedMeshWaypoints[floor].Count - 1
                                && IsValidWayPoint(roadMesh.generatedMeshWaypoints[floor][lane][index].currentWayPointType)
                                && IsValidWayPoint(roadMesh.generatedMeshWaypoints[floor][lane + 1][index].currentWayPointType)
                                && (IsBlockedWayPoint(roadMesh.generatedMeshWaypoints[floor][lane][nextIndex].currentWayPointType) == true)
                                && (IsBlockedWayPoint(roadMesh.generatedMeshWaypoints[floor][lane + 1][nextIndex].currentWayPointType) == true)
                                )
                            {
                                Vector3 pos1 = roadMesh.generatedMeshWaypoints[floor][lane][index].GetPosition();
                                Vector3 pos2 = roadMesh.generatedMeshWaypoints[floor][lane + 1][index].GetPosition();

                                float length = Vector3.Distance(roadMesh.generatedMeshWaypoints[floor][lane][index].GetPosition(), roadMesh.generatedMeshWaypoints[floor][lane][nextIndex].GetPosition());
                                float width = 0.5f;
                                float height = 0.7f;


                                vertices.AddRange(CubeMesh_vertices(pos1, pos2, length, width, height));
                                triangles.AddRange(CubeMesh_triangles(counter));
                                uvs.AddRange(CubeMesh_uv());

                                counter += CubMesh_vertices_count;
                            }

                            //�ĸ�
                            if (lane < roadMesh.generatedMeshWaypoints[floor].Count - 1
                                && IsValidWayPoint(roadMesh.generatedMeshWaypoints[floor][lane][index].currentWayPointType)
                                && IsValidWayPoint(roadMesh.generatedMeshWaypoints[floor][lane + 1][index].currentWayPointType)
                                && IsValidWayPoint(roadMesh.generatedMeshWaypoints[floor][lane][nextIndex].currentWayPointType)
                                && (IsBlockedWayPoint(roadMesh.generatedMeshWaypoints[floor][lane][prevIndex].currentWayPointType) == true)
                                && (IsBlockedWayPoint(roadMesh.generatedMeshWaypoints[floor][lane + 1][prevIndex].currentWayPointType) == true)
                                )
                            {
                                Vector3 pos1 = roadMesh.generatedMeshWaypoints[floor][lane][index].GetPosition();
                                Vector3 pos2 = roadMesh.generatedMeshWaypoints[floor][lane + 1][index].GetPosition();

                                float length = Vector3.Distance(roadMesh.generatedMeshWaypoints[floor][lane][index].GetPosition(), roadMesh.generatedMeshWaypoints[floor][lane][nextIndex].GetPosition());
                                float width = 0.5f;
                                float height = 0.7f;


                                vertices.AddRange(CubeMesh_vertices(pos1, pos2, length, width, height));
                                triangles.AddRange(CubeMesh_triangles(counter));
                                uvs.AddRange(CubeMesh_uv());

                                counter += CubMesh_vertices_count;
                            }



                            //������ �밢��
                            if (lane < roadMesh.generatedMeshWaypoints[floor].Count - 1
                                && IsValidWayPoint(roadMesh.generatedMeshWaypoints[floor][lane][index].currentWayPointType)
                                && IsValidWayPoint(roadMesh.generatedMeshWaypoints[floor][lane + 1][nextIndex].currentWayPointType)
                                && (IsBlockedWayPoint(roadMesh.generatedMeshWaypoints[floor][lane][nextIndex].currentWayPointType) == true)
                                )
                            {
                                Vector3 pos1 = roadMesh.generatedMeshWaypoints[floor][lane][index].GetPosition();
                                Vector3 pos2 = roadMesh.generatedMeshWaypoints[floor][lane + 1][nextIndex].GetPosition();

                                float length = Vector3.Distance(roadMesh.generatedMeshWaypoints[floor][lane][index].GetPosition(), roadMesh.generatedMeshWaypoints[floor][lane][nextIndex].GetPosition());
                                float width = 0.5f;
                                float height = 0.7f;


                                vertices.AddRange(CubeMesh_vertices(pos1, pos2, length, width, height));
                                triangles.AddRange(CubeMesh_triangles(counter));
                                uvs.AddRange(CubeMesh_uv());

                                counter += CubMesh_vertices_count;
                            }

                            //���� �밢��
                            if (lane < roadMesh.generatedMeshWaypoints[floor].Count - 1
                                && IsValidWayPoint(roadMesh.generatedMeshWaypoints[floor][lane][index].currentWayPointType)
                                && IsValidWayPoint(roadMesh.generatedMeshWaypoints[floor][lane - 1][nextIndex].currentWayPointType)
                                && (IsBlockedWayPoint(roadMesh.generatedMeshWaypoints[floor][lane][nextIndex].currentWayPointType) == true)
                                )
                            {
                                Vector3 pos1 = roadMesh.generatedMeshWaypoints[floor][lane][index].GetPosition();
                                Vector3 pos2 = roadMesh.generatedMeshWaypoints[floor][lane - 1][nextIndex].GetPosition();

                                float length = Vector3.Distance(roadMesh.generatedMeshWaypoints[floor][lane][index].GetPosition(), roadMesh.generatedMeshWaypoints[floor][lane][nextIndex].GetPosition());
                                float width = 0.5f;
                                float height = 0.7f;


                                vertices.AddRange(CubeMesh_vertices(pos1, pos2, length, width, height));
                                triangles.AddRange(CubeMesh_triangles(counter));
                                uvs.AddRange(CubeMesh_uv());

                                counter += CubMesh_vertices_count;
                            }


                            //������ �밢�� v2
                            if (lane < roadMesh.generatedMeshWaypoints[floor].Count - 1
                                && IsValidWayPoint(roadMesh.generatedMeshWaypoints[floor][lane][index].currentWayPointType)
                                && IsValidWayPoint(roadMesh.generatedMeshWaypoints[floor][lane + 1][nextIndex].currentWayPointType)
                                && (IsBlockedWayPoint(roadMesh.generatedMeshWaypoints[floor][lane + 1][index].currentWayPointType) == true)
                                )
                            {
                                Vector3 pos1 = roadMesh.generatedMeshWaypoints[floor][lane][index].GetPosition();
                                Vector3 pos2 = roadMesh.generatedMeshWaypoints[floor][lane + 1][nextIndex].GetPosition();

                                float length = Vector3.Distance(roadMesh.generatedMeshWaypoints[floor][lane][index].GetPosition(), roadMesh.generatedMeshWaypoints[floor][lane][nextIndex].GetPosition());
                                float width = 0.5f;
                                float height = 0.7f;


                                vertices.AddRange(CubeMesh_vertices(pos1, pos2, length, width, height));
                                triangles.AddRange(CubeMesh_triangles(counter));
                                uvs.AddRange(CubeMesh_uv());

                                counter += CubMesh_vertices_count;
                            }

                            //���� �밢�� v2
                            if (lane < roadMesh.generatedMeshWaypoints[floor].Count - 1
                                && IsValidWayPoint(roadMesh.generatedMeshWaypoints[floor][lane][index].currentWayPointType)
                                && IsValidWayPoint(roadMesh.generatedMeshWaypoints[floor][lane - 1][nextIndex].currentWayPointType)
                                && (IsBlockedWayPoint(roadMesh.generatedMeshWaypoints[floor][lane - 1][index].currentWayPointType) == true)
                                )
                            {
                                Vector3 pos1 = roadMesh.generatedMeshWaypoints[floor][lane][index].GetPosition();
                                Vector3 pos2 = roadMesh.generatedMeshWaypoints[floor][lane - 1][nextIndex].GetPosition();

                                float length = Vector3.Distance(roadMesh.generatedMeshWaypoints[floor][lane][index].GetPosition(), roadMesh.generatedMeshWaypoints[floor][lane][nextIndex].GetPosition());
                                float width = 0.5f;
                                float height = 0.7f;


                                vertices.AddRange(CubeMesh_vertices(pos1, pos2, length, width, height));
                                triangles.AddRange(CubeMesh_triangles(counter));
                                uvs.AddRange(CubeMesh_uv());

                                counter += CubMesh_vertices_count;
                            }
                        }
                    }
                }
                else
                    continue;
            }

            mesh.vertices = vertices.ToArray();
            mesh.normals = normals.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();

            boundaryMesh.meshFilter.mesh = mesh;
            boundaryMesh.meshCollider.sharedMesh = mesh;
            return mesh;
        }


        private List<Vector3> CubeMesh_vertices(Vector3 position1, Vector3 position2, float length, float width, float height)
        {
            Vector3 dir = (position2 - position1).normalized;
            Vector3 orthoDir = Vector3.Cross(dir, Vector3.up);

            float halfWidth = width / 2f;

            List<Vector3> vertices = new List<Vector3>  {

                    new Vector3(position1.x - orthoDir.x * halfWidth, position1.y - orthoDir.y * halfWidth, position1.z - orthoDir.z * halfWidth),

                    new Vector3(position1.x + orthoDir.x * halfWidth, position1.y + orthoDir.y * halfWidth, position1.z + orthoDir.z * halfWidth),
                    new Vector3(position1.x + orthoDir.x * halfWidth, position1.y + orthoDir.y * halfWidth + height, position1.z + orthoDir.z * halfWidth),

                    new Vector3(position1.x - orthoDir.x * halfWidth, position1.y - orthoDir.y * halfWidth + height, position1.z - orthoDir.z * halfWidth),
                    new Vector3(position2.x - orthoDir.x * halfWidth, position2.y - orthoDir.y * halfWidth, position2.z - orthoDir.z * halfWidth),

                    new Vector3(position2.x + orthoDir.x * halfWidth, position2.y + orthoDir.y * halfWidth, position2.z + orthoDir.z * halfWidth),
                    new Vector3(position2.x + orthoDir.x * halfWidth, position2.y + orthoDir.y * halfWidth + height, position2.z + orthoDir.z * halfWidth),

                    new Vector3(position2.x - orthoDir.x * halfWidth, position2.y - orthoDir.y * halfWidth + height, position2.z - orthoDir.z * halfWidth),


                    /*    
                    new Vector3(-1f, 1f, -1f),
                    new Vector3(1f, 1f, -1f),
                    new Vector3(1f, -1f, -1f),
                    new Vector3(-1f, -1f, -1f),
                    new Vector3(-1f, 1f, 1f),
                    new Vector3(1f, 1f, 1f),
                    new Vector3(1f, -1f, 1f),
                    new Vector3(-1f, -1f, 1f),
                    */
            };

            return vertices;
        }

        private int CubMesh_vertices_count = 8;

        private List<int> CubeMesh_triangles(int counter)
        {
            List<int> triangles = new List<int> 
            {
                counter + 0, counter + 1, counter + 2, counter + 2, counter + 3, counter + 0,    // front face
                counter + 1, counter + 5, counter + 6, counter + 6, counter + 2, counter + 1,    // right face
                counter + 5, counter + 4, counter + 7, counter + 7, counter + 6, counter + 5,    // back face
                counter + 4, counter + 0, counter + 3, counter + 3, counter + 7, counter + 4,    // left face
                counter + 4, counter + 5, counter + 1, counter + 1, counter + 0, counter + 4,    // top face
                counter + 3, counter + 2, counter + 6, counter + 6, counter + 7, counter + 3     // bottom face
            };

            return triangles;
        }

        private List<Vector2> CubeMesh_uv()
        {
            List<Vector2> uv = new List<Vector2>
            {
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(1, 0),
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(1, 0),
                new Vector2(0, 0)
            };

            return uv;
        }
    }

}