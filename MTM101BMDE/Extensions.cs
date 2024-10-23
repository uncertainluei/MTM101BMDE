﻿using BepInEx;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI
{
    public static class Extensions
    {
        /// <summary>
        /// Convert the GameObject into a prefab by moving its transform inside an internal GameObject marked as HideAndDontSave. It is automatically done for NPCs made with NPCBuilders and Items made with ItemBuilder.
        /// Use this method in AssetsLoaded.
        /// </summary>
        /// <param name="me"></param>
        /// <param name="setActive">If true, then the GameObject will be set to active. The components code won't run anyways.</param>
        /// <exception cref="NullReferenceException"></exception>
        public static void ConvertToPrefab(this GameObject me, bool setActive)
        {
            if (MTM101BaldiDevAPI.PrefabSubObject == null)
            {
                throw new NullReferenceException("Attempted to ConvertToPrefab before AssetsLoaded!");
            }
            me.MarkAsNeverUnload();
            me.transform.SetParent(MTM101BaldiDevAPI.PrefabSubObject.transform);
            if (setActive)
            {
                me.SetActive(true);
            }
        }

        public static CustomLevelObject GetCustomLevelObject(this SceneObject sceneObj)
        {
            if (sceneObj.levelObject is CustomLevelObject)
            {
                return (CustomLevelObject)sceneObj.levelObject;
            }
            return null;
        }

        public static T GetOrAddComponent<T>(this GameObject me) where T : Component
        {
            T foundComponent = me.GetComponent<T>();
            if (foundComponent) return foundComponent;
            return me.AddComponent<T>();
        }

        static MethodInfo _EndTransition = AccessTools.Method(typeof(GlobalCam), "EndTransition");
        static FieldInfo _transitioner = AccessTools.Field(typeof(GlobalCam), "transitioner");
        /// <summary>
        /// Stops the currently running transition.
        /// </summary>
        /// <param name="me"></param>
        public static void StopCurrentTransition(this GlobalCam me)
        {
            IEnumerator transitioner = (IEnumerator)_transitioner.GetValue(me);
            if (transitioner == null) { return; }
            me.StopCoroutine(transitioner);
            _EndTransition.Invoke(me, null);
        }

        /// <summary>
        /// Repeatedly calls MoveNext until the IEnumerator is empty
        /// </summary>
        /// <param name="numerator"></param>
        /// <returns>All the things returned by the IEnumerator</returns>
        public static List<object> MoveUntilDone(this IEnumerator numerator)
        {
            List<object> returnValue = new List<object>();
            while (numerator.MoveNext())
            {
                returnValue.Add(numerator.Current);
            }
            return returnValue;
        }

        /// <summary>
        /// Makes an object never unload from memory.
        /// </summary>
        /// <param name="me"></param>
        public static void MarkAsNeverUnload(this UnityEngine.Object me)
        {
            if (!MTM101BaldiDevAPI.keepInMemory.Contains(me))
            {
                MTM101BaldiDevAPI.keepInMemory.Add(me);
            }
        }

        /// <summary>
        /// Allows an object unload from memory.
        /// </summary>
        /// <param name="me"></param>
        public static void RemoveUnloadMark(this UnityEngine.Object me)
        {
            MTM101BaldiDevAPI.keepInMemory.Remove(me);
        }

        /// <summary>
        /// Makes an object never unload from memory.
        /// </summary>
        /// <param name="me"></param>
        public static void MarkAsNeverUnload(this ScriptableObject me)
        {
            if (!MTM101BaldiDevAPI.keepInMemory.Contains(me))
            {
                MTM101BaldiDevAPI.keepInMemory.Add(me);
            }
        }

        /// <summary>
        /// Allows an object unload from memory.
        /// </summary>
        /// <param name="me"></param>
        public static void RemoveUnloadMark(this ScriptableObject me)
        {
            MTM101BaldiDevAPI.keepInMemory.Remove(me);
        }

        /// <summary>
        /// Get the index of the cell at the specified position, returns -1 if there is no cell with that position.
        /// </summary>
        /// <param name="me"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>The index in the cells array of the cell with the specific position, -1 if there is no cell in that position.</returns>
        public static int GetCellIndexAt(this RoomAsset me, int x, int y)
        {
            for (int i = 0; i < me.cells.Count; i++)
            {
                if ((me.cells[i].pos.x == x) && (me.cells[i].pos.z == y))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Gets the cell at the specified position, returns null if there is no cell at that position.
        /// </summary>
        /// <param name="me"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>The cell at the specified position or null if it does not exist.</returns>
        public static CellData GetCellAt(this RoomAsset me, int x, int y)
        {
            int index = me.GetCellIndexAt(x, y);
            if (index == -1) return null;
            return me.cells[index];
        }

        /// <summary>
        /// Sets the main texture of the material, uses the appropiate variable names for BB+ shaders.
        /// </summary>
        /// <param name="me"></param>
        /// <param name="texture"></param>
        public static void SetMainTexture(this Material me, Texture texture)
        {
            me.SetTexture("_MainTex", texture);
        }

        /// <summary>
        /// Sets the mask texture of the material.
        /// </summary>
        /// <param name="me"></param>
        /// <param name="texture"></param>
        public static void SetMaskTexture(this Material me, Texture texture)
        {
            me.SetTexture("_Mask", texture);
        }

        /// <summary>
        /// Applies the StandardDoorMats materials to the specified StandardDoor, optionally changing the mask.
        /// </summary>
        /// <param name="me"></param>
        /// <param name="materials"></param>
        /// <param name="mask"></param>
        public static void ApplyDoorMaterials(this StandardDoor me, StandardDoorMats materials, Material mask = null)
        {
            me.overlayShut[0] = materials.shut;
            me.overlayShut[1] = materials.shut;
            me.overlayOpen[0] = materials.open;
            me.overlayOpen[1] = materials.open;
            if (mask != null)
            {
                me.mask[0] = mask;
                me.mask[1] = mask;
            }
            me.UpdateTextures();
        }
    }
}

namespace MTM101BaldAPI.Registers
{
    public static class MetaExtensions
    {
        public static ItemMetaData AddMeta(this ItemObject me, BaseUnityPlugin plugin, ItemFlags flags)
        {
            ItemMetaData meta = new ItemMetaData(plugin.Info, me);
            meta.flags = flags;
            MTM101BaldiDevAPI.itemMetadata.Add(me, meta);
            return meta;
        }

        public static ItemMetaData AddMeta(this ItemObject me, ItemMetaData meta)
        {
            MTM101BaldiDevAPI.itemMetadata.Add(me, meta);
            return meta;
        }

        public static NPCMetadata AddMeta(this NPC me, BaseUnityPlugin plugin, NPCFlags flags)
        {
            NPCMetadata existingMeta = NPCMetaStorage.Instance.Find(x => x.character == me.Character);
            if (existingMeta != null)
            {
                MTM101BaldiDevAPI.Log.LogInfo("NPC " + EnumExtensions.GetExtendedName<Character>((int)me.Character) + " already has meta! Adding prefab instead...");
                existingMeta.prefabs.Add(me.name, me);
                return existingMeta;
            }
            NPCMetadata npcMeta = new NPCMetadata(plugin.Info, new NPC[1] { me }, me.name, flags);
            NPCMetaStorage.Instance.Add(npcMeta);
            return npcMeta;
        }

        /// <summary>
        /// Converts metadata into a list of the metadata's values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="me"></param>
        /// <returns></returns>
        public static List<T> ToValues<T>(this List<IMetadata<T>> me)
        {
            List<T> returnL = new List<T>();
            me.Do(x =>
            {
                if (x.value != null)
                {
                    returnL.Add(x.value);
                }
            });
            return returnL;
        }

        /// <summary>
        /// Converts metadata into an array of the metadata's values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="me"></param>
        /// <returns></returns>
        public static T[] ToValues<T>(this IMetadata<T>[] me)
        {
            T[] returnL = new T[me.Length];
            for (int i = 0; i < me.Length; i++)
            {
                returnL[i] = me[i].value;
            }
            return returnL;
        }

        public static ItemMetaData GetMeta(this ItemObject me)
        {
            return MTM101BaldiDevAPI.itemMetadata.Get(me);
        }

        public static NPCMetadata GetMeta(this NPC me)
        {
            return NPCMetaStorage.Instance.Get(me.Character);
        }

        public static bool AddMetaPrefab(this NPC me)
        {
            return NPCMetaStorage.Instance.AddPrefab(me);
        }

        public static RandomEventMetadata GetMeta(this RandomEvent randomEvent)
        {
            return RandomEventMetaStorage.Instance.Get(randomEvent.Type);
        }
    }
}