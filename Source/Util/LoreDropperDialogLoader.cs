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

                if (NameDialogs.ContainsKey(dialog.Name))
                {
                    NameDialogs[dialog.Name].Add(dialog.ID, dialog);

                }
                else
                {
                    NameDialogs.Add(dialog.Name, new Dictionary<string, LoreDropperDialog>());
                    NameDialogs[dialog.Name].Add(dialog.ID, dialog);
                }
            }

        }
    }
}
