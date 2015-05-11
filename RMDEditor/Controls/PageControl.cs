using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RMDEditor
{
    public class PageControl : TabControl
    {
        [DllImport("user32.dll")]
        private static extern IntPtr SetFocus(IntPtr hWnd);

        protected unsafe override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0007)
            {
                // Return focus to the old control upon gaining focus.
                SetFocus(m.WParam);
                m.Result = new IntPtr(1);
            }
            else if (m.Msg == 0x008)
            {
                // Re-enable the tab page after it rejects focus and before it gets painted.
                SelectedTab.Enabled = true;
                base.WndProc(ref m);
            }
            else if (m.Msg == 0x1328 && DesignMode)
            {
                // Trim off the extra borders while in design mode.
                Rectangle* rect = (Rectangle*)m.LParam;
                rect->X -= 4;
                rect->Width += 4;
                rect->Height += 4;
                
                base.WndProc(ref m);
            }
            else if (m.Msg == 0x1328 && !DesignMode)
            {
                // Hide tabs and remove padding by trapping the TCM_ADJUSTRECT message.
                m.Result = new IntPtr(1);
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            // Disable tab upon selecting it.
            SelectedTab.Enabled = false;
            base.OnSelectedIndexChanged(e);
        }
    }
}
