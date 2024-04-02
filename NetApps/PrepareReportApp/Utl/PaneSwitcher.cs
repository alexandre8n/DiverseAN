using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace PrepareReport.Utl
{
    public class PaneSwitcher
    {
        private Visual _startingControl = null;
        public PaneSwitcher(Visual startingControl)
        {
            _startingControl = startingControl;
        }

        public void Show1(string controlName)
        {
            List<Visual> lst = WndUtl.GetSiblingsStartingFrom(_startingControl);
            foreach (Visual vCtl in lst)
            {
                string name = WndUtl.GetName(vCtl);
                if (string.Compare(controlName, name, true) == 0)
                {
                    ((UIElement)vCtl).Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    ((UIElement)vCtl).Visibility = System.Windows.Visibility.Collapsed;
                }
            }

        }
    }
}
