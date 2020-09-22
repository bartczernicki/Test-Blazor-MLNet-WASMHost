using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_Blazor_MLNet_WASMHost.Shared
{
    public sealed class BaseballDataSampleService
    {
        private static readonly BaseballDataSampleService instance = new BaseballDataSampleService();

        private BaseballDataSampleService()
        {
            SampleBaseBallData = GetSampleBaseballData().Result;
        }

        static BaseballDataSampleService()
        {
        }

        public static BaseballDataSampleService Instance
        {
            get
            {
                return instance;
            }
        }

        public IEnumerable<string> ReadLines(Func<Stream> streamProvider,
                             Encoding encoding)
        {
            using (var stream = streamProvider())
            using (var reader = new StreamReader(stream, encoding))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        public Task<List<MLBBaseballBatter>> GetSampleBaseballData()
        {
            // Return sample baseball players (batters)
            // Mix of fictitious, active & retired players of all skills

            // Note: In a production system this service would load the list of batters
            // from distributed persisted storage, searched in information retrieval engine (i.e. Azure Search, Lucene),
            // a relational database etc.

            // Load MLB baseball batters from local CSV file


            var assembly = typeof(Test_Blazor_MLNet_WASMHost.Shared.Util).Assembly;

            var lines = ReadLines(() => Util.GetBaseballData(), Encoding.UTF8);

            var batters = lines
                        .Skip(1)
                        .Select(v => MLBBaseballBatter.FromCsv(v));

            return Task.FromResult(
                batters.ToList()
            ); ;
        }

        public List<MLBBaseballBatter> SampleBaseBallData
        {
            get;
            set;
        }
   
    }
}