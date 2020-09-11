using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WindowsFormsApp
{
    public class Item
    {
        public Dictionary<string, int> Stats = new Dictionary<string, int>();

        public Item(Dictionary<string, string> data) {
            foreach (var stat in data) {
                var numericRepresentation = Regex.Match(stat.Value, @"-?\d+").Value;
                Stats[stat.Key] = int.Parse(numericRepresentation);
            }
        }
    }
}