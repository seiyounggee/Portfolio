using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using static InGameManager;
using PNIX.ReferenceTable;
using System.Resources;

public partial class DataManager : MonoSingleton<DataManager>, IEventBus<DataManager.EventBusKey>
{
    FirebaseApp app;
    [ReadOnly] public bool isFirebaseConnected = false;
    [ReadOnly] private string currentUserId = "";

    #region Event Bus Pattern
    public enum EventBusKey
    {
        None, 
    }
    static Dictionary<EventBusKey, List<Action>> events => IEventBus<EventBusKey>.events;

    public void SubscribeEvent(EventBusKey key, Action ac)
    {
        if (events.ContainsKey(key) == false)
        {
            var l = new List<Action>();
            l.Add(ac);
            events.Add(key, l);
        }
        else
        {
            events[key].Add(ac);
        }
    }

    public void UnSubscribeEvent(EventBusKey key, System.Action ac)
    {
        if (events.ContainsKey(key) == true)
        {
            var l = events[key];
            if (l != null)
            {
                foreach (var i in l)
                {
                    if (i.Equals(ac))
                    {
                        l.Remove(i);
                        break;
                    }
                }
            }
        }
    }

    public void UnSubscribeAllEvent(EventBusKey key)
    {
        if (events.ContainsKey(key) == true)
            events.Remove(key);
    }

    public void ExcecuteEvent(EventBusKey key)
    {
        if (events.ContainsKey(key))
        {
            foreach (var i in events[key])
                i?.Invoke();
        }
    }
    #endregion

    public void Initialize()
    {
        isFirebaseConnected = false;
        currentUserId = "";

        isPnixReferenceDataLoaded = false;
    }

    public void SaveUserData()
    { 
    
    }



    public T GetCommonConfig<T>(string _key)
    {
        var data = CReferenceManager.Instance.FindRefCommonConfig(_key);
        if (data != null)
        {
            return ToType<T>(data.value);
        }
        else
        {
            Debug.LogError("GetCommonConfig() _key Error Key=" + _key);
            return default(T);
        }
    }

    public T GetGameConfig<T>(string _key)
    {
        var data = CReferenceManager.Instance.FindRefGameConfig(_key);
        if (data != null)
        {
            return ToType<T>(data.value);
        }
        else
        {
            Debug.LogError("GetGameConfig() _key Error Key=" + _key);
            return default(T);
        }
    }

    public static T ToType<T>(string value)
    {
        object parsedValue = default(T);
        Type type = typeof(T);

        try
        {
            parsedValue = Convert.ChangeType(value, type, System.Globalization.CultureInfo.InvariantCulture);
        }
        catch
        {
        }

        return (T)parsedValue;
    }



}
