namespace Inkybot.Helpers
{
    public static class Numbers
    {
        public static int? Parse(string number) {
            if (number == "-")
                return null;
            return int.Parse(number);
        }
        
        public static string ToString(int? threshold) {
            return threshold?.ToString()
                   ?? "-";
        }
    }
}
