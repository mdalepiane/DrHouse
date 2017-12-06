using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace DrHouse.Core
{
    [DataContract]
    public class HealthData
    {
        public HealthData(string appName)
        {
            Name = appName;
            DependenciesStatus = new List<HealthData>();
        }

        [DataMember]
        public string Name { get; private set; }

        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public string ErrorMessage { get; set; }

        [DataMember]
        private bool _isOk;
        public bool IsOK
        {
            get
            {
                return _isOk && DependenciesStatus.Where(d => d.IsOK == false).Any() == false;
            }
            set
            {
                _isOk = value;
            }
        }

        [DataMember]
        public List<HealthData> DependenciesStatus { get; set; }

        public bool ShouldSerializeDependenciesStatus()
        {
            return DependenciesStatus.Any();
        }

        public bool ShouldSerializeType()
        {
            return string.IsNullOrEmpty(Type) == false;
        }

        public bool ShouldSerializeErrorMessage()
        {
            return string.IsNullOrEmpty(ErrorMessage) == false;
        }
    }
}
