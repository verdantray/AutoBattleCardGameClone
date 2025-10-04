using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectABC.Engine.Scene.PostFX
{
    public enum TiltShiftBlurMethod
    {
        Gaussian = 0,
        Kawase = 1
    }
    
    [Serializable][VolumeComponentMenu("Post-processing/ProjectABC/TiltShift")]
    public class TiltShift : VolumeComponent, IPostProcessComponent
    {
        [Tooltip("0=Gaussian, 1=Kawase")]
        public ClampedIntParameter method = new ClampedIntParameter((int)TiltShiftBlurMethod.Kawase, 0, 1);


        [Header("Band / Strength")]
        public ClampedFloatParameter maxStrength = new ClampedFloatParameter(1.0f, 0f, 2f);
        public Vector2Parameter tiltCenter = new Vector2Parameter(new Vector2(0.5f, 0.5f));
        public ClampedFloatParameter tiltAngle = new ClampedFloatParameter(0f, -180f, 180f); // degrees
        public ClampedFloatParameter bandHalfWidth = new ClampedFloatParameter(0.15f, 0.0f, 0.5f); // UV units
        public ClampedFloatParameter feather = new ClampedFloatParameter(0.15f, 0.0f, 1.0f); // UV units


        [Header("Gaussian")]
        public ClampedIntParameter kernelRadius = new ClampedIntParameter(8, 1, 64);
        public ClampedIntParameter quality = new ClampedIntParameter(2, 1, 4);
        public BoolParameter useAnamorphic = new BoolParameter(false);
        public Vector2Parameter anamorphXY = new Vector2Parameter(new Vector2(1f, 1f));


        [Header("Kawase")]
        public ClampedFloatParameter kawaseStep = new ClampedFloatParameter(1.0f, 0.25f, 6.0f);
        public ClampedIntParameter iterations = new ClampedIntParameter(6, 1, 12);


        [Header("Sampling")]
        [Tooltip("Downsample scale: 1=None, 2=Half, 4=Quarter")]
        public ClampedIntParameter downsample = new ClampedIntParameter(2, 1, 4);
        
        public bool IsActive()
        {
            return active
                   && maxStrength.value > 0.0f
                   && (((TiltShiftBlurMethod)method.value == TiltShiftBlurMethod.Gaussian && kernelRadius.value > 0.0f)
                       || ((TiltShiftBlurMethod)method.value == TiltShiftBlurMethod.Kawase));
        }

        public bool IsTileCompatible() => false;
    }
}
