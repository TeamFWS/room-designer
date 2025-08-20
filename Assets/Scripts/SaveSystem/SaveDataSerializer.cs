using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace SaveSystem
{
    public static class SaveDataSerializer
    {
        private static readonly string SaveDirectory = Path.Combine(Application.persistentDataPath, "RoomLayouts");

        static SaveDataSerializer()
        {
            Directory.CreateDirectory(SaveDirectory);
        }

        public static async Task SaveLayout(MRAnchoredLayoutData layout, string filename)
        {
            try
            {
                var json = JsonUtility.ToJson(layout, true);
                var path = Path.Combine(SaveDirectory, $"{filename}.json");
                await File.WriteAllTextAsync(path, json);
                Debug.Log($"Layout saved to {path}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving layout: {e.Message}");
                throw;
            }
        }

        public static async Task<MRAnchoredLayoutData> LoadLayout(string filename)
        {
            try
            {
                var path = Path.Combine(SaveDirectory, $"{filename}.json");
                if (!File.Exists(path))
                {
                    Debug.LogWarning($"No save file found at {path}");
                    return null;
                }

                var json = await File.ReadAllTextAsync(path);
                return JsonUtility.FromJson<MRAnchoredLayoutData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading layout: {e.Message}");
                throw;
            }
        }

        public static string[] GetSavedLayouts()
        {
            return Directory.GetFiles(SaveDirectory, "*.json")
                .Select(Path.GetFileNameWithoutExtension)
                .ToArray();
        }
    }
}