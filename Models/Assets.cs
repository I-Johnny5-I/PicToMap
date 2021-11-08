using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

namespace PicToMap.Models
{
    public class Assets
    {
        public Dictionary<string, string[]> BlocksAndColors { get; set; }
        public Bitmap Source { get; set; }

        public Assets(string imagePath)
        {
            Source = new Bitmap(imagePath);
            var json = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory() + "block_colors.json"));
            var dict = JsonSerializer.Deserialize<Dictionary<string, string[]>>(json);
            if (dict == null) throw new FormatException();
            BlocksAndColors = dict;
        }
    }
}
