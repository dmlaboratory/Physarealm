﻿using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Physarealm.Setting
{
    public class GuideSettingComponent :AbstractSettingComponent
    {
        private double guide_factor;
        /// <summary>
        /// Initializes a new instance of the GuideSettingComponent class.
        /// </summary>
        public GuideSettingComponent()
            : base("Guide Setting", "GuiSet",
                "Description",
                null, "DF4150A8-ED9A-496D-854B-9A877EF41220")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Vertical Guide Factor", "VGF", "Vertical Guide Factor", GH_ParamAccess.item, 0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Guide Setting", "GSet", "Guide Setting", GH_ParamAccess.item);
        }


        protected override bool GetInputs(IGH_DataAccess da)
        {
            if (!da.GetData(0, ref guide_factor)) return false;
            return true;
        }

        protected override void SetOutputs(IGH_DataAccess da)
        {
            AbstractSettingType gset = new GuideSettingType(guide_factor);
            da.SetData(0, gset);
        }
    }
}