using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Source.Util.JsonContainers
{
    public class LoreDropperContainer
    {
        [JsonProperty("LoreDroppers")]
        public List<LoreDropperDialog> LoreDroppers = new List<LoreDropperDialog>();
    }

    public class LoreDropperDialog
    {
        [JsonProperty("ID")]
        public string ID { get; set; }

        [JsonProperty("Name")]
        public LoreDropper.LoreDropperName Name { get; set; }

        [JsonProperty("Dialog")]
        public string Dialog { get; set; }

        [JsonProperty("NextDialogID")]
        public string NextDialogID { get; set; } = null;


        public enum DialogPlaySpeed { Slow, Default, Fast}


        [JsonProperty("DialogPlaySpeed")]
        public DialogPlaySpeed PlaySpeed { get; set; } = DialogPlaySpeed.Default;


    }
}
