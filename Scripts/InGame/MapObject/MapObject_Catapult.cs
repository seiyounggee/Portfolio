using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapObject_Catapult : MonoBehaviour
{
    [ReadOnly] public int id = -1;

    public Transform endPoint = null;

    public TriggerChecker triggerChecker = null;

    private NetworkInGameRPCManager rpcManager => PhotonNetworkManager.Instance.MyNetworkInGameRPCManager;

    [System.Serializable]
    public class HitDelayInfo
    {
        public PlayerMovement pm;
        public float counter = 0;
        public bool isHitAvailable { get { if (counter <= 0) return true; else return false; } }
    }

    [HideInInspector] private List<HitDelayInfo> hitDelayList = new List<HitDelayInfo>();

    private IEnumerator moveTrigger = null;

    Trajectory trajectory = new Trajectory();
    [Range(2f, 89f)] public float degree = 45f;
    [Range(1f, 4f)] public float speed = 2f;
    [Range(0f, 30f)] public float coolTime = 1f;
    [Range(0f, 30f)] public float startDelay = 0f;
    [Range(0f, 5f)] public float groundStayTime = 1f;

    public bool isMoving = false;

    private void Awake()
    {
        if (triggerChecker != null)
        {
            triggerChecker._OnTriggerEnter = Event_OnTriggerEnter;
            triggerChecker._OnTriggerStay = Event_OnTriggerStay;
            triggerChecker._OnTriggerExit = Event_OnTriggerExit;
        }
    }

    private void FixedUpdate()
    {
        if (IsActiveCondition())
        {
            if (hitDelayList != null && hitDelayList.Count > 0)
                hitDelayList.Clear();
            return;
        }

        if (hitDelayList != null && hitDelayList.Count > 0)
        {
            foreach (var i in hitDelayList)
            {
                if (i.isHitAvailable == false)
                    i.counter -= Time.fixedDeltaTime;
            }
        }
    }

    public void SetData(int id)
    {
        this.id = id;

    }

    public void ActivateCatapult()
    {
        if (triggerChecker != null)
            UtilityCoroutine.StartCoroutine(ref moveTrigger, MoveTrigger(), this);
    }

    private IEnumerator MoveTrigger()
    {
        var path = CalcPath();
        if (path == null || path.Count <= 0)
            yield break;

        triggerChecker.transform.position = path[0];
        triggerChecker.gameObject.SafeSetActive(true);

        isMoving = true;

        int index = 0;
        while (true)
        {
            triggerChecker.transform.position = path[index];

            yield return new WaitForFixedUpdate();
            index += (int)speed;

            if (index >= path.Count - 1)
            {
                InGameManager.Instance.ActivateFX(InGameManager.FX_Type.Hit_03, triggerChecker.transform.position);
                break;
            }

        }

        InGameManager.Instance.ActivateFX(InGameManager.FX_Type.Hit_02, triggerChecker.transform.position);

        yield return new WaitForSecondsRealtime(groundStayTime);

        triggerChecker.transform.position = path[0];
        isMoving = false;
    }

    private void Event_OnTriggerEnter(Collider other)
    {
        if (IsActiveCondition() == false)
            return;


        if (other.CompareTag(CommonDefine.TAG_NetworkPlayer_TriggerChecker))
        {
            var cc = other.GetComponent<PlayerTriggerChecker>();
            if (cc != null && cc.currentCheckParts == PlayerTriggerChecker.CheckParts.Body)
            {
                var pm = cc.pm;
                if (pm == null)
                    return;

                if (pm.isStunned || pm.isFlipped || pm.isOutOfBoundary)
                    return;

                var p = hitDelayList.Find(x => x.pm.Equals(pm));
                if (p != null && p.isHitAvailable == false)
                    return;

                float coolTime = 3f;
                if (pm.IsMine)
                {
                    if (pm.isShield)
                        rpcManager.RPC_StopShield(pm.networkPlayerID);

                    rpcManager.RPC_GetFlipped(pm.networkPlayerID, pm.transform.position, pm.transform.rotation);
                }

                //남의 hit 정보는 정확하지 않을수 있음...
                if (p == null)
                    hitDelayList.Add(new HitDelayInfo() { pm = pm, counter = coolTime });
                else
                    p.counter = coolTime;
            }
        }
    }

    private void Event_OnTriggerStay(Collider other)
    {
        if (IsActiveCondition() == false)
            return;
    }

    private void Event_OnTriggerExit(Collider other)
    {
        if (IsActiveCondition() == false)
            return;
    }

    private bool IsActiveCondition()
    {
        if (PhaseManager.Instance.CurrentPhase != CommonDefine.Phase.InGame)
            return false;

        if (InGameManager.Instance.gameState == InGameManager.GameState.Initialize
            || InGameManager.Instance.gameState == InGameManager.GameState.IsGameReady
            || InGameManager.Instance.gameState == InGameManager.GameState.StartCountDown
            || InGameManager.Instance.gameState == InGameManager.GameState.EndGame)
            return false;

        return true;
    }

    private List<Vector3> CalcPath()
    {
        List<Vector3> pathList = new List<Vector3>();


        if (endPoint != null && degree > 0)
        {
            Vector3 start = transform.position;
            Vector3 end = endPoint.position;


            float flightTime = Trajectory.GetFlightTime(start, end, degree, 0f);
            var vel = Trajectory.GetInitVelocityByAngle(start, end, degree, 0f);
            var pos = start;
            float time = 0f;

            while (true)
            {
                if (time + Time.fixedDeltaTime >= flightTime)
                    break;

                time += Time.fixedDeltaTime;
                vel += Physics.gravity * Time.fixedDeltaTime;
                pos += vel * Time.fixedDeltaTime;

                pathList.Add(pos);
            }

        }


        return pathList;
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (true)
        {
            if (transform != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position, 1.3f);
            }

            if (endPoint != null)
            {
                SphereCollider col = null;
                if (triggerChecker != null)
                    col = triggerChecker.GetComponent<SphereCollider>();

                Gizmos.color = Color.magenta;

                if (col == null)
                    Gizmos.DrawWireSphere(endPoint.transform.position, 1.3f);
                else
                    Gizmos.DrawWireSphere(endPoint.transform.position, col.radius * col.transform.localScale.magnitude * 0.5f);
            }

            if (CalcPath() != null && CalcPath().Count > 0)
            {
                var path = CalcPath();
                for (int i = 0; i < path.Count - 1; i++)
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawLine(path[i], path[i + 1]);
                }
            }
        }
    }

