using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Contrib.ImportExport.Extensions;
using Contrib.ImportExport.Models;
using Contrib.ImportExport.Providers;
using Contrib.ImportExport.Services;
using Contrib.ImportExport.ViewModels;
using Orchard;
using Orchard.Blogs.Services;
using Orchard.Localization;
using Orchard.UI.Admin;
using Orchard.Utility.Extensions;

namespace Contrib.ImportExport.Controllers {
    [ValidateInput(false), Admin]
    public class AdminController : Controller {
        private readonly IImportService _importService;
        private readonly IBlogService _blogService;
        private readonly IEnumerable<IBlogAssembler> _assemblers;

        public AdminController(IOrchardServices services, IImportService importService, IBlogService blogService, IEnumerable<IBlogAssembler> assemblers) {
            Services = services;
            _importService = importService;
            _blogService = blogService;
            _assemblers = assemblers;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        public ActionResult Import() {
            if (!Services.Authorizer.Authorize(Permissions.ImportBlog, T("Cannot Import Blog")))
                return new HttpUnauthorizedResult();

            var blogs = _blogService.Get().Select(o => new KeyValuePair<int, string>(o.Id, o.Name)).ToReadOnlyCollection();
            
            return View(
                new ImportAdminViewModel
                    {
                        SupportedSchemas = _assemblers.Where(x => !x.IsFeed).Select(o => o.Name).ToReadOnlyCollection(),
                        Settings = new ImportSettings { SlugPattern = @"/([^/]+)\.aspx" }, 
                        Blogs = blogs
                    });
        }

        [HttpPost, ActionName("Import")]
        public ActionResult ImportPost(ImportAdminViewModel viewModel) {
            if (!Services.Authorizer.Authorize(Permissions.ImportBlog, T("Cannot Import Blog")))
                return new HttpUnauthorizedResult();

            TryUpdateModel(viewModel.Settings);

            if (string.IsNullOrWhiteSpace(viewModel.Settings.SelectedSchema))
                ModelState.AddModelError("SelectedSchema", "You must select a schema");

            if (ModelState.IsValid) {
                if (!string.IsNullOrWhiteSpace(viewModel.Settings.UrlItemPath)) {
                    if (viewModel.Settings.UrlItemPath.IsValidUrl())
                        _importService.Import(viewModel.Settings.UrlItemPath, viewModel.Settings);
                    else
                        ModelState.AddModelError("File", T("Invalid Url specified").ToString());
                }
                else {
                    var httpPostedFileBase = Request.Files[0];
                    if (httpPostedFileBase != null && !string.IsNullOrWhiteSpace(httpPostedFileBase.FileName)) {
                        foreach (HttpPostedFileBase file in from string fileName in Request.Files select Request.Files[fileName]) {
                            _importService.Import(file, viewModel.Settings);
                        }
                    }
                    else
                        ModelState.AddModelError("File", T("Select a file to upload").ToString());
                }
            }

            viewModel.Blogs = _blogService.Get().Select(o => new KeyValuePair<int, string>(o.Id, o.Name)).ToReadOnlyCollection();
            viewModel.SupportedSchemas = _assemblers.Where(x => !x.IsFeed).Select(o => o.Name).ToReadOnlyCollection();

            return View(viewModel);
        }

        public ActionResult ImportFeed() {
            if (!Services.Authorizer.Authorize(Permissions.ImportBlog, T("Cannot Import Blog")))
                return new HttpUnauthorizedResult();

            var blogs = _blogService.Get().Select(o => new KeyValuePair<int, string>(o.Id, o.Name)).ToReadOnlyCollection();

            return View(
                new ImportFeedAdminViewModel {
                    SupportedSchemas = _assemblers.Where(x => x.IsFeed).Select(o => o.Name).ToReadOnlyCollection(),
                    Settings = new ImportSettings { SlugPattern = @"/([^/]+)\.aspx" },
                    Blogs = blogs
                });
        }

        //public ActionResult Export(int id) {
        //    if (!Services.Authorizer.Authorize(Permissions.ExportBlog, T("Cannot Export Blog")))
        //        return new HttpUnauthorizedResult();

        //    _exportService.Export(id);

        //    return Redirect("~/Admin/Blogs");
        //}
    }
}