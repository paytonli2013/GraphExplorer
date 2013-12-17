using GraphX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using YAXLib;

namespace Orc.GraphExplorer
{
    public class DataVertex: VertexBase
    {
        public string Title { get; set; }
        public int Id { get; set; }

        public Dictionary<string, string> Properties { get; set; }

        [YAXDontSerialize]
        public ImageSource Icon { get; set; }

        #region Calculated or static props
        [YAXDontSerialize]
        public DataVertex Self
        {
            get { return this; }
        }

        public override string ToString()
        {
            return Title;
        }

        #endregion

        /// <summary>
        /// Default constructor for this class
        /// (required for serialization).
        /// </summary>
        public DataVertex():this(-1,"")
        {
        }

        private static readonly Random Rand = new Random();

        public DataVertex(int id,string title = "")
        {
            base.ID = id;
            Id = id;
            Title = (title==string.Empty)?id.ToString():title;
        }
    }
}
