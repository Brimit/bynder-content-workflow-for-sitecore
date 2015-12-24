﻿using System;
using System.Web.Mvc;
using GatherContent.Connector.Managers.Managers;
using GatherContent.Connector.Managers.Models.TemplateModel;
using Sitecore.Diagnostics;
using Sitecore.Mvc.Controllers;

namespace GatherContent.Connector.Website.Controllers
{
    public class TemplatesMappingController : SitecoreController
    { 
        private readonly TemplatesManager _templateManager;

        public TemplatesMappingController()
        {
            _templateManager = new TemplatesManager();
        }

        public ActionResult Get()
        {
            try
            {
                var model = _templateManager.GetTemplateMappingModel();
                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Log.Error("GatherContent message: " + e.Message + e.StackTrace, e);
                
                return Json(new { status = "error", message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Post(TemplateMappingModel model)
        {
            try
            {
                _templateManager.PostTemplate(model);
                return new EmptyResult();
            }
            catch (Exception e)
            {
                Log.Error("GatherContent message: " + e.Message + e.StackTrace, e);
                
                return Json(new { status = "error", message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
