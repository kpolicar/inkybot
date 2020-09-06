using System.Drawing;
using System.Security.Permissions;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;

namespace WindowsFormsApp.Controls
{
    public class MousePositionStatusBarPanel : StatusBarPanel
    {
        private IKeyboardMouseEvents m_GlobalHook;
        private Control control;

        public void Subscribe(Control relativeTo) {
            control = relativeTo;
            m_GlobalHook = Hook.GlobalEvents();
            m_GlobalHook.MouseMove += GlobalHookMouseMoveExt;
        }

        private void GlobalHookMouseMoveExt(object sender, MouseEventArgs e) {
            var pos = control.PointToClient(e.Location);
            if (pos.X < 0 || pos.X > control.Size.Width || pos.Y < 0 || pos.Y > control.Size.Height)
                return;
            Text = $@"x: {pos.X}, y: {pos.Y}";
        }

        public void Unsubscribe()
        {
            m_GlobalHook.MouseMove -= GlobalHookMouseMoveExt;
        }

        
        protected override void Dispose(bool disposing) {
            if (disposing)
                m_GlobalHook.Dispose();
            base.Dispose(disposing);
        }
    }
}