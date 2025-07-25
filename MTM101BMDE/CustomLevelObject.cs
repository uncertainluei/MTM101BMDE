﻿using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI
{

    internal class FakeGameInit : GameInitializer
    {
        public override void Initialize()
        {
            // how about NO
        }
    }

    public static class CustomLevelObjectExtensions
    {
        /// <summary>
        /// Get all the level objects referenced by the specified SceneObject.
        /// </summary>
        /// <param name="sceneObj"></param>
        /// <returns></returns>
        public static CustomLevelObject[] GetCustomLevelObjects(this SceneObject sceneObj)
        {
            List<CustomLevelObject> levelObjects = new List<CustomLevelObject>();
            if (sceneObj.randomizedLevelObject != null)
            {
                if (sceneObj.randomizedLevelObject.Length > 0)
                {
                    for (int i = 0; i < sceneObj.randomizedLevelObject.Length; i++)
                    {
                        levelObjects.Add((CustomLevelObject)sceneObj.randomizedLevelObject[i].selection);
                    }
                    return levelObjects.ToArray();
                }
            }
            if (sceneObj.levelObject != null)
            {
                levelObjects.Add((CustomLevelObject)sceneObj.levelObject);
            }


            return levelObjects.ToArray();
        }


        readonly static FieldInfo _sceneObject = AccessTools.Field(typeof(GameInitializer), "sceneObject");
        readonly static MethodInfo _GetControlledRandomLevelData = AccessTools.Method(typeof(GameInitializer), "GetControlledRandomLevelData");

        /// <summary>
        /// Returns the current level object for the specified SceneObject
        /// CoreGameManager MUST exist for this to not return null.
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        public static CustomLevelObject GetCurrentCustomLevelObject(this SceneObject me)
        {
            if (Singleton<CoreGameManager>.Instance == null) return null;
            if (me.randomizedLevelObject.Length == 0) return (CustomLevelObject)me.levelObject;
            FakeGameInit fInit = MTM101BaldiDevAPI.Instance.fakeInit;
            _sceneObject.SetValue(fInit, me);
            CustomLevelObject clm = (CustomLevelObject)_GetControlledRandomLevelData.Invoke(fInit, null);
            return clm;
        }
    }


    /// <summary>
    /// A custom version of the LevelObject class, currently doesn't contain much else but it serves as a good base to make extending level generator functionality easy in the future.
    /// </summary>
    public class CustomLevelObject : LevelObject
    {

        internal Dictionary<string, Dictionary<string, object>> customModDatas = new Dictionary<string, Dictionary<string, object>>();

        public object GetCustomModValue(string modUUID, string key)
        {
            if (!customModDatas.ContainsKey(modUUID)) return null;
            if (!customModDatas[modUUID].ContainsKey(key)) return null;
            return customModDatas[modUUID][key];
        }

        public object GetCustomModValue(PluginInfo pluginInfo, string key)
        {
            return GetCustomModValue(pluginInfo.Metadata.GUID, key);
        }

        /// <summary>
        /// Makes a clone of this CustomLevelObject, preserving the custom mod data.
        /// </summary>
        /// <returns></returns>
        public CustomLevelObject MakeClone()
        {
            CustomLevelObject obj = CustomLevelObject.Instantiate(this);
            foreach (KeyValuePair<string, Dictionary<string, object>> kvp in customModDatas)
            {
                foreach (KeyValuePair<string, object> internalKvp in kvp.Value)
                {
                    obj.SetCustomModValue(kvp.Key, internalKvp.Key, internalKvp.Value);
                }
            }
            return obj;
        }

        /// <summary>
        /// Adds the specified key/value to the CustomLevelObject, allowing for storing extra, mod specific generator settings.
        /// </summary>
        /// <param name="pluginInfo"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetCustomModValue(PluginInfo pluginInfo, string key, object value)
        {
            string modUUID = pluginInfo.Metadata.GUID;
            if (!customModDatas.ContainsKey(modUUID))
            {
                customModDatas.Add(modUUID, new Dictionary<string, object>());
            }
            if (customModDatas[modUUID].ContainsKey(key))
            {
                customModDatas[modUUID][key] = value;
                return;
            }
            customModDatas[modUUID].Add(key, value);
        }

        public void SetCustomModValue(string modUUID, string key, object value)
        {
            if (!customModDatas.ContainsKey(modUUID))
            {
                customModDatas.Add(modUUID, new Dictionary<string, object>());
            }
            if (customModDatas[modUUID].ContainsKey(key))
            {
                customModDatas[modUUID][key] = value;
                return;
            }
            customModDatas[modUUID].Add(key, value);
        }

        //hacky way of adding the Obsolete tag, but it works?
        [Obsolete("BB+ no longer uses .previousLevels change .previousLevels in the SceneObject instead!", true)]
        public new LevelObject[] previousLevels = new LevelObject[0];

        [Obsolete("BB+ no longer uses .items, use .forcedItems or .potentialItems instead!", true)]
        public new WeightedItemObject[] items;

        [Obsolete("BB+ no longer uses .totalShopItems, change totalShopItems in the SceneObject instead!", true)]
        public new int totalShopItems;

        [Obsolete("BB+ no longer uses .shopItems, change shopItems in the SceneObject instead!", true)]
        public new WeightedItemObject[] shopItems;

        [Obsolete("BB+ no longer uses .mapPrice, change mapPrice in the SceneObject instead!", true)]
        public new int mapPrice;

        [Obsolete("BB+ no longer uses .potentialNPCs, change potentialNPCs in the SceneObject instead!", true)]
        public new List<WeightedNPC> potentialNPCs;
    }
}
