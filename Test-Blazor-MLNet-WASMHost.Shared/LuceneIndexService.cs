using Lucene;
using Lucene.Net;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_Blazor_MLNet_WASMHost.Shared
{
    public sealed class LuceneIndexService
    {
        private static readonly LuceneIndexService instance = new LuceneIndexService();

        private LuceneIndexService()
        {
            var assembly = typeof(Test_Blazor_MLNet_WASMHost.Shared.LuceneIndexService).Assembly;
            var test = assembly.GetManifestResourceNames();
                                                                        
            Stream resource = assembly.GetManifestResourceStream($"Test_Blazor_MLNet_WASMHost.Shared.LuceneIndex.LuceneIndex.zip");
            Console.WriteLine("LuceneIndexService - Retrieved Index Stream");

            var indexPath = Path.Combine(Environment.CurrentDirectory, "LuceneIndex.zip");
            Console.WriteLine("LuceneIndexService - Retrieved Index Stream");

            var fileStream = File.Create(indexPath);
            Console.WriteLine("LuceneIndexService - Created file stream");

            resource.CopyTo(fileStream);
            Console.WriteLine("LuceneIndexService - Copied To Stream");

            ZipFile.ExtractToDirectory(indexPath, Environment.CurrentDirectory, true);
            Console.WriteLine("LuceneIndexService - Extracted index to dir");

            var zipDirectory = FSDirectory.Open(Environment.CurrentDirectory);
            Console.WriteLine("LuceneIndexService - Opened FSI Lucene Index Dir");

            this.IndexReader = DirectoryReader.Open(zipDirectory);
            this.IndexSearcher = new IndexSearcher(this.IndexReader);
        }

        static LuceneIndexService()
        {
        }

        public static LuceneIndexService Instance
        {
            get
            {
                return instance;
            }
        }

        public DirectoryReader IndexReader
        {
            get;
            set;
        }
        public IndexSearcher IndexSearcher
        {
            get;
            set;
        }
    }
}
