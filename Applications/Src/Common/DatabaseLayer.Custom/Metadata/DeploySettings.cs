using System.Collections.Generic;

namespace DatabaseLayer.Metadata
{
    public class DeploySettings
    {
        public DeploySettings()
        {
            this.IncludeDefaultData = true;
            this.DropColumnsNotPresentInTableStuctures = true;
            this.PostDeploymentScripts = new List<string>();
        }

        public bool DropColumnsNotPresentInTableStuctures { get; set; }

        public bool IncludeDefaultData { get; set; }

        public List<string> PostDeploymentScripts { get; set; }
    }
}
