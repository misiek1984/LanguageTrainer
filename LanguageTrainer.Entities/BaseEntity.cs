using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;

using MK.Data;

//using Raven.Imports.Newtonsoft.Json;

namespace LanguageTrainer.Entities
{
    public abstract class BaseEntity : DataEntity
    {
        [XmlIgnore]
        //[JsonIgnore]
        public override bool IsValid
        {
            get
            {
                return base.IsValid;
            }
        }

        [XmlIgnore]
        //[JsonIgnore]
        public override string Error
        {
            get
            {
                return base.Error;
            }
        }

        [XmlIgnore]
        //[JsonIgnore]
        [NotMapped]
        public override string Name
        {
            get
            {
                return base.Name;
            }
            set
            {
                base.Name = value;
            }
        }

        [XmlIgnore]
        //[JsonIgnore]
        [NotMapped]
        public bool Deleted { get; set; }
    }
}