#endif




    public class Trajectory
    {
        public static Vector3 GetInitVelocity(Vector3 start_pos, Vector3 target_pos, float angle_degree)
        {
            float _rad = angle_degree * Mathf.Deg2Rad;

            float _dist = Vector2.Distance(new Vector2(start_pos.x, start_pos.z), new Vector2(target_pos.x, target_pos.z));
            float _y = start_pos.y - target_pos.y;

            float _speed = Mathf.Sqrt(-Physics.gravity.y * Mathf.Pow(_dist, 2.0f) / (2 * Mathf.Pow(Mathf.Cos(_rad), 2.0f) * (_dist * Mathf.Tan(_rad) + _y)));
            if (float.IsNaN(_speed) == true)
            {
                return Vector3.zero;
            }
            else
            {
                return (new Vector3(target_pos.x - start_pos.x, _dist * Mathf.Tan(_rad), target_pos.z - start_pos.z).normalized * _speed);
            }
        }


        public static Vector3 GetInitVelocityByAngle(Vector3 start_pos, Vector3 target_pos, float rot_pitch_degree, float rot_yaw_degree = 0.0f)
        {
            float _time = Trajectory.GetFlightTime(start_pos, target_pos, rot_pitch_degree, rot_yaw_degree);
            if (_time <= float.Epsilon)
            {
                return Vector3.zero;
            }

            return GetInitVelocity(_time, start_pos, target_pos, rot_yaw_degree);
        }

        public static Vector3 GetInitVelocity(float flight_time, Vector3 start_pos, Vector3 target_pos, float rot_yaw_degree)
        {
            Vector3 _additional_acc = Trajectory.GetAdditionalAcc((target_pos - start_pos).normalized, rot_yaw_degree);
            return Trajectory.GetInitVelocityByTime(start_pos, target_pos, Physics.gravity + _additional_acc, flight_time);
        }

        public static float GetFlightTime(Vector3 start_pos, Vector3 target_pos, float rot_pitch_degree, float rot_yaw_degree)
        {
            if (rot_pitch_degree < 0.0f || rot_pitch_degree >= 90.0f)
            {
                return 0.0f;
            }

            if (rot_yaw_degree >= 90.0f)
            {
                return 0.0f;
            }

            var _temp = start_pos;
            _temp.y = target_pos.y;
            var _dir = (target_pos - _temp).normalized;
            Quaternion _rot = Quaternion.identity;
            if (_dir != Vector3.zero)
                _rot = Quaternion.LookRotation(_dir, Vector3.up);

            var _matrix = Matrix4x4.TRS(_temp, _rot, Vector3.one);
            _matrix = _matrix.inverse;
            start_pos = _matrix.MultiplyPoint3x4(start_pos);
            target_pos = _matrix.MultiplyPoint3x4(target_pos);


            float _radian = rot_pitch_degree * Mathf.Deg2Rad;

            float _diff_z = Mathf.Abs(target_pos.z - start_pos.z);
            float _diff_y = Mathf.Abs(target_pos.y - start_pos.y);

            float _root_value = Mathf.Sqrt(_diff_y * _diff_y + _diff_z * _diff_z);
            float _init_vel = Mathf.Sqrt(-Physics.gravity.y * (_diff_y + _root_value));

            float _time = -2f * _init_vel * Mathf.Sin(_radian) / Physics.gravity.y;

            return _time;
        }

        public static Vector3 GetAdditionalAcc(Vector3 forward, float yaw_degree)
        {
            Vector3 _left = -Vector3.Cross(Vector3.up, forward);

            return _left * Mathf.Abs(Physics.gravity.y) * Mathf.Tan(yaw_degree * Mathf.Deg2Rad);
        }

        public static Vector3 GetUnityPhysicsAdjustableVelocity(Vector3 additional_acc, float fixed_time)
        {
            return (-0.5f * fixed_time * (Physics.gravity + additional_acc));
        }

        public static Vector3 GetInitVelocityByTime(Vector3 start_pos, Vector3 target_pos, Vector3 acc, float move_time)
        {
            return ((target_pos - start_pos) / move_time - 0.5f * move_time * acc);
        }

        public static float GetPeakHeight(Vector3 velocity)
        {
            //  float _angle_value = Mathf.Sin(pitch_degree * Mathf.Deg2Rad);
            //return (velocity.y * velocity.y * _angle_value * _angle_value) / (2.0f * Mathf.Abs(Physics.gravity.y));
            return (velocity.y * velocity.y) / (2.0f * Mathf.Abs(Physics.gravity.y));
        }

        //
        public static double GetMaxQuadricEquationRoot(double a, double b, double c)
        {
            double[] _values = CalcQuadricEquation(a, b, c);
            if (_values == null)
            {
                return double.NaN;
            }


            double _re = double.MinValue;
            for (int i = 0, _max = _values.Length; i < _max; ++i)
            {
                if (_values[i] > _re)
                {
                    _re = _values[i];
                }
            }

            return _re;
        }

        public static double[] CalcQuadricEquation(double a, double b, double c)
        {
            double[] _re = null;

            double _D = b * b - 4 * a * c;
            if (System.Math.Abs(_D) < double.Epsilon)
            {
                _re = new double[1];
                _re[0] = (-b) / (2 * a);
            }
            else if (_D > 0.0)
            {
                _re = new double[2];

                _re[0] = (-b - System.Math.Pow(_D, 0.5)) / (2 * a);
                _re[1] = (-b + System.Math.Pow(_D, 0.5)) / (2 * a);
            }
            else
            {
                // ....
            }

            return _re;
        }

        public static double CalcQuarticEquationScaleValue(double a, double b, double c, double d, double e, double fx, double fy)
        {
            // a (zx)^4 + b ( zx ) ^3 + c ( zx )^2 + d ( zx ) + e = 0
            // f(fx) = fy
            double[] _result = CalcQuarticEquation(
                a * System.Math.Pow(fx, 4), b * System.Math.Pow(fx, 3), c * System.Math.Pow(fx, 2), d * fx, -fy);

            double _re = double.NaN;
            for (int i = 0, _max = _result.Length; i < _max; ++i)
            {
                double _target = _result[i];

                if (_target < double.Epsilon)
                {
                    continue;
                }

                double _calced = a * System.Math.Pow(fx, 4) * System.Math.Pow(_target, 4) +
                                 b * System.Math.Pow(fx, 3) * System.Math.Pow(_target, 3) +
                                 c * System.Math.Pow(fx, 2) * System.Math.Pow(_target, 2) +
                                 d * fx * _target +
                                 e;
                if (System.Math.Abs(_calced - fy) < 0.001)
                {   // check differential equation...
                    double[] _cubics = CalcCubicEquation(
                                         a * System.Math.Pow(_target, 4) * 4,
                                         b * System.Math.Pow(_target, 3) * 3,
                                         c * System.Math.Pow(_target, 2) * 2,
                                         d * _target);

                    double _max_value = double.MinValue;
                    for (int j = 0, _maxj = _cubics.Length; j < _maxj; ++j)
                    {
                        if (double.IsNaN(_cubics[j]) == true)
                        {
                            continue;
                        }

                        if (_max_value < _cubics[j])
                        {
                            _max_value = _cubics[j];
                        }
                    }

                    if (_max_value > double.MinValue)
                    {
                        if (_max_value < fx)
                        {
                            _re = _target;
                        }
                    }
                }

            }

            return _re;
        }

        public static double[] CalcCubicEquation(double a, double b, double c, double d)
        {
            double _Delta = 18 * a * b * c * d - 4 * b * b * b * d + b * b * c * c - 4 * a * c * c * c - 27 * a * a * d * d;
            double _Delta0 = b * b - 3 * a * c;
            double _Delta1 = 2 * b * b * b - 9 * a * b * c + 27 * a * a * d;

            double[] _result = new double[3] { double.NaN, double.NaN, double.NaN };

            double _value = _Delta1 * _Delta1 - 4 * _Delta0 * _Delta0 * _Delta0;
            if (_Delta < 0)
            {
                double _A = CubicRoot(0.5 * (_Delta1 + System.Math.Sqrt(_value)));
                double _B = CubicRoot(0.5 * (_Delta1 - System.Math.Sqrt(_value)));

                _result[0] = -b / (3 * a) - 1 / (3 * a) * _A - 1 / (3 * a) * _B;

                // imaginary values...
            }
            else if (System.Math.Abs(_Delta) < double.Epsilon)
            {
                if (System.Math.Abs(_Delta0) < double.Epsilon)
                {
                    _result[0] = -b / (3 * a);
                }
                else
                {
                    _result[0] = (9 * a * d - b * c) / (2 * _Delta0);
                    _result[1] = (4 * a * b * c - 9 * a * a * d - b * b * b) / (a * _Delta0);
                }
            }
            else
            {
                Debug.LogError("Not Yet!!");
                // ToDo..
                // Get 3 real roots...

                // double _A = CubicRoot(0.5 * (_Delta1 + System.Math.Sqrt(_value)));
                // double _B = CubicRoot(0.5 * (_Delta1 - System.Math.Sqrt(_value)));
                //
                //
                // _result[0] = -b / (3 * a) - 1 / (3 * a) * _A - 1 / (3 * a) * _B;
            }

            return _result;
        }

        public static double[] CalcQuarticEquation(double a, double b, double c, double d, double e)
        {
            double _S = (b * b) / (4 * a * a) - (2 * c) / (3 * a);
            double _T = (b * b * b) / (4 * a * a * a) - (b * c) / (a * a) + (2 * d) / a;
            double _U = 72 * a * c * e - 27 * a * d * d - 27 * b * b * e + 9 * b * c * d - 2 * c * c * c;
            double _V = 12 * a * e - 3 * b * d + c * c;

            double _UV = System.Math.Pow(_U * _U - 4 * _V * _V * _V, 0.5);
            double _a = CubicRoot(_U + _UV);
            double _b = CubicRoot(_U - _UV);
            double _W = (_a + _b) / (3 * a * CubicRoot(2));

            double[] _result = new double[4];

            double _P0 = System.Math.Pow(_S - _W, 0.5);
            double _P1 = System.Math.Pow(2 * _S + _W - _T / _P0, 0.5);

            _result[0] = -b / (4 * a) + 0.5 * _P0 + 0.5 * _P1;
            _result[1] = -b / (4 * a) + 0.5 * _P0 - 0.5 * _P1;
            _result[2] = -b / (4 * a) - 0.5 * _P0 + 0.5 * _P1;
            _result[3] = -b / (4 * a) - 0.5 * _P0 - 0.5 * _P1;

            return _result;
        }

        public static double CubicRoot(double value)
        {
            if (value < 0)
            {
                return -System.Math.Pow(-value, 1 / 3f);
            }
            else
            {
                return System.Math.Pow(value, 1 / 3f);
            }
        }
    }
}
