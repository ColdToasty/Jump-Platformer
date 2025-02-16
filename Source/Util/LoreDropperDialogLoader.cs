using Godot;
using Platformer.Source.Util.JsonContainers;
using System.Collections.Generic;

namespace Platformer.Source.Util
{
    public static class LoreDropperDialogLoader
    {
        public static Dictionary<LoreDropper.LoreDropperName, Dictionary<string, LoreDropperDialog>> NameDialogs = new Dictionary<LoreDropper.LoreDropperName, Dictionary<string, LoreDropperDialog>>();

        //name > id > LoreDropperDialog


        public static void LoadLoreDropperDialogs()
        {
            //load as a list of dialogs
            List<LoreDropperDialog> dialogs = ContentDeserializer.DeserializeJsonToObject<LoreDropperContainer>("res://Source/Data/LoreDropperDialogs.json").LoreDroppers;
         
            foreach(LoreDropperDialog dialog in dialogs)
            {
                //If we have the loreDropper name already present
                if (NameDialogs.ContainsKey(dialog.Name))
                {
                    //checks if the dialog hasn't already been added
                    if (!NameDialogs[dialog.Name].ContainsKey(dialog.ID))
                    {
                        //add it 
                        NameDialogs[dialog.Name].Add(dialog.ID, dialog);
                    }
  
                }
                //if it is a new loreDropper
                else
                {
                    //Create a new dictionary
                    NameDialogs.Add(dialog.Name, new Dictionary<string, LoreDropperDialog>());
                    //add the dialog to the new loredroper 
                    NameDialogs[dialog.Name].Add(dialog.ID, dialog);
                }
            }

        }
    }
}
