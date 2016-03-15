﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using GatherContent.Connector.Entities;
using GatherContent.Connector.Entities.Entities;
using GatherContent.Connector.GatherContentService.Interfaces;
using GatherContent.Connector.IRepositories.Interfaces;
using GatherContent.Connector.IRepositories.Models.Import;
using GatherContent.Connector.Managers.Interfaces;
using GatherContent.Connector.Managers.Models.ImportItems;
using GatherContent.Connector.Managers.Models.UpdateItems;
using GatherContent.Connector.SitecoreRepositories.Repositories;
using Sitecore.Diagnostics;

namespace GatherContent.Connector.Managers.Managers
{
    /// <summary>
    /// 
    /// </summary>
    public class UpdateManager : BaseManager, IUpdateManager
    {
        protected IItemsRepository ItemsRepository;

        protected IItemsService ItemsService;

        protected IMappingManager MappingManager;

        protected IImportManager ImportManager;

        protected GCAccountSettings GcAccountSettings;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemsRepository"></param>
        /// <param name="itemsService"></param>
        /// <param name="mappingManager"></param>
        /// <param name="importManager"></param>
        /// <param name="accountsService"></param>
        /// <param name="projectsService"></param>
        /// <param name="templateService"></param>
        /// <param name="cacheManager"></param>
        /// <param name="gcAccountSettings"></param>
        public UpdateManager(
            IItemsRepository itemsRepository,
            IItemsService itemsService,
            IMappingManager mappingManager,
            IImportManager importManager,
            IAccountsService accountsService,
            IProjectsService projectsService,
            ITemplatesService templateService,
            ICacheManager cacheManager,
            GCAccountSettings gcAccountSettings)
            : base(accountsService, projectsService, templateService, cacheManager)
        {
            ItemsRepository = itemsRepository;

            ItemsService = itemsService;

            MappingManager = mappingManager;

            ImportManager = importManager;

            GcAccountSettings = gcAccountSettings;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public SelectItemsForUpdateModel GetItemsForUpdate(string itemId)
        {
            var cmsItems = ItemsRepository.GetItems(itemId, "").ToList();

            List<GCTemplate> templates;
            List<GCStatus> statuses;
            List<UpdateListItem> models;
            List<Project> projects;
            TryToGetModelData(cmsItems, out templates, out statuses, out models, out projects);

            var result = new SelectItemsForUpdateModel(models, statuses, templates, projects);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmsItems"></param>
        /// <param name="templates"></param>
        /// <param name="statuses"></param>
        /// <param name="items"></param>
        /// <param name="projects"></param>
        /// <returns></returns>
        private bool TryToGetModelData(List<CmsItem> cmsItems, out List<GCTemplate> templates, out List<GCStatus> statuses, out List<UpdateListItem> items, out List<Project> projects)
        {
            var projectsDictionary = new Dictionary<int, Project>();
            var templatesDictionary = new Dictionary<int, GCTemplate>();

            statuses = new List<GCStatus>();
            items = new List<UpdateListItem>();

            foreach (var cmsItem in cmsItems)
            {
                var idField = cmsItem.Fields.FirstOrDefault(f => f.TemplateField.FieldName == "GC Content Id");
                if (idField != null && !string.IsNullOrEmpty(idField.Value.ToString()))
                {
                    ItemEntity entity = null;
                    try
                    {
                        entity = ItemsService.GetSingleItem(idField.Value.ToString());
                    }
                    catch (WebException exception)
                    {
                        Log.Error("GatherContent message. Api Server error has happened during getting Item with id = " + idField.Value.ToString(), exception);
                        using (var response = exception.Response)
                        {
                            var httpResponse = (HttpWebResponse)response;
                            if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                            {
                                throw;
                            }
                        }
                    }
                    if (entity != null)
                    {
                        GCItem gcItem = entity.Data;
                        Project project = GetProject(projectsDictionary, gcItem.ProjectId);
                        if (gcItem.TemplateId.HasValue)
                        {
                            GCTemplate template = GetTemplate(templatesDictionary, gcItem.TemplateId.Value);

                            string gcLink = null;
                            if (!string.IsNullOrEmpty(GcAccountSettings.GatherContentUrl))
                            {
                                gcLink = GcAccountSettings.GatherContentUrl + "/item/" + gcItem.Id;
                            }
                            var dateFormat = GcAccountSettings.DateFormat;
                            if (string.IsNullOrEmpty(dateFormat))
                            {
                                dateFormat = Constants.DateFormat;
                            }
                            var cmsLink =
                                string.Format(
                                    "http://{0}/sitecore/shell/Applications/Content Editor?fo={1}&sc_content=master&sc_bw=1",
                                    HttpContext.Current.Request.Url.Host, cmsItem.Id);


                            var lastUpdate = cmsItem.Fields.FirstOrDefault(f => f.TemplateField.FieldName == "Last Sync Date");

                            var cmsUpdateItem = new CMSUpdateItem(cmsItem.Id, cmsItem.Title, cmsItem.Template.TemplateId, idField.Value.ToString(), (DateTime)lastUpdate.Value);
                            var listItem = new UpdateListItem(gcItem, template, cmsUpdateItem, dateFormat, project.Name,
                                cmsLink, gcLink);
                            items.Add(listItem);

                            GCStatus status = gcItem.Status.Data;
                            if (statuses.All(i => i.Id != status.Id))
                            {
                                statuses.Add(status);
                            }
                        }
                    }
                }
            }

            items = items.OrderBy(item => item.Status.Name).ToList();
            templates = templatesDictionary.Values.ToList();
            projects = projectsDictionary.Values.ToList();

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="templates"></param>
        /// <param name="templateId"></param>
        /// <returns></returns>
        private GCTemplate GetTemplate(Dictionary<int, GCTemplate> templates, int templateId)
        {
            GCTemplate template;
            templates.TryGetValue(templateId, out template);

            if (template == null)
            {
                template = GetGcTemplateEntity(templateId.ToString()).Data;
                templates.Add(templateId, template);
            }

            return template;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projects"></param>
        /// <param name="projectId"></param>
        /// <returns></returns>
        private Project GetProject(Dictionary<int, Project> projects, int projectId)
        {
            Project project;
            projects.TryGetValue(projectId, out project);

            if (project == null)
            {
                project = GetGcProjectEntity(projectId.ToString()).Data;
                projects.Add(projectId, project);
            }

            return project;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="models"></param>
        /// <returns></returns>
        public UpdateResultModel UpdateItems(string itemId, List<UpdateListIds> models)
        {
            List<GCItem> gcItems = GetGCItemsByModels(models);
            List<MappingResultModel> resultItems = ImportManager.MapItems(gcItems);
            List<MappingResultModel> successfulyUpdated = resultItems.Where(i => i.IsImportSuccessful).ToList();

            foreach (var mappingResultModel in successfulyUpdated)
            {
                ItemsRepository.UpdateItem(new CmsItem
                {
                    
                    
                });
            }

            var result = new UpdateResultModel(resultItems);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        private List<GCItem> GetGCItemsByModels(List<UpdateListIds> models)
        {
            var result = new List<GCItem>();

            foreach (var item in models)
            {
                GCItem gcItem = ItemsService.GetSingleItem(item.GCId).Data;
                var gcItemWithCmsId = new UpdateGCItem(gcItem, item.CMSId);
                result.Add(gcItemWithCmsId);
            }

            return result;
        }
    }
}
