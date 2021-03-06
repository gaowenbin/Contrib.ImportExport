using System;
using System.Collections.Generic;
using Contrib.ImportExport.InternalSchema.Post;
using Contrib.ImportExport.Models;
using Orchard.ContentManagement;
using Orchard.Events;

namespace Contrib.ImportExport.Services.Strategies {
    public interface IScheduledBlogPostCollectionImport : IEventHandler {
            void Import(ImportSettings importSettings, ContentItem parentContentItem, ICollection<Post> posts, int batchNumber);
    }

    public class ScheduledBlogPostCollectionImport : IScheduledBlogPostCollectionImport {
        private readonly IBlogPostImportStrategy _blogPostImportStrategy;
        private readonly IContentManager _contentManager;

        public ScheduledBlogPostCollectionImport(
            IBlogPostImportStrategy blogPostImportStrategy, IContentManager contentManager) {
            _blogPostImportStrategy = blogPostImportStrategy;
            _contentManager = contentManager;
        }

        public void Import(ImportSettings importSettings, ContentItem parentContentItem, ICollection<Post> posts, int batchNumber) {

            Console.WriteLine("Started Batch Number {0}", batchNumber);
            int i = 0;
            foreach (var post in posts) {
                _blogPostImportStrategy.Import(importSettings, post, parentContentItem);

                _contentManager.Clear();
                i++;
                Console.WriteLine("Batch Number {0}, Imported record {1}", batchNumber, i);
            }
            Console.WriteLine("Finished Batch Number {0}", batchNumber);
        }
    }
}