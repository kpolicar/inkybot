namespace WindowsFormsApp
{
    internal static partial class Win32
    {
        //assorted constants needed
        public static int GWL_STYLE = -16;
        public static int WS_SIZEBOX = 0x00040000;
        public static int WS_BORDER = 0x00800000;
        public static int WS_DLGFRAME = 0x00400000;
        public static int WS_CAPTION = WS_BORDER | WS_DLGFRAME | WS_SIZEBOX; //window with a title bar
    }
}
