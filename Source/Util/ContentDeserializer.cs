using Newtonsoft.Json;
using System;
using System.Xml.Serialization;
using Godot;

namespace Platformer.Source.Util
{
    public class ContentDeserializer
    {

        public static T DeserializeJsonToObject<T>(string jsonFilePath)
        {
   
            try
            {

                FileAccess file = FileAccess.Open(jsonFilePath, FileAccess.ModeFlags.Read); // Open File
                
                string jsonContent = file.GetAsText();
  
                Console.WriteLine("File content successfully read.");
                T deserializedObject = JsonConvert.DeserializeObject<T>(jsonContent);

                if (deserializedObject == null)
                {
                    Console.WriteLine("Deserialization returned null.");
                }
                else
                {
                    Console.WriteLine("Deserialization succeeded.");
                }

                return deserializedObject;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deserializing JSON: {ex.Message}");
                return default;
            }

        }
    }
}
