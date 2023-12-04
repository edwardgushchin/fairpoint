using System.Collections.Generic;
using System.Threading.Tasks;
using OpenQA.Selenium.Chrome;

namespace Fairpoint
{
    public abstract class Platform
    {
        public abstract string BaseUrl { get; }

        public abstract void Initialize();

        public abstract List<Advert> Update();

        public abstract Task<List<Advert>> GetProjectList(string url);
    }
}