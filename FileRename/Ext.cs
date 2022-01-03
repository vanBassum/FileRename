using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace FileRename
{
    public static class Ext
    {
        public static void InvokeIfRequired(this ISynchronizeInvoke obj, MethodInvoker action)
        {
            if (obj.InvokeRequired)
            {
                var args = new object[0];
                obj.Invoke(action, args);
            }
            else
            {
                action();
            }
        }


    }

}

