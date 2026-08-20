using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace DeathScreen
{
    public class CutoffMaskUI : Image
    {
        public override Material materialForRendering
        {
            get
            {
                Material tmpMaterial = new Material(base.materialForRendering);
                tmpMaterial.SetInt("_StencilComp", (int)CompareFunction.NotEqual);
                return tmpMaterial;
            }
        }
    }
}
