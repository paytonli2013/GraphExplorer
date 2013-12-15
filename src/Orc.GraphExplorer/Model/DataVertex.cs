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
        public DateTime DateTimeValue;
        public string Title { get; set; }
        public int Id { get; set; }

        [YAXDontSerialize]
        public ImageSource DataImage { get; set; }

        [YAXDontSerialize]
        public ImageSource PersonImage { get; set; }

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

        private string[] imgArray = new string[4]
        {
            @"pack://application:,,,/GraphX;component/Images/help_black.png",
            @"pack://application:,,,/ShowcaseExample;component/Images/skull_bw.png",
            @"pack://application:,,,/ShowcaseExample;component/Images/wrld.png",
            @"pack://application:,,,/ShowcaseExample;component/Images/birdy.png",
        };
        private string[] textArray = new string[4]
        {
            @"",
            @"Skully",
            @"Worldy",
            @"Birdy",
        };

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
            Id = id;
            Title = title;
        }
    }
}
