using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OpalStudio.Echo.Profiler.Editor.Services
{
      public static class ScriptLocator
      {
            private static readonly Dictionary<Type, MonoScript> Cache = new();

            public static bool OpenType(Type type, UnityEngine.Object liveTarget = null)
            {
                  if (type == null)
                  {
                        return false;
                  }

                  MonoScript script = ResolveMonoScript(type, liveTarget);

                  if (script == null)
                  {
                        return false;
                  }

                  return AssetDatabase.OpenAsset(script);
            }

            public static MonoScript ResolveMonoScript(Type type, UnityEngine.Object liveTarget = null)
            {
                  if (type == null)
                  {
                        return null;
                  }

                  if (Cache.TryGetValue(type, out MonoScript cached) && cached != null)
                  {
                        return cached;
                  }

                  switch (liveTarget)
                  {
                        case MonoBehaviour mb:
                              cached = MonoScript.FromMonoBehaviour(mb);

                              break;

                        case ScriptableObject so:
                              cached = MonoScript.FromScriptableObject(so);

                              break;
                  }

                  if (cached != null && cached.GetClass() == type)
                  {
                        Cache[type] = cached;

                        return cached;
                  }

                  string[] guids = AssetDatabase.FindAssets("t:MonoScript");

                  for (int i = 0; i < guids.Length; i++)
                  {
                        string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                        MonoScript candidate = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

                        if (candidate == null)
                        {
                              continue;
                        }

                        if (candidate.GetClass() == type)
                        {
                              Cache[type] = candidate;

                              return candidate;
                        }
                  }

                  return null;
            }
      }
}