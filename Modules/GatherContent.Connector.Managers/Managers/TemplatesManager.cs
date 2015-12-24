﻿using System.Collections.Generic;
using System.Linq;
using GatherContent.Connector.Entities;
using GatherContent.Connector.Entities.Entities;
using GatherContent.Connector.GatherContentService.Services;
using GatherContent.Connector.Managers.Models.TemplateModel;
using GatherContent.Connector.SitecoreRepositories;
using GatherContent.Connector.SitecoreRepositories.Repositories;

namespace GatherContent.Connector.Managers.Managers
{
    public class TemplatesManager //: BaseManager
    {

        private readonly MappingRepository _mappingRepository;
        private readonly TemplatesRepository _templatesRepository;
        private readonly ProjectsRepository _projectsRepository;


        private readonly AccountsService _accountsService;
        private readonly TemplatesService _templateService;
        private readonly ProjectsService _projectService;


        private readonly GCAccountSettings _accountSettings;

        public TemplatesManager()
        {
            var accountsRepository = new AccountsRepository();
            _accountSettings = accountsRepository.GetAccountSettings();

            _mappingRepository = new MappingRepository();
            _projectsRepository = new ProjectsRepository();
            _templatesRepository = new TemplatesRepository();

            _accountsService = new AccountsService(_accountSettings);
            _templateService = new TemplatesService(_accountSettings);
            _projectService = new ProjectsService(_accountSettings);
        }

        protected Account GetAccount()
        {
            var accounts = _accountsService.GetAccounts();
            return accounts.Data.FirstOrDefault();

        }

        protected List<Project> GetProjects(int accountId)
        {
            var projects = _projectService.GetProjects(accountId);
            return projects.Data;
        }

        public TemplateMappingModel GetTemplateMappingModel()
        {
            var model = new TemplateMappingModel();
            var account = GetAccount();

            var projects = GetProjects(account.Id);
            foreach (var project in projects)
            {
                var p = new GcProjectModel
                {
                    ProjectId = project.Id,
                    ProjectName = project.Name
                };
                var templates = _templateService.GetTemplates(project.Id.ToString());
                if (templates.Data.Count > 0)
                {
                    foreach (var template in templates.Data)
                    {
                        var isEnabled = _templatesRepository.TemplateIsEnabled(project.Id, template.Id);
                        p.Templates.Add(new GcTemplateModel
                        {
                            TemplateName = template.Name,
                            TemplateId = template.Id,
                            Enabled = isEnabled
                        });
                        if (!isEnabled)
                        {
                            model.Selected.Add(template.Id);
                        }
                    }
                    
                    model.Projects.Add(p);
                }
            }
            return model;
        }

        public void PostTemplate(TemplateMappingModel model)
        {
            foreach (var project in model.Projects)
            {
                var gcSelectedTemplates = project.Templates.Where(gcTemplateModel => model.Selected.Contains(gcTemplateModel.TemplateId)).ToList();
                if (gcSelectedTemplates.Any())
                {
                    _projectsRepository.CreateOrGetProject(_accountSettings.AccountItemId, new Project { Name = project.ProjectName, Id = project.ProjectId });

                    foreach (var gcTemplate in gcSelectedTemplates)
                    {
                        var isEnabled = _templatesRepository.TemplateIsEnabled(project.ProjectId, gcTemplate.TemplateId);
                        if (isEnabled)
                        {
                            var newTemplate = new GCTemplate
                            {
                                Name = gcTemplate.TemplateName,
                                Id = gcTemplate.TemplateId
                            };
                            _templatesRepository.CreateTemplate(project.ProjectId.ToString(), newTemplate);
                        }
                    }
                } 
            }
        }
    }
}