using System;
using System.Collections.Generic;
using OpenTKGUI;
using ThereBeMonsters.Back_end;

namespace ThereBeMonsters.Front_end.Controls
{
    class FloatControl : EditorControl
    {
        public FloatControl(ModuleNodeControl parentNode, string paramName)
            : base(parentNode, paramName)
        {
            Textbox t =new Textbox();
            Client = t;
            t.TextEntered += (text) =>
            {
                float value;
                if(float.TryParse(text, out value)) {
                    ModuleParameterValue = value;
                }
            };
            
        }

        public override double PreferredHeight
        {
            get { return 30.0; }
        }
        public override void OnValueChanged(object sender, ModuleParameterEventArgs e)
        {
        }
    }
}
